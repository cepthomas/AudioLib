using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Design;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json.Serialization;
using NBagOfTricks;
using AudioLib;
using NAudio.Wave;

namespace AudioLib.Test
{
    public partial class TestHost : Form
    {
        public TestHost()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            AudioSettings.LibSettings = new();

            Location = new(20, 20);

            ///// Misc controls.
            pot1.ValueChanged += Pot1_ValueChanged;

            pan1.ValueChanged += Pan1_ValueChanged;

            volume1.ValueChanged += Volume1_ValueChanged;

            volume2.ValueChanged += Volume2_ValueChanged;

            ///// Wave viewer.
            // Simple sin.
            float[] data1 = new float[150];
            for (int i = 0; i < data1.Length; i++)
            {
                data1[i] = (float)Math.Sin(Math.PI * i / 180.0);
            }
            waveViewer1.Mode = WaveViewer.DrawMode.Raw;
            waveViewer1.DrawColor = Color.Green;
            waveViewer1.Init(data1, 1.0f);
            waveViewer1.Marker1 = 20;
            waveViewer1.Marker2 = 130;

            // Real data.
            string[] sdata = File.ReadAllLines(@"..\..\wav.txt");
            float[] data2 = new float[sdata.Length];
            for (int i = 0; i < sdata.Length; i++)
            {
                data2[i] = float.Parse(sdata[i]);
            }
            waveViewer2.Mode = WaveViewer.DrawMode.Envelope;
            waveViewer2.DrawColor = Color.Green;
            waveViewer2.Init(data2, 1.0f);
            waveViewer2.Marker1 = -1; // hide
            waveViewer2.Marker2 = data2.Length / 2;

            ///// Time bar.
            timeBar.SnapMsec = 10;
            timeBar.Length = new TimeSpan(0, 0, 1, 23, 456);
            timeBar.Start = new TimeSpan(0, 0, 0, 10, 333);
            timeBar.End = new TimeSpan(0, 0, 0, 44, 777);
            timeBar.CurrentTimeChanged += TimeBar_CurrentTimeChanged1;
            timeBar.ProgressColor = Color.CornflowerBlue;
            timeBar.BackColor = Color.Salmon;

            // Go-go-go.
            timer1.Enabled = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Inspect.
            //var at = ftree.AllTags;
            //var tp = ftree.TaggedPaths;
            //var po = _testClass;
            base.OnFormClosing(e);
        }

        void EditSettings()
        {
            PropertyGrid pg = new()
            {
                Dock = DockStyle.Fill,
                PropertySort = PropertySort.Categorized,
                SelectedObject = AudioSettings.LibSettings
            };

            using Form f = new()
            {
                ClientSize = new(450, 450),
                AutoScaleMode = AutoScaleMode.None,
                Location = Cursor.Position,
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                ShowIcon = false,
                ShowInTaskbar = false
            };

            f.Controls.Add(pg);

            f.ShowDialog();
        }

        void Timer1_Tick(object? sender, EventArgs e)
        {
            if (chkRunBars.Checked)
            {
                // Update time bar.
                timeBar.IncrementCurrent(timer1.Interval + 3); // not-real time for testing
                if (timeBar.Current >= timeBar.End) // done/reset
                {
                    timeBar.Current = timeBar.Start;
                }
            }
        }

        void TimeBar_CurrentTimeChanged1(object? sender, EventArgs e)
        {
        }

        void Pot1_ValueChanged(object? sender, EventArgs e)
        {
            // 25 -> 50
            meterLog.AddValue(pot1.Value);
        }

        void Volume1_ValueChanged(object? sender, EventArgs e)
        {
            // meterLog -60 -> +3
            // meterLinear 0 -> 100
            // meterDots -10 -> +10

            meterLinear.AddValue(volume1.Value * 100.0);
        }

        void Volume2_ValueChanged(object? sender, EventArgs e)
        {
            meterDots.AddValue(volume2.Value * 20.0 - 10.0);
        }

        void Pan1_ValueChanged(object? sender, EventArgs e)
        {
            meterLog.AddValue(pan1.Value * 50.0 + 50.0);
        }

        void TimeBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
            txtInfo.AppendText($"Current time:{timeBar.Current}");
        }
        void Settings_Click(object sender, EventArgs e)
        {
            EditSettings();

            txtInfo.AppendText(AudioSettings.LibSettings.ToString());
        }
    }
}
