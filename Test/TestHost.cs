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
        readonly string _testFilesDir = @"C:\Dev\repos\TestAudioFiles\";
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

            _provSwap = new ClipSampleProvider(_testFilesDir + "test.wav", StereoCoercion.Mono);

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
            // ambi_swoosh.flac SampleRate:44100 Channels:2 BitsPerSample:16  Length:176400 TotalTime:00:00:01  
            // avTouch_sample.m4a SampleRate:22050 Channels:1 BitsPerSample:16 Length:450559 TotalTime:00:00:10.2167573  
            // bass_woodsy_c.flac SampleRate:44100 Channels:2 BitsPerSample:16  Length:529200 TotalTime:00:00:03  
            // Cave Ceremony 01.wav SampleRate:44100 Channels:2 BitsPerSample:16  Length:846720 TotalTime:00:00:04.8000000  
            // Fat Box 01.wav SampleRate:44100 Channels:1 BitsPerSample:16 Length:211832 TotalTime:00:00:02.4017233  
            // Horns 01.wav SampleRate:44100 Channels:2 BitsPerSample:24  Length:1306722 TotalTime:00:00:04.9384807
            // _kidch.mp3 SampleRate:44100 Channels:1 BitsPerSample:0 Length:??? TotalTime:???   
            // one-sec.mp3 SampleRate:44100 Channels:1 BitsPerSample:0 Length:92160 TotalTime:00:00:01.0448979  
            // one-sec.wav SampleRate:44100 Channels:1 BitsPerSample:16 Length:88384 TotalTime:00:00:01.0020861  
            // Orchestra 03.wav SampleRate:44100 Channels:1 BitsPerSample:24  Length:605331 TotalTime:00:00:04.5754421  
            // ref-stereo.wav SampleRate:44100 Channels:2 BitsPerSample:16  Length:176400 TotalTime:00:00:01  
            // sin-stereo-audible.wav SampleRate:44100 Channels:2 BitsPerSample:16  Length:176400 TotalTime:00:00:01  
            // sin.wav SampleRate:44100 Channels:1 BitsPerSample:16 Length:88200 TotalTime:00:00:01  
            // test.wav SampleRate:44100 Channels:1 BitsPerSample:16 Length:447488 TotalTime:00:00:05.0735600

            _player.Run(false);
            btnPlayer.Checked = false;
            btnSwap.Checked = false;

            try
            {
                // ClipSampleProvider is mono only. Use AudioFileReader for stereo.

                switch (sender!.ToString())
                {
                    case "wav":
                        {
                            string fn = _testFilesDir + "ref-stereo.wav";
                            //var prov = new ClipSampleProvider(fn, StereoCoercion.Mono);
                            var prov = new AudioFileReader(fn);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        break;

                    case "mp3":
                        {
                            string fn = _testFilesDir + "one-sec.mp3";
                            var prov = new ClipSampleProvider(fn, StereoCoercion.Mono);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        break;

                    case "flac":
                        {
                            string fn = _testFilesDir + "ambi_swoosh.flac";
                            var prov = new AudioFileReader(fn);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        break;

                    case "m4a":
                        {
                            string fn = _testFilesDir + "avTouch_sample.m4a"; // other sample rate - breaks
                            var prov = new AudioFileReader(fn);
                            if (prov.WaveFormat.SampleRate != AudioLibDefs.SAMPLE_RATE)
                            {
                                prov = prov.Resample();
                            }
                            //var prov = new ClipSampleProvider(fn, StereoCoercion.Mono); // breaks
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        break;

                    case "sin":
                        {
                            // Draw a sin wave.
                            var data = new float[10000];
                            for (int i = 0; i < data.Length; i++) { data[i] = (float)Math.Sin(Math.PI * i / 30.0); }
                            var prov = new ClipSampleProvider(data);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        break;

                    case "txt":
                        {
                            // Wave from csv file.
                            var sdata = File.ReadAllLines(_testFilesDir + "one-sec.txt");
                            var data = new float[sdata.Length];
                            for (int i = 0; i < sdata.Length; i++) { data[i] = float.Parse(sdata[i]); }
                            var prov = new ClipSampleProvider(data);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                LogLine("!!! " + e.Message);
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
            // Dump all test files.
            bool verbose = false;
            string[] files = new[] {
                "ambi_swoosh.flac", "avTouch_sample.m4a", "bass_woodsy_c.flac", "Cave Ceremony 01.wav", "Fat Box 01.wav",
                "Horns 01.wav", "one-sec.mp3", "_kidch.mp3", "one-sec.wav", "Orchestra 03.wav", "ref-stereo.wav",
                "sin-stereo-audible.wav", "sin.wav", "test.wav" };

            files.ForEach(f =>
            {
                string s = AudioFileInfo.GetFileInfo(_testFilesDir + f, verbose);
                txtInfo.AppendText(s + Environment.NewLine);
            });

            string s = AudioFileInfo.GetFileInfo(@"C:\Users\cepth\OneDrive\Audio\SoundFonts\FluidR3 GM.sf2", verbose);
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
