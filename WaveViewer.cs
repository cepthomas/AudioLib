using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NBagOfTricks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;


namespace AudioLib
{
    /// <summary>Simple mono wave display.</summary>
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

        /// <summary>The data buffer.</summary>
        float[] _vals = Array.Empty<float>();
        #endregion

        #region Properties
        /// <summary>The provider from client. Mono only.</summary>
        public ISampleProvider SampleProvider { set { _vals = value.ReadAll(); Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _pen.Color; } set { _pen.Color = value; Invalidate(); } }

        /// <summary>Y adjustment.</summary>
        public float YGain { get { return _yGain; } set { _yGain = value; Invalidate(); } }
        float _yGain = 0.8f;

        /// <summary>Marker sample index.</summary>
        public int Marker { get { return _marker; } set { _marker = value; Invalidate(); } }
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

        /// <summary>
        /// Hard reset.
        /// </summary>
        public void Reset()
        {
            _vals = Array.Empty<float>();
            _marker = -1;
            Invalidate();
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Paint the waveform.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Setup.
            pe.Graphics.Clear(BackColor);

            if(_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, Brushes.Gray, ClientRectangle, _format);
            }
            else
            {
                int samplesPerPixel = _vals.Length / Width;
                var peaks = PeakProvider.GetPeaks(_vals, 0, samplesPerPixel, Width);

                for (int i = 0; i < peaks.Count; i++)
                {
                    // +1 => 0  -1 => Height
                    //int border = 5;
                    //float yMax = MathUtils.Map(peaks[i].max, 1.0f, -1.0f, border, Height - border);
                    //float yMin = MathUtils.Map(peaks[i].min, 1.0f, -1.0f, border, Height - border);
                    float yMax = MathUtils.Map(peaks[i].max * _yGain, 1.0f, -1.0f, 0, Height);
                    float yMin = MathUtils.Map(peaks[i].min * _yGain, 1.0f, -1.0f, 0, Height);
                    pe.Graphics.DrawLine(_pen, i, yMax, i, yMin);
                }

                // 0 line
                pe.Graphics.DrawLine(_penMarker, 0, Height / 2, Width, Height / 2);

                // marker
                int xmarker = MathUtils.Map(_marker, 0, _vals.Length, 0, Width);
                pe.Graphics.DrawLine(_penMarker, xmarker, 0, xmarker, Height);
            }
        }
        #endregion
    }
}
