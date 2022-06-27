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
            this.meterDots = new AudioLib.Meter();
            this.meterLog = new AudioLib.Meter();
            this.volume2 = new AudioLib.Volume();
            this.pan1 = new AudioLib.Pan();
            this.meterLinear = new AudioLib.Meter();
            this.pot1 = new AudioLib.Pot();
            this.volume1 = new AudioLib.Volume();
            this.waveViewer1 = new AudioLib.WaveViewer();
            this.waveViewer2 = new AudioLib.WaveViewer();
            this.btnSettings = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.RichTextBox();
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
            this.chkRunBars.Location = new System.Drawing.Point(755, 21);
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
            this.timeBar.Location = new System.Drawing.Point(755, 56);
            this.timeBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.timeBar.MarkerColor = System.Drawing.Color.Black;
            this.timeBar.Name = "timeBar";
            this.timeBar.ProgressColor = System.Drawing.Color.Orange;
            this.timeBar.Size = new System.Drawing.Size(353, 64);
            this.timeBar.SnapMsec = 0;
            this.timeBar.TabIndex = 24;
            // 
            // meterDots
            // 
            this.meterDots.BackColor = System.Drawing.Color.Gainsboro;
            this.meterDots.DrawColor = System.Drawing.Color.Violet;
            this.meterDots.Label = "meter dots";
            this.meterDots.Location = new System.Drawing.Point(306, 16);
            this.meterDots.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.meterDots.Maximum = 10D;
            this.meterDots.MeterType = AudioLib.MeterType.ContinuousDots;
            this.meterDots.Minimum = -10D;
            this.meterDots.Name = "meterDots";
            this.meterDots.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.meterDots.Size = new System.Drawing.Size(180, 60);
            this.meterDots.TabIndex = 23;
            // 
            // meterLog
            // 
            this.meterLog.BackColor = System.Drawing.Color.Gainsboro;
            this.meterLog.DrawColor = System.Drawing.Color.Azure;
            this.meterLog.Label = "meter log";
            this.meterLog.Location = new System.Drawing.Point(306, 89);
            this.meterLog.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.meterLog.Maximum = 3D;
            this.meterLog.MeterType = AudioLib.MeterType.Log;
            this.meterLog.Minimum = -60D;
            this.meterLog.Name = "meterLog";
            this.meterLog.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.meterLog.Size = new System.Drawing.Size(180, 60);
            this.meterLog.TabIndex = 22;
            // 
            // volume2
            // 
            this.volume2.BackColor = System.Drawing.Color.Gainsboro;
            this.volume2.DrawColor = System.Drawing.Color.SlateBlue;
            this.volume2.Label = "Vertical";
            this.volume2.Location = new System.Drawing.Point(685, 16);
            this.volume2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.volume2.Name = "volume2";
            this.volume2.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.volume2.Size = new System.Drawing.Size(42, 133);
            this.volume2.TabIndex = 21;
            this.volume2.Value = 0.60000000000000009D;
            // 
            // pan1
            // 
            this.pan1.BackColor = System.Drawing.Color.Gainsboro;
            this.pan1.DrawColor = System.Drawing.Color.Crimson;
            this.pan1.Location = new System.Drawing.Point(111, 89);
            this.pan1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pan1.Name = "pan1";
            this.pan1.Size = new System.Drawing.Size(175, 60);
            this.pan1.TabIndex = 20;
            this.pan1.Value = 0D;
            // 
            // meterLinear
            // 
            this.meterLinear.BackColor = System.Drawing.Color.Gainsboro;
            this.meterLinear.DrawColor = System.Drawing.Color.Orange;
            this.meterLinear.Label = "meter lin";
            this.meterLinear.Location = new System.Drawing.Point(512, 89);
            this.meterLinear.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.meterLinear.Maximum = 100D;
            this.meterLinear.MeterType = AudioLib.MeterType.Linear;
            this.meterLinear.Minimum = 0D;
            this.meterLinear.Name = "meterLinear";
            this.meterLinear.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.meterLinear.Size = new System.Drawing.Size(153, 60);
            this.meterLinear.TabIndex = 19;
            // 
            // pot1
            // 
            this.pot1.BackColor = System.Drawing.Color.Gainsboro;
            this.pot1.DrawColor = System.Drawing.Color.Green;
            this.pot1.ForeColor = System.Drawing.Color.Black;
            this.pot1.Label = "p99";
            this.pot1.Location = new System.Drawing.Point(14, 56);
            this.pot1.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.pot1.Maximum = 50D;
            this.pot1.Minimum = 25D;
            this.pot1.Name = "pot1";
            this.pot1.Resolution = 0.5D;
            this.pot1.Size = new System.Drawing.Size(81, 91);
            this.pot1.TabIndex = 17;
            this.pot1.Taper = AudioLib.Taper.Linear;
            this.pot1.Value = 35D;
            // 
            // volume1
            // 
            this.volume1.BackColor = System.Drawing.Color.Gainsboro;
            this.volume1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.volume1.DrawColor = System.Drawing.Color.Orange;
            this.volume1.Label = "Horizontal";
            this.volume1.Location = new System.Drawing.Point(512, 16);
            this.volume1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.volume1.Name = "volume1";
            this.volume1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.volume1.Size = new System.Drawing.Size(153, 60);
            this.volume1.TabIndex = 18;
            this.volume1.Value = 0.30000000000000004D;
            // 
            // waveViewer1
            // 
            this.waveViewer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewer1.DrawColor = System.Drawing.Color.Orange;
            this.waveViewer1.Location = new System.Drawing.Point(14, 174);
            this.waveViewer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewer1.Marker1 = -1;
            this.waveViewer1.Marker2 = -1;
            this.waveViewer1.MarkerColor = System.Drawing.Color.Black;
            this.waveViewer1.Mode = AudioLib.WaveViewer.DrawMode.Envelope;
            this.waveViewer1.Name = "waveViewer1";
            this.waveViewer1.Size = new System.Drawing.Size(353, 87);
            this.waveViewer1.TabIndex = 26;
            // 
            // waveViewer2
            // 
            this.waveViewer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.waveViewer2.DrawColor = System.Drawing.Color.Orange;
            this.waveViewer2.Location = new System.Drawing.Point(391, 174);
            this.waveViewer2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.waveViewer2.Marker1 = -1;
            this.waveViewer2.Marker2 = -1;
            this.waveViewer2.MarkerColor = System.Drawing.Color.Black;
            this.waveViewer2.Mode = AudioLib.WaveViewer.DrawMode.Envelope;
            this.waveViewer2.Name = "waveViewer2";
            this.waveViewer2.Size = new System.Drawing.Size(353, 87);
            this.waveViewer2.TabIndex = 27;
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(881, 16);
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
            this.txtInfo.Location = new System.Drawing.Point(784, 145);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(395, 120);
            this.txtInfo.TabIndex = 29;
            this.txtInfo.Text = "";
            // 
            // TestHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1191, 281);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.waveViewer2);
            this.Controls.Add(this.waveViewer1);
            this.Controls.Add(this.chkRunBars);
            this.Controls.Add(this.timeBar);
            this.Controls.Add(this.meterDots);
            this.Controls.Add(this.meterLog);
            this.Controls.Add(this.volume2);
            this.Controls.Add(this.pan1);
            this.Controls.Add(this.meterLinear);
            this.Controls.Add(this.pot1);
            this.Controls.Add(this.volume1);
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
        private Meter meterDots;
        private Meter meterLog;
        private Volume volume2;
        private Pan pan1;
        private Meter meterLinear;
        private Pot pot1;
        private Volume volume1;
        private WaveViewer waveViewer1;
        private WaveViewer waveViewer2;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.RichTextBox txtInfo;
    }
}