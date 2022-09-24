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

        /// <summary>How to snap.</summary>
        readonly SnapType _snap = SnapType.Fine;
        #endregion

        #region Backing fields
        readonly SolidBrush _brushProgress = new(Color.White);
        int _length = 0;
        int _start = 0;
        int _current = 0;
        #endregion

        #region Designer fields
        readonly ToolTip toolTip;
        readonly IContainer components;
        #endregion

        #region Properties
        /// <summary>Where we be now in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Current { get { return _current; } set { _current = value; Invalidate(); } }

        /// <summary>Total length in samples.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public int Length { get { return _length; } set { _length = value; Invalidate(); } }

        /// <summary>For styling.</summary>
        public Color ProgressColor { get { return _brushProgress.Color; } set { _brushProgress.Color = value; } }

        /// <summary>Big font.</summary>
        public Font FontLarge { get; set; } = new("Microsoft Sans Serif", 20, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>Baby font.</summary>
        public Font FontSmall { get; set; } = new("Microsoft Sans Serif", 10, FontStyle.Regular, GraphicsUnit.Point, 0);
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
                _brushProgress.Dispose();
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
            // Setup.
            pe.Graphics.Clear(BackColor);

            // Validate times.
            _current = MathUtils.Constrain(_current, 0, _length);

            // Draw the progress.
            int dstart = SampleToPixel(_start);// _marker1;
            int dend = SampleToPixel(_current);// _marker2
            pe.Graphics.FillRectangle(_brushProgress, dstart, 0, dend, Height);

            // Draw text.
            _format.Alignment = StringAlignment.Center;
            pe.Graphics.DrawString(ConverterOps.Format(_current), FontLarge, Brushes.Black, ClientRectangle, _format);
            _format.Alignment = StringAlignment.Near;
            pe.Graphics.DrawString(ConverterOps.Format(_start), FontSmall, Brushes.Black, ClientRectangle, _format);
            _format.Alignment = StringAlignment.Far;
            pe.Graphics.DrawString(ConverterOps.Format(_length), FontSmall, Brushes.Black, ClientRectangle, _format);
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
                sample = ConverterOps.SnapSample(sample, _snap);
                toolTip.SetToolTip(this, ConverterOps.Format(sample));
                _lastXPos = e.X;
                // need this for anything other than tooltip: Invalidate();
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
