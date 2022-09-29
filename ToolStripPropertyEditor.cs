using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Forms.Design;
using System.Drawing;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>Simple toolstrip container for the property editor.</summary>
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ContextMenuStrip)]
    public class ToolStripPropertyEditor : ToolStripControlHost
    {
        #region Fields
        /// <summary>Contained control.</summary>
        TextBox _ed = new();

        ///// <summary>Backer.</summary>
        //int _value;

        /// <summary>OK color.</summary>
        readonly Color _validColor = SystemColors.Window;

        /// <summary>Not OK color.</summary>
        readonly Color _invalidColor = Color.LightPink;
        #endregion

        #region Properties
        /// <summary>Current value (sample) or -1 if invalid.</summary>
        public int Value
        {
            get { return Globals.ConverterOps.Parse(_ed.Text); }
            set { _ed.Text = value < 0 ? "" : Globals.ConverterOps.Format(value); }
        }

        /// <summary>Tool tip or other label.</summary>
        public string Label { set { ToolTipText = value; } }
        #endregion

        #region Events
        /// <summary>Slider value changed event.</summary>
        public event EventHandler? ValueChanged;
        #endregion

        /// <summary>
        /// Make one.
        /// </summary>
        public ToolStripPropertyEditor() : base(new TextBox())
        {
            _ed = (TextBox)Control;
            _ed.BorderStyle = BorderStyle.None;

            AutoSize = false;
            Width = _ed.Width;
            Height = _ed.Height;

            _ed.KeyDown += Ed_KeyDown;
            _ed.KeyPress += Ed_KeyPress;
            _ed.Leave += Ed_Leave;
        }

        /// <summary>
        /// Look at what the user entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Ed_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ValidateProperty();
            }
        }

        /// <summary>
        /// Initial sanity check.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Ed_KeyPress(object? sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            e.Handled = !((c >= '0' && c <= '9') || (c == '.') || (c == '\b'));
        }

        /// <summary>
        /// Look at what the user entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Ed_Leave(object? sender, EventArgs e)
        {
            ValidateProperty();
        }

        /// <summary>
        /// Executed when done editing.
        /// </summary>
        void ValidateProperty()
        {
            int sample = Globals.ConverterOps.Parse(_ed.Text);

            if (sample >= 0)
            {
                ValueChanged?.Invoke(this, EventArgs.Empty);
                _ed.BackColor = _validColor;
            }
            else
            {
                _ed.BackColor = _invalidColor;
            }
        }
    }
}