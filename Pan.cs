using System;
using System.Drawing;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>
    /// Pan slider control
    /// </summary>
    public partial class Pan : UserControl
    {
        #region Fields
        /// <summary> </summary>
        double _value;

        /// <summary>The brush.</summary>
        readonly SolidBrush _brush = new(Color.White);

        /// <summary>For drawing text.</summary>
        readonly StringFormat _format = new() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
        #endregion

        #region Properties
        /// <summary>The current Pan setting.</summary>
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = MathUtils.Constrain(value, -1.0, 1.0, 0.01);
                ValueChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        /// <summary>For styling.</summary>
        public Color DrawColor { get { return _brush.Color; } set { _brush.Color = value; } }
        #endregion

        #region Events
        /// <summary>True when pan value changed.</summary>
        public event EventHandler? ValueChanged;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Creates a new PanSlider control.
        /// </summary>
        public Pan()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            Load += Pan_Load;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Pan_Load(object? sender, EventArgs e)
        {
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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

        /// <summary>
        /// Draw control.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // Setup.
            pe.Graphics.Clear(BackColor);

            // Draw data.
            string panValue;
            if (_value == 0.0)
            {
                pe.Graphics.FillRectangle(_brush, Width / 2 - 1, 0, 2, Height);
                panValue = "C";
            }
            else if (_value > 0)
            {
                pe.Graphics.FillRectangle(_brush, Width / 2, 0, (int)(Width / 2 * _value), Height);
                panValue = $"{_value * 100:F0}%R";
            }
            else // < 0
            {
                int w = (int)(Width / 2 * -_value);
                pe.Graphics.FillRectangle(_brush, (Width / 2 - w), 0, w, Height);
                panValue = $"{_value * -100:F0}%L";
            }

            // Draw text.
            pe.Graphics.DrawString(panValue, Font, Brushes.Black, ClientRectangle, _format);
        }

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
            switch (e.Button)
            {
                case MouseButtons.Left:
                    SetValueFromMouse(e);
                    break;

                case MouseButtons.Right:
                    Value = 0;
                    break;
            }

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Calculate position.
        /// </summary>
        /// <param name="e"></param>
        void SetValueFromMouse(MouseEventArgs e)
        {
            Value = ((double)e.X / Width * 2.0f) - 1.0f;
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
                    Value -= 0.01f;
                }
                else if (e.KeyCode == Keys.Up)
                {
                    Value += 0.01f;
                }
            }

            base.OnKeyDown(e);
        }
    }
}
