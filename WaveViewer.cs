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
using NAudio.Mixer;




//TODO1 cmbSelMode: Sample, Beat, Time + Snap on/off
// - Beats mode:
//   - Establish timing by select two samples and identify corresponding number of beats.
//   - Show in waveform.
//   - Subsequent selections are by beat using snap.
// - Time mode:
//   - Select two times using ?? resolution.
//   - Shows number of samples and time in UI.
// - Sample mode:
//   - Select two samples using ?? resolution.
//   - Shows number of samples and time in UI.


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
        readonly Brush _textBrush = Brushes.Black;

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

        /// <summary>For painting. Essentially the zoom factor.</summary>
        int _samplesPerPixel = 0;

        /// <summary>Means fully zoomed out.</summary>
        int _samplesPerPixelMax = 0;

        /// <summary>Simple display only.</summary>
        bool _simple = false;
        #endregion

        #region Backing fields
        float _gain = 1.0f;
        int _visibleStart = -1;
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




        //>>>>>> TODO1 these need to be updated from main form.
        /// <summary>Snap control.</summary>
        public bool Snap { get; set; } = true;

        /// <summary>How to select wave.</summary>
        public WaveSelectionMode SelectionMode { get; set; } = WaveSelectionMode.Sample;

        public double BPM { get; set; } = 100.0;



        //>>>>>> TODO these could be from user settings.
        /// <summary>UI gain adjustment.</summary>
        public float GainIncrement { get; set; } = 0.05f;

        /// <summary>How fast the mouse wheel goes.</summary>
        public int WheelResolution { get; set; } = 8;

        /// <summary>Zoom increment.</summary>
        public int ZoomIncrement { get; set; } = 20;

        /// <summary>Number of pixels to x shift by.</summary>
        public int ShiftIncrement { get; set; } = 10;





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

        /// <summary>General purpose marker location.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Marker { get { return _marker; } set { _marker = value; Invalidate(); } }

        /// <summary>Visible start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisibleStart { get { return _visibleStart; } set { _visibleStart = value; Invalidate(); } }

        /// <summary>Visible length in samples. Always positive.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisibleLength { get { return Width * _samplesPerPixel; } }
        #endregion

        #region Events
        /// <summary>Value changed by user.</summary>
        public event EventHandler? GainChangedEvent;

        /// <summary>Value changed by user.</summary>
        public event EventHandler? MarkerChangedEvent;
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
        /// <param name="simple">If true simple display only.</param>
        public void Init(ISampleProvider prov, bool simple = false)
        {
            _simple = simple;

            var (vals, max, min) = prov.ReadAll();
            _vals = vals;
            _min = min;
            _max = max;

            _selStart = -1;
            _selLength = 0;
            _marker = -1;

            if(_simple)
            {
                FitGain();
            }
            
            ResetView();

            Invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        void ResetView()
        {
            _visibleStart = 0;
            _samplesPerPixel = _vals.Length / Width;
            _samplesPerPixelMax = _samplesPerPixel;
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

            if (!_simple)
            {
                int numToRead = Math.Min(count, _selLength - _position);
                for (int n = 0; n < numToRead; n++)
                {
                    buffer[n + offset] = _vals[_position];
                    _position++;
                    numRead++;
                }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        public void Center(int sample)
        {
            // Recenter.
            _visibleStart = sample - VisibleLength / 2;
            Invalidate();
        }
        #endregion

        #region UI handlers
        /// <summary>
        /// Handle mouse wheel.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (_simple)
            {
                return;
            }

            HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

            // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
            int delta = WheelResolution * e.Delta / SystemInformation.MouseWheelScrollDelta;

            if (ModifierKeys == Keys.Control) // x zoom
            {
                // Get current center sample - or mouse or marker... TODO1?
                int center = PixelToSample(Width / 2);
                //int center = PixelToSample(MouseX());
                //int center = _marker;
                //int centerSample = _marker > 0 ? _marker : PixelToSample(MouseX());

                // Modify the zoom factor.  TODO1 should be muliplier/nonlinear? get rid of _samplesPerPixelMax then.  doesn't stay quite centered
                int incr = _samplesPerPixelMax / ZoomIncrement;
                _samplesPerPixel += delta > 0 ? -incr : incr; // in or out
                _samplesPerPixel = MathUtils.Constrain(_samplesPerPixel, 0, _samplesPerPixelMax);

                // Recenter.
                Center(center);

                // original:
                //// Update visible. Note these calcs will be checked in ValidateProperties().
                //// Zooming is around the mouse position or marker if provided.
                //int center = Marker > 0 ? Marker : VisStart + VisLength / 2;
                //int visSamples = Width * _samplesPerPixel;

                //Invalidate();
            }
            else if (ModifierKeys == Keys.None) // no mods = x shift/pan
            {
                int incr = _samplesPerPixel * ShiftIncrement;
                _visibleStart += delta > 0 ? incr : -incr; // left or right
                _visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length);
                //_visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length - VisibleLength);
                Invalidate();

                // was
                //int incr = _samplesPerPixel * PanFactor;

                //if (delta > 0) // pan right
                //{
                //    VisStart += incr;
                //    VisStart = MathUtils.Constrain(VisStart, 0, _vals.Length - VisLength - 1);
                //    Invalidate();
                //}
                //else if (delta < 0) // pan left
                //{
                //    VisStart -= incr;
                //    VisStart = MathUtils.Constrain(VisStart, 0, _vals.Length - VisLength - 1);
                //    Invalidate();
                //}
                //// else ignore
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
                        _marker = PixelToSample(MouseX());
                        Invalidate();
                        MarkerChangedEvent?.Invoke(this, EventArgs.Empty);
                    }
                    else if (!_simple && ModifierKeys == Keys.Control) // sel start
                    {
                        if(_selLength > 0)
                        {
                            var ends = _selStart + _selLength;
                            _selStart = PixelToSample(MouseX());
                            _selLength = ends - _selStart;
                        }
                        else
                        {
                            _selStart = PixelToSample(MouseX());
                        }

                        Invalidate();
                    }
                    else if (!_simple && ModifierKeys == Keys.Shift && _selStart != -1) // sel end
                    {
                        var sel = PixelToSample(MouseX());
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
            if (_simple)
            {
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.G: // reset gain
                    _gain = 1.0f;
                    Invalidate();
                    e.Handled = true;
                    break;

                case Keys.H: // reset to initial view
                    ResetView();
                    Invalidate();
                    e.Handled = true;
                    break;

                case Keys.M: // go to marker
                    if(_marker != -1)
                    {
                        Center(_marker);
                        //Invalidate();
                        e.Handled = true;
                    }
                    break;

                case Keys.S: // go to selection
                    if (_selStart != -1)
                    {
                        Center(_selStart);
                        //Invalidate();
                        e.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Resize handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            // Recalc scale.
            _samplesPerPixel = _vals.Length / Width;
            _samplesPerPixelMax = _samplesPerPixel;

            Invalidate();
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Paint the waveform.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            const int NUM_Y_GRID = 5;
            const float Y_GRID_SPACING = 0.25f;


            //for (int i = -5; i <= 5; i++)
            //{
            //    float val = i * 0.25f;
            //    float yGrid = MathUtils.Map(val, -1.25f, 1.25f, 0, Height);





            // Setup.
            pe.Graphics.Clear(BackColor);

            // Do a few sanity checks.
            _selStart = MathUtils.Constrain(_selStart, -1, _vals.Length);
            _selLength = MathUtils.Constrain(_selLength, -_vals.Length, _vals.Length);
            _marker = MathUtils.Constrain(_marker, -1, _vals.Length);
            _visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length);

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, _textBrush, ClientRectangle, _format);
            }
            else
            {
                // Draw everything from bottom up.
                if (!_simple)
                {
                    // Y grid lines.
                    _penGrid.Width = 1;
                    for (int i = -NUM_Y_GRID; i <= NUM_Y_GRID; i++)
                    {
                        float val = i * Y_GRID_SPACING;
                        float yGrid = MathUtils.Map(val, -NUM_Y_GRID * Y_GRID_SPACING, NUM_Y_GRID * Y_GRID_SPACING, 0, Height);

                        // Some special treatments.
                        switch(i)
                        {
                            case 0:
                                // A bit thicker.
                                _penGrid.Width = 5;
                                pe.Graphics.DrawLine(_penGrid, 50, yGrid, Width, yGrid);
                                _penGrid.Width = 1;
                                pe.Graphics.DrawString($"{-val:0.00}", _textFont, _textBrush, 25, yGrid, _format);
                                break;
                            case NUM_Y_GRID:
                            case -NUM_Y_GRID:
                                // No label.
                                break;
                            default:
                                pe.Graphics.DrawLine(_penGrid, 50, yGrid, Width, yGrid);
                                pe.Graphics.DrawString($"{-val:0.00}", _textFont, _textBrush, 25, yGrid, _format);
                                break;
                        }
                    }

                    // X grid lines.
                    int numLines = 10; // user prop?
                    _penGrid.Width = 1;

                    switch (SelectionMode)
                    {
                        case WaveSelectionMode.Time:
                            TimeSpan start = AudioLibUtils.SampleToTime(VisibleStart);
                            TimeSpan end = AudioLibUtils.SampleToTime(VisibleStart + VisibleLength);
                            TimeSpan tlen = end - start;
                            // anywhere from 10 msec to MaxClipSize (10 min)
                            TimeSpan incr = tlen / numLines;

                            int sincr = VisibleLength / numLines;

                            for (int xs = 0; xs < VisibleLength; xs += sincr)
                            {
                                float xGrid = MathUtils.Map(xs, 0, VisibleLength, 0, Width);
                                pe.Graphics.DrawLine(_penGrid, xGrid, 0, xGrid, Height);
                                pe.Graphics.DrawString($"{xs}", _textFont, _textBrush, xGrid, 10, _format);
                            }

                            break;

                        case WaveSelectionMode.Beat:

                            break;

                        case WaveSelectionMode.Sample:

                            break;
                    }




                    // Info.
                    //pe.Graphics.DrawString($"Gain:{_gain:0.00}", _textFont, _textBrush, 50, 10);
                    pe.Graphics.DrawString($"Gain:{_gain:0.00} Vstart:{_visibleStart} Mark:{Marker}", _textFont, _textBrush, 50, 10);
                    pe.Graphics.DrawString($"Spp:{_samplesPerPixel} SppMax:{_samplesPerPixelMax} VisibleLength:{VisibleLength}", _textFont, _textBrush, 50, 30);
                }

                // Then the data.
                if (_samplesPerPixel > 0)
                {
                    var peaks = PeakProvider.GetPeaks(_vals, _visibleStart, _samplesPerPixel, Width);

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

                // Selection and markers.
                if (!_simple)
                {
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
        /// Convert x pos to sample index.
        /// </summary>
        /// <param name="pixel">UI location.</param>
        /// <returns>The sample or -1 if not visible.</returns>
        int PixelToSample(int pixel)
        {
            int sample = -1;

            if(pixel >= 0 && pixel < Width)
            {
                sample = pixel * _samplesPerPixel + _visibleStart;
            }

            // was:
            //if (pixel < 0)
            //{
            //    pixel = PointToClient(MousePosition).X;
            //}
            //int sample = MathUtils.Map(pixel, 0, Width, _visibleStart, _visibleStart + VisibleLength);

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

            if (_samplesPerPixel > 0)
            {
                int offset = sample - _visibleStart;
                pixel = offset / _samplesPerPixel;

                if (pixel < 0 || pixel >= Width)
                {
                    pixel = -1;
                }
            }

            // was:
            //if (sample > _visibleStart && sample < _visibleStart + VisibleLength)
            //{
            //    pixel = MathUtils.Map(sample, _visibleStart, _visibleStart + VisLength, 0, Width);
            //}

            return pixel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int MouseX()
        {
            return PointToClient(MousePosition).X;
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
