namespace AudioLib.Test
{
    partial class TestHost
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.chkRunBars = new System.Windows.Forms.CheckBox();
            this.timeBar = new AudioLib.TimeBar();
            this.waveViewer1 = new AudioLib.WaveViewer();
            this.waveViewer2 = new AudioLib.WaveViewer();
            this.btnSettings = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.RichTextBox();
            this.waveViewer3 = new AudioLib.WaveViewer();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // chkRunBars
            // 
            this.chkRunBars.AutoSize = true;
            this.chkRunBars.BackColor = System.Drawing.Color.Pink;
            this.chkRunBars.Checked = true;
            this.chkRunBars.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRunBars.Font = new System.Drawing.Font("Cooper Black", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.chkRunBars.Location = new System.Drawing.Point(12, 15);
            this.chkRunBars.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkRunBars.Name = "chkRunBars";
            this.chkRunBars.Size = new System.Drawing.Size(101, 21);
            this.chkRunBars.TabIndex = 25;
            this.chkRunBars.Text = "Run Bars";
            this.chkRunBars.UseVisualStyleBackColor = false;
            // 
            // timeBar
            // 
            this.timeBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.timeBar.FontLarge = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.timeBar.FontSmall = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.timeBar.Location = new System.Drawing.Point(427, 15);
            this.timeBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.timeBar.MarkerColor = System.Drawing.Color.Black;
            this.timeBar.Name = "timeBar";
            this.timeBar.ProgressColor = System.Drawing.Color.Orange;
            this.timeBar.Size = new System.Drawing.Size(545, 64);
            this.timeBar.SnapMsec = 0;
            this.timeBar.TabIndex = 24;
            // 
            // waveViewer1
            // 
            this.waveViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewer1.DrawColor = System.Drawing.Color.Orange;
            this.waveViewer1.Location = new System.Drawing.Point(427, 92);
            this.waveViewer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewer1.Marker = 0;
            this.waveViewer1.Name = "waveViewer1";
            this.waveViewer1.Size = new System.Drawing.Size(545, 115);
            this.waveViewer1.TabIndex = 26;
            // 
            // waveViewer2
            // 
            this.waveViewer2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewer2.DrawColor = System.Drawing.Color.Orange;
            this.waveViewer2.Location = new System.Drawing.Point(427, 215);
            this.waveViewer2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewer2.Marker = 0;
            this.waveViewer2.Name = "waveViewer2";
            this.waveViewer2.Size = new System.Drawing.Size(545, 115);
            this.waveViewer2.TabIndex = 27;
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(138, 10);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(94, 29);
            this.btnSettings.TabIndex = 28;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.Settings_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtInfo.Location = new System.Drawing.Point(12, 44);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(395, 408);
            this.txtInfo.TabIndex = 29;
            this.txtInfo.Text = "";
            // 
            // waveViewer3
            // 
            this.waveViewer3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.waveViewer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewer3.DrawColor = System.Drawing.Color.Orange;
            this.waveViewer3.Location = new System.Drawing.Point(427, 338);
            this.waveViewer3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewer3.Marker = 0;
            this.waveViewer3.Name = "waveViewer3";
            this.waveViewer3.Size = new System.Drawing.Size(545, 115);
            this.waveViewer3.TabIndex = 30;
            // 
            // TestHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 471);
            this.Controls.Add(this.waveViewer3);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.waveViewer2);
            this.Controls.Add(this.waveViewer1);
            this.Controls.Add(this.chkRunBars);
            this.Controls.Add(this.timeBar);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "TestHost";
            this.Text = "TestHost";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox chkRunBars;
        private TimeBar timeBar;
        private WaveViewer waveViewer1;
        private WaveViewer waveViewer2;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.RichTextBox txtInfo;
        private WaveViewer waveViewer3;
    }
}