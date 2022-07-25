using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>
    /// Volume control.
    /// </summary>
    public class Volume : UserControl
    {
        // TODO log/db.

        #region Fields
        /// <summary>The brush.</summary>
        readonly SolidBrush _brush = new(Color.White);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        #endregion

        #region Properties
        /// <summary>Optional label.</summary>
        public string Label { get; set; } = "";

        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _brush.Color; } set { _brush.Color = value; } }

        /// <summary>Fader orientation</summary>
        public Orientation Orientation { get; set; } = Orientation.Horizontal;
        /// <summary>Per step resolution of this pot.</summary>
        public double Resolution
        {
            get { return _resolution; }
            set { _resolution = MathUtils.Constrain(value, _minimum, _maximum); Rescale(); }
        }
        double _resolution = AudioLibDefs.VOLUME_MAX / 20;

        /// <summary>Minimum Value of the pot.</summary>
        public double Minimum
        {
            get { return _minimum; }
            set { _minimum = value; Rescale(); }
        }
        double _minimum = AudioLibDefs.VOLUME_MIN;

        /// <summary>Maximum Value of the pot.</summary>
        public double Maximum
        {
            get { return _maximum; }
            set { _maximum = value; Rescale(); }
        }
        double _maximum = AudioLibDefs.VOLUME_MAX;

        /// <summary>The current value of the pot.</summary>
        public double Value
        {
            get { return _value; }
            set { _value = MathUtils.Constrain(value, _minimum, _maximum, _resolution); Invalidate(); }
        }
        double _value = AudioLibDefs.VOLUME_MAX / 2;
        #endregion

        #region Events
        /// <summary>Volume value changed event.</summary>
        public event EventHandler? ValueChanged;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Creates a new Volume control.
        /// </summary>
        public Volume()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _brush.Dispose();
                _format.Dispose();
            }
            base.Dispose(disposing);
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

            // Draw the bar.
            if (Orientation == Orientation.Horizontal)
            {
                double x = (_value - _minimum) / (_maximum - _minimum);
                pe.Graphics.FillRectangle(_brush, ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width * (float)x, ClientRectangle.Height);
            }
            else
            {
                double y = 1.0 - (_value - _minimum) / (_maximum - _minimum);
                pe.Graphics.FillRectangle(_brush, ClientRectangle.Left, ClientRectangle.Height * (float)y, ClientRectangle.Width, ClientRectangle.Bottom);
            }

            // Text.
            string sval = _value.ToString("#0." + new string('0', MathUtils.DecPlaces(_resolution)));
            if (Label != "")
            {
                Rectangle r = new(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height / 2);
                pe.Graphics.DrawString(Label, Font, Brushes.Black, r, _format);

                r = new(ClientRectangle.X, ClientRectangle.Height / 2, ClientRectangle.Width, ClientRectangle.Height / 2);
                pe.Graphics.DrawString(sval, Font, Brushes.Black, r, _format);
            }
            else
            {
                pe.Graphics.DrawString(sval, Font, Brushes.Black, ClientRectangle, _format);
            }
        }
        #endregion

        #region Mouse events
        /// <summary>
        /// Handle dragging.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SetValueFromMouse(e);
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Handle dragging.
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.Left:
                    SetValueFromMouse(e);
                    break;

                //case MouseButtons.Right:
                //    Value = _resetVal;
                //    break;
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// ommon updater.
        /// </summary>
        /// <param name="e"></param>
        void SetValueFromMouse(MouseEventArgs e)
        {
            double oldval = Value;
            // Calculate the new value.
            double newval = Orientation == Orientation.Horizontal ?
                _minimum + e.X * (_maximum - _minimum) / Width :
                _minimum + (Height - e.Y) * (_maximum - _minimum) / Height;

            // This factors in the resolution.
            Value = newval;
            if(oldval != Value)
            {
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handle the nudge key.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.Down)
                {
                    Value -= _resolution;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.Up)
                {
                    Value += _resolution;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// If min or max or resolution changed by client.
        /// </summary>
        void Rescale()
        {
            _minimum = MathUtils.Constrain(_minimum, _minimum, _maximum, _resolution);
            _maximum = MathUtils.Constrain(_maximum, _minimum, _maximum, _resolution);
            _value = MathUtils.Constrain(_value, _minimum, _maximum, _resolution);
            Invalidate();
        }
        #endregion
    }
}
