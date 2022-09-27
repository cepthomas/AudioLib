using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NBagOfTricks;
using static AudioLib.Globals;


namespace AudioLib
{
    /// <summary>The control.</summary>
    public partial class ProgressBar : UserControl
    {
        #region Fields
        /// <summary>For tracking mouse moves.</summary>
        int _lastXPos = 0;

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        /// <summary>For drawing text.</summary>
        readonly SolidBrush _textBrush = new(Color.Black);

        /// <summary>For drawing markers.</summary>
        readonly Pen _penMark = new(Color.Red, 2);

        /// <summary>For drawing progress.</summary>
        readonly Pen _penProgress = new(Color.LightGray, 2);

        /// <summary>How to snap.</summary>
        readonly SnapType _snap = SnapType.Fine;
        #endregion

        #region Backing fields
        int _selStart = 0;
        int _selLength = 0;
        int _length = 0;
        int _current = 0;
        Bitmap? _thumbnail = null;
        #endregion

        #region Designer fields
        readonly ToolTip toolTip;
        readonly IContainer components;
        #endregion

        #region Properties
        /// <summary>Where we be now in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Current { get { return _current; } set { _current = value; Invalidate(); } } // Refresh

        /// <summary>Total length in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Length { get { return _length; } set { _length = value; Invalidate(); } }

        /// <summary>Selection start sample.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelStart { get { return _selStart; } set { _selStart = value; Invalidate(); } }

        /// <summary>Selection length in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int SelLength { get { return _selLength; } set { _selLength = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color ProgressColor { set { _penProgress.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color MarkColor { set { _penMark.Color = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color TextColor { set { _textBrush.Color = value; Invalidate(); } }

        /// <summary>Big font.</summary>
        public Font FontLarge { get; set; } = new("Microsoft Sans Serif", 20, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>Baby font.</summary>
        public Font FontSmall { get; set; } = new("Microsoft Sans Serif", 10, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>Optional background.</summary>
        public Bitmap? Thumbnail { get { return _thumbnail; } set { _thumbnail = value; Invalidate(); } }
        #endregion

        #region Events
        /// <summary>Value changed by user.</summary>
        public event EventHandler? CurrentChanged;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public ProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            components = new Container();
            toolTip = new(components);
            SuspendLayout();
            Name = "ProgressBar";
            ResumeLayout(false);
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
                _textBrush.Dispose();
                _penMark.Dispose();
                _penProgress.Dispose();
                _format.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Public functions

        #endregion

        #region Drawing
        /// <summary>
        /// Draw the slider.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Validate times.
            _current = MathUtils.Constrain(_current, 0, _length);

            // Draw the thumbnail.
            if (Thumbnail is not null)
            {
                pe.Graphics.DrawImage(Thumbnail, 0, 0);
            }
            else // simple background
            {
                pe.Graphics.Clear(BackColor);
            }

            // Show the selection.
            int start = SelStart;
            int end = SelLength == 0 ? Length : SelStart + SelLength;
            int xst = SampleToPixel(start);
            int xend = SampleToPixel(end);
            pe.Graphics.DrawLine(_penMark, xst, 0, xst, Height);
            pe.Graphics.DrawLine(_penMark, xend, 0, xend, Height);
            //pe.Graphics.DrawRectangle(_penMark, xst, 0, xend, Height);

            // Draw the progress.
            int x = SampleToPixel(_current);
            pe.Graphics.DrawLine(_penProgress, x, 0, x, Height);
            //int dstart = SampleToPixel(_start);
            //int dend = SampleToPixel(_current);
            //pe.Graphics.FillRectangle(_brushProgress, dstart, 0, dend, Height);

            // Draw text.
            _format.Alignment = StringAlignment.Center;
            pe.Graphics.DrawString(ConverterOps.Format(_current), FontLarge, _textBrush, ClientRectangle, _format);
            //_format.Alignment = StringAlignment.Near;
            //pe.Graphics.DrawString(ConverterOps.Format(_selStart), FontSmall, _textBrush, ClientRectangle, _format);
            _format.Alignment = StringAlignment.Far;
            pe.Graphics.DrawString(ConverterOps.Format(_length), FontSmall, _textBrush, ClientRectangle, _format);
        }
        #endregion

        #region UI handlers
        /// <summary>
        /// Handle mouse position changes.
        /// </summary>
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
        /// Handle dragging.
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _current = PixelToSample(e.X);
            CurrentChanged?.Invoke(this, new EventArgs());
            base.OnMouseDown(e);
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
            if (pixel >= 0 && pixel < Width)
            {
                sample = pixel * _length / Width;
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
            int pixel = 0;
            if (_length > 0)
            {
                pixel = MathUtils.Map(sample, 0, _length, 0, Width);
            }
            return pixel;
        }
        #endregion
    }
}
