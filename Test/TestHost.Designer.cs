namespace AudioLib.Test
{
    partial class TestHost
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtInfo = new System.Windows.Forms.RichTextBox();
            this.timeBar = new AudioLib.TimeBar();
            this.waveViewer1 = new AudioLib.WaveViewer();
            this.waveViewer2 = new AudioLib.WaveViewer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRunBars = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.wavToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mp3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flacToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m4aToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sinToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnPlayer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSwap = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnFileInfo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.sldGain = new NBagOfUis.Slider();
            this.navBar = new System.Windows.Forms.HScrollBar();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnResample = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtInfo.Location = new System.Drawing.Point(10, 482);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(1157, 96);
            this.txtInfo.TabIndex = 29;
            this.txtInfo.Text = "";
            // 
            // timeBar
            // 
            this.timeBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timeBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.timeBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.timeBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.timeBar.Location = new System.Drawing.Point(10, 44);
            this.timeBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.timeBar.MarkerColor = System.Drawing.Color.Black;
            this.timeBar.Name = "timeBar";
            this.timeBar.ProgressColor = System.Drawing.Color.Orange;
            this.timeBar.Size = new System.Drawing.Size(555, 64);
            this.timeBar.SnapMsec = 0;
            this.timeBar.TabIndex = 24;
            // 
            // waveViewer1
            // 
            this.waveViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewer1.DrawColor = System.Drawing.Color.Orange;
            this.waveViewer1.Gain = 1F;
            this.waveViewer1.GainIncrement = 0.05F;
            this.waveViewer1.GridColor = System.Drawing.Color.LightGray;
            this.waveViewer1.Location = new System.Drawing.Point(10, 129);
            this.waveViewer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewer1.MarkColor = System.Drawing.Color.Red;
            this.waveViewer1.Name = "waveViewer1";
            this.waveViewer1.PanFactor = 10;
            this.waveViewer1.Size = new System.Drawing.Size(1157, 130);
            this.waveViewer1.TabIndex = 26;
            this.waveViewer1.WheelResolution = 8;
            this.waveViewer1.ZoomFactor = 20;
            // 
            // waveViewer2
            // 
            this.waveViewer2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewer2.DrawColor = System.Drawing.Color.Orange;
            this.waveViewer2.Gain = 1F;
            this.waveViewer2.GainIncrement = 0.05F;
            this.waveViewer2.GridColor = System.Drawing.Color.LightGray;
            this.waveViewer2.Location = new System.Drawing.Point(10, 258);
            this.waveViewer2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewer2.MarkColor = System.Drawing.Color.Red;
            this.waveViewer2.Name = "waveViewer2";
            this.waveViewer2.PanFactor = 10;
            this.waveViewer2.Size = new System.Drawing.Size(1157, 130);
            this.waveViewer2.TabIndex = 32;
            this.waveViewer2.WheelResolution = 8;
            this.waveViewer2.ZoomFactor = 20;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRunBars,
            this.toolStripSeparator1,
            this.btnSettings,
            this.toolStripSeparator2,
            this.toolStripDropDownButton1,
            this.toolStripSeparator3,
            this.btnPlayer,
            this.toolStripSeparator4,
            this.btnSwap,
            this.toolStripSeparator5,
            this.btnFileInfo,
            this.toolStripSeparator6,
            this.btnResample,
            this.toolStripSeparator7});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1175, 27);
            this.toolStrip1.TabIndex = 31;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnRunBars
            // 
            this.btnRunBars.CheckOnClick = true;
            this.btnRunBars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRunBars.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRunBars.Name = "btnRunBars";
            this.btnRunBars.Size = new System.Drawing.Size(66, 24);
            this.btnRunBars.Text = "run bars";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // btnSettings
            // 
            this.btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(64, 24);
            this.btnSettings.Text = "settings";
            this.btnSettings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wavToolStripMenuItem,
            this.mp3ToolStripMenuItem,
            this.flacToolStripMenuItem,
            this.m4aToolStripMenuItem,
            this.txtToolStripMenuItem,
            this.sinToolStripMenuItem,
            this.shortToolStripMenuItem});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(53, 24);
            this.toolStripDropDownButton1.Text = "load";
            // 
            // wavToolStripMenuItem
            // 
            this.wavToolStripMenuItem.Name = "wavToolStripMenuItem";
            this.wavToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.wavToolStripMenuItem.Text = "wav";
            this.wavToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // mp3ToolStripMenuItem
            // 
            this.mp3ToolStripMenuItem.Name = "mp3ToolStripMenuItem";
            this.mp3ToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.mp3ToolStripMenuItem.Text = "mp3";
            this.mp3ToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // flacToolStripMenuItem
            // 
            this.flacToolStripMenuItem.Name = "flacToolStripMenuItem";
            this.flacToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.flacToolStripMenuItem.Text = "flac";
            this.flacToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // m4aToolStripMenuItem
            // 
            this.m4aToolStripMenuItem.Name = "m4aToolStripMenuItem";
            this.m4aToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.m4aToolStripMenuItem.Text = "m4a";
            this.m4aToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // txtToolStripMenuItem
            // 
            this.txtToolStripMenuItem.Name = "txtToolStripMenuItem";
            this.txtToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.txtToolStripMenuItem.Text = "txt";
            this.txtToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // sinToolStripMenuItem
            // 
            this.sinToolStripMenuItem.Name = "sinToolStripMenuItem";
            this.sinToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.sinToolStripMenuItem.Text = "sin";
            this.sinToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // shortToolStripMenuItem
            // 
            this.shortToolStripMenuItem.Name = "shortToolStripMenuItem";
            this.shortToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.shortToolStripMenuItem.Text = "short";
            this.shortToolStripMenuItem.Click += new System.EventHandler(this.Load_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnPlayer
            // 
            this.btnPlayer.CheckOnClick = true;
            this.btnPlayer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnPlayer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPlayer.Name = "btnPlayer";
            this.btnPlayer.Size = new System.Drawing.Size(54, 24);
            this.btnPlayer.Text = "player";
            this.btnPlayer.Click += new System.EventHandler(this.Player_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // btnSwap
            // 
            this.btnSwap.CheckOnClick = true;
            this.btnSwap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSwap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(47, 24);
            this.btnSwap.Text = "swap";
            this.btnSwap.Click += new System.EventHandler(this.Swap_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 27);
            // 
            // btnFileInfo
            // 
            this.btnFileInfo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFileInfo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFileInfo.Name = "btnFileInfo";
            this.btnFileInfo.Size = new System.Drawing.Size(64, 24);
            this.btnFileInfo.Text = "file info";
            this.btnFileInfo.Click += new System.EventHandler(this.FileInfo_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 27);
            // 
            // sldGain
            // 
            this.sldGain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sldGain.DrawColor = System.Drawing.Color.CornflowerBlue;
            this.sldGain.Label = "gain";
            this.sldGain.Location = new System.Drawing.Point(593, 44);
            this.sldGain.Maximum = 3D;
            this.sldGain.Minimum = 0D;
            this.sldGain.Name = "sldGain";
            this.sldGain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldGain.Resolution = 0.05D;
            this.sldGain.Size = new System.Drawing.Size(188, 64);
            this.sldGain.TabIndex = 33;
            this.sldGain.Value = 1D;
            // 
            // navBar
            // 
            this.navBar.Location = new System.Drawing.Point(10, 411);
            this.navBar.Name = "navBar";
            this.navBar.Size = new System.Drawing.Size(1157, 26);
            this.navBar.TabIndex = 34;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblInfo});
            this.statusStrip1.Location = new System.Drawing.Point(0, 571);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1175, 26);
            this.statusStrip1.TabIndex = 35;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblInfo
            // 
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(151, 20);
            this.lblInfo.Text = "toolStripStatusLabel1";
            // 
            // btnResample
            // 
            this.btnResample.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnResample.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnResample.Name = "btnResample";
            this.btnResample.Size = new System.Drawing.Size(74, 24);
            this.btnResample.Text = "resample";
            this.btnResample.Click += new System.EventHandler(this.Resample_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 27);
            // 
            // TestHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1175, 597);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.navBar);
            this.Controls.Add(this.sldGain);
            this.Controls.Add(this.waveViewer2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.waveViewer1);
            this.Controls.Add(this.timeBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TestHost";
            this.Text = "TestHost";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private TimeBar timeBar;
        private WaveViewer waveViewer1;
        private WaveViewer waveViewer2;
        private System.Windows.Forms.RichTextBox txtInfo;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRunBars;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem wavToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mp3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flacToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem txtToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sinToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnPlayer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnSwap;
        private System.Windows.Forms.ToolStripMenuItem m4aToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnFileInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private NBagOfUis.Slider sldGain;
        private System.Windows.Forms.HScrollBar navBar;
        private System.Windows.Forms.ToolStripMenuItem shortToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblInfo;
        private System.Windows.Forms.ToolStripButton btnResample;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    }
}