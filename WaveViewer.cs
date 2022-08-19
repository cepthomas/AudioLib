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
        /// <summary>For drawing data.</summary>
        readonly Pen _penDraw = new(Color.Black, 1);

        /// <summary>For drawing grid.</summary>
        readonly Pen _penGrid = new(Color.LightGray, 1);

        /// <summary>For drawing text.</summary>
        readonly Font _textFont = new("Cascadia", 12, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        /// <summary>The data buffer.</summary>
        float[] _vals = Array.Empty<float>();

        /// <summary>Current Y gain.</summary>
        float _yGain = 1.0f;

        /// <summary>Maximum Y gain.</summary>
        float _maxGain = 5.0f;

        /// <summary>Grid Y resolution. Assumes +-1.0f range.</summary>
        float _gridStep = 0.25f;
        #endregion

        #region Properties
        /// <summary>The waveform color.</summary>
        public Color DrawColor { get { return _penDraw.Color; } set { _penDraw.Color = value; Invalidate(); } }

        // /// <summary>For styling.</summary>
        // public Color MarkerColor { get { return _penMarker.Color; } set { _penMarker.Color = value; } }

        /// <summary>Y adjustment.</summary>
        public float YGain { get { return _yGain; } set { _yGain = value; Invalidate(); } }

        /// <summary>If there isn't enough data to fill full width.</summary>
        public bool Frozen { get; private set; } = false;

        /// <summary>Snap to this increment value.</summary>
        public float SnapTODO { get; set; } = 0;

        /// <summary>Selection start.</summary>
        public int SelStart { get; set; } = -1;

        /// <summary>Selection length.</summary>
        public int SelLength { get; set; } = 0;

        /// <summary>One marker sample index.</summary>
        public int Marker1 { get; set; } = -1;

        /// <summary>Other marker sample index.</summary>
        public int Marker2 { get; set; } = -1;

        /// <summary>Visible start sample.</summary>
        public int VisStart { get; set; } = 0;

        /// <summary>Visible length.</summary>
        public int VisLength { get; set; } = 0;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Default constructor.
        /// </summary>
        public WaveViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Set everything up from data source. Do this before setting properties by client.
        /// </summary>
        /// <param name="prov">Source</param>
        public void Init(ISampleProvider prov)
        {
            _vals = prov.ReadAll();

            _yGain = 1.0f;
            SelStart = -1;
            SelLength = 0;
            Marker1 = -1;
            Marker2 = -1;
            VisStart = 0;
            VisLength = _vals.Length;

            Invalidate();
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
        #endregion

        #region UI handlers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

            if (ModifierKeys == Keys.Control) // x zoom TODO
            {
                if(!Frozen)
                {
                    Invalidate();
                }
            }
            else if (ModifierKeys == Keys.Shift) // y gain
            {
                _yGain += hme.Delta > 0 ? 0.1f : -0.1f;
                _yGain = (float)MathUtils.Constrain(_yGain, 0.0f, _maxGain);
                Invalidate();
            }
            else if (ModifierKeys == Keys.None) // no mods = x shift TODO
            {
                if (!Frozen)
                {
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Handle mouse wheel.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:

                    if (ModifierKeys == Keys.None)
                    {
                        Marker1 = GetSampleFromMouse();
                        Invalidate();
                    }
                    else if (ModifierKeys == Keys.Control)
                    {
                        Marker2 = GetSampleFromMouse();
                        Invalidate();
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //case Keys.Escape:
                case Keys.G:
                    //_firstPaint = true;
                    _yGain = 1.0f;
                    Invalidate();
                    break;

                case Keys.H:
                    //_firstPaint = true;
                    VisStart = 0;
                    VisLength = _vals.Length;
                    Invalidate();
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            //_firstPaint = true; // Need to recalc the grid too.
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

            ValidateProperties(); // good time to do this

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, Brushes.Gray, ClientRectangle, _format);
            }
            else
            {
                // First the grid lines.
                _penGrid.Width = 1;
                for (float gs = -5 * _gridStep; gs <= 5 * _gridStep; gs += _gridStep)
                {
                    float yGrid = MathUtils.Map(gs, 1.0f, -1.0f, 0, Height);
                    pe.Graphics.DrawLine(_penGrid, 0, yGrid, Width, yGrid);
                }

                // Y zero.
                _penGrid.Width = 3;
                float yZero = MathUtils.Map(0.0f, 1.0f, -1.0f, 0, Height);
                pe.Graphics.DrawLine(_penGrid, 0, yZero, Width, yZero);

                // Then the data.
                if(!Frozen)
                {
                    int samplesPerPixel = VisLength / Width;
                    var peaks = PeakProvider.GetPeaks(_vals, VisStart, samplesPerPixel, Width);
                    for (int i = 0; i < peaks.Count; i++)
                    {
                        // +1 => 0  -1 => Height
                        int yMax = (int)MathUtils.Map(peaks[i].max * _yGain, 1.0f, -1.0f, 0, Height);
                        int yMin = (int)MathUtils.Map(peaks[i].min * _yGain, 1.0f, -1.0f, 0, Height);

                        // Make sure there's at least one dot.
                        if (yMax == yMin)
                        {
                            if (yMax > 0) { yMin--; }
                            else { yMax++; }
                        }

                        pe.Graphics.DrawLine(_penDraw, i, yMax, i, yMin);
                    }
                }
                else
                {
                    // Not enough data - just show what we have.
                    for (int i = 0; i < _vals.Length; i++)
                    {
                        // +1 => 0  -1 => Height
                        int yVal = (int)MathUtils.Map(_vals[i] * _yGain, 1.0f, -1.0f, 0, Height);
                        pe.Graphics.DrawRectangle(_penDraw, i, yVal, 1, 1);
                    }
                }

                // Then draw markers.
                if (Marker1 != -1)
                {
                    int x = MathUtils.Map(Marker1, 0, _vals.Length, 0, Width);
                    pe.Graphics.DrawLine(_penDraw, x, 0, x, Height);
                }
                if (Marker2 != -1)
                {
                    int x = MathUtils.Map(Marker2, 0, _vals.Length, 0, Width);
                    pe.Graphics.DrawLine(_penDraw, x, 0, x, Height);
                }
            }
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Check sanity of client selections.
        /// </summary>
        void ValidateProperties()
        {
            SelStart = MathUtils.Constrain(SelStart, -1, _vals.Length);
            SelLength = MathUtils.Constrain(SelLength, 0, _vals.Length);

            Marker1 = MathUtils.Constrain(Marker1, -1, _vals.Length);
            Marker2 = MathUtils.Constrain(Marker2, -1, _vals.Length);

            VisStart = MathUtils.Constrain(VisStart, 0, _vals.Length);
            if (VisLength == 0) { VisLength = _vals.Length; }
            VisLength = MathUtils.Constrain(VisLength, 0, _vals.Length - VisStart);

            int samplesPerPixel = VisLength / Width;
            Frozen = samplesPerPixel == 0;
        }

        /// <summary>
        /// Convert x pos to sample index.
        /// </summary>
        /// <param name="x">UI loc or -1 if get current.</param>
        int GetSampleFromMouse(int x = -1)
        {
            if(x < 0)
            {
                x = PointToClient(Cursor.Position).X;
            }
            int sample = MathUtils.Map(x, 0, Width, VisStart, VisStart + VisLength);
            return sample;
        }

        ///// <summary>
        ///// Convert x pos to TimeSpan.
        ///// </summary>
        ///// <param name="x"></param>
        //TimeSpan GetTimeFromMouse(int x)
        //{
        //    int msec = 0;

        //    if (_current.TotalMilliseconds < _length.TotalMilliseconds)
        //    {
        //        msec = x * (int)_length.TotalMilliseconds / Width;
        //        msec = MathUtils.Constrain(msec, 0, (int)_length.TotalMilliseconds);
        //        msec = DoSnap(msec);
        //    }

        //    return new TimeSpan(0, 0, 0, 0, msec);
        //}

        /// <summary>
        /// Snap to user preference.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        int DoSnap(int sample) // TODO
        {
            //int smsec = 0;

            //if (SnapMsec > 0)
            //{
            //    smsec = (msec / SnapMsec) * SnapMsec;
            //    if (SnapMsec > (msec % SnapMsec) / 2)
            //    {
            //        smsec += SnapMsec;
            //    }
            //}

            return sample;
        }

        ///// <summary>
        ///// Utility helper function.
        ///// </summary>
        ///// <param name="val"></param>
        ///// <param name="lower"></param>
        ///// <param name="upper"></param>
        ///// <returns></returns>
        //TimeSpan Constrain(TimeSpan val, TimeSpan lower, TimeSpan upper)
        //{
        //    return TimeSpan.FromMilliseconds(MathUtils.Constrain(val.TotalMilliseconds, lower.TotalMilliseconds, upper.TotalMilliseconds));
        //}

        ///// <summary>
        ///// Map from time to UI pixels.
        ///// </summary>
        ///// <param name="val"></param>
        ///// <returns></returns>
        //int Scale(TimeSpan val)
        //{
        //    return (int)(val.TotalMilliseconds * Width / _length.TotalMilliseconds);
        //}

        #endregion

    }
}
