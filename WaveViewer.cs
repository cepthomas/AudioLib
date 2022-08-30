using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using NBagOfTricks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Runtime;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


// TODO make mouse etc commands configurable.


namespace AudioLib
{
    /// <summary>
    /// Simple mono wave display.
    /// </summary>
    public partial class WaveViewer : UserControl, ISampleProvider
    {
        #region Fields
        /// <summary>For drawing text.</summary>
        readonly Font _textFont = new("Cascadia", 9, FontStyle.Bold, GraphicsUnit.Point, 0);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        /// <summary>The data buffer.</summary>
        float[] _vals = Array.Empty<float>();

        /// <summary>Extent of _vals.</summary>
        float _min = 0;

        /// <summary>Extent of _vals.</summary>
        float _max = 0;

        /// <summary>Make this look like a stream for sample provider.</summary>
        int _position = 0;

        /// <summary>Grid Y resolution. Assumes +-1.0f range.</summary>
        const float GRID_STEP = 0.25f;

        /// <summary>For painting. Essentially the zoom factor.</summary>
        int _samplesPerPixel = 0;

        /// <summary>Means fully zoomed out.</summary>
        int _samplesPerPixelMax = 0;
        #endregion

        #region Backing fields
        float _gain = 1.0f;
        int _selStart = -1;
        int _selLength = 0;
        int _marker = -1;
        readonly Pen _penDraw = new(Color.Black, 1);
        readonly Pen _penGrid = new(Color.LightGray, 1);
        readonly Pen _penMark = new(Color.Red, 1);
        readonly SolidBrush _brushMark = new(Color.White);
        #endregion

        #region Properties
        /// <summary>Gets the WaveFormat of this Sample Provider. ISampleProvider implementation.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibDefs.SAMPLE_RATE, 1);

        /// <summary>The waveform color.</summary>
        public Color DrawColor { get { return _penDraw.Color; } set { _penDraw.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color GridColor { get { return _penGrid.Color; } set { _penGrid.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color MarkColor { get { return _penMark.Color; } set { _penMark.Color = value; _brushMark.Color = value; Invalidate(); } }

        /// <summary>Client gain adjustment.</summary>
        public float Gain { get { return _gain; } set { _gain = value; Invalidate(); } }

        /// <summary>UI gain adjustment.</summary>
        public float GainIncrement { get; set; } = 0.05f;

        /// <summary>How fast the mouse wheel goes.</summary>
        public int WheelResolution { get; set; } = 8;

        /// <summary>Zoom increment.</summary>
        public int ZoomFactor { get; set; } = 20;

        /// <summary>Pan increment.</summary>
        public int PanFactor { get; set; } = 10;

        /// <summary>Length of the clip in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Length { get { return _vals.Length; } }

        /// <summary>Length of the clip in seconds.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TimeSpan TotalTime { get { return TimeSpan.FromSeconds((double)Length / WaveFormat.SampleRate); } }

        /// <summary>Selection start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelStart { get { return _selStart; } set { _selStart = value; Invalidate(); } }

        /// <summary>Selection length in samples. Could be negative.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelLength { get { return _selLength; } set { _selLength = value; Invalidate(); } }

        /// <summary>Current cursor location.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Marker { get { return _marker; } set { _marker = value; Invalidate(); } }

        /// <summary>Visible start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisStart { get; private set; } = 0;

        /// <summary>Visible length in samples. Always positive.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisLength { get; private set; } = 0;
        #endregion

        #region Events
        /// <summary>Value changed by user.</summary>
        public event EventHandler? GainChangedEvent;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Default constructor. Mainly for designer.
        /// </summary>
        public WaveViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        /// <summary>
        /// Set everything from data source. Client must do this before setting properties as some are overwritten.
        /// </summary>
        /// <param name="prov">Source</param>
        public void Init(ISampleProvider prov)
        {
            var all = prov.ReadAll();
            _vals = all.vals;
            _min = all.min;
            _max = all.max;

            _selStart = -1;
            _selLength = 0;
            _marker = -1;
            VisStart = 0;
            VisLength = _vals.Length;

            _samplesPerPixel = 0;
            _samplesPerPixelMax = 0;

            ValidateProperties();

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
                _brushMark.Dispose();
                _penDraw.Dispose();
                _penGrid.Dispose();
                _penMark.Dispose();
                _format.Dispose();
                _textFont.Dispose();
           }
           base.Dispose(disposing);
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Put back to area of interest.
        /// </summary>
        public void Rewind()
        {
            _position = _selStart;
        }

        /// <summary>
        /// Fill the buffer with selected data. ISampleProvider implementation.
        /// </summary>
        /// <param name="buffer">The buffer to fill with samples.</param>
        /// <param name="offset">Offset into buffer.</param>
        /// <param name="count">The number of samples to read.</param>
        /// <returns>the number of samples written to the buffer.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int numRead = 0;

            int numToRead = Math.Min(count, _selLength - _position);
            for (int n = 0; n < numToRead; n++)
            {
                buffer[n + offset] = _vals[_position];
                _position++;
                numRead++;
            }
            return numRead;
        }

        /// <summary>
        /// Fit the wave exactly.
        /// </summary>
        public void FitGain()
        {
            float max = Math.Max(Math.Abs(_max), Math.Abs(_min));
            Gain = 1.0f / max;
        }
        #endregion

        #region UI handlers
        /// <summary>
        /// Handle mouse wheel.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

            // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
            int delta = WheelResolution * e.Delta / SystemInformation.MouseWheelScrollDelta;

            if (ModifierKeys == Keys.Control) // x zoom
            {
                int incr = _samplesPerPixelMax / ZoomFactor; // zoom factor TODO1 too fast for zoom in.

                if (delta > 0) // zoom in
                {
                    _samplesPerPixel -= incr;
                }
                else if (delta < 0) // zoom out
                {
                    _samplesPerPixel += incr;
                }

                _samplesPerPixel = MathUtils.Constrain(_samplesPerPixel, 1, _samplesPerPixelMax);

                // Update visible. Note these calcs will be checked in ValidateProperties().
                // Zooming is around the mouse position or marker if provided.
                int center = _marker > 0 ? _marker : VisStart + VisLength / 2;
                int visSamples = Width * _samplesPerPixel;

                VisStart = center - visSamples / 2;
                VisLength = visSamples;

                Invalidate();
            }
            else if (ModifierKeys == Keys.None) // no mods = x shift TODO1 not quite right
            {
                int incr = _samplesPerPixel * PanFactor;

                if (delta > 0) // pan right
                {
                    VisStart += incr;
                    VisStart = MathUtils.Constrain(VisStart, 0, _vals.Length - VisLength - 1);
                    Invalidate();
                }
                else if (delta < 0) // pan left
                {
                    VisStart -= incr;
                    VisStart = MathUtils.Constrain(VisStart, 0, _vals.Length - VisLength - 1);
                    Invalidate();
                }
                // else ignore
            }
            else if (ModifierKeys == Keys.Shift) // gain
            {
                _gain += delta > 0 ? GainIncrement : -GainIncrement;
                _gain = (float)MathUtils.Constrain(_gain, 0.0f, AudioLibDefs.MAX_GAIN);
                GainChangedEvent?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>
        /// Handle mouse clicks.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (ModifierKeys == Keys.None) // marker
                    {
                        _marker = PixelToSample();
                        Invalidate();
                    }
                    else if (ModifierKeys == Keys.Control) // sel start
                    {
                        if(_selLength > 0)
                        {
                            var ends = _selStart + _selLength;
                            _selStart = PixelToSample();
                            _selLength = ends - _selStart;
                        }
                        else
                        {
                            _selStart = PixelToSample();
                        }

                        Invalidate();
                    }
                    else if (ModifierKeys == Keys.Shift && _selStart != -1) // sel end
                    {
                        var sel = PixelToSample();
                        _selLength = sel - _selStart;
                        Invalidate();
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle mouse move.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // if (e.Button == MouseButtons.Left)
            // {
            //     _current = GetTimeFromMouse(e.X);
            //     CurrentTimeChanged?.Invoke(this, new EventArgs());
            // }
            // else
            // {
            //     if (e.X != _lastXPos)
            //     {
            //         TimeSpan ts = GetTimeFromMouse(e.X);
            //         _toolTip.SetToolTip(this, ts.ToString(AudioLibDefs.TS_FORMAT));
            //         _lastXPos = e.X;
            //     }
            // }
            //
            // Invalidate();

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Key press.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.G: // reset gain
                    _gain = 1.0f;
                    Invalidate();
                    e.Handled = true;
                    break;

                case Keys.H: // reset to initial view
                    VisStart = 0;
                    VisLength = _vals.Length;
                    Invalidate();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Resize handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
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
                pe.Graphics.DrawString("No data", _textFont, Brushes.DarkGray, ClientRectangle, _format);
            }
            else
            {
                // Draw everything from bottom up.

                // Y grid lines.
                _penGrid.Width = 1;
                float yMin = -5 * GRID_STEP;
                float yMax = 5 * GRID_STEP;
                for (float gs = yMin; gs <= yMax; gs += GRID_STEP)
                {
                    float yGrid = MathUtils.Map(gs, yMin, yMax, 0, Height);
                    pe.Graphics.DrawLine(_penGrid, 50, yGrid, Width, yGrid);
                    pe.Graphics.DrawString($"{-gs:0.00}", _textFont, _penGrid.Brush, 25, yGrid, _format);
                }

                // Y zero is a bit thicker.
                _penGrid.Width = 5;
                float yZero = MathUtils.Map(0.0f, 1.0f, -1.0f, 0, Height);
                pe.Graphics.DrawLine(_penGrid, 0, yZero, Width, yZero);

                // Info.
                //pe.Graphics.DrawString($"Gain:{_gain:0.00}", _textFont, _penGrid.Brush, 50, 10);
                pe.Graphics.DrawString($"Gain:{_gain:0.00} VST:{VisStart} VLN:{VisLength} MRK:{Marker}", _textFont, Brushes.Black, 50, 10);

                // Then the data.
                if (_samplesPerPixel > 1)
                {
                    var peaks = PeakProvider.GetPeaks(_vals, VisStart, _samplesPerPixel, Width);

                    for (int i = 0; i < peaks.Count; i++)
                    {
                        // +1 => 0  -1 => Height
                        int max = (int)MathUtils.Map(peaks[i].max * _gain, 1.0f, -1.0f, 0, Height);
                        int min = (int)MathUtils.Map(peaks[i].min * _gain, 1.0f, -1.0f, 0, Height);

                        // Make sure there's always at least one dot.
                        if (max == min)
                        {
                            if (max > 0) { min--; }
                            else { max++; }
                        }

                        pe.Graphics.DrawLine(_penDraw, i, max, i, min);
                    }
                }
                else
                {
                    // Not enough data - just show what we have.
                    for (int i = 0; i < _vals.Length; i++)
                    {
                        // +1 => 0  -1 => Height
                        int yVal = (int)MathUtils.Map(_vals[i] * _gain, 1.0f, -1.0f, 0, Height);
                        pe.Graphics.DrawRectangle(_penDraw, i, yVal, 1, 1);
                    }
                }

                // Selection markers and cursor.
                if (_selStart != -1)
                {
                    int x = SampleToPixel(_selStart);
                    if (x >= 0)
                    {
                        pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                        pe.Graphics.DrawRectangle(_penMark, x, 10, 10, 10);
                    }
                }

                if (_selLength > 0)
                {
                    int x = SampleToPixel(_selStart + _selLength);
                    if (x >= 0)
                    {
                        pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                        pe.Graphics.DrawRectangle(_penMark, x - 10, 10, 10, 10);
                    }
                }

                if (_marker != -1)
                {
                    int x = SampleToPixel(_marker);
                    if (x >= 0)
                    {
                        pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                    }
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
            _selStart = MathUtils.Constrain(_selStart, -1, _vals.Length);
            _selLength = MathUtils.Constrain(_selLength, -_vals.Length, _vals.Length);

            _marker = MathUtils.Constrain(_marker, -1, _vals.Length);

            VisStart = MathUtils.Constrain(VisStart, 0, _vals.Length);

            if (VisLength == 0)
            {
                VisLength = _vals.Length;
            }
            VisLength = MathUtils.Constrain(VisLength, 0, _vals.Length - VisStart);

            // Get resolution, rounds up to ensure always visible.
            _samplesPerPixel = VisLength / Width + 1;
            _samplesPerPixelMax = _vals.Length / Width + 1;
        }

        /// <summary>
        /// Convert x pos to sample index.
        /// </summary>
        /// <param name="pixel">UI loc or -1 if get current mouse.</param>
        int PixelToSample(int pixel = -1)
        {
            if (pixel < 0)
            {
                pixel = PointToClient(MousePosition).X;
            }
            int sample = MathUtils.Map(pixel, 0, Width, VisStart, VisStart + VisLength);
            return sample;
        }

        /// <summary>
        /// Find sample visible location.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns>The pixel or -1 if not visible.</returns>
        int SampleToPixel(int sample)
        {
            int pixel = -1;

            if (sample > VisStart && sample < VisStart + VisLength)
            {
                int offset = sample - VisStart;
                pixel = offset / _samplesPerPixel;


                //pixel = MathUtils.Map(sample, VisStart, VisStart + VisLength, 0, Width);
            }
            return pixel;
        }

        /// <summary>
        /// Snap to user preference.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        int DoSnap(int sample) // TODO1
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
        #endregion
    }
}
