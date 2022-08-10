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
        readonly string _filesDir = @"C:\Dev\repos\TestAudioFiles\";

        float[] _data = Array.Empty<float>();

        ClipSampleProvider? _clip;

        ClipSampleProvider? _clip2;

        readonly SwappableSampleProvider _swapper = new();

        readonly AudioPlayer _player = new("Microsoft Sound Mapper", 200);


        public TestHost()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            AudioSettings.LibSettings = new();

            Location = new(20, 20);

            ///// Time bar.
            timeBar.SnapMsec = 10;
            //timeBar.Length = new TimeSpan(0, 0, 1, 23, 456);
            //timeBar.Start = new TimeSpan(0, 0, 0, 10, 333);
            //timeBar.End = new TimeSpan(0, 0, 0, 44, 777);
            timeBar.CurrentTimeChanged += TimeBar_CurrentTimeChanged;
            timeBar.ProgressColor = Color.CornflowerBlue;
            timeBar.BackColor = Color.Salmon;

            _player.Volume = 0.5;
            _player.PlaybackStopped += (_, __) =>
            {
                LogLine("player finished");
                this.InvokeIfRequired(_ => { btnPlayer.Checked = false; });
            };

            // Go-go-go.
            timer1.Enabled = true;
        }

        private void Load_Click(object sender, EventArgs e)
        {
            switch(sender.ToString())
            {
                case "sin":
                    {
                        // Draw a sin wave.
                        _data = new float[1000];
                        for (int i = 0; i < _data.Length; i++)
                        {
                            _data[i] = (float)Math.Sin(Math.PI * i / 30.0);
                        }
                        waveViewer1.DrawColor = Color.Green;
                        waveViewer1.Values = _data;
                        waveViewer1.Marker = 333;
                    }
                    break;

                case "txt":
                    {
                        // one-sec.txt
                        var sdata = File.ReadAllLines(_filesDir + "one-sec.txt");
                        _data = new float[sdata.Length];
                        for (int i = 0; i < sdata.Length; i++)
                        {
                            _data[i] = float.Parse(sdata[i]);
                        }
                        waveViewer1.DrawColor = Color.Blue;
                        waveViewer1.Values = _data;
                        waveViewer1.Marker = 20000;
                    }
                    break;

                case "wav":
                    {
                        // From file.
                        var fn =
                         "one-sec.wav";
                        // Cave Ceremony 01.wav
                        // Fat Box 01.wav
                        // Horns 01.wav
                        // Orchestra 03.wav
                        // ref-stereo.wav
                        // sin-stereo-audible.wav
                        // sin.wav
                        // test.wav

                        _clip = new ClipSampleProvider(_filesDir + fn);
                        _data = AudioUtils.ReadAll(_clip);
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Values = _data;
                        waveViewer1.Marker = 30000;
                    }
                    break;

                case "mp3":
                    {
                        // kidch.mp3
                        // one-sec.mp3
                        _clip = new ClipSampleProvider(_filesDir + "_kidch.mp3");
                        _data = AudioUtils.ReadAll(_clip);
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Values = _data;
                        waveViewer1.Marker = 30000;
                    }
                    break;

                case "flac":
                    {
                        // ambi_swoosh.flac
                        // bass_woodsy_c.flac
                        _clip = new ClipSampleProvider(_filesDir + "ambi_swoosh.flac");
                        _data = AudioUtils.ReadAll(_clip);
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Values = _data;
                        waveViewer1.Marker = 30000;
                    }
                    break;

                case "m4a":
                    {
                        // avTouch_sample.m4a
                        _clip = new ClipSampleProvider(_filesDir + "avTouch_sample.m4a");
                        _data = AudioUtils.ReadAll(_clip);
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Values = _data;
                        waveViewer1.Marker = 30000;
                    }
                    break;
            }
        }

        void Player_Click(object sender, EventArgs e)
        {
            if (_clip is not null)
            {
                _clip.Position = 0;
                _player.SetProvider(_clip);
                _player.Run(btnPlayer.Checked);
            }
            else
            {
                LogLine("open a file first please");
            }
        }

        void Swap_Click(object sender, EventArgs e)
        {
            if(_clip2 is null)
            {
                _clip2 = new ClipSampleProvider(_filesDir + "one-sec.wav");
            }

            if (_clip is not null)
            {
                if(btnSwap.Checked)
                {
                    _clip2.Position = 0;
                    _swapper.SetInput(_clip2);
                }
                else
                {
                    _clip.Position = 0;
                    _swapper.SetInput(_clip);
                }

                var data = AudioUtils.ReadAll(_swapper);
                waveViewer1.Values = data;
            }
            else
            {
                LogLine("open a file first please");
            }
        }

        void Timer1_Tick(object? sender, EventArgs e)
        {
            if (btnRunBars.Checked)
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
            //LogLine($"Current time:{timeBar.Current}");
            waveViewer1.Marker = 999;//TODO
        }
        void Settings_Click(object sender, EventArgs e)
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
            _player?.Dispose();

            base.Dispose(disposing);
        }

        void LogLine(string s)
        {
            this.InvokeIfRequired(_ => { txtInfo.AppendText(s + Environment.NewLine); });
        }
    }
}
