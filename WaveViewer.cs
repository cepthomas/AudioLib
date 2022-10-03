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
using static AudioLib.Globals;
using static AudioLib.ToolStripParamEditor;


namespace AudioLib
{
    /// <summary>Simple mono wave display.</summary>
    public partial class WaveViewer : UserControl
    {
        #region Fields
        /// <summary>For drawing text.</summary>
        readonly SolidBrush _textBrush = new(Color.Black);

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

        /// <summary>Keep this around for context menu.</summary>
        ToolStripParamEditor _edSelStart = new();

        /// <summary>Keep this around for context menu.</summary>
        ToolStripParamEditor _edSelLength = new();

        /// <summary>Keep this around for context menu.</summary>
        ToolStripParamEditor _edMarker = new();
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
        readonly Pen _penWave = new(Color.Black, 1);
        readonly Pen _penGrid = new(Color.LightGray, 1);
        readonly Pen _penMark = new(Color.Red, 1);
        #endregion

        #region Designer fields
        readonly ToolTip toolTip;
        readonly IContainer components;
        #endregion

        #region Properties
        /// <summary>The waveform color.</summary>
        public Color WaveColor { set { _penWave.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color GridColor { set { _penGrid.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color MarkColor { set { _penMark.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color TextColor { set { _textBrush.Color = value; Invalidate(); } }

        /// <summary>For drawing text.</summary>
        public Font TextFont { get; set; } = new("Calibri", 10, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>Client gain adjustment.</summary>
        public float Gain { get { return _gain; } set { _gain = value; Invalidate(); } }

        /// <summary>Length of the clip in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Length { get { return _vals.Length; } }

        /// <summary>Length of the clip in msec.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int TotalTime { get { return (int)((double)Length / AudioLibDefs.SAMPLE_RATE / 1000.0f); } }

        /// <summary>Selection start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelStart { get { return _selStart; } set { _selStart = value; Invalidate(); } }

        /// <summary>Selection length in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelLength { get { return _selLength; } set { _selLength = value; Invalidate(); } }

        /// <summary>General purpose marker location.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Marker { get { return _marker; } set { _marker = value; CheckProperties(); Invalidate(); } }

        /// <summary>Visible start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisibleStart { get { return _visibleStart; } set { _visibleStart = value; Invalidate(); } }

        /// <summary>Visible length in samples. Always positive.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int VisibleLength { get { return Width * _samplesPerPixel; } }
        #endregion

        #region Events
        /// <summary>Value changed by user. Notify owner for display.</summary>
        public event EventHandler<ViewerChangeEventArgs>? ViewerChangeEvent;
        public class ViewerChangeEventArgs : EventArgs
        {
            public ParamChange Change { get; set; } = ParamChange.Marker;
            public object? Value { get; set; } = null;
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

            CreateContextMenu();
        }

        /// <summary>
        /// Set everything from data source. Client must do this before setting properties as some are overwritten.
        /// </summary>
        /// <param name="prov">Source</param>
        public void Init(ISampleProvider prov)
        {
            _vals = prov.ReadAll();
            _max = _vals.Length > 0 ? _vals.Max() : 0;
            _min = _vals.Length > 0 ? _vals.Min() : 0;
            prov.Rewind();

            SelStart = 0;
            SelLength = 0;
            Marker = 0;

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
                toolTip.Dispose();
                _penWave.Dispose();
                _penGrid.Dispose();
                _penMark.Dispose();
                _format.Dispose();
                TextFont.Dispose();
           }
           base.Dispose(disposing);
        }
        #endregion

        #region Context menu
        /// <summary>
        /// Init the menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContextMenuStrip_Opening(object? sender, CancelEventArgs e)
        {
            _edSelStart.Value = _selStart;
            _edSelLength.Value = _selLength;
            _edMarker.Value = _marker;
        }

        /// <summary>
        /// Create context menu.
        /// </summary>
        void CreateContextMenu()
        {
            // Set up main menu. TODO Set menu item enables according to system states.
            ContextMenuStrip = new ContextMenuStrip(components);

            ContextMenuStrip.Items.Add("Reset View", null, (_, __) => ResetView());
            ContextMenuStrip.Items.Add("Fit Gain", null, (_, __) => FitGain());
            ContextMenuStrip.Items.Add("Reset Gain", null, (_, __) => ResetGain());
            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add("Go To Marker", null, (_, __) => GoToMarker());
            ContextMenuStrip.Items.Add("Remove Marker", null, (_, __) => { Marker = 0; Invalidate(); });
            ContextMenuStrip.Items.Add("Go To Selection", null, (_, __) => GoToSelection());
            ContextMenuStrip.Items.Add("Remove Selection", null, (_, __) => { SelStart = 0; SelLength = 0; Invalidate(); });

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            ContextMenuStrip.Items.Add("Snap Coarse", null, (_, __) => SetSnap(SnapType.Coarse));
            ContextMenuStrip.Items.Add("Snap Fine", null, (_, __) => SetSnap(SnapType.Fine));
            ContextMenuStrip.Items.Add("Snap Off", null, (_, __) => SetSnap(SnapType.Off));

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _edSelStart.ParamChanged += (object? sender, ParamChangedEventArgs args) => { SelStart = args.Value; ContextMenuStrip.Close(); };
            ContextMenuStrip.Items.Add(new ToolStripLabel("Selection Start:"));
            ContextMenuStrip.Items.Add(_edSelStart);

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _edSelLength.ParamChanged += (object? sender, ParamChangedEventArgs args) => { SelLength = args.Value; ContextMenuStrip.Close(); };
            ContextMenuStrip.Items.Add(new ToolStripLabel("Selection Length:"));
            ContextMenuStrip.Items.Add(_edSelLength);

            ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _edMarker.ParamChanged += (object? sender, ParamChangedEventArgs args) => { Marker = args.Value; ContextMenuStrip.Close(); };
            ContextMenuStrip.Items.Add(new ToolStripLabel("Marker:"));
            ContextMenuStrip.Items.Add(_edMarker);

            ContextMenuStrip.Opening += ContextMenuStrip_Opening;
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
            HandledMouseEventArgs hme = (HandledMouseEventArgs)e;
            hme.Handled = true; // This prevents the mouse wheel event from getting back to the parent.

            // Number of detents the mouse wheel has rotated.
            int wheelDelta = WHEEL_RESOLUTION * e.Delta / SystemInformation.MouseWheelScrollDelta;

            switch (ModifierKeys)
            {
                case Keys.None: // x pan
                    {
                        int incr = _samplesPerPixel * PAN_INCREMENT;
                        _visibleStart += wheelDelta > 0 ? incr : -incr; // left or right
                        _visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length);
                        Invalidate();
                    }
                    break;

                case Keys.Control: // x zoom TOODO not exactly perfect.
                    {
                        // Get sample to center about.
                        int center = PixelToSample(Width / 2); // or mouse or _marker
                        double incr = Math.Round(ZOOM_RATIO * _samplesPerPixel);
                        if (incr == 0 && _samplesPerPixel > 1) // close in
                        {
                            incr = 1;
                        }
                        _samplesPerPixel += (int)Math.Round(wheelDelta > 0 ? -incr : incr); // in or out
                        int samplesPerPixelMax = _vals.Length / Width;
                        _samplesPerPixel = MathUtils.Constrain(_samplesPerPixel, 0, samplesPerPixelMax);
                        Recenter(center);
                    }
                    break;

                case Keys.Shift: // y gain
                    {
                        _gain += wheelDelta > 0 ? GAIN_INCREMENT : -GAIN_INCREMENT;
                        _gain = (float)MathUtils.Constrain(_gain, 0.0, AudioLibDefs.MAX_GAIN);
                        ViewerChangeEvent?.Invoke(this, new() { Change = ParamChange.Gain, Value = _gain });
                        Invalidate();
                    }
                    break;
            };

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Handle mouse clicks to select things.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            var sample = PixelToSample(e.X);
            sample = ConverterOps.Snap(sample, _snap);
            ParamChange changed = ParamChange.None;
            int newval = -1;

            if (sample >= 0)
            {
                switch (e.Button, ModifierKeys)
                {
                    case (MouseButtons.Left, Keys.None):
                        _marker = sample;
                        CheckProperties();
                        changed = ParamChange.Marker;
                        newval = _marker;
                        break;

                    case (MouseButtons.Left, Keys.Control):
                        _selStart = sample;
                        CheckProperties();
                        changed = ParamChange.SelStart;
                        newval = _selStart;
                        break;

                    case (MouseButtons.Left, Keys.Shift):
                        _selLength = sample - _selStart;
                        CheckProperties();
                        changed = ParamChange.SelLength;
                        newval = _selLength;
                        break;

                    default:
                        // Nada.
                        break;
                }

                if (changed != ParamChange.None)
                {
                    ViewerChangeEvent?.Invoke(this, new() { Change = changed, Value = newval });
                    Invalidate();
                }
            }

            base.OnMouseDoubleClick(e);
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
                sample = ConverterOps.Snap(sample, _snap);
                toolTip.SetToolTip(this, ConverterOps.Format(sample));
                _lastXPos = e.X;
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Key press.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;

            switch (e.KeyCode)
            {
                case Keys.H: // reset to initial full view
                    ResetView();
                    break;

                case Keys.M: // go to marker
                    GoToMarker();
                    break;

                case Keys.S: // go to selection
                    GoToSelection();
                    break;

                case Keys.F: // snap fine
                    SetSnap(SnapType.Fine);
                    break;

                case Keys.C: // snap coarse
                    SetSnap(SnapType.Coarse);
                    break;

                case Keys.N: // snap none
                    SetSnap(SnapType.Off);
                    break;

                default:
                    e.Handled = false;
                    break;
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
            const int X_NUM_LINES = 10; // approximately

            // Setup.
            pe.Graphics.Clear(BackColor);

            if (_vals is null || _vals.Length == 0)
            {
                pe.Graphics.DrawString("No data", TextFont, _textBrush, ClientRectangle, _format);
                return;
            }

            // Draw everything from bottom up.

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
                        pe.Graphics.DrawString($"{-val:0.00}", TextFont, _textBrush, 25, yGrid, _format);
                        break;
                            
                    case Y_NUM_LINES:
                    case -Y_NUM_LINES:
                        // No label.
                        break;
                            
                    default:
                        // The main lines.
                        pe.Graphics.DrawLine(_penGrid, 50, yGrid, Width, yGrid);
                        pe.Graphics.DrawString($"{-val:0.00}", TextFont, _textBrush, 25, yGrid, _format);
                        break;
                }
            }

            // X grid lines.
            // Calc the x increment and fit to a fine or coarse set.
            int sampincr = RoundGranular(VisibleLength / X_NUM_LINES);
            HashSet<int> set = new();

            // Try coarse.
            for (int incr = VisibleStart; incr < VisibleStart + VisibleLength; incr++)
            {
                set.Add(ConverterOps.Snap(incr, SnapType.Coarse));
            }
            if (set.Count < 5)
            {
                // Try fine.
                for (int incr = VisibleStart; incr < VisibleStart + VisibleLength; incr++)
                {
                    set.Add(ConverterOps.Snap(incr, SnapType.Fine));
                }
            }
            var list = set.OrderBy(x => x).ToList();

            // Shorten if too long.
            if (list.Count > 10)
            {
                int prune = list.Count / 10 + 1;
                List<int> newList = new();
                for (int i = 0; i < list.Count; i++)
                {
                    if (i % prune == 0)
                    {
                        newList.Add(list[i]);
                    }
                }
                list = newList;
            }

            // Show them.
            _format.Alignment = StringAlignment.Far;
            for (int xs = 1; xs < list.Count; xs++)
            {
                float xGrid = MathUtils.Map(list[xs], VisibleStart, VisibleStart + VisibleLength, 0, Width);
                pe.Graphics.DrawLine(_penGrid, xGrid, 0, xGrid, Height);
                pe.Graphics.DrawString($"{ConverterOps.Format(list[xs])}", TextFont, _textBrush, xGrid, 10, _format);
            }
            _format.Alignment = StringAlignment.Center;

            // Show info.
            var info = new List<string>
            {
                $"Gain:{_gain:0.00}",
                $"Snap:{_snap}",
                $"SelStart:{ConverterOps.Format(_selStart)}",
                $"SelLength:{ConverterOps.Format(_selLength)}",
                $"Marker:{ConverterOps.Format(_marker)}"
            };
            //info.Add($"VisStart:{ConverterOps.Format(_visibleStart)}");
            //info.Add($"VisLength:{ConverterOps.Format(VisibleLength)}");
            pe.Graphics.DrawString(string.Join("  ", info), TextFont, _textBrush, Width / 2, Height - 10, _format);

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

                    pe.Graphics.DrawLine(_penWave, i, max, i, min);
                }
            }
            else // Not enough data - just show what we have.
            {
                for (int i = 0; i < _vals.Length; i++)
                {
                    // +1 => 0  -1 => Height
                    int yVal = (int)MathUtils.Map(_vals[i] * _gain, 1.0f, -1.0f, 0, Height);
                    pe.Graphics.DrawRectangle(_penWave, i, yVal, 1, 1);
                }
            }

            // Lastly selection and markers.
            if (_selStart > 0)
            {
                int x = SampleToPixel(_selStart);
                if (x >= 0)
                {
                    pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                    pe.Graphics.FillPolygon(_penMark.Brush, new PointF[] { new(x, 10), new(x + 10, 15), new(x, 20) });
                }
            }

            if (_selLength > 0)
            {
                int x = SampleToPixel(_selStart + _selLength);
                if (x >= 0)
                {
                    pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                    pe.Graphics.FillPolygon(_penMark.Brush, new PointF[] { new(x, 10), new(x - 10, 15), new(x, 20) });
                }
            }

            if (_marker > 0)
            {
                int x = SampleToPixel(_marker);
                if (x >= 0)
                {
                    pe.Graphics.DrawLine(_penMark, x, 0, x, Height);
                    pe.Graphics.FillEllipse(_penMark.Brush, x - 5, 10, 10, 10);
                }
            }

            // Local func.
            int RoundGranular(int val)
            {
                var numdigits = MathUtils.NumDigits(val);
                var granularity = Math.Pow(10, numdigits - 1);
                return MathUtils.Clamp(val, (int)granularity, false);
            }
        }

        /// <summary>
        /// Render a bitmap suitable for navigation.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="drawColor"></param>
        /// <param name="backColor"></param>
        /// <param name="fit"></param>
        /// <returns></returns>
        public Bitmap RenderThumbnail(int width, int height, Color drawColor, Color backColor, bool fit)
        {
            Bitmap bmp = new(width, height);

            using (Graphics graphics = Graphics.FromImage(bmp))
            using (Pen penDraw = new(drawColor, 1))
            {
                graphics.Clear(backColor);

                float gain = fit ? 0.9f / Math.Max(Math.Abs(_max), Math.Abs(_min)) : _gain;
                int samplesPerPixel = _vals.Length / width;

                if (samplesPerPixel > 0)
                {
                    var peaks = PeakProvider.GetPeaks(_vals, 0, samplesPerPixel, width);

                    for (int i = 0; i < peaks.Count; i++)
                    {
                        // +1 => 0  -1 => height
                        int max = (int)MathUtils.Map(peaks[i].max * gain, 1.0f, -1.0f, 0, height);
                        int min = (int)MathUtils.Map(peaks[i].min * gain, 1.0f, -1.0f, 0, height);

                        // Make sure there's always at least one dot.
                        if (max == min)
                        {
                            if (max > 0) { min--; }
                            else { max++; }
                        }

                        graphics.DrawLine(penDraw, i, max, i, min);
                    }
                }
                else // Not enough data - just show what we have.
                {
                    for (int i = 0; i < _vals.Length; i++)
                    {
                        // +1 => 0  -1 => height
                        int yVal = (int)MathUtils.Map(_vals[i] * gain, 1.0f, -1.0f, 0, height);
                        graphics.DrawRectangle(penDraw, i, yVal, 1, 1);
                    }
                }
            }

            return bmp;
        }
        #endregion

        #region Helpers for private and public use
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
            _gain = 1.0f;
            Invalidate();
        }

        /// <summary>
        /// Pan to new center location.
        /// </summary>
        /// <param name="sample">Center around this.</param>
        public void Recenter(int sample)
        {
            _visibleStart = sample - VisibleLength / 2;
            CheckProperties();
            Invalidate();
        }

        /// <summary>
        /// Show original view.
        /// </summary>
        public void ResetView()
        {
            _visibleStart = 0;
            _samplesPerPixel = _vals.Length / Width;
            Invalidate();
        }

        /// <summary>
        /// Go to the current marker.
        /// </summary>
        public void GoToMarker()
        {
            if (_marker > 0)
            {
                Recenter(_marker);
            }
        }

        /// <summary>
        /// Go to the current selection.
        /// </summary>
        public void GoToSelection()
        {
            if (_selStart > 0)
            {
                Recenter(_selStart);
            }
        }

        /// <summary>
        /// Set snap.
        /// </summary>
        public void SetSnap(SnapType snap)
        {
            _snap = snap;
            Invalidate();
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Do a few sanity checks.
        /// </summary>
        void CheckProperties()
        {
            _selStart = MathUtils.Constrain(_selStart, 0, _vals.Length);
            _selLength = MathUtils.Constrain(_selLength, 0, _vals.Length - _selStart);
            _marker = MathUtils.Constrain(_marker, 0, _vals.Length);
            _visibleStart = MathUtils.Constrain(_visibleStart, 0, _vals.Length);
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
        #endregion
    }
}
