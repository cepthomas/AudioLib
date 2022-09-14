﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;


// TODO make mouse etc commands configurable.

namespace AudioLib
{
    /// <summary>
    /// Simple mono wave display.
    /// </summary>
    public partial class WaveViewer : UserControl
    {
        #region Fields
        /// <summary>For drawing text.</summary>
        readonly Font _textFont = new("Calibri", 10, FontStyle.Regular, GraphicsUnit.Point, 0);

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

        /// <summary>For painting. Essentially the zoom factor.</summary>
        int _samplesPerPixel = 0;

        /// <summary>Simple display only.</summary>
        bool _simple = false;

        /// <summary>Last pixel.</summary>
        int _lastXPos = 0;
        #endregion

        #region Constants
        /// <summary>UI gain adjustment.</summary>
        const float GAIN_INCREMENT = 0.05f;

        /// <summary>How fast the mouse wheel goes.</summary>
        const int WHEEL_RESOLUTION = 8;

        /// <summary>Zoom increment.</summary>
        const int ZOOM_INCREMENT = 20;

        /// <summary>Number of pixels to x shift by.</summary>
        const int PAN_INCREMENT = 10;
        #endregion

        #region Backing fields
        float _gain = 1.0f;
        WaveSelectionMode _selectionMode = WaveSelectionMode.Sample;//TODO1
        int _visibleStart = 0;
        int _selStart = 0;
        int _selLength = 0;
        int _marker = 0;
        readonly Pen _penDraw = new(Color.Black, 1);
        readonly Pen _penGrid = new(Color.LightGray, 1);
        readonly Pen _penMark = new(Color.Red, 1);
        private ToolTip toolTip;
        private IContainer components;
        readonly SolidBrush _brushMark = new(Color.White);
        #endregion

        #region Properties
        /// <summary>The waveform color.</summary>
        public Color DrawColor { set { _penDraw.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color GridColor { set { _penGrid.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color MarkColor { set { _penMark.Color = value; _brushMark.Color = value; Invalidate(); } }

        /// <summary>Client gain adjustment.</summary>
        public float Gain { get { return _gain; } set { _gain = value; Invalidate(); } }

        /// <summary>Length of the clip in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Length { get { return _vals.Length; } }

        /// <summary>Length of the clip in seconds.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TimeSpan TotalTime { get { return TimeSpan.FromSeconds((double)Length / AudioLibDefs.SAMPLE_RATE); } }

        /// <summary>Selection start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelStart { get { return _selStart; } } // set { _selStart = value; Invalidate(); } }

        /// <summary>Selection length in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelLength { get { return _selLength; } } // set { _selLength = value; Invalidate(); } }

        /// <summary>General purpose marker location.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Marker { get { return _marker; } set { _marker = value; CheckSel(); Invalidate(); } }

        /// <summary>Visible start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisibleStart { get { return _visibleStart; } } // set { _visibleStart = value; Invalidate(); } }

        /// <summary>Visible length in samples. Always positive.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisibleLength { get { return Width * _samplesPerPixel; } }
        #endregion

        #region Events
        /// <summary>Value changed by user.</summary>
        public event EventHandler? GainChangedEvent;

        /// <summary>Value changed by user.</summary>
        public event EventHandler? SelectionChangedEvent;

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
            //InitializeComponent();
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            this.Name = "WaveViewer";
            this.ResumeLayout(false);
        }


        /// <summary>
        /// Set everything from data source. Client must do this before setting properties as some are overwritten.
        /// </summary>
        /// <param name="prov">Source</param>
        /// <param name="simple">If true simple display only.</param>
        public void Init(ISampleProvider prov, bool simple = false)
        {
            _simple = simple;
            _vals = prov.ReadAll();
            _max = _vals.Length > 0 ? _vals.Max() : 0;
            _min = _vals.Length > 0 ? _vals.Min() : 0;

            _selStart = 0;
            _selLength = 0;
            _marker = 0;

            if(_simple)
            {
                FitGain();
            }
            
            ResetView();

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
        /// Fit the wave exactly.
        /// </summary>
        public void FitGain()
        {
            float max = Math.Max(Math.Abs(_max), Math.Abs(_min));
            Gain = 1.0f / max;
        }

        /// <summary>
        /// Pan to new center location.
        /// </summary>
        /// <param name="sample">Center around this.</param>
        public void Recenter(int sample)
        {
            // Recenter.
            _visibleStart = sample - VisibleLength / 2;
            CheckSel();
            Invalidate();
        }
        #endregion

        #region UI handlers
        /// <summary>
        /// Handle mouse wheel.
        ///  - If ctrl, X zoom.
        ///  - If shift, Y gain.
        ///  - Else X pan.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!_simple)
            {
                HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
                hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

                // Number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant.
                int delta = WHEEL_RESOLUTION * e.Delta / SystemInformation.MouseWheelScrollDelta;

                switch(ModifierKeys)
                {
                    case Keys.None: //  x pan
                        int incr = _samplesPerPixel * PAN_INCREMENT;
                        _visibleStart += delta > 0 ? incr : -incr; // left or right
                        _visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length);
                        Invalidate();
                        break;

                    case Keys.Control: // x zoom
                        // Get sample to center about.
                        int center = _marker;
                        // Or? PixelToSample(Width / 2), PixelToSample(MouseX());

                        // Modify the zoom factor.
                        int samplesPerPixelMax = _vals.Length / Width;
                        incr = _samplesPerPixel / ZOOM_INCREMENT;
                        _samplesPerPixel += delta > 0 ? -incr : incr; // in or out
                        _samplesPerPixel = MathUtils.Constrain(_samplesPerPixel, 0, samplesPerPixelMax);

                        Recenter(center);
                        break;

                    case Keys.Shift: // gain
                        _gain += delta > 0 ? GAIN_INCREMENT : -GAIN_INCREMENT;
                        _gain = (float)MathUtils.Constrain(_gain, 0.0f, AudioLibDefs.MAX_GAIN);
                        GainChangedEvent?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                        break;
                };
            }

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Handle mouse clicks to select things.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            SnapType snap = GetSnapType();
            var sample = PixelToSample(e.X);

            switch (e.Button, _selectionMode, ModifierKeys)
            {
                case (MouseButtons.Left, WaveSelectionMode.Sample, Keys.None): // marker
                    _marker = Converters.SnapSample(sample, snap);
                    CheckSel();
                    MarkerChangedEvent?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Time, Keys.None): // marker TODO1
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Beat, Keys.None): // marker TODO1

                    break;

                case (MouseButtons.Left, WaveSelectionMode.Sample, Keys.Control): // sel start
                    if(!_simple)
                    {
                        if (_selLength == 0)
                        {
                            _selStart = sample;
                        }
                        else
                        {
                            var ends = _selStart + _selLength;
                            _selStart = sample;
                            _selLength = ends - _selStart;
                        }
                        CheckSel();
                        SelectionChangedEvent?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                    }
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Time, Keys.Control): // sel start TODO1
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Beat, Keys.Control): // sel start TODO1
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Sample, Keys.Shift): // sel end
                    if (!_simple && _selStart > 0)
                    {
                        var sel = sample;
                        _selLength = sel - _selStart;
                        CheckSel();
                        SelectionChangedEvent?.Invoke(this, EventArgs.Empty);
                        Invalidate();
                    }
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Time, Keys.Shift): // sel end TODO1
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Beat, Keys.Shift): // sel end TODO1
                    break;
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Handle mouse move.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.X != _lastXPos)
            {
                SnapType snap = GetSnapType();
                var sample = PixelToSample(e.X);

                switch (_selectionMode)
                {
                    case WaveSelectionMode.Time:
                        TimeSpan tm = Converters.SampleToTime(sample, snap);
                        toolTip.SetToolTip(this, tm.ToString(AudioLibDefs.TS_FORMAT));
                        break;

                    case WaveSelectionMode.Beat:

                        break;

                    case WaveSelectionMode.Sample:
                        sample = Converters.SnapSample(sample, snap);
                        toolTip.SetToolTip(this, sample.ToString());
                        break;
                }

                _lastXPos = e.X;

                Invalidate();
            }


            if (e.Button == MouseButtons.Left)
            {
                //_current = GetTimeFromMouse(e.X);
                //CurrentTimeChanged?.Invoke(this, new EventArgs());
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Key press.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!_simple)
            {
                switch (e.KeyCode)
                {
                    case Keys.G: // reset gain
                        _gain = 1.0f;
                        Invalidate();
                        e.Handled = true;
                        break;

                    case Keys.H: // reset to initial full view
                        ResetView();
                        Invalidate();
                        e.Handled = true;
                        break;

                    case Keys.M: // go to marker
                        if (_marker > 0)
                        {
                            Recenter(_marker);
                            e.Handled = true;
                        }
                        break;

                    case Keys.S: // go to selection
                        if (_selStart > 0)
                        {
                            Recenter(_selStart);
                            e.Handled = true;
                        }
                        break;
                }
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Resize handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            // Recalc scale.
            _samplesPerPixel = Width > 0 ? _vals.Length / Width : 0;
            Invalidate();
            base.OnResize(e);
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Paint the waveform.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            const int Y_NUM_LINES = 5;
            const float Y_SPACING = 0.25f;
            const int X_NUM_LINES = 10; // approx

            // Setup.
            pe.Graphics.Clear(BackColor);

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, _textBrush, ClientRectangle, _format);
            }
            else
            {
                // Draw everything from bottom up.
                if (!_simple)
                {
              //      SnapType snap = GetSnapType();

                    // Y grid lines.
                    _penGrid.Width = 1;
                    for (int i = -Y_NUM_LINES; i <= Y_NUM_LINES; i++)
                    {
                        float val = i * Y_SPACING;
                        float yGrid = MathUtils.Map(val, -Y_NUM_LINES * Y_SPACING, Y_NUM_LINES * Y_SPACING, 0, Height);

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
                            case Y_NUM_LINES:
                            case -Y_NUM_LINES:
                                // No label.
                                break;
                            default:
                                pe.Graphics.DrawLine(_penGrid, 50, yGrid, Width, yGrid);
                                pe.Graphics.DrawString($"{-val:0.00}", _textFont, _textBrush, 25, yGrid, _format);
                                break;
                        }
                    }

                    // X grid lines.
                    switch (_selectionMode)
                    {
                        case WaveSelectionMode.Time:
                            //TimeSpan tstart = Converters.SampleToTime(VisibleStart, snap);
                            //TimeSpan tend = Converters.SampleToTime(VisibleStart + VisibleLength, snap);
                            //TimeSpan tlen = tend - tstart;


                            //// anywhere from 10 msec to MaxClipSize (10 min)
                            //// 0.01 -> 600.0
                            //TimeSpan incr = tlen / X_NUM_LINES;

                            //int sincr = VisibleLength / X_NUM_LINES;
                            break;

                        case WaveSelectionMode.Beat:
                            // - Beats mode:
                            //   - Establish timing by select two samples and identify corresponding number of beats.
                            //   - Show in waveform.
                            //   - Subsequent selections are by beat using snap.

                            // 123:2.456.
                            // 

                            break;

                        case WaveSelectionMode.Sample:
                            // - Sample mode:
                            //   - Select two samples using ?? resolution.
                            //   - Shows number of samples and time in UI.
                            int sampincr = VisibleLength / X_NUM_LINES;
                            // Round to a reasonable value.
                            int digits = MathUtils.NumDigits(sampincr);

                            for (int xs = 0; xs < VisibleLength; xs += sampincr)
                            {
                                float xGrid = MathUtils.Map(xs, 0, VisibleLength, 0, Width);
                                pe.Graphics.DrawLine(_penGrid, xGrid, 0, xGrid, Height);
                                pe.Graphics.DrawString($"{xs}", _textFont, _textBrush, xGrid, 10, _format);
                            }
                            break;
                    }

                    // Show info.
                    var sinfo = $"Gain:{_gain:0.00}  VisStart:{_visibleStart}  Mark:{Marker}  SPP:{_samplesPerPixel}  VisLength:{VisibleLength}";
                    pe.Graphics.DrawString(sinfo, _textFont, _textBrush, Width / 2, Height - 20, _format);
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
                    if (_selStart > 0)
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

                if (_marker > 0)
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
        /// Make sure user entered values are harmonious.
        /// </summary>
        void CheckSel()
        {
            // Do a few sanity checks.
            _selStart = MathUtils.Constrain(_selStart, 0, _vals.Length);
            _selLength = MathUtils.Constrain(_selLength, 0, _vals.Length - _selStart);
            _marker = MathUtils.Constrain(_marker, 0, _vals.Length);
            _visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        void ResetView()
        {
            _visibleStart = 0;
            _samplesPerPixel = _vals.Length / Width;
        }

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

            return pixel;
        }

        /// <summary>
        /// Helper.
        /// </summary>
        /// <returns></returns>
        int MouseX()
        {
            return PointToClient(MousePosition).X;
        }

        /// <summary>
        /// Snap to user preference.
        /// </summary>
        /// <returns></returns>
        SnapType GetSnapType()
        {
            SnapType snap = ModifierKeys switch
            {
                Keys.Control => SnapType.None,
                Keys.Shift => SnapType.None,
                _ => SnapType.None, //TODO1 or get setting?
            };

            return snap;
        }
        #endregion
    }
}
