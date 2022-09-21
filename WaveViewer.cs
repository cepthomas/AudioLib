using System;
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
    /// <summary>Simple mono wave display.</summary>
    public partial class WaveViewer : UserControl
    {
        #region Fields
        /// <summary>Function set.</summary>
        ViewerMode _viewMode = ViewerMode.Full;
        enum ViewerMode { Full, Thumbnail }

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

        /// <summary>Last pixel.</summary>
        int _lastXPos = 0;

        /// <summary>How to snap.</summary>
        SnapType _snap = SnapType.Fine;
        #endregion

        #region Constants
        /// <summary>UI gain adjustment.</summary>
        const float GAIN_INCREMENT = 0.05f;

        /// <summary>Scroll rate. Smaller means faster.</summary>
        const int WHEEL_RESOLUTION = 8;

        /// <summary>Zoom ratio.</summary>
        const float ZOOM_RATIO = 0.05f;

        /// <summary>Number of pixels to x pan by.</summary>
        const int PAN_INCREMENT = 10;
        #endregion

        #region Backing fields
        float _gain = 1.0f;
        int _visibleStart = 0;
        int _selStart = 0;
        int _selLength = 0;
        int _marker = 0;
        readonly Pen _penDraw = new(Color.Black, 1);
        readonly Pen _penGrid = new(Color.LightGray, 1);
        readonly Pen _penMark = new(Color.Red, 1);
        readonly ToolTip toolTip;
        readonly IContainer components;
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

        /// <summary>Length of the clip.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public AudioTime TotalTime { get { return new((float)Length / AudioLibDefs.SAMPLE_RATE); } }

        /// <summary>Selection start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelStart { get { return _selStart; } set { _selStart = value; Invalidate(); } }

        /// <summary>Selection length in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelLength { get { return _selLength; } set { _selLength = value; Invalidate(); } }

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
        /// <summary>Value changed by user. Notify owner for display.</summary>
        public event EventHandler<ViewerChangeEventArgs>? ViewerChangeEvent;
        public class ViewerChangeEventArgs
        {
            public Property Change { get; set; } = Property.Marker;
            public string Text { get; set; } = "";
        }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Default constructor. Mainly for designer.
        /// </summary>
        public WaveViewer()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            //InitializeComponent():
            components = new Container();
            toolTip = new(components);
            SuspendLayout();
            Name = "WaveViewer";
            ResumeLayout(false);
        }

        /// <summary>
        /// Set everything from data source. Client must do this before setting properties as some are overwritten.
        /// </summary>
        /// <param name="prov">Source</param>
        /// <param name="simple">If true simple display only.</param>
        public void Init(ISampleProvider prov, bool simple = false)
        {
            _viewMode = simple ? ViewerMode.Thumbnail : ViewerMode.Full;
            // _simple = simple;
            _vals = prov.ReadAll();
            _max = _vals.Length > 0 ? _vals.Max() : 0;
            _min = _vals.Length > 0 ? _vals.Min() : 0;

            _selStart = 0;
            _selLength = 0;
            _marker = 0;

            if(_viewMode == ViewerMode.Thumbnail)
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
        /// Go to unity gain.
        /// </summary>
        public void ResetGain()
        {
            Gain = 1.0f;
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

        /// <summary>
        /// Owner is updating a property. This does syntax checking.
        /// </summary>
        /// <param name="change">The property</param>
        /// <param name="val">The new value</param>
        /// <returns>True if valid.</returns>
        public bool UpdateProperty(Property change, string val)//TODO1 a lot of repetitive code.
        {
            bool ok = false;

            switch (change, Globals.SelectionMode)
            {
                case (Property.Marker, WaveSelectionMode.Sample):
                    if (int.TryParse(val, out int marker))
                    {
                        _marker = marker;
                        ok = true;
                    }
                    break;

                case (Property.Marker, WaveSelectionMode.Time):
                    var time = AudioTime.Parse(val);
                    if (time is not null)
                    {
                        _marker = time.ToSample();
                        ok = true;
                    }
                    break;

                case (Property.Marker, WaveSelectionMode.Beat):
                    var bb = BarBeat.Parse(val);
                    if (bb is not null)
                    {
                        _marker = bb.ToSample(Globals.BPM);
                        ok = true;
                    }
                    break;

                case (Property.SelStart, WaveSelectionMode.Sample):
                    if (int.TryParse(val, out int selstart))
                    {
                        _selStart = selstart;
                        ok = true;
                    }
                    break;

                case (Property.SelStart, WaveSelectionMode.Time):
                    time = AudioTime.Parse(val);
                    if (time is not null)
                    {
                        _selStart = time.ToSample();
                        ok = true;
                    }
                    break;

                case (Property.SelStart, WaveSelectionMode.Beat):
                    bb = BarBeat.Parse(val);
                    if (bb is not null)
                    {
                        _selStart = bb.ToSample(Globals.BPM);
                        ok = true;
                    }
                    break;

                case (Property.SelLength, WaveSelectionMode.Sample):
                    if (int.TryParse(val, out int sellen))
                    {
                        _selLength = sellen;
                        ok = true;
                    }
                    break;

                case (Property.SelLength, WaveSelectionMode.Time):
                    time = AudioTime.Parse(val);
                    if (time is not null)
                    {
                        _selLength = time.ToSample();
                        ok = true;
                    }
                    break;

                case (Property.SelLength, WaveSelectionMode.Beat):
                    bb = BarBeat.Parse(val);
                    if (bb is not null)
                    {
                        _selLength = bb.ToSample(Globals.BPM);
                        ok = true;
                    }
                    break;

                default:
                    // Later.
                    break;
            }

            return ok;
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
            if(_viewMode == ViewerMode.Full)
            {
                HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
                hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

                // Number of detents the mouse wheel has rotated.
                int wheelDelta = WHEEL_RESOLUTION * e.Delta / SystemInformation.MouseWheelScrollDelta;

                switch(ModifierKeys)
                {
                    case Keys.None: // x pan
                        int incr = _samplesPerPixel * PAN_INCREMENT;
                        _visibleStart += wheelDelta > 0 ? incr : -incr; // left or right
                        _visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length);
                        Invalidate();
                        break;

                    case Keys.Control: // x zoom
                        // Get sample to center about.
                        int center = _marker;  // Or? PixelToSample(Width / 2), PixelToSample(MouseX());
                        // Modify the zoom factor.
                        int samplesPerPixelMax = _vals.Length / Width;
                        incr = (int)(ZOOM_RATIO * _samplesPerPixel);
                        if(incr == 0 && _samplesPerPixel > 1) // close in
                        {
                            incr = 1;
                        }
                        _samplesPerPixel += wheelDelta > 0 ? -incr : incr; // in or out
                        _samplesPerPixel = MathUtils.Constrain(_samplesPerPixel, 0, samplesPerPixelMax);
                        Recenter(center);
                        break;

                    case Keys.Shift: // y gain
                        _gain += wheelDelta > 0 ? GAIN_INCREMENT : -GAIN_INCREMENT;
                        _gain = (float)MathUtils.Constrain(_gain, 0.0f, AudioLibDefs.MAX_GAIN);
                        ViewerChangeEvent?.Invoke(this, new() { Change = Property.Gain });
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
            var sample = PixelToSample(e.X);

            switch (e.Button, Globals.SelectionMode, ModifierKeys, _viewMode)
            {
                case (MouseButtons.Left, WaveSelectionMode.Sample, Keys.None, ViewerMode.Full): // sample marker
                case (MouseButtons.Left, WaveSelectionMode.Sample, Keys.None, ViewerMode.Thumbnail):
                    _marker = Converters.SnapSample(sample, _snap);
                    CheckSel();
                    ViewerChangeEvent?.Invoke(this, new() { Change = Property.Marker });
                    Invalidate();
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Time, Keys.None, ViewerMode.Full): // time marker TODO1
                case (MouseButtons.Left, WaveSelectionMode.Time, Keys.None, ViewerMode.Thumbnail):
                    
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Beat, Keys.None, ViewerMode.Full): // beat marker TODO1
                case (MouseButtons.Left, WaveSelectionMode.Beat, Keys.None, ViewerMode.Thumbnail):

                    break;

                case (MouseButtons.Left, WaveSelectionMode.Sample, Keys.Control, ViewerMode.Full): // sample sel start
                    _selStart = Converters.SnapSample(sample, _snap);
                    //var ends = _selLength > 0 ? _selStart + _selLength : 0;
                    CheckSel();
                    ViewerChangeEvent?.Invoke(this, new() { Change = Property.SelStart });
                    Invalidate();
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Time, Keys.Control, ViewerMode.Full): // time sel start TODO1

                    break;

                case (MouseButtons.Left, WaveSelectionMode.Beat, Keys.Control, ViewerMode.Full): // beat sel start TODO1

                    break;

                case (MouseButtons.Left, WaveSelectionMode.Sample, Keys.Shift, ViewerMode.Full): // sample sel end
                    var sel = Converters.SnapSample(sample, _snap);
                    _selLength = sel - _selStart;
                    CheckSel();
                    ViewerChangeEvent?.Invoke(this, new() { Change = Property.SelLength });
                    Invalidate();
                    break;

                case (MouseButtons.Left, WaveSelectionMode.Time, Keys.Shift, ViewerMode.Full): // time sel end TODO1

                    break;

                case (MouseButtons.Left, WaveSelectionMode.Beat, Keys.Shift, ViewerMode.Full): // beat sel end TODO1

                    break;
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Handle mouse move. Just for tooltip currently.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.X != _lastXPos)
            {
                var sample = PixelToSample(e.X);

                switch (Globals.SelectionMode)
                {
                    case WaveSelectionMode.Sample:
                        sample = Converters.SnapSample(sample, _snap);
                        toolTip.SetToolTip(this, sample.ToString());
                        break;

                    case WaveSelectionMode.Time:
                        var tm = new AudioTime(sample);
                        tm.Snap(_snap);
                        toolTip.SetToolTip(this, tm.ToString());
                        break;

                    case WaveSelectionMode.Beat:
                        var bb = new BarBeat(sample, Globals.BPM);
                        bb.Snap(_snap);
                        toolTip.SetToolTip(this, bb.ToString());
                        break;
                }

                _lastXPos = e.X;

                // need this for anything other than tooltip: Invalidate();
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
            if(_viewMode == ViewerMode.Full)
            {
                switch (e.KeyCode)
                {
                    case Keys.G: // reset gain
                        _gain = 1.0f;
                        e.Handled = true;
                        break;

                    case Keys.H: // reset to initial full view
                        ResetView();
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

                    case Keys.F: // snap fine
                        _snap = SnapType.Fine;
                        e.Handled = true;
                        break;

                    case Keys.C: // snap coarse
                        _snap = SnapType.Coarse;
                        e.Handled = true;
                        break;

                    case Keys.N: // snap none
                        _snap = SnapType.None;
                        e.Handled = true;
                        break;
                }

                if(e.Handled)
                {
                    Invalidate();
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
        /// Paint the waveform. In simple mode support just the waveform with no zoom, pan, etc.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            const int Y_NUM_LINES = 5;
            const float Y_SPACING = 0.25f;
            const int X_NUM_LINES = 10; // approx
            var auxInfo = "";

            // Setup.
            pe.Graphics.Clear(BackColor);

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", _textFont, _textBrush, ClientRectangle, _format);
            }
            else
            {
                // Draw everything from bottom up. Thumbnail mode only gets the basics.
                if(_viewMode == ViewerMode.Full)
                {
                    // Y grid lines.
                    _penGrid.Width = 1;
                    for (int i = -Y_NUM_LINES; i <= Y_NUM_LINES; i++)
                    {
                        float val = i * Y_SPACING;
                        float yGrid = MathUtils.Map(val, -Y_NUM_LINES * Y_SPACING, Y_NUM_LINES * Y_SPACING, 0, Height);

                        // Some special treatments.
                        switch (i)
                        {
                            case 0:
                                // Origin is a bit thicker.
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
                                // The main lines.
                                pe.Graphics.DrawLine(_penGrid, 50, yGrid, Width, yGrid);
                                pe.Graphics.DrawString($"{-val:0.00}", _textFont, _textBrush, 25, yGrid, _format);
                                break;
                        }
                    }

                    // X grid lines.
                    switch (Globals.SelectionMode)
                    {
                        case WaveSelectionMode.Sample:
                            // TODO1 simplify/refactor this::::


                            int sampincr = VisibleLength / X_NUM_LINES;
                            // Round to a reasonable value.
                            var digits1 = MathUtils.NumDigits(sampincr);
                            var gran1 = Math.Pow(10, digits1 - 1);
                            sampincr = Converters.Clamp(sampincr, (int)gran1, false);

                            // Start outside visible.
                            var digits2 = MathUtils.NumDigits(VisibleStart);
                            var gran2 = Math.Pow(10, digits2 - 1);
                            //var start = VisibleStart;
                            var start = Converters.Clamp(VisibleStart, (int)gran2, false);
                            auxInfo = $"gran1:{gran1} sampincr:{sampincr} gran2:{gran2} start:{start}";

                            for (int xs = start; xs < VisibleStart + VisibleLength; xs += sampincr)
                            {
                                float xGrid = MathUtils.Map(xs, VisibleStart, VisibleStart + VisibleLength, 0, Width);
                                pe.Graphics.DrawLine(_penGrid, xGrid, 0, xGrid, Height);
                                pe.Graphics.DrawString($"{xs}", _textFont, _textBrush, xGrid, 10, _format);
                            }


                            //// fixed markers, variable values.
                            //int sampincr = VisibleLength / X_NUM_LINES;
                            //// Round to a reasonable value.
                            //var digits1 = MathUtils.NumDigits(sampincr);
                            //var gran1 = Math.Pow(10, digits1 - 1);
                            //sampincr = Converters.Clamp(sampincr, (int)gran1, false);

                            //// Start outside visible.
                            //var digits2 = MathUtils.NumDigits(VisibleStart);
                            //var gran2 = Math.Pow(10, digits2 - 1);
                            //var start = VisibleStart;
                            ////var start = Converters.Clamp(VisibleStart, (int)gran2, false);
                            //auxInfo = $"gran1:{gran1} sampincr:{sampincr} gran2:{gran2} start:{start}";

                            //for (int xs = start; xs < start + VisibleLength; xs += sampincr)
                            //{
                            //    float xGrid = MathUtils.Map(xs, start, start + VisibleLength, 0, Width);
                            //    pe.Graphics.DrawLine(_penGrid, xGrid, 0, xGrid, Height);
                            //    pe.Graphics.DrawString($"{xs}", _textFont, _textBrush, xGrid, 10, _format);
                            //}

                            //int sampincr = VisibleLength / X_NUM_LINES;
                            //// Round to a reasonable value.
                            //var digits1 = MathUtils.NumDigits(sampincr);
                            //var gran1 = Math.Pow(10, digits1 - 1);
                            //sampincr = Converters.Clamp(sampincr, (int)gran1, false);

                            //// Start outside visible.
                            //var digits2 = MathUtils.NumDigits(VisibleStart);
                            //var gran2 = Math.Pow(10, digits2 - 1);
                            //var start = Converters.Clamp(VisibleStart, (int)gran2, false);
                            //auxInfo = $"gran1:{gran1} sampincr:{sampincr} gran2:{gran2} start:{start}";

                            //for (int xs = start; xs < start + VisibleLength; xs += sampincr)
                            //{
                            //    float xGrid = MathUtils.Map(xs, start, start + VisibleLength, 0, Width);
                            //    pe.Graphics.DrawLine(_penGrid, xGrid, 0, xGrid, Height);
                            //    pe.Graphics.DrawString($"{xs}", _textFont, _textBrush, xGrid, 10, _format);
                            //}
                            break;

                        case WaveSelectionMode.Time: // TODO1 paint time
                            //AudioTime tstart = Converters.SampleToTime(VisibleStart, snap);
                            //AudioTime tend = Converters.SampleToTime(VisibleStart + VisibleLength, snap);
                            //AudioTime tlen = tend - tstart;

                            //// anywhere from 10 msec to MaxClipSize (10 min)
                            //// 0.01 -> 600.0
                            //AudioTime incr = tlen / X_NUM_LINES;

                            //int sincr = VisibleLength / X_NUM_LINES;
                            break;

                        case WaveSelectionMode.Beat: // TODO1 paint beat
                            // - Beats mode:
                            //   - Establish timing by select two samples and identify corresponding number of beats.
                            //   - Show in waveform.
                            //   - Subsequent selections are by beat using snap.
                            // 123:2.456.
                            break;
                    }

                    // Show info.
                    var sinfo1 = $"Gain:{_gain:0.00}  Snap:{_snap}";
                    var sinfo2 = $"VisStart:{_visibleStart}  Mark:{Marker}  SPP:{_samplesPerPixel}  VisLength:{VisibleLength}";
                    var sinfo3 = $"VisStart:{_visibleStart / 44100f}  Mark:{Marker / 44100f}  VisLength:{VisibleLength / 44100f}";
                    pe.Graphics.DrawString(sinfo1, _textFont, _textBrush, Width / 2, Height - 20, _format);
                    pe.Graphics.DrawString(auxInfo, _textFont, _textBrush, Width / 2, Height - 40, _format);
                }

                // Then the data - for all modes.
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
                else // Not enough data - just show what we have.
                {
                    for (int i = 0; i < _vals.Length; i++)
                    {
                        // +1 => 0  -1 => Height
                        int yVal = (int)MathUtils.Map(_vals[i] * _gain, 1.0f, -1.0f, 0, Height);
                        pe.Graphics.DrawRectangle(_penDraw, i, yVal, 1, 1);
                    }
                }

                // Selection and markers.
                if (_viewMode == ViewerMode.Full && _selStart > 0)
                {
                    int x = SampleToPixel(_selStart);
                    if (x >= 0)
                    {
                        pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                        pe.Graphics.DrawRectangle(_penMark, x, 10, 10, 10);
                    }
                }

                if (_viewMode == ViewerMode.Full && _selLength > 0)
                {
                    int x = SampleToPixel(_selStart + _selLength);
                    if (x >= 0)
                    {
                        pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                        pe.Graphics.DrawRectangle(_penMark, x - 10, 10, 10, 10);
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
        /// Do a few sanity checks.
        /// </summary>
        void CheckSel()
        {
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
        #endregion
    }
}
