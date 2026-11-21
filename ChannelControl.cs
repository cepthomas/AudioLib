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


// old - useful???
// - UserRenderer
//     - Used to add UI or graphics to a customized `ChannelControl`.
//
// Options:  def=volume/level/info/Solo/Mute   all=meter(s)/fx/pan
//     public bool ShowSoloMute { get; set; } = false;
//     public bool ShowPan { get; set; } = false;
//     public bool ShowFx { get; set; } = false;
//     public bool ShowMeter { get; set; } = false;
//     /// <summary>Some flavors of control may need to be defeatured.</summary>
//     [Flags]
//     public enum DisplayOptions { None = 0x00, SoloMute = 0x01, Fx = 0x02, Pan = 0x04, All = 0x0F, }


namespace Ephemera.AudioLib
{
    #region Types
    /// <summary>Channel playing.</summary>
    public enum ChannelState { Normal, Solo, Mute }
    #endregion


    [DesignTimeVisible(true)]
    public partial class ChannelControl2 : UserControl
    {
        #region Fields
        /// <summary>Initial bg color.</summary>
        Color _bgColor;

        /// <summary>Control is selected</summary>
        bool _selected = false;

        ChannelState _state = ChannelState.Normal;

        // backing:
        //UserRenderer? _userRenderer = null;

        Color _selectedColor = Color.Red;

        ///// <summary>Control is selected</summary>
        //bool _selected = false;
        #endregion

        #region Properties
        ///// <summary>Display options.</summary>
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public DisplayOptions Options { get; set; } = DisplayOptions.All;

        /// <summary>For muting/soloing.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ChannelState State
        {
           get { return _state; }
            set { _state = value; }// UpdateUi(); }
        }

        /// <summary>User selection.</summary>
        public bool Selected
        {
           get { return _selected; }
           private set { _selected = value; lblInfo.BackColor = _selected ? SelectedColor : BackColor; }
        }

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

        ///// <summary>For muting/soloing.</summary>
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        ////[EditorBrowsable(EditorBrowsableState.Never)]
        //[Browsable(false)]
        //public ChannelState State { get; set; }

        /// <summary>Current volume.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        public double Volume
        {
            get { return sldVolume.Value; }
            set { sldVolume.Value = value; }
        }

        ///// <summary>User selection.</summary>
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //[Browsable(false)]
        //public bool Selected
        //{
        //    get { return _selected; }
        //    set { _selected = value; BackColor = _selected ? SelectedColor : _bgColor;  }
        //}
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
        public ChannelControl2()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            //// Dummy channel to satisfy designer. Will be overwritten by the real one.
            //var dev = new NullOutputDevice("nullout:DUMMY_DEVICE");
            //BoundChannel = new OutputChannel(dev, 9, 0);

            InitializeComponent();
        }

        /// <summary>
        /// Apply customization from system. Properties should be valid now.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            _bgColor = BackColor;
            toolTip.InitialDelay = 100;


            lblInfo.Click += ChannelInfo_Click;
//            sldVolume.ValueChanged += (sender, e) => BoundChannel.Volume = (sender as Slider)!.Value;
            lblSolo.Click += SoloMute_Click;
            lblMute.Click += SoloMute_Click;

            //if (Options.HasFlag(DisplayOptions.Fx))
            //{
  
            //}

            //if (_userRenderer is not null)
            //{
            //    _userRenderer.Location = new(PAD, 100 + PAD);
            //    //_userRenderer.Location = new(PAD, yMax + PAD);
            //    //xMax = Math.Max(_userRenderer.Right, xPos);
            //    //yMax = Math.Max(_userRenderer.Bottom, yMax);
            //    Controls.Add(_userRenderer);
            //}

            // Control itself.
//            Size = new Size(xMax + PAD, yMax + PAD);

            //// Other inits.
            //sldVolume.Value = BoundChannel.Volume;
            //UpdateUi();


            // Volume slider.
            sldVolume.Minimum = 0.0;
            sldVolume.Maximum = AudioLibDefs.MAX_VOLUME;
            sldVolume.Resolution = AudioLibDefs.RES_VOLUME;
            sldVolume.Label = "volume";
            sldVolume.ValueChanged += SldVolume_ValueChanged;

            //if (Options.HasFlag(DisplayOptions.SoloMute))
            {
                // Solo/mute checkboxes.
                lblSolo.Text = "S";
                lblSolo.Click += SoloMute_Click;

                lblMute.Text = "M";
                lblMute.Click += SoloMute_Click;
            }

            // Info textbox.
            lblInfo.AutoEllipsis = true;
            lblInfo.Click += ChannelInfo_Click;

            // Finalize the control.
            toolTip.InitialDelay = 100;

            base.OnLoad(e);
        }

        ///// <summary>
        ///// A custom renderer - optional.
        ///// </summary>
        ///// <param name="rend"></param>
        //public void SetRenderer(UserRenderer rend)
        //{
        //    _userRenderer = rend;

        //    //// Handle custom renderer events. Fix the channel number.
        //    //rend.SendMidi += (object? sender, BaseEvent e) =>
        //    //{
        //    //    e.ChannelNumber = BoundChannel.ChannelNumber;
        //    //    BoundChannel.Send(e);
        //    // //   SendMidi?.Invoke(this, e);
        //    //};

        //    rend.Location = new(PAD, Height);
        //    Height += rend.Height + PAD;
        //    Controls.Add(rend);
        //}
        #endregion

        #region Handlers for user selections
        // /// <summary>User clicked info aka selected it. Maybe host would like to do something.</summary>
        void ChannelInfo_Click(object? sender, EventArgs e)
        {
            InfoClick?.Invoke(this, new());
        }

        /// <summary>User changed volume. Usually not needed by host.</summary>
        void SldVolume_ValueChanged(object? sender, EventArgs e)
        {
        }

        /// <summary>Handles solo and mute.</summary>
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
        // /// <summary>Draw mode checkboxes etc.</summary>
        // void UpdateUi()
        // {
        //     StringBuilder sb = new();

        //     sb.AppendLine($"Channel {BoundChannel.ChannelNumber} {BoundChannel.ChannelName}");
        //     sb.AppendLine($"{BoundChannel.PatchName} {BoundChannel.Patch:000}");
        //     sb.AppendLine(BoundChannel.DeviceInfo);
        //     toolTip.SetToolTip(lblInfo, sb.ToString());

        //     lblInfo.Text = HandleOps.Format(BoundChannel.Handle);

        //     lblSolo.BackColor = _state == ChannelState.Solo ? SelectedColor :  BackColor;
        //     lblMute.BackColor = _state == ChannelState.Mute ? SelectedColor : BackColor;
        // }

        /// <summary>Read me.</summary>
        public override string ToString()
        {
            return lblInfo.Text;
        }
        #endregion
    }

    #region Supplementary classes
    // /// <summary>Notify host of UI changes.</summary>
    // public class ChannelChangeEventArgs : EventArgs
    // {
    //     /// <summary>User clicked info.</summary>
    //     public bool Info { get; set; } = false;
    //     /// <summary>Solo/mute.</summary>
    //     public bool State { get; set; } = false;
    // }

    ///// <summary>Base class for custom renderers.</summary>
    //public class UserRenderer : UserControl
    //{
    //    /// <summary>Parent hooks this.</summary>
    //    public event EventHandler<BaseEvent>? SendMidi;

    //    /// <summary>Derived control helper.</summary>
    //    protected void OnSendMidi(BaseEvent e)
    //    {
    //        SendMidi?.Invoke(this, e);
    //    }
    //}
    #endregion
}
