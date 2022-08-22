using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>The control.</summary>
    public partial class TimeBar : UserControl
    {
        #region Fields
        /// <summary>For tracking mouse moves.</summary>
        int _lastXPos = 0;

        /// <summary>Tooltip for mousing.</summary>
        readonly ToolTip _toolTip = new();

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

        /// <summary>Constant.</summary>
        static readonly int LARGE_CHANGE = 1000;

        /// <summary>Constant.</summary>
        static readonly int SMALL_CHANGE = 100;
        #endregion

        #region Backing fields
        readonly SolidBrush _brushProgress = new(Color.White);
        readonly Pen _penMarker = new(Color.Black, 1);
        TimeSpan _current = new();
        TimeSpan _length = new();
        TimeSpan _marker1 = new();
        TimeSpan _marker2 = new();
        #endregion

        #region Properties
        /// <summary>Where we be now.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TimeSpan Current { get { return _current; } set { _current = value; Invalidate(); } }

        /// <summary>Total length.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TimeSpan Length { get { return _length; } set { _length = value; Invalidate(); } }

        /// <summary>One marker.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TimeSpan Marker1 { get { return _marker1; } set { _marker1 = value; Invalidate(); } }

        /// <summary>Other marker.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public TimeSpan Marker2 { get { return _marker2; } set { _marker2 = value; Invalidate(); } }

        /// <summary>Snap to this increment value.</summary>
        public int SnapMsec { get; set; } = 0;

        /// <summary>For styling.</summary>
        public Color ProgressColor { get { return _brushProgress.Color; } set { _brushProgress.Color = value; } }

        /// <summary>For styling.</summary>
        public Color MarkerColor { get { return _penMarker.Color; } set { _penMarker.Color = value; } }

        /// <summary>Big font.</summary>
        public Font FontLarge { get; set; } = new("Microsoft Sans Serif", 20, FontStyle.Regular, GraphicsUnit.Point, 0);

        /// <summary>Baby font.</summary>
        public Font FontSmall { get; set; } = new("Microsoft Sans Serif", 10, FontStyle.Regular, GraphicsUnit.Point, 0);
        #endregion

        #region Events
        /// <summary>Value changed by user.</summary>
        public event EventHandler? CurrentTimeChanged;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeBar()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
                _penMarker.Dispose();
                _brushProgress.Dispose();
                _format.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Update current time.
        /// </summary>
        /// <param name="msec"></param>
        public void IncrementCurrent(int msec)
        {
            int smsec = DoSnap(msec);
            _current = (smsec > 0) ? _current.Add(new TimeSpan(0, 0, 0, 0, smsec)) : _current.Subtract(new TimeSpan(0, 0, 0, 0, -smsec));

            if (_current > _length)
            {
                _current = _length;
            }

            if (_current < TimeSpan.Zero)
            {
                _current = TimeSpan.Zero;
            }
            else if (_current >= _length)
            {
                _current = _length;
            }
            else if (_marker2 != TimeSpan.Zero && _current >= _marker2)
            {
                _current = _marker2;
            }

            Invalidate();
        }
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
            _marker1 = Constrain(_marker1, TimeSpan.Zero, _length);
            _marker1 = Constrain(_marker1, TimeSpan.Zero, _marker2);
            _marker2 = Constrain(_marker2, TimeSpan.Zero, _length);
            _marker2 = Constrain(_marker2, _marker1, _length);
            _current = Constrain(_current, _marker1, _marker2);

            if (_marker2 == TimeSpan.Zero && _length != TimeSpan.Zero)
            {
                _marker2 = _length;
            }

            // Draw the bar.
            if (_current < _length)
            {
                int dstart = Scale(_marker1);
                int dend = _current > _marker2 ? Scale(_marker2) : Scale(_current);
                pe.Graphics.FillRectangle(_brushProgress, dstart, 0, dend - dstart, Height);
            }

            // Draw start/end markers.
            if (_marker1 != TimeSpan.Zero || _marker2 != _length)
            {
                int mstart = Scale(_marker1);
                int mend = Scale(_marker2);
                pe.Graphics.DrawLine(_penMarker, mstart, 0, mstart, Height);
                pe.Graphics.DrawLine(_penMarker, mend, 0, mend, Height);
            }

            // Text.
            _format.Alignment = StringAlignment.Center;
            pe.Graphics.DrawString(_current.ToString(AudioLibDefs.TS_FORMAT), FontLarge, Brushes.Black, ClientRectangle, _format);
            _format.Alignment = StringAlignment.Near;
            pe.Graphics.DrawString(_marker1.ToString(AudioLibDefs.TS_FORMAT), FontSmall, Brushes.Black, ClientRectangle, _format);
            _format.Alignment = StringAlignment.Far;
            pe.Graphics.DrawString(_marker2.ToString(AudioLibDefs.TS_FORMAT), FontSmall, Brushes.Black, ClientRectangle, _format);
        }
        #endregion

        #region UI handlers
        /// <summary>
        /// Handle selection operations.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Add:
                case Keys.Up:
                    IncrementCurrent(e.Shift ? SMALL_CHANGE : LARGE_CHANGE);
                    e.Handled = true;
                    break;

                case Keys.Subtract:
                case Keys.Down:
                    IncrementCurrent(e.Shift ? -SMALL_CHANGE : -LARGE_CHANGE);
                    e.Handled = true;
                    break;

                case Keys.Escape:
                    // Reset.
                    _marker1 = TimeSpan.Zero;
                    _marker2 = _length;
                    e.Handled = true;
                    Invalidate();
                    break;
            }

            if(e.Handled)
            {
                Invalidate();
            }
        }

        /// <summary>
        /// Hook to intercept keys.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Up:
                case Keys.Down:
                    e.IsInputKey = true;
                    break;
            }
        }

        /// <summary>
        /// Handle mouse position changes.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _current = GetTimeFromMouse(e.X);
                CurrentTimeChanged?.Invoke(this, new EventArgs());
            }
            else
            {
                if (e.X != _lastXPos)
                {
                    TimeSpan ts = GetTimeFromMouse(e.X);
                    _toolTip.SetToolTip(this, ts.ToString(AudioLibDefs.TS_FORMAT));
                    _lastXPos = e.X;
                }
            }

            Invalidate();
            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handle dragging.
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                _marker1 = GetTimeFromMouse(e.X);
            }
            else if (ModifierKeys.HasFlag(Keys.Alt))
            {
                _marker2 = GetTimeFromMouse(e.X);
            }
            else
            {
                _current = GetTimeFromMouse(e.X);
            }

            CurrentTimeChanged?.Invoke(this, new EventArgs());
            Invalidate();
            base.OnMouseDown(e);
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Convert x pos to TimeSpan.
        /// </summary>
        /// <param name="x"></param>
        TimeSpan GetTimeFromMouse(int x)
        {
            int msec = 0;

            if(_current.TotalMilliseconds < _length.TotalMilliseconds)
            {
                msec = x * (int)_length.TotalMilliseconds / Width;
                msec = MathUtils.Constrain(msec, 0, (int)_length.TotalMilliseconds);
                msec = DoSnap(msec);
            }
            return new TimeSpan(0, 0, 0, 0, msec);
        }

        /// <summary>
        /// Snap to user preference.
        /// </summary>
        /// <param name="msec"></param>
        /// <returns></returns>
        int DoSnap(int msec)
        {
            int smsec = 0;
            if (SnapMsec > 0)
            {
                smsec = (msec / SnapMsec) * SnapMsec;
                if(SnapMsec > (msec % SnapMsec) / 2)
                {
                    smsec += SnapMsec;
                }
            }

            return smsec;
        }

        /// <summary>
        /// Utility helper function.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        TimeSpan Constrain(TimeSpan val, TimeSpan lower, TimeSpan upper)
        {
            return TimeSpan.FromMilliseconds(MathUtils.Constrain(val.TotalMilliseconds, lower.TotalMilliseconds, upper.TotalMilliseconds));
        }

        /// <summary>
        /// Map from time to UI pixels.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int Scale(TimeSpan val)
        {
            return (int)(val.TotalMilliseconds * Width / _length.TotalMilliseconds);
        }
        #endregion
    }
}
