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
            components = new System.ComponentModel.Container();
            timer1 = new System.Windows.Forms.Timer(components);
            tvInfo = new Ephemera.NBagOfUis.TextViewer();
            timeBar = new TimeBar();
            wv1 = new WaveViewer();
            wv2 = new WaveViewer();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            btnRunBars = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            btnSettings = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            btnOpen = new System.Windows.Forms.ToolStripDropDownButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            btnSwap = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            btnFileInfo = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            btnResample = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            btnCaps = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            cmbSelMode = new System.Windows.Forms.ToolStripComboBox();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            btnTest = new System.Windows.Forms.ToolStripButton();
            sldGain = new Ephemera.NBagOfUis.Slider();
            chkPlay = new System.Windows.Forms.CheckBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripComboBox1 = new System.Windows.Forms.ToolStripComboBox();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            cmsItem1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            btnRewind = new System.Windows.Forms.Button();
            toolStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // timer1
            // 
            timer1.Tick += Timer1_Tick;
            // 
            // tvInfo
            // 
            tvInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tvInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            tvInfo.Location = new System.Drawing.Point(6, 410);
            tvInfo.Name = "tvInfo";
            tvInfo.Size = new System.Drawing.Size(985, 154);
            tvInfo.TabIndex = 29;
            // 
            // timeBar
            // 
            timeBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            timeBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            timeBar.Location = new System.Drawing.Point(332, 42);
            timeBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            timeBar.Name = "timeBar";
            timeBar.Size = new System.Drawing.Size(659, 46);
            timeBar.TabIndex = 24;
            // 
            // wv1
            // 
            wv1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            wv1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            wv1.Location = new System.Drawing.Point(6, 95);
            wv1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            wv1.Name = "wv1";
            wv1.Size = new System.Drawing.Size(985, 150);
            wv1.TabIndex = 26;
            // 
            // wv2
            // 
            wv2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            wv2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            wv2.Location = new System.Drawing.Point(6, 253);
            wv2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            wv2.Name = "wv2";
            wv2.Size = new System.Drawing.Size(985, 150);
            wv2.TabIndex = 32;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { btnRunBars, toolStripSeparator1, btnSettings, toolStripSeparator2, btnOpen, toolStripSeparator3, btnSwap, toolStripSeparator5, btnFileInfo, toolStripSeparator6, btnResample, toolStripSeparator7, btnCaps, toolStripSeparator8, toolStripLabel1, cmbSelMode, toolStripSeparator4 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(1000, 27);
            toolStrip1.TabIndex = 31;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnRunBars
            // 
            btnRunBars.CheckOnClick = true;
            btnRunBars.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnRunBars.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnRunBars.Name = "btnRunBars";
            btnRunBars.Size = new System.Drawing.Size(64, 24);
            btnRunBars.Text = "run bars";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // btnSettings
            // 
            btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new System.Drawing.Size(61, 24);
            btnSettings.Text = "settings";
            btnSettings.Click += Settings_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // btnOpen
            // 
            btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new System.Drawing.Size(54, 24);
            btnOpen.Text = "open";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // btnSwap
            // 
            btnSwap.CheckOnClick = true;
            btnSwap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnSwap.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnSwap.Name = "btnSwap";
            btnSwap.Size = new System.Drawing.Size(44, 24);
            btnSwap.Text = "swap";
            btnSwap.Click += Swap_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(6, 27);
            // 
            // btnFileInfo
            // 
            btnFileInfo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnFileInfo.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnFileInfo.Name = "btnFileInfo";
            btnFileInfo.Size = new System.Drawing.Size(57, 24);
            btnFileInfo.Text = "file info";
            btnFileInfo.Click += FileInfo_Click;
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(6, 27);
            // 
            // btnResample
            // 
            btnResample.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnResample.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnResample.Name = "btnResample";
            btnResample.Size = new System.Drawing.Size(68, 24);
            btnResample.Text = "resample";
            btnResample.Click += Resample_Click;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(6, 27);
            // 
            // btnCaps
            // 
            btnCaps.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnCaps.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnCaps.Name = "btnCaps";
            btnCaps.Size = new System.Drawing.Size(40, 24);
            btnCaps.Text = "caps";
            btnCaps.Click += Caps_Click;
            // 
            // toolStripSeparator8
            // 
            toolStripSeparator8.Name = "toolStripSeparator8";
            toolStripSeparator8.Size = new System.Drawing.Size(6, 27);
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new System.Drawing.Size(67, 24);
            toolStripLabel1.Text = "sel mode:";
            // 
            // cmbSelMode
            // 
            cmbSelMode.AutoSize = false;
            cmbSelMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbSelMode.Name = "cmbSelMode";
            cmbSelMode.Size = new System.Drawing.Size(80, 27);
            cmbSelMode.ToolTipText = "Selection mode";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 27);
            // 
            // btnTest
            // 
            btnTest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            btnTest.ImageTransparentColor = System.Drawing.Color.Magenta;
            btnTest.Name = "btnTest";
            btnTest.Size = new System.Drawing.Size(37, 24);
            btnTest.Text = "test";
            // 
            // sldGain
            // 
            sldGain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            sldGain.Location = new System.Drawing.Point(168, 42);
            sldGain.Name = "sldGain";
            sldGain.Size = new System.Drawing.Size(96, 46);
            sldGain.TabIndex = 33;
            // 
            // chkPlay
            // 
            chkPlay.Appearance = System.Windows.Forms.Appearance.Button;
            chkPlay.BackColor = System.Drawing.SystemColors.Control;
            chkPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            chkPlay.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            chkPlay.Location = new System.Drawing.Point(6, 42);
            chkPlay.Name = "chkPlay";
            chkPlay.Size = new System.Drawing.Size(57, 46);
            chkPlay.TabIndex = 36;
            chkPlay.Text = "play";
            chkPlay.UseVisualStyleBackColor = false;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripComboBox1, toolStripMenuItem1, cmsItem1ToolStripMenuItem, toolStripTextBox1, toolStripSeparator10, toolStripSeparator11 });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(182, 123);
            // 
            // toolStripComboBox1
            // 
            toolStripComboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            toolStripComboBox1.Items.AddRange(new object[] { "sel1", "sel2", "sel3" });
            toolStripComboBox1.Name = "toolStripComboBox1";
            toolStripComboBox1.Size = new System.Drawing.Size(121, 27);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(181, 24);
            toolStripMenuItem1.Text = "Log it";
            // 
            // cmsItem1ToolStripMenuItem
            // 
            cmsItem1ToolStripMenuItem.Name = "cmsItem1ToolStripMenuItem";
            cmsItem1ToolStripMenuItem.Size = new System.Drawing.Size(181, 24);
            cmsItem1ToolStripMenuItem.Text = "MenuItem1";
            // 
            // toolStripTextBox1
            // 
            toolStripTextBox1.Name = "toolStripTextBox1";
            toolStripTextBox1.Size = new System.Drawing.Size(100, 26);
            toolStripTextBox1.Text = "This is a textbox";
            // 
            // toolStripSeparator10
            // 
            toolStripSeparator10.Name = "toolStripSeparator10";
            toolStripSeparator10.Size = new System.Drawing.Size(178, 6);
            // 
            // toolStripSeparator11
            // 
            toolStripSeparator11.Name = "toolStripSeparator11";
            toolStripSeparator11.Size = new System.Drawing.Size(178, 6);
            // 
            // btnRewind
            // 
            btnRewind.BackColor = System.Drawing.SystemColors.Control;
            btnRewind.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnRewind.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Bold);
            btnRewind.Location = new System.Drawing.Point(69, 43);
            btnRewind.Name = "btnRewind";
            btnRewind.Size = new System.Drawing.Size(81, 45);
            btnRewind.TabIndex = 37;
            btnRewind.Text = "Rewind";
            btnRewind.UseVisualStyleBackColor = false;
            // 
            // TestHost
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1000, 576);
            Controls.Add(btnRewind);
            Controls.Add(chkPlay);
            Controls.Add(tvInfo);
            Controls.Add(sldGain);
            Controls.Add(wv2);
            Controls.Add(toolStrip1);
            Controls.Add(wv1);
            Controls.Add(timeBar);
            Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            Name = "TestHost";
            Text = "TestHost";
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            contextMenuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private TimeBar timeBar;
        private WaveViewer wv1;
        private WaveViewer wv2;
        private NBagOfUis.TextViewer tvInfo;
        private NBagOfUis.Slider sldGain;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRunBars;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSettings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton btnOpen;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnSwap;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnFileInfo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton btnResample;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton btnTest;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.Button btnRewind;
        private System.Windows.Forms.ToolStripButton btnCaps;
    }
}