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
        ISampleProvider? _prov;
        readonly ISampleProvider _provSwap;
        readonly SwappableSampleProvider _waveOutSwapper;
        readonly AudioPlayer _player;

        public TestHost()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            AudioSettings.LibSettings = new();

            Location = new(20, 20);

            ///// Time bar.
            timeBar.SnapMsec = 10;
            timeBar.CurrentTimeChanged += TimeBar_CurrentTimeChanged;
            timeBar.ProgressColor = Color.CornflowerBlue;
            timeBar.BackColor = Color.Salmon;

            _waveOutSwapper = new();
            _player = new("Microsoft Sound Mapper", 200, _waveOutSwapper) { Volume = 0.5 };
            _player.PlaybackStopped += (_, __) =>
            {
                LogLine("player finished");
                this.InvokeIfRequired(_ => { btnPlayer.Checked = false; });
            };

            _provSwap = new ClipSampleProvider(_filesDir + "test.wav", StereoCoercion.Mono);

            // Go-go-go.
            timer1.Enabled = true;
        }

        void TimeBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
            //LogLine($"Current time:{timeBar.Current}");
           // waveViewer1.Marker = (int)timeBar.Current.TotalMilliseconds;
        }

        private void Load_Click(object? sender, EventArgs args)
        {
            _player.Run(false);
            btnPlayer.Checked = false;
            btnSwap.Checked = false;

            switch(sender!.ToString())
            {
                case "wav":
                    {
                        try
                        {
                            // Cave Ceremony 01.wav   Fat Box 01.wav  Horns 01.wav  one-sec.wav
                            // Orchestra 03.wav  ref-stereo.wav  sin-stereo-audible.wav  sin.wav  test.wav
                            string fn = _filesDir + "ref-stereo.wav";
                            var prov = new AudioFileReader(fn);
                            _prov = prov;
                            //var prov = new ClipSampleProvider(fn, StereoCoercion.Mono);
                            ShowWave(prov, prov.Length);
                        }
                        catch (Exception e)
                        {
                            LogLine("!!! " + e.Message);
                        }
                    }
                    break;

                case "mp3":
                    {
                        try
                        {
                            // kidch.mp3   one-sec.mp3
                            string fn = _filesDir + "one-sec.mp3";
                            var prov = new ClipSampleProvider(fn, StereoCoercion.Mono);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        catch (Exception e)
                        {
                            LogLine("!!! " + e.Message);
                        }
                    }
                    break;

                case "flac":
                    {
                        try
                        {
                            // ambi_swoosh.flac  bass_woodsy_c.flac
                            string fn = _filesDir + "ambi_swoosh.flac";
                            var prov = new AudioFileReader(fn);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        catch (Exception e)
                        {
                            LogLine("!!! " + e.Message);
                        }
                    }
                    break;

                case "m4a":
                    {
                        try
                        {
                            // avTouch_sample.m4a
                            string fn = _filesDir + "avTouch_sample.m4a";
                            var prov = new ClipSampleProvider(fn, StereoCoercion.Mono);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        catch (Exception e)
                        {
                            LogLine("!!! " + e.Message);
                        }
                    }
                    break;

                case "sin":
                    {
                        try
                        {
                            // Draw a sin wave. Store in ClipSampleProvider.
                            var data = new float[10000];
                            for (int i = 0; i < data.Length; i++)
                            {
                                data[i] = (float)Math.Sin(Math.PI * i / 30.0);
                            }
                            var prov = new ClipSampleProvider(data);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        catch (Exception e)
                        {
                            LogLine("!!! " + e.Message);
                        }
                    }
                    break;

                case "txt":
                    {
                        try
                        {
                            // Wave from csv file. Store in ClipSampleProvider.
                            var sdata = File.ReadAllLines(_filesDir + "one-sec.txt");
                            var data = new float[sdata.Length];
                            for (int i = 0; i < sdata.Length; i++)
                            {
                                data[i] = float.Parse(sdata[i]);
                            }
                            var prov = new ClipSampleProvider(data);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        catch (Exception e)
                        {
                            LogLine("!!! " + e.Message);
                        }
                    }
                    break;
            }
        }

        void ShowWave(ISampleProvider prov, long length = 0)
        {
            _waveOutSwapper.SetInput(prov);

            int bytesPerSample = prov.WaveFormat.BitsPerSample / 8;
            int sclen = (int)(length / bytesPerSample);

            int ht = waveViewer2.Bottom - waveViewer1.Top;
            int wd = waveViewer1.Width;

            // If it's stereo split into two monos, one viewer per.
            if (prov.WaveFormat.Channels == 2) // stereo
            {
                prov.SetPosition(0);
                waveViewer1.Size = new(wd, ht / 2);
                waveViewer1.DrawColor = Color.Red;
                waveViewer1.Marker = sclen / 3;
                waveViewer1.SampleProvider = new StereoToMonoSampleProvider(prov) { LeftVolume = 1.0f, RightVolume = 0.0f };

                prov.SetPosition(0);
                waveViewer2.Visible = true;
                waveViewer2.Size = new(wd, ht / 2);
                waveViewer2.DrawColor = Color.Blue;
                waveViewer2.Marker = sclen / 4;
                waveViewer2.SampleProvider = new StereoToMonoSampleProvider(prov) { LeftVolume = 0.0f, RightVolume = 1.0f };
            }
            else // mono
            {
                waveViewer2.Visible = false;
                waveViewer1.Size = new(wd, ht);
                waveViewer1.DrawColor = Color.Red;
                waveViewer1.Marker = sclen / 2;
                waveViewer1.SampleProvider = prov;
            }

            prov.SetPosition(0);
            Text = NAudioEx.GetInfo(prov);

            timeBar.Start = new TimeSpan();
            timeBar.End = new TimeSpan();
            //int days, int hours, int minutes, int seconds, int milliseconds
            timeBar.Length = new(0, 0, 0, 0, 1000 * sclen / prov.WaveFormat.SampleRate); // msec;
        }

        void Player_Click(object? sender, EventArgs args)
        {
            if (_prov is null)
            {
                LogLine("open a file first please");
            }
            else
            {
                _player.Run(btnPlayer.Checked);
            }
        }

        void Swap_Click(object? sender, EventArgs args)
        {
            if (_prov is null)
            {
                LogLine("open a file first please");
            }
            else
            {
                var newProv = btnSwap.Checked ? _provSwap : _prov;
                ShowWave(newProv);
                Text = NAudioEx.GetInfo(newProv);
            }
        }

        void FileInfo_Click(object? sender, EventArgs args)
        {
            // Cave Ceremony 01.wav   Fat Box 01.wav  Horns 01.wav  one-sec.wav
            // Orchestra 03.wav  ref-stereo.wav  sin-stereo-audible.wav  sin.wav  test.wav
            // kidch.mp3   one-sec.mp3
            // ambi_swoosh.flac  bass_woodsy_c.flac
            // avTouch_sample.m4a

            bool verbose = false;

            string s = AudioFileInfo.GetFileInfo(_filesDir + "one-sec.mp3", verbose);
            txtInfo.AppendText(s + Environment.NewLine);
            s = AudioFileInfo.GetFileInfo(_filesDir + "Cave Ceremony 01.wav", verbose);
            txtInfo.AppendText(s + Environment.NewLine);
            s = AudioFileInfo.GetFileInfo(_filesDir + "ambi_swoosh.flac", verbose);
            txtInfo.AppendText(s + Environment.NewLine);
            s = AudioFileInfo.GetFileInfo(_filesDir + "avTouch_sample.m4a", verbose);
            txtInfo.AppendText(s + Environment.NewLine);
            s = AudioFileInfo.GetFileInfo(@"C:\Users\cepth\OneDrive\Audio\SoundFonts\FluidR3 GM.sf2", verbose);
            txtInfo.AppendText(s + Environment.NewLine);
        }

        void Timer1_Tick(object? sender, EventArgs args)
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

        void Settings_Click(object? sender, EventArgs args)
        {
            using Form f = new() { ClientSize = new(450, 450) };
            f.Controls.Add(new PropertyGrid() { Dock = DockStyle.Fill, SelectedObject = AudioSettings.LibSettings });
            f.ShowDialog();
        }

        void LogLine(string s)
        {
            this.InvokeIfRequired(_ => { txtInfo.AppendText(s + Environment.NewLine); });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            _player?.Dispose();

            base.Dispose(disposing);
        }
    }
}
