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

        ISampleProvider? _sprov1;

        ISampleProvider _sprov2;

        readonly SwappableSampleProvider _swapper = new();

        readonly AudioPlayer _player = new("Microsoft Sound Mapper", 200);


        public TestHost()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            AudioSettings.LibSettings = new();

            Location = new(20, 20);

            ///// Time bar. TODO impl
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

            _sprov2 = new ClipSampleProvider(_filesDir + "one-sec.wav");
            _swapper.SetInput(_sprov2);
            waveViewer1.SampleProvider = _swapper;

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
                        _data = new float[10000];
                        for (int i = 0; i < _data.Length; i++)
                        {
                            _data[i] = (float)Math.Sin(Math.PI * i / 30.0);
                        }
                        _sprov1 = new ClipSampleProvider(_data);
                        waveViewer1.DrawColor = Color.Green;
                        waveViewer1.Marker = 333;
                        waveViewer1.SampleProvider = _sprov1;
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
                        _sprov1 = new ClipSampleProvider(_data);
                        waveViewer1.DrawColor = Color.Blue;
                        waveViewer1.Marker = 20000;
                        waveViewer1.SampleProvider = _sprov1;
                    }
                    break;

                case "wav":
                    {
                        // From file.
                        // Cave Ceremony 01.wav   Fat Box 01.wav  Horns 01.wav  one-sec.wav
                        // Orchestra 03.wav  ref-stereo.wav  sin-stereo-audible.wav  sin.wav  test.wav
                        var fn = "one-sec.wav";
                        _sprov1 = new ClipSampleProvider(_filesDir + fn);
                        _data = AudioUtils.ReadAll(_sprov1);
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Marker = 30000;
                        waveViewer1.SampleProvider = _sprov1;
                    }
                    break;

                case "mp3":
                    {
                        // Uses ClipSampleProvider.
                        // kidch.mp3   one-sec.mp3
                        _sprov1 = new ClipSampleProvider(_filesDir + "one-sec.mp3");
                        _data = AudioUtils.ReadAll(_sprov1);
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Marker = 30000;
                        waveViewer1.SampleProvider = _sprov1;
                    }
                    break;

                case "flac":
                    {
                        // Uses AudioFileReader.
                        // ambi_swoosh.flac  bass_woodsy_c.flac
                        var audioFileReader = new AudioFileReader(_filesDir + "ambi_swoosh.flac");
                        var length = audioFileReader.TotalTime;
                        _sprov1 = audioFileReader;
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Marker = 30000;
                        waveViewer1.SampleProvider = _sprov1;
                    }
                    break;

                case "m4a":
                    {
                        // Uses ClipSampleProvider.
                        // avTouch_sample.m4a
                        _sprov1 = new ClipSampleProvider(_filesDir + "avTouch_sample.m4a");
                        _data = AudioUtils.ReadAll(_sprov1);
                        waveViewer1.DrawColor = Color.Red;
                        waveViewer1.Marker = 30000;
                        waveViewer1.SampleProvider = _sprov1;
                    }
                    break;
            }
        }

        void Player_Click(object sender, EventArgs e)
        {
            if (_sprov1 is null)
            {
                LogLine("open a file first please");
            }
            else
            {
                AudioUtils.SetProviderPosition(_sprov1, 0);
                AudioUtils.SetProviderPosition(_sprov2, 0);
                _player.Run(btnPlayer.Checked);
            }
        }

        void Swap_Click(object sender, EventArgs e)
        {
            if (_sprov1 is null)
            {
                LogLine("open a file first please");
            }
            else
            {
                AudioUtils.SetProviderPosition(_sprov1, 0);
                AudioUtils.SetProviderPosition(_sprov2, 0);
                _swapper.SetInput(btnSwap.Checked ? _sprov2 : _sprov1);
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
