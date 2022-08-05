using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NBagOfTricks;


namespace AudioLib
{
    public partial class WaveViewer : UserControl
    {
        #region Fields
        /// <summary>The full buffer from client.</summary>
        float[] _vals = Array.Empty<float>();

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
        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _pen.Color; } set { _pen.Color = value; } }

        /// <summary>Marker index.</summary>
        public int Marker
        {
            get { return _marker; }
            set { _marker = MathUtils.Constrain(value, 0, _vals.Length); Invalidate(); }
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

        #region Public functions
        /// <summary>
        /// Populate with data in +/-1.0f units.
        /// </summary>
        /// <param name="vals">Values to display</param>
        public void Init(float[] vals)
        {
            _vals = vals;
            Invalidate();
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
                // https://stackoverflow.com/a/1215472
                int border = 5;
                float fitWidth = Width - (2 * border);
                float fitHeight = Height - (2 * border);
                float numVals = _vals.Length;


                //float zoom = 0.01f;
                //size *= zoom;

                for (int index = 0; index < fitWidth; index++)
                {
                    // Determine start and end points within vals.
                    float start = index * (numVals / fitWidth);
                    float end = (index + 1) * (numVals / fitWidth);
                    float min = float.MaxValue;
                    float max = float.MinValue;
                    for (int i = (int)start; i < end; i++)
                    {
                        float val = _vals[i];
                        min = val < min ? val : min;
                        max = val > max ? val : max;
                    }
                    float yMax = border + fitHeight - ((max + 1) * 0.5f * fitHeight);
                    float yMin = border + fitHeight - ((min + 1) * 0.5f * fitHeight);
                    pe.Graphics.DrawLine(_pen, index + border, yMax, index + border, yMin);
                }

                // Draw  marker.
                if (_marker > 0)
                {
                    int mpos = (int)(_marker * (fitWidth / numVals));
                    pe.Graphics.DrawLine(_penMarker, mpos, 0, mpos, Height);
                }
            }
        }
        #endregion
    }
}
