using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>
    /// Volume control. TODOX also log/db.
    /// </summary>
    public class Volume : UserControl
    {
        #region Constants
        public const double MIN_VOLUME = 0.0;
        public const double MAX_VOLUME = 2.0;
        public const double DEFAULT_VOLUME = 0.8;
        public const double RESOLUTION = 0.1;
        #endregion

        #region Fields
        /// <summary>Current value.</summary>
        double _value = DEFAULT_VOLUME;

        /// <summary>Min value.</summary>
        double _minimum = MIN_VOLUME;

        /// <summary>Max value.</summary>
        double _maximum = MAX_VOLUME;

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

        /// <summary>The current value of the slider.</summary>
        public double Value
        {
            get { return _value; }
            set { _value = MathUtils.Constrain(value, _minimum, _maximum, RESOLUTION); Invalidate(); }
        }
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
                double x = (_value - MIN_VOLUME) / (MAX_VOLUME - MIN_VOLUME);
                pe.Graphics.FillRectangle(_brush, ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width * (float)x, ClientRectangle.Height);
            }
            else
            {
                double y = 1.0 - (_value - MIN_VOLUME) / (MAX_VOLUME - MIN_VOLUME);
                pe.Graphics.FillRectangle(_brush, ClientRectangle.Left, ClientRectangle.Height * (float)y, ClientRectangle.Width, ClientRectangle.Bottom);
            }

            // Text.
            string sval = _value.ToString("#0." + new string('0', MathUtils.DecPlaces(RESOLUTION)));
            if (Label != "")
            {
                Rectangle r = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height / 2);
                pe.Graphics.DrawString(Label, Font, Brushes.Black, r, _format);

                r = new Rectangle(ClientRectangle.X, ClientRectangle.Height / 2, ClientRectangle.Width, ClientRectangle.Height / 2);
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
                MIN_VOLUME + e.X * (MAX_VOLUME - MIN_VOLUME) / Width :
                MIN_VOLUME + (Height - e.Y) * (MAX_VOLUME - MIN_VOLUME) / Height;

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
                    Value -= RESOLUTION;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.Up)
                {
                    Value += RESOLUTION;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnKeyDown(e);
        }
        #endregion
    }
}
