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
            this.txtInfo = new NBagOfUis.TextViewer();
            this.timeBar = new AudioLib.TimeBar();
            this.wv1 = new AudioLib.WaveViewer();
            this.wv2 = new AudioLib.WaveViewer();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRunBars = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.LoadButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSwap = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnFileInfo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.btnResample = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTest = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.btnClipProvider = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.sldGain = new NBagOfUis.Slider();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.chkPlay = new System.Windows.Forms.CheckBox();
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
            this.txtInfo.Location = new System.Drawing.Point(361, 44);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(806, 113);
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
            this.timeBar.Location = new System.Drawing.Point(6, 44);
            this.timeBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.timeBar.MarkerColor = System.Drawing.Color.Black;
            this.timeBar.Name = "timeBar";
            this.timeBar.ProgressColor = System.Drawing.Color.Orange;
            this.timeBar.Size = new System.Drawing.Size(345, 64);
            this.timeBar.SnapMsec = 0;
            this.timeBar.TabIndex = 24;
            // 
            // wv1
            // 
            this.wv1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wv1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wv1.Gain = 1F;
            this.wv1.Location = new System.Drawing.Point(6, 164);
            this.wv1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.wv1.Name = "wv1";
            this.wv1.Size = new System.Drawing.Size(1157, 300);
            this.wv1.TabIndex = 26;
            // 
            // wv2
            // 
            this.wv2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wv2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wv2.Gain = 1F;
            this.wv2.Location = new System.Drawing.Point(6, 472);
            this.wv2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.wv2.Name = "wv2";
            this.wv2.Size = new System.Drawing.Size(1157, 95);
            this.wv2.TabIndex = 32;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRunBars,
            this.toolStripSeparator1,
            this.btnSettings,
            this.toolStripSeparator2,
            this.LoadButton,
            this.toolStripSeparator3,
            this.btnSwap,
            this.toolStripSeparator5,
            this.btnFileInfo,
            this.toolStripSeparator6,
            this.btnResample,
            this.toolStripSeparator7,
            this.btnTest,
            this.toolStripSeparator8,
            this.btnClipProvider,
            this.toolStripSeparator9});
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
            // LoadButton
            // 
            this.LoadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.LoadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(53, 24);
            this.LoadButton.Text = "load";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
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
            // btnTest
            // 
            this.btnTest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnTest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(37, 24);
            this.btnTest.Text = "test";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 27);
            // 
            // btnClipProvider
            // 
            this.btnClipProvider.CheckOnClick = true;
            this.btnClipProvider.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnClipProvider.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClipProvider.Name = "btnClipProvider";
            this.btnClipProvider.Size = new System.Drawing.Size(71, 24);
            this.btnClipProvider.Text = "clip prov";
            this.btnClipProvider.ToolTipText = "Select ClipSampleProvider or AudioFileReader";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 27);
            // 
            // sldGain
            // 
            this.sldGain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sldGain.DrawColor = System.Drawing.Color.CornflowerBlue;
            this.sldGain.Label = "gain";
            this.sldGain.Location = new System.Drawing.Point(75, 115);
            this.sldGain.Maximum = 3D;
            this.sldGain.Minimum = 0D;
            this.sldGain.Name = "sldGain";
            this.sldGain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldGain.Resolution = 0.05D;
            this.sldGain.Size = new System.Drawing.Size(96, 42);
            this.sldGain.TabIndex = 33;
            this.sldGain.Value = 1D;
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
            // chkPlay
            // 
            this.chkPlay.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPlay.BackColor = System.Drawing.Color.Fuchsia;
            this.chkPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkPlay.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.chkPlay.Location = new System.Drawing.Point(6, 115);
            this.chkPlay.Name = "chkPlay";
            this.chkPlay.Size = new System.Drawing.Size(57, 42);
            this.chkPlay.TabIndex = 36;
            this.chkPlay.Text = "play";
            this.chkPlay.UseVisualStyleBackColor = false;
            // 
            // TestHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1175, 597);
            this.Controls.Add(this.chkPlay);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.sldGain);
            this.Controls.Add(this.wv2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.wv1);
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
        private WaveViewer wv1;
        private WaveViewer wv2;
        private NBagOfUis.TextViewer txtInfo;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRunBars;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton LoadButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnSwap;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnFileInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private NBagOfUis.Slider sldGain;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblInfo;
        private System.Windows.Forms.ToolStripButton btnResample;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton btnTest;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton btnClipProvider;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.CheckBox chkPlay;
    }
}