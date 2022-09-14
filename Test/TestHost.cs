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
using AudioLib;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;
using NBagOfTricks.PNUT;



namespace AudioLib.Test
{
    public partial class TestHost : Form
    {
        readonly string _testFilesDir = @"C:\Dev\repos\TestAudioFiles";
        ISampleProvider? _prov;
        readonly ClipSampleProvider _provSwap;
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
            waveViewer1.DrawColor = Color.Red;
            waveViewer1.BackColor = Color.Cyan;
            waveViewer1.GainChangedEvent += (_, __) => sldGain.Value = waveViewer1.Gain;
            sldGain.ValueChanged += (_, __) =>
            {
                waveViewer1.Gain = (float)sldGain.Value;
                waveViewer1.Invalidate();
            };

            waveViewer2.DrawColor = Color.Blue;
            waveViewer2.BackColor = Color.LightYellow;

            // Player.
            _waveOutSwapper = new();
            _player = new("Microsoft Sound Mapper", 200, _waveOutSwapper) { Volume = 0.5 };
            _player.PlaybackStopped += (_, __) =>
            {
                LogLine("player finished");
                this.InvokeIfRequired(_ => chkPlay.Checked = false);
                _prov?.Rewind();
            };
            _provSwap = new ClipSampleProvider(Path.Join(_testFilesDir, "test.wav"), StereoCoercion.Mono);
            chkPlay.Click += (_, __) => Play_Click();

            // File openers.
            foreach (var fn in new[] { "ref-stereo.wav", "one-sec.mp3", "ambi_swoosh.flac", "Tracy.m4a",
                "avTouch_sample_22050.m4a", "tri-ref.txt", "short_samples.txt", "generated.sin" })
            {
                LoadButton.DropDownItems.Add(fn, null, Load_Click);
            }

            btnTest.Click += (_, __) => UnitTests();

            // Go-go-go.
            timer1.Interval = 100;
            timer1.Enabled = true;
        }

        void Load_Click(object? sender, EventArgs args)
        {
            _player.Run(false);
            chkPlay.Checked = false;
            btnSwap.Checked = false;

            var ext = Path.GetExtension(sender!.ToString());

            try
            {
                ISampleProvider? prov = null;
                string fn = Path.Join(_testFilesDir, sender!.ToString());

                switch (ext)
                {
                    case ".sin": // Generate a sin wave.
                        var data = new float[2000];
                        for (int i = 0; i < data.Length; i++) { data[i] = (float)Math.Sin(i * 0.02); }
                        prov = new ClipSampleProvider(data);
                        break;

                    case ".txt": // Wave from csv file.
                        var sdata = File.ReadAllLines(fn);
                        var tdata = new float[sdata.Length];
                        for (int i = 0; i < sdata.Length; i++) { tdata[i] = float.Parse(sdata[i]); }
                        prov = new ClipSampleProvider(tdata);
                        break;


                    default: // Audio file.
                        prov = btnClipProvider.Checked ? new ClipSampleProvider(fn, StereoCoercion.Mono) : new AudioFileReader(fn);
                        break;
                }
                SetProvider(prov);
            }
            catch (Exception e)
            {
                LogLine("!!! " + e.Message);
            }
        }

        // Helper to manage resources.
        void SetProvider(ISampleProvider? prov)
        {
            if (_prov is AudioFileReader)
            {
                (_prov as AudioFileReader)!.Dispose();
            }
            _prov = prov;

            ShowWave(prov);
            _waveOutSwapper.SetInput(prov);
        }

        // Boilerplate helper.
        void ShowWave(ISampleProvider? prov)
        {
            if(prov is null)
            {
                return;
            }

            int sclen = prov.Length();

            // If it's stereo split into two monos, one viewer per.
            if (prov.WaveFormat.Channels == 2) // stereo
            {
                prov.Rewind();
                waveViewer1.Init(new ClipSampleProvider(prov, StereoCoercion.Left));
                //waveViewer1.SelStart = sclen / 3;
                //waveViewer1.SelLength = sclen / 4;
                waveViewer1.Marker = 2 * sclen / 3;

                prov.Rewind();
                waveViewer2.Init(new ClipSampleProvider(prov, StereoCoercion.Right));
                //waveViewer2.SelStart = sclen / 4;
                //waveViewer1.SelLength = sclen / 4;
                waveViewer2.Marker = 3 * sclen / 4;
            }
            else // mono
            {
                waveViewer1.Init(new ClipSampleProvider(prov, StereoCoercion.None));
                //waveViewer1.SelStart = sclen / 10;
                //waveViewer1.SelLength = 9 * sclen / 10;
                waveViewer1.Marker = sclen / 4;

                waveViewer2.Init(new ClipSampleProvider(Array.Empty<float>()));
            }

            prov.Rewind();
            lblInfo.Text = prov.GetInfoString();

            int msec = 1000 * sclen / prov.WaveFormat.SampleRate;
            timeBar.Marker1 = new TimeSpan(0, 0, 0, 0, msec / 3);
            timeBar.Marker2 = new TimeSpan(0, 0, 0, 0, msec / 2);
            timeBar.Length = new(0, 0, 0, 0, msec); // msec;
        }

        void Play_Click()
        {
            if (_prov is null)
            {
                LogLine("open a file first please");
            }
            else
            {
                _player.Run(chkPlay.Checked);
            }
        }

        void Resample_Click(object? sender, EventArgs e)
        {
            string fn = Path.Join(_testFilesDir, "Tracy.m4a");
            string newfn = Path.Join(_testFilesDir, "Tracy.wav");

            NAudioEx.Resample(fn, newfn);
        }

        // Swap test for SwappableSampleProvider.
        void Swap_Click(object? sender, EventArgs args)
        {
            if (_prov is null)
            {
                LogLine("open a file first please");
            }
            else
            {
                var newProv = btnSwap.Checked ? _provSwap : _prov;
                _waveOutSwapper.SetInput(newProv); // For listen.
                ShowWave(newProv);
                lblInfo.Text = newProv.GetInfoString();
            }
        }

        void FileInfo_Click(object? sender, EventArgs args)
        {
            // Dump all test files.
            bool verbose = false;
            string[] files = new[] {
                "ambi_swoosh.flac", "avTouch_sample_22050.m4a", "bass_woodsy_c.flac", "Cave Ceremony 01.wav", "Fat Box 01.wav",
                "Horns 01.wav", "one-sec.mp3", "_kidch.mp3", "one-sec.wav", "Orchestra 03.wav", "ref-stereo.wav",
                "sin-stereo-audible.wav", "sin.wav", "test.wav" };

            files.ForEach(f =>
            {
                string s = AudioFileInfo.GetFileInfo(Path.Join(_testFilesDir, f), verbose);
                txtInfo.AppendText(s + Environment.NewLine);
            });

            string s = AudioFileInfo.GetFileInfo(@"C:\Users\cepth\OneDrive\Audio\SoundFonts\FluidR3 GM.sf2", verbose);
            txtInfo.AppendText(s + Environment.NewLine);
        }

        void TimeBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
            LogLine($"Current time:{timeBar.Current}");
            waveViewer1.Marker = (int)timeBar.Current.TotalMilliseconds;
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

        void UnitTests()
        {
            // Run pnut tests from cmd line.
            Size stxt = txtInfo.Size;
            txtInfo.Size = new Size(stxt.Width, Height - 200);

            TestRunner runner = new(OutputFormat.Readable);
            var cases = new[] { "CONVERTERS" };
            runner.RunSuites(cases);
            runner.Context.OutputLines.ForEach(l => LogLine(l));
            //File.WriteAllLines(@"..\..\out\test_out.txt", runner.Context.OutputLines);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            SetProvider(null);

            _player?.Dispose();

            base.Dispose(disposing);
        }
    }
}
