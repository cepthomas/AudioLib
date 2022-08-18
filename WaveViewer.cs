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
        readonly Pen _penDraw = new(Color.Black, 1);

        /// <summary>For drawing.</summary>
        readonly Pen _penMarker = new(Color.LightGray, 2);

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

        /// <summary>The waveform color.</summary>
        public Color DrawColor { get { return _penDraw.Color; } set { _penDraw.Color = value; Invalidate(); } }

        /// <summary>Y adjustment.</summary>
        public float YGain { get { return _yGain; } set { _yGain = value; Invalidate(); } }
        float _yGain = 1.0f;

        /// <summary>Marker sample index.</summary>
        public int Marker { get { return _marker; } set { _marker = value; Invalidate(); } }
        int _marker = -1;

        /// <summary>Maximum Y gain.</summary>
        float _maxGain = 5.0f;

        /// <summary>Grid resolution.</summary>
        float _gridStep = 0.25f;
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
                _penDraw.Dispose();
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

        #region UI events
        /// <summary>
        /// Zoom Y.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

            // If mouse is within control
            if (hme.X <= Width && hme.Y <= Height)
            {
                _yGain += hme.Delta > 0 ? 0.1f : -0.1f;
                _yGain = (float)MathUtils.Constrain(_yGain, 0.0f, _maxGain);
                Invalidate();
            }

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Handle reset.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                _yGain = 1.0f;
                Invalidate();
            }

            base.OnKeyDown(e);
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

                // grid lines
                for (float gs = -5 * _gridStep; gs <= 5 * _gridStep; gs += _gridStep)
                {
                    float yGrid = MathUtils.Map(gs, 1.0f, -1.0f, 0, Height);
                    pe.Graphics.DrawLine(_penMarker, 0, yGrid, Width, yGrid);
                }

                // vals
                for (int i = 0; i < peaks.Count; i++)
                {
                    // +1 => 0  -1 => Height
                    //int border = 5;
                    //float yMax = MathUtils.Map(peaks[i].max, 1.0f, -1.0f, border, Height - border);
                    //float yMin = MathUtils.Map(peaks[i].min, 1.0f, -1.0f, border, Height - border);
                    int yMax = (int)MathUtils.Map(peaks[i].max * _yGain, 1.0f, -1.0f, 0, Height);
                    int yMin = (int)MathUtils.Map(peaks[i].min * _yGain, 1.0f, -1.0f, 0, Height);
                    
                    // Make sure there's at least one dot.
                    if (yMax == yMin)
                    {
                        if(yMax > 0) { yMin--; }
                        else { yMax++; }
                    }

                    pe.Graphics.DrawLine(_penDraw, i, yMax, i, yMin);
                }

                // marker
                int xmarker = MathUtils.Map(_marker, 0, _vals.Length, 0, Width);
                pe.Graphics.DrawLine(_penDraw, xmarker, 0, xmarker, Height);
            }
        }
        #endregion
    }
}
