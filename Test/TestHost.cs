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

            // Time bar.
            timeBar.SnapMsec = 10;
            timeBar.CurrentTimeChanged += TimeBar_CurrentTimeChanged;
            timeBar.ProgressColor = Color.CornflowerBlue;
            timeBar.BackColor = Color.Salmon;

            // Controls.
            waveViewer1.StatusEvent += (_, __) => { };
            sldGain.ValueChanged += (_, __) => { waveViewer1.Gain = (float)sldGain.Value; waveViewer1.Invalidate(); };

            // Player.
            _waveOutSwapper = new();
            _player = new("Microsoft Sound Mapper", 200, _waveOutSwapper) { Volume = 0.5 };
            _player.PlaybackStopped += (_, __) =>
            {
                LogLine("player finished");
                this.InvokeIfRequired(_ => { btnPlayer.Checked = false; });
            };
            _provSwap = new ClipSampleProvider(_testFilesDir + "test.wav", StereoCoercion.Mono);

            // Go-go-go.
            timer1.Interval = 100;
            timer1.Enabled = true;
        }

        void Load_Click(object? sender, EventArgs args)
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
                            var prov = new AudioFileReader(fn); // TODO these need to be disposed.
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
                            var data = new float[500];
                            for (int i = 0; i < data.Length; i++) { data[i] = (float)Math.Sin(i * 0.1); }
                            var prov = new ClipSampleProvider(data);
                            _prov = prov;
                            ShowWave(prov, prov.Length);
                        }
                        break;

                    case "txt":
                        {
                            // Wave from csv file.
                            var sdata = File.ReadAllLines(_testFilesDir + "tri-ref.txt");
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
            _waveOutSwapper.SetInput(prov); // TODO this is hear not show

            int bytesPerSample = prov.WaveFormat.BitsPerSample / 8;
            int sclen = (int)(length / bytesPerSample);

            int ht = waveViewer2.Bottom - waveViewer1.Top;
            int wd = waveViewer1.Width;

            // If it's stereo split into two monos, one viewer per.
            if (prov.WaveFormat.Channels == 2) // stereo
            {
                prov.SetPosition(0);
                waveViewer1.Init(new StereoToMonoSampleProvider(prov) { LeftVolume = 1.0f, RightVolume = 0.0f });
                waveViewer1.Size = new(wd, ht / 2);
                waveViewer1.DrawColor = Color.Red;
                waveViewer1.BackColor = Color.Cyan;
                waveViewer1.SelStart = sclen / 3;
                waveViewer1.SelLength = sclen / 4;
                waveViewer1.ViewCursor = 2 * sclen / 3;

                prov.SetPosition(0);
                waveViewer2.Init(new StereoToMonoSampleProvider(prov) { LeftVolume = 0.0f, RightVolume = 1.0f });
                waveViewer2.Visible = true;
                waveViewer2.Size = new(wd, ht / 2);
                waveViewer2.DrawColor = Color.Blue;
                waveViewer2.BackColor = Color.LightYellow;
                waveViewer2.SelStart = sclen / 4;
                waveViewer1.SelLength = sclen / 4;
                waveViewer2.ViewCursor = 3 * sclen / 4;
            }
            else // mono
            {
                waveViewer1.Init(prov);
                waveViewer2.Visible = false;
                waveViewer1.Size = new(wd, ht);
                waveViewer1.DrawColor = Color.Green;
                waveViewer1.SelStart = sclen / 10;
                waveViewer1.SelLength = sclen / 4;
                waveViewer1.ViewCursor = 9 * sclen / 10;
            }

            prov.SetPosition(0);
            Text = prov.GetInfo();

            int msec = 1000 * sclen / prov.WaveFormat.SampleRate;
            timeBar.Marker1 = new TimeSpan(0, 0, 0, 0, msec / 3);
            timeBar.Marker2 = new TimeSpan(0, 0, 0, 0, msec / 2);
            timeBar.Length = new(0, 0, 0, 0, msec); // msec;
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
                Text = newProv.GetInfo();
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

        void TimeBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
            LogLine($"Current time:{timeBar.Current}");
            waveViewer1.ViewCursor = (int)timeBar.Current.TotalMilliseconds;
        }

        void Timer1_Tick(object? sender, EventArgs args)
        {
            if (btnRunBars.Checked)
            {
                // Update time bar. Ticks are 100 msec.
                timeBar.IncrementCurrent(10); // not-real time for testing
                if (timeBar.Current >= timeBar.Marker2) // done/reset
                {
                    timeBar.Current = timeBar.Marker1;
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

        void OtherStuff()
        {
            // Conversion tests.
            int sample = 123456;
            float msec = AudioLibUtils.SampleToMsec(sample);
            int sampout1 = AudioLibUtils.MsecToSample(msec);
            int diff1 = Math.Abs(sampout1 - sample);
            float msec1 = AudioLibUtils.SampleToMsec(diff1);

            TimeSpan ts = AudioLibUtils.SampleToTime(sample);
            int sampout2 = AudioLibUtils.TimeToSample(ts);
            int diff2 = Math.Abs(sampout2 - sample);
            float msec2 = AudioLibUtils.SampleToMsec(diff2);
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
