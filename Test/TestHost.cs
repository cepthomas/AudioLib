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
using NAudio.Wave.SampleProviders;

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

            ///// Wave viewers.

            // Simple sin.
            var data1 = new float[1000];
            for (int i = 0; i < data1.Length; i++)
            {
                data1[i] = (float)Math.Sin(Math.PI * i / 30.0);
            }
            waveViewer1.DrawColor = Color.Green;
            waveViewer1.Init(data1);
            waveViewer1.Marker = 333;

            // Real data.
            var sdata = File.ReadAllLines(@"..\..\one-sec.txt");
            var data2 = new float[sdata.Length];
            for (int i = 0; i < sdata.Length; i++)
            {
                data2[i] = float.Parse(sdata[i]);
            }
            waveViewer2.DrawColor = Color.Blue;
            waveViewer2.Init(data2);
            waveViewer2.Marker = 20000;

            // From file.
            var dir = @"C:\Dev\repos\TestAudioFiles\";
            var fn = 
                //"ambi_swoosh.flac";
                //"one-sec.mp3";
                //"one-sec.wav";
                "avTouch_sample.m4a";
                //"ambi_swoosh.flac;
                //"Cave Ceremony 01.wav";
                //"3-04 Kid Charlemagne.mp3";
                //"sin-stereo-audible.wav";
            using (var _reader = new AudioFileReader(dir + fn))
            {
                txtInfo.AppendText($"{fn}:{_reader.WaveFormat}{Environment.NewLine}");

                int len = (int)_reader.Length / (_reader.WaveFormat.BitsPerSample / 8);
                var data3 = new float[len];
                int offset = 0;
                bool done = false;
                while (!done)
                {
                    int toread = 50000;
                    if (len - offset < toread)
                    {
                        toread = len - offset;
                        done = true; // last bunch
                    }
                    offset += _reader.Read(data3, offset, toread);
                }
                waveViewer3.DrawColor = Color.Red;
                waveViewer3.Init(data3);
                waveViewer3.Marker = 30000;
            }

            ///// Time bar.
            timeBar.SnapMsec = 10;
            timeBar.Length = new TimeSpan(0, 0, 1, 23, 456);
            timeBar.Start = new TimeSpan(0, 0, 0, 10, 333);
            timeBar.End = new TimeSpan(0, 0, 0, 44, 777);
            timeBar.CurrentTimeChanged += TimeBar_CurrentTimeChanged;
            timeBar.ProgressColor = Color.CornflowerBlue;
            timeBar.BackColor = Color.Salmon;

            // Go-go-go.
            timer1.Enabled = true;
        }

        void EditSettings()
        {
            PropertyGrid pg = new()
            {
                Dock = DockStyle.Fill,
                SelectedObject = AudioSettings.LibSettings
            };

            using Form f = new()
            {
                ClientSize = new(450, 450),
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

        void TimeBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
            //txtInfo.AppendText($"Current time:{timeBar.Current}");
            waveViewer1.Marker = 999;//TODO
            waveViewer2.Marker = 999;
        }
        void Settings_Click(object sender, EventArgs e)
        {
            EditSettings();

            txtInfo.AppendText(AudioSettings.LibSettings.ToString());
        }
    }
}
