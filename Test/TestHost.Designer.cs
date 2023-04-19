namespace Ephemera.AudioLib.Test
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestHost));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.txtInfo = new Ephemera.NBagOfUis.TextViewer();
            this.progBar = new Ephemera.AudioLib.ProgressBar();
            this.wv1 = new Ephemera.AudioLib.WaveViewer();
            this.wv2 = new Ephemera.AudioLib.WaveViewer();
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
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAfReader = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.cmbSelMode = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTest = new System.Windows.Forms.ToolStripButton();
            this.sldGain = new Ephemera.NBagOfUis.Slider();
            this.chkPlay = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsItem1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripParamEditor1 = new Ephemera.AudioLib.ToolStripParamEditor();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRewind = new System.Windows.Forms.Button();
            this.btnCaps = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtInfo.Location = new System.Drawing.Point(6, 429);
            this.txtInfo.MaxText = 50000;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Prompt = "";
            this.txtInfo.Size = new System.Drawing.Size(985, 135);
            this.txtInfo.TabIndex = 29;
            this.txtInfo.WordWrap = true;
            // 
            // progBar
            // 
            this.progBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.progBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.progBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.progBar.Location = new System.Drawing.Point(332, 44);
            this.progBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(659, 48);
            this.progBar.TabIndex = 24;
            this.progBar.Thumbnail = null;
            // 
            // wv1
            // 
            this.wv1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wv1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wv1.Gain = 1F;
            this.wv1.Location = new System.Drawing.Point(6, 100);
            this.wv1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.wv1.Name = "wv1";
            this.wv1.Size = new System.Drawing.Size(985, 196);
            this.wv1.TabIndex = 26;
            this.wv1.TextFont = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            // 
            // wv2
            // 
            this.wv2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wv2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wv2.Gain = 1F;
            this.wv2.Location = new System.Drawing.Point(6, 304);
            this.wv2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.wv2.Name = "wv2";
            this.wv2.Size = new System.Drawing.Size(985, 118);
            this.wv2.TabIndex = 32;
            this.wv2.TextFont = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
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
            this.btnCaps,
            this.toolStripSeparator8,
            this.btnAfReader,
            this.toolStripSeparator9,
            this.toolStripLabel1,
            this.cmbSelMode,
            this.toolStripSeparator4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1000, 28);
            this.toolStrip1.TabIndex = 31;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnRunBars
            // 
            this.btnRunBars.CheckOnClick = true;
            this.btnRunBars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnRunBars.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRunBars.Name = "btnRunBars";
            this.btnRunBars.Size = new System.Drawing.Size(66, 25);
            this.btnRunBars.Text = "run bars";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // btnSettings
            // 
            this.btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(64, 25);
            this.btnSettings.Text = "settings";
            this.btnSettings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // LoadButton
            // 
            this.LoadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.LoadButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.LoadButton.Name = "LoadButton";
            this.LoadButton.Size = new System.Drawing.Size(53, 25);
            this.LoadButton.Text = "load";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // btnSwap
            // 
            this.btnSwap.CheckOnClick = true;
            this.btnSwap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnSwap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(47, 25);
            this.btnSwap.Text = "swap";
            this.btnSwap.Click += new System.EventHandler(this.Swap_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 28);
            // 
            // btnFileInfo
            // 
            this.btnFileInfo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnFileInfo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFileInfo.Name = "btnFileInfo";
            this.btnFileInfo.Size = new System.Drawing.Size(64, 25);
            this.btnFileInfo.Text = "file info";
            this.btnFileInfo.Click += new System.EventHandler(this.FileInfo_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 28);
            // 
            // btnResample
            // 
            this.btnResample.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnResample.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnResample.Name = "btnResample";
            this.btnResample.Size = new System.Drawing.Size(74, 25);
            this.btnResample.Text = "resample";
            this.btnResample.Click += new System.EventHandler(this.Resample_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 28);
            // 
            // btnAfReader
            // 
            this.btnAfReader.CheckOnClick = true;
            this.btnAfReader.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAfReader.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAfReader.Name = "btnAfReader";
            this.btnAfReader.Size = new System.Drawing.Size(65, 25);
            this.btnAfReader.Text = "afr prov";
            this.btnAfReader.ToolTipText = "Select AudioFileReader";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(73, 25);
            this.toolStripLabel1.Text = "sel mode:";
            // 
            // cmbSelMode
            // 
            this.cmbSelMode.AutoSize = false;
            this.cmbSelMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSelMode.Name = "cmbSelMode";
            this.cmbSelMode.Size = new System.Drawing.Size(80, 28);
            this.cmbSelMode.ToolTipText = "Selection mode";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 28);
            // 
            // btnTest
            // 
            this.btnTest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnTest.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(37, 24);
            this.btnTest.Text = "test";
            // 
            // sldGain
            // 
            this.sldGain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.sldGain.DrawColor = System.Drawing.Color.CornflowerBlue;
            this.sldGain.Label = "gain";
            this.sldGain.Location = new System.Drawing.Point(168, 44);
            this.sldGain.Maximum = 3D;
            this.sldGain.Minimum = 0D;
            this.sldGain.Name = "sldGain";
            this.sldGain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.sldGain.Resolution = 0.05D;
            this.sldGain.Size = new System.Drawing.Size(96, 48);
            this.sldGain.TabIndex = 33;
            this.sldGain.Value = 1D;
            // 
            // chkPlay
            // 
            this.chkPlay.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkPlay.BackColor = System.Drawing.SystemColors.Control;
            this.chkPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkPlay.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.chkPlay.Location = new System.Drawing.Point(6, 44);
            this.chkPlay.Name = "chkPlay";
            this.chkPlay.Size = new System.Drawing.Size(57, 48);
            this.chkPlay.TabIndex = 36;
            this.chkPlay.Text = "play";
            this.chkPlay.UseVisualStyleBackColor = false;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComboBox1,
            this.toolStripMenuItem1,
            this.cmsItem1ToolStripMenuItem,
            this.toolStripTextBox1,
            this.toolStripSeparator10,
            this.toolStripParamEditor1,
            this.toolStripSeparator11});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(232, 148);
            // 
            // toolStripComboBox1
            // 
            this.toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.toolStripComboBox1.Items.AddRange(new object[] {
            "sel1",
            "sel2",
            "sel3"});
            this.toolStripComboBox1.Name = "toolStripComboBox1";
            this.toolStripComboBox1.Size = new System.Drawing.Size(121, 28);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(231, 24);
            this.toolStripMenuItem1.Text = "Log it";
            // 
            // cmsItem1ToolStripMenuItem
            // 
            this.cmsItem1ToolStripMenuItem.Name = "cmsItem1ToolStripMenuItem";
            this.cmsItem1ToolStripMenuItem.Size = new System.Drawing.Size(231, 24);
            this.cmsItem1ToolStripMenuItem.Text = "MenuItem1";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(100, 27);
            this.toolStripTextBox1.Text = "This is a textbox";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(228, 6);
            // 
            // toolStripParamEditor1
            // 
            this.toolStripParamEditor1.AutoSize = false;
            this.toolStripParamEditor1.Name = "toolStripParamEditor1";
            this.toolStripParamEditor1.Size = new System.Drawing.Size(171, 20);
            this.toolStripParamEditor1.Value = -1;
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(228, 6);
            // 
            // btnRewind
            // 
            this.btnRewind.BackColor = System.Drawing.SystemColors.Control;
            this.btnRewind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRewind.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnRewind.Location = new System.Drawing.Point(69, 45);
            this.btnRewind.Name = "btnRewind";
            this.btnRewind.Size = new System.Drawing.Size(81, 47);
            this.btnRewind.TabIndex = 37;
            this.btnRewind.Text = "Rewind";
            this.btnRewind.UseVisualStyleBackColor = false;
            // 
            // btnCaps
            // 
            this.btnCaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCaps.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCaps.Name = "btnCaps";
            this.btnCaps.Size = new System.Drawing.Size(43, 25);
            this.btnCaps.Text = "caps";
            this.btnCaps.Click += new System.EventHandler(this.Caps_Click);
            // 
            // TestHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 576);
            this.Controls.Add(this.btnRewind);
            this.Controls.Add(this.chkPlay);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.sldGain);
            this.Controls.Add(this.wv2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.wv1);
            this.Controls.Add(this.progBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TestHost";
            this.Text = "TestHost";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private ProgressBar progBar;
        private WaveViewer wv1;
        private WaveViewer wv2;
        private NBagOfUis.TextViewer txtInfo;
        private NBagOfUis.Slider sldGain;
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
        private System.Windows.Forms.ToolStripButton btnResample;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton btnTest;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton btnAfReader;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.CheckBox chkPlay;
        private System.Windows.Forms.ToolStripComboBox cmbSelMode;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem cmsItem1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private ToolStripParamEditor toolStripParamEditor1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.Button btnRewind;
        private System.Windows.Forms.ToolStripButton btnCaps;
    }
}