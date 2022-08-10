using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NBagOfTricks;
using NAudio.Wave;

namespace AudioLib
{
    public partial class WaveViewer : UserControl
    {
        #region Fields
        /// <summary>For drawing.</summary>
        readonly Pen _pen = new(Color.Black, 1);

        /// <summary>For drawing.</summary>
        readonly Pen _penMarker = new(Color.Black, 2);

        /// <summary>For drawing text.</summary>
        readonly Font _textFont = new("Cascadia", 12, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        #endregion

        #region Properties
        /// <summary>The full buffer from client.</summary>
        public float[] Values { get { return _vals; } set { _vals = value; Invalidate(); } }
        float[] _vals = Array.Empty<float>();

        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _pen.Color; } set { _pen.Color = value; } }

        /// <summary>Marker index.</summary>
        public int Marker
        {
            get { return _marker; }
            set { _marker = value; Invalidate(); }
        }
        int _marker = -1;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public WaveViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
           if (disposing)
           {
                _pen.Dispose();
                _textFont.Dispose();
                _format.Dispose();
           }
           base.Dispose(disposing);
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Paints the waveform.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Setup.
            pe.Graphics.Clear(BackColor);

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, Brushes.Gray, ClientRectangle, _format);
            }
            else
            {
                int border = 5;
                int fitWidth = Width - (2 * border);
                int fitHeight = Height - (2 * border);
                int numVals = _vals.Length;
                int samplesPerPixel = _vals.Length / fitWidth;

                var peaks = PeakProvider.GetPeaks(_vals, 0, samplesPerPixel, fitWidth);

                for (int i = 0; i < peaks.Count; i++)
                {
                    float yMax = border + fitHeight - ((peaks[i].max + 1) * 0.5f * fitHeight);
                    float yMin = border + fitHeight - ((peaks[i].min + 1) * 0.5f * fitHeight);
                    pe.Graphics.DrawLine(_pen, i + border, yMax, i + border, yMin);
                }

                // Draw  marker.
                _marker = MathUtils.Constrain(_marker, 0, _vals.Length);
                if (_marker > 0)
                {
                    int mpos = _marker * (fitWidth / numVals);
                    pe.Graphics.DrawLine(_penMarker, mpos, 0, mpos, Height);
                }
            }
        }
        #endregion
    }
}
