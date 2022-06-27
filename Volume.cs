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
        /// <summary>Current value.</summary>
        double _value = VolumeDefs.DEFAULT;

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
            set { _value = MathUtils.Constrain(value, VolumeDefs.MIN, VolumeDefs.MAX, VolumeDefs.STEP); Invalidate(); }
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
                double x = (_value - VolumeDefs.MIN) / (VolumeDefs.MAX - VolumeDefs.MIN);
                pe.Graphics.FillRectangle(_brush, ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width * (float)x, ClientRectangle.Height);
            }
            else
            {
                double y = 1.0 - (_value - VolumeDefs.MIN) / (VolumeDefs.MAX - VolumeDefs.MIN);
                pe.Graphics.FillRectangle(_brush, ClientRectangle.Left, ClientRectangle.Height * (float)y, ClientRectangle.Width, ClientRectangle.Bottom);
            }

            // Text.
            string sval = _value.ToString("#0." + new string('0', MathUtils.DecPlaces(VolumeDefs.STEP)));
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
                VolumeDefs.MIN + e.X * (VolumeDefs.MAX - VolumeDefs.MIN) / Width :
                VolumeDefs.MIN + (Height - e.Y) * (VolumeDefs.MAX - VolumeDefs.MIN) / Height;

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
                    Value -= VolumeDefs.STEP;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (e.KeyCode == Keys.Up)
                {
                    Value += VolumeDefs.STEP;
                    ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            base.OnKeyDown(e);
        }
        #endregion
    }
}
