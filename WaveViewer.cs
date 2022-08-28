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


// TODO make mouse etc commands configurable. Does client need to know about these ops.


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
        #endregion


        // Navigation.
        // navBar.SmallChange = 1;
        // navBar.LargeChange = 100;
        //
        // //     Gets or sets a numeric value that represents the current position of the scroll box on the scroll bar control.
        // public int Value { get; set; }
        // //     Gets or sets the value to be added to or subtracted from the System.Windows.Forms.ScrollBar.Value property when the scroll box is moved a small distance.
        // public int SmallChange { get; set; }
        // //     Gets or sets the lower limit of values of the scrollable range.
        // public int Minimum { get; set; }
        // //     Gets or sets a value to be added to or subtracted from the System.Windows.Forms.ScrollBar.Value property when the scroll box is moved a large distance.
        // public int LargeChange { get; set; }
        // //     Gets or sets the foreground color of the scroll bar control.
        // public override Color ForeColor { get; set; }
        // //     Gets or sets the background image layout as defined in the System.Windows.Forms.ImageLayout enumeration.
        // public override ImageLayout BackgroundImageLayout { get; set; }
        // //     Gets or sets the background image displayed in the control.
        // public override Image? BackgroundImage { get; set; }
        // //     Gets or sets the upper limit of values of the scrollable range.
        // public int Maximum { get; set; }
        // //     Gets or sets a value indicating whether the System.Windows.Forms.ScrollBar is automatically resized to fit its contents.
        // public override bool AutoSize { get; set; }
        // //     Gets or sets the background color for the control.
        // public override Color BackColor { get; set; }

        // // Events:
        // public event MouseEventHandler? MouseClick;
        // public event EventHandler? DoubleClick;
        // public event MouseEventHandler? MouseDoubleClick;
        // public event MouseEventHandler? MouseDown;
        // public event MouseEventHandler? MouseUp;
        // public event MouseEventHandler? MouseMove;
        // public event ScrollEventHandler? Scroll;
        // public event EventHandler? ValueChanged;
        // protected override void OnMouseWheel(MouseEventArgs e);
        // protected virtual void OnScroll(ScrollEventArgs se);
        // protected virtual void OnValueChanged(EventArgs e);



        /// <summary>For painting. Essentially the zoom factor.</summary>
        int _samplesPerPixel = 0;

        /// <summary>Means fully zoomed out.</summary>
        int _samplesPerPixelMax = 0;




        #region Backing fields
        float _gain = 1.0f;
  //      readonly SolidBrush _brushSel = new(Color.White);
        readonly Pen _penDraw = new(Color.Black, 1);
        readonly Pen _penGrid = new(Color.LightGray, 1);
        readonly Pen _penMark = new(Color.Red, 1);
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
        public Color MarkColor { get { return _penMark.Color; } set { _penMark.Color = value; Invalidate(); } }
        //public Color SelColor { get { return _brushSel.Color; } set { _brushSel.Color = value; Invalidate(); } }

        /// <summary>Client gain adjustment.</summary>
        public float Gain { get { return _gain; } set { _gain = value; Invalidate(); } }

        /// <summary>UI gain adjustment.</summary>
        public float GainIncrement { get; set; } = 0.05f;

        ///// <summary>There isn't enough data to fill full width so disallow navigation.</summary>
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        //public bool Frozen { get; private set; } = false;

        /// <summary>Length of the clip in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Length { get { return _vals.Length; } }

        /// <summary>Length of the clip in seconds.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TimeSpan TotalTime { get { return TimeSpan.FromSeconds((double)Length / WaveFormat.SampleRate); } }

        /// <summary>Selection start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelStart { get; set; } = 0; //TODO1 should I invalidate or client?

        /// <summary>Selection length in samples. Could be negative.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelLength { get; set; } = 0;

        /// <summary>Current cursor location.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int ViewerCursor { get; set; } = -1;

        /// <summary>Visible start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisStart { get; set; } = 0;

        /// <summary>Visible length in samples. Always positive.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisLength { get; set; } = 0;
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
        /// Set everything from data source. Do this before setting properties as some are overwritten.
        /// </summary>
        /// <param name="prov">Source</param>
        public void Init(ISampleProvider prov)
        {
            var all = prov.ReadAll();
            _vals = all.vals;
            _min = all.min;
            _max = all.max;

            SelStart = -1;
            SelLength = 0;
            ViewerCursor = -1;
            VisStart = 0;
            VisLength = _vals.Length;

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
                _penDraw.Dispose();
                _textFont.Dispose();
                _format.Dispose();
           }
           base.Dispose(disposing);
        }
        #endregion

        #region Public functions
        /// <summary>Put back to area of interest.</summary>
        public void Reset()
        {
            _position = SelStart;
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

            int numToRead = Math.Min(count, SelLength - _position);
            if (numToRead > 0)
            {
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
        #endregion



        /// <summary>How fast the mouse wheel goes.</summary>
        public int WheelResolution { get; set; } = 8;

        public int ZoomFactor { get; set; } = 20;

        public int PanFactor { get; set; } = 10;



        #region UI handlers
        /// <summary>
        /// Handle mouse wheel.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

            if (ModifierKeys == Keys.Control) // x zoom TODO1
            {
                // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
                int delta = WheelResolution * e.Delta / SystemInformation.MouseWheelScrollDelta;
                int incr = _samplesPerPixelMax / ZoomFactor; // zoom factor

                if (delta > 0) // zoom in
                {
                    _samplesPerPixel -= incr;
                    _samplesPerPixel = MathUtils.Constrain(_samplesPerPixel, 1, _samplesPerPixelMax);
                    Invalidate();
                }
                else if (delta < 0) // zoom out
                {
                    _samplesPerPixel += incr;
                    _samplesPerPixel = MathUtils.Constrain(_samplesPerPixel, 1, _samplesPerPixelMax);
                    Invalidate();
                }
                // else ignore
            }
            else if (ModifierKeys == Keys.None) // no mods = x shift TODO1
            {
                int delta = WheelResolution * e.Delta / SystemInformation.MouseWheelScrollDelta;
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
                _gain += hme.Delta > 0 ? 0.01f : -0.01f;
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

                    if (ModifierKeys == Keys.None)
                    {
                        ViewerCursor = PixelToSample();
                        Invalidate();
                    }
                    else if (ModifierKeys == Keys.Control)
                    {
                        SelStart = PixelToSample();
                        Invalidate();
                    }
                    else if (ModifierKeys == Keys.Shift && SelStart != -1)
                    {
                        var sel = PixelToSample();
                        SelLength = sel - SelStart;
                        Invalidate();
                    }
                    break;
            }
        }

        // /// <summary>
        // /// Handle mouse move.
        // /// </summary>
        // /// <param name="e"></param>
        // protected override void OnMouseMove(MouseEventArgs e)
        // {
        //     if (e.Button == MouseButtons.Left)
        //     {
        //         _current = GetTimeFromMouse(e.X);
        //         CurrentTimeChanged?.Invoke(this, new EventArgs());
        //     }
        //     else
        //     {
        //         if (e.X != _lastXPos)
        //         {
        //             TimeSpan ts = GetTimeFromMouse(e.X);
        //             _toolTip.SetToolTip(this, ts.ToString(AudioLibDefs.TS_FORMAT));
        //             _lastXPos = e.X;
        //         }
        //     }

        //     // Invalidate();
        //     base.OnMouseMove(e);
        // }

        /// <summary>
        /// Key press.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                //case Keys.Escape:
                case Keys.G: // reset gain
                    _gain = 1.0f;
                    Invalidate();
                    break;

                case Keys.H: // reset to initial view
                    VisStart = 0;
                    VisLength = _vals.Length;
                    Invalidate();
                    break;
            }
        }

        /// <summary>
        /// Resize.
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

                /////// First selection area.
                //if (!Frozen)
                //{
                //    if (SelStart != -1 && SelLength > 0)
                //    {
                //        for (int i = 0; i < SelLength; i++)
                //        {

                //        }
                //    }
                //}
                //else
                //{

                //}
                
                ///// Grid.
                // Y grid lines
                _penGrid.Width = 1;
                for (float gs = -5 * GRID_STEP; gs <= 5 * GRID_STEP; gs += GRID_STEP)
                {
                    float yGrid = MathUtils.Map(gs, GainIncrement, -GainIncrement, 0, Height);
                    pe.Graphics.DrawLine(_penGrid, 0, yGrid, Width, yGrid);
                    pe.Graphics.DrawString($"{gs:0.00}", _textFont, _penGrid.Brush, 25, yGrid, _format);
                }

                // Y zero is a bit thicker.
                _penGrid.Width = 3;
                float yZero = MathUtils.Map(0.0f, 1.0f, -1.0f, 0, Height);
                pe.Graphics.DrawLine(_penGrid, 0, yZero, Width, yZero);

                // Info.
                pe.Graphics.DrawString($"Gain:{_gain:0.00}", _textFont, _penGrid.Brush, 100, 10, _format);


                // Then the data.
                if (_samplesPerPixel > 1)
                {
                    var peaks = PeakProvider.GetPeaks(_vals, VisStart, _samplesPerPixel, Width);
                    for (int i = 0; i < peaks.Count; i++)
                    {
                        // +1 => 0  -1 => Height
                        int yMax = (int)MathUtils.Map(peaks[i].max * _gain, 1.0f, -1.0f, 0, Height);
                        int yMin = (int)MathUtils.Map(peaks[i].min * _gain, 1.0f, -1.0f, 0, Height);

                        // Make sure there's always at least one dot.
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
                        int yVal = (int)MathUtils.Map(_vals[i] * _gain, 1.0f, -1.0f, 0, Height);
                        pe.Graphics.DrawRectangle(_penDraw, i, yVal, 1, 1);
                    }
                }

                // Selection markers and cursor.
                if (SelStart != -1)
                {
                    int x = SampleToPixel(SelStart);
                    if (x >= 0)
                    {
                        pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                        pe.Graphics.DrawRectangle(_penMark, x, 10, 10, 10);
                    }
                }

                if (SelLength > 0)
                {
                    int x = SampleToPixel(SelStart + SelLength);
                    if (x >= 0)
                    {
                        pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                        pe.Graphics.DrawRectangle(_penMark, x - 10, 10, 10, 10);
                    }
                }

                if (ViewerCursor != -1)
                {
                    int x = SampleToPixel(ViewerCursor);
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
            SelStart = MathUtils.Constrain(SelStart, 0, _vals.Length);
            SelLength = MathUtils.Constrain(SelLength, -_vals.Length, _vals.Length);

            ViewerCursor = MathUtils.Constrain(ViewerCursor, -1, _vals.Length);

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
            if(pixel < 0)
            {
                pixel = PointToClient(Cursor.Position).X;
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

            if(sample > VisStart && sample < VisStart + VisLength)
            {
                pixel = MathUtils.Map(sample, VisStart, VisStart + VisLength, 0, Width);
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
