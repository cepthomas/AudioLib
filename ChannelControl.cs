using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;


namespace Ephemera.AudioLib
{
    #region Types
    /// <summary>Channel playing.</summary>
    public enum ChannelState { Normal, Solo, Mute }
    #endregion


    [DesignTimeVisible(true)]
    public partial class ChannelControl : UserControl
    {
        #region Fields
        /// <summary>Initial bg color.</summary>
        Color _bgColor;

        readonly Color _selectedColor = Color.Red;

        /// <summary>Control is selected</summary>
        bool _selected = false;
        #endregion

        #region Properties - functional
        /// <summary>For muting/soloing.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ChannelState State { get; set; }

        /// <summary>User selection.</summary>
        public bool Selected
        {
            get { return _selected; }
            private set { _selected = value; BackColor = _selected ? SelectedColor : _bgColor; }
        }

        /// <summary>Current volume.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public double Volume
        {
            get { return sldVolume.Value; }
            set { sldVolume.Value = value; }
        }

        /// <summary>Current pan.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public double Pan
        {
            get { return panChannel.Value; }
            set { panChannel.Value = value; }
        }
        #endregion

        #region Properties - cosmetic
        /// <summary>Main info for user.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public string Info1
        {
            get { return lblInfo.Text; }
            set { lblInfo.Text = value; }
        }

        /// <summary>Extra text for tooltip.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public string Info2
        {
            set { toolTip.SetToolTip(lblInfo, value); }
        }

        /// <summary>Drawing the active elements of a control.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color DrawColor
        {
            set
            {
                sldVolume.DrawColor = value;
                panChannel.DrawColor = value;
                //  >>>>  and others
            }
            get
            {
                return sldVolume.DrawColor;
            }
        }

        /// <summary>Drawing the control when selected.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Color SelectedColor { get; set; } = Color.LightPink;
        #endregion


        #region Events
        /// <summary>Notify host of UI changes.</summary>
        public class StateChangeEventArgs : EventArgs { }
        public event EventHandler<StateChangeEventArgs>? StateChange;

        /// <summary>Notify host of UI changes.</summary>
        public class SelectionChangeEventArgs : EventArgs { public bool MultiSelect { get; set; } = false; }
        public event EventHandler<SelectionChangeEventArgs>? SelectionChange;

        /// <summary>Notify host of UI changes.</summary>
        public class InfoClickEventArgs : EventArgs { }
        public event EventHandler<InfoClickEventArgs>? InfoClick;
        #endregion

        #region Lifecycle
        /// <summary>
        /// Normal constructor.
        /// </summary>
        public ChannelControl()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            //// Dummy channel to satisfy designer. Will be overwritten by the real one.
            //var dev = new NullOutputDevice("nullout:DUMMY_DEVICE");
            //BoundChannel = new OutputChannel(dev, 9, 0);

            InitializeComponent();

            _bgColor = BackColor;


            lblInfo.Click += ChannelInfo_Click;
            //sldVolume.ValueChanged += (sender, e) => BoundChannel.Volume = (sender as Slider)!.Value;
            lblSolo.Click += SoloMute_Click;
            lblMute.Click += SoloMute_Click;

            //??? if (Options.HasFlag(DisplayOptions.Fx))

            // Volume slider.
            sldVolume.Minimum = 0.0;
            sldVolume.Maximum = AudioLibDefs.MAX_VOLUME;
            sldVolume.Resolution = AudioLibDefs.RES_VOLUME;
            sldVolume.Label = "volume";
            //sldVolume.ValueChanged += Volume_ValueChanged;

            // Pan
            panChannel.Value = 0;
            //panChannel.ValueChanged += PanChannel_ValueChanged;

            // Solo/mute checkboxes.
            lblSolo.Text = "S";
            lblSolo.Click += SoloMute_Click;
            lblMute.Text = "M";
            lblMute.Click += SoloMute_Click;

            // FX
            btnFx.Click += Fx_Click;

            // Info textbox.
            lblInfo.AutoEllipsis = true;
            lblInfo.Click += ChannelInfo_Click;

            // Finalize the control.
            toolTip.InitialDelay = 100;
        }

        /// <summary>
        /// Apply customization from system. Properties should be valid now.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        #endregion

        #region Handlers for user selections
        /// <summary>
        /// User clicked info aka selected it. Maybe host would like to do something.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ChannelInfo_Click(object? sender, EventArgs e)
        {
            InfoClick?.Invoke(this, new());
        }

        /// <summary>
        /// Handles solo and mute.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="AudioLibException"></exception>
        void SoloMute_Click(object? sender, EventArgs e)
        {
            var lbl = sender as Label;

            // Figure out state.
            if (lbl == lblSolo)
            {
                if (lblSolo.BackColor == DrawColor) // on
                {
                    State = ChannelState.Normal;
                    lblSolo.BackColor = _bgColor;
                }
                else // off
                {
                    State = ChannelState.Solo;
                    lblSolo.BackColor = DrawColor;
                    lblMute.BackColor = _bgColor;
                }
            }
            else if (lbl == lblMute)
            {
                if (lblMute.BackColor == DrawColor) // on
                {
                    State = ChannelState.Normal;
                    lblMute.BackColor = _bgColor;
                }
                else // off
                {
                    State = ChannelState.Mute;
                    lblMute.BackColor = DrawColor;
                    lblSolo.BackColor = _bgColor;
                }
            }
            else
            {
                throw new AudioLibException($"Unknown label {lbl}");
            }

            // Notify host.
            StateChange?.Invoke(this, new());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        void Fx_Click(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// User item selection(s).
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseClick(MouseEventArgs e)
        {
            // Debug.WriteLine($"OnMouseClick {_ctrl}");

            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // Multiselect
                Selected = !Selected;
                SelectionChange?.Invoke(this, new() { MultiSelect = true });
            }
            else
            {
                // Single select
                Selected = !Selected;
                SelectionChange?.Invoke(this, new() { MultiSelect = false });
            }

            base.OnMouseClick(e);
        }

        // protected override void OnMouseEnter(EventArgs e)
        // protected override void OnMouseLeave(EventArgs e)
        // protected override void OnMouseDown(MouseEventArgs e)
        // protected override void OnMouseUp(MouseEventArgs e)

        #endregion

        #region Misc
        /// <summary>Read me.</summary>
        public override string ToString()
        {
            return lblInfo.Text;
        }
        #endregion
    }
}
