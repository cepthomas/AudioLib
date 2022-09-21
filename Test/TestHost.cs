using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.Json.Serialization;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;
using NBagOfTricks.PNUT;
using NBagOfUis;
using AudioLib;


namespace AudioLib.Test
{
    public partial class TestHost : Form
    {
        /// <summary>Where the files are.</summary>
        readonly string _testFilesDir = @"C:\Dev\repos\TestAudioFiles";

        /// <summary>The current audio provider.</summary>
        ISampleProvider _prov = new NullSampleProvider();

        /// <summary>For testing swapping providers.</summary>
        readonly ClipSampleProvider _provSwap;

        /// <summary>Input to the player.</summary>
        readonly SwappableSampleProvider _waveOutSwapper;

        /// <summary>Renders to audio.</summary>
        readonly AudioPlayer _player;

        /// <summary>Test stuff.</summary>
        readonly TestSettings _settings = new();

        /// <summary>
        /// Constructor.
        /// </summary>
        public TestHost()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            AudioSettings.LibSettings = new();
            // Must do this first before initializing.
            _settings = (TestSettings)Settings.Load(".", typeof(TestSettings));
            AudioSettings.LibSettings = _settings.AudioSettings;

            Location = new(20, 20);

            Globals.SelectionMode = WaveSelectionMode.Sample;
            Globals.BPM = 100;

            // Time bar.
            timeBar.SnapMsec = 10;
            timeBar.CurrentTimeChanged += TimeBar_CurrentTimeChanged;
            timeBar.ProgressColor = Color.CornflowerBlue;
            timeBar.BackColor = Color.Salmon;

            // Wave viewers.
            wv1.DrawColor = Color.Red;
            wv1.BackColor = Color.Cyan;
            wv1.ViewerChangeEvent += ProcessViewerChangeEvent;
            sldGain.ValueChanged += (_, __) => wv1.Gain = (float)sldGain.Value;

            wv2.DrawColor = Color.Blue;
            wv2.BackColor = Color.LightYellow;
            wv2.ViewerChangeEvent += ProcessViewerChangeEvent;

            // Create reader.
            // var sampleChannel = new SampleChannel(_audioFileReader, false);
            // sampleChannel.PreVolumeMeter += SampleChannel_PreVolumeMeter;
            // var postVolumeMeter = new MeteringSampleProvider(sampleChannel);
            // //postVolumeMeter.StreamVolume += PostVolumeMeter_StreamVolume;
            // _waveOutSwapper.SetInput(postVolumeMeter);
            // _audioFileReader.Position = 0; // rewind
            _waveOutSwapper = new();
            _provSwap = new ClipSampleProvider(Path.Join(_testFilesDir, "test.wav"), StereoCoercion.Mono);

            var postVolumeMeter = new MeteringSampleProvider(_waveOutSwapper, _waveOutSwapper.WaveFormat.SampleRate / 10);
            postVolumeMeter.StreamVolume += (object? sender, StreamVolumeEventArgs e) =>
            {
                // Get the position of the source provider.
                long pos = _prov.GetPosition();
                if(pos >= 0)
                {
                    timeBar.Current = new(0, 0, 0, 0, (int)(1000 * pos / _waveOutSwapper.WaveFormat.SampleRate));
                }
            };

            _player = new("Microsoft Sound Mapper", 200, postVolumeMeter) { Volume = 0.5 };
            _player.PlaybackStopped += (_, __) =>
            {
                LogLine("player finished");
                this.InvokeIfRequired(_ => chkPlay.Checked = false);
                _prov?.SetPosition(0);
            };

            // File openers.
            foreach (var fn in new[] { "ref-stereo.wav", "one-sec.mp3", "ambi_swoosh.flac", "Tracy.m4a",
                "avTouch_sample_22050.m4a", "tri-ref.txt", "short_samples.txt", "generate.sin" })
            {
                LoadButton.DropDownItems.Add(fn, null, LoadViewer_Click);
            }

            chkPlay.Click += (_, __) => Play_Click();

            btnTest.Click += (_, __) => UnitTests();

            // Go-go-go.
            timer1.Interval = 100;
            timer1.Enabled = true;
        }

        void LoadViewer_Click(object? sender, EventArgs args)
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

        void ProcessViewerChangeEvent(object? sender, WaveViewer.ViewerChangeEventArgs e)
        {
            switch (e.Change)
            {
                case Property.Gain when sender == wv1:
                    sldGain.Value = wv1.Gain;
                    break;

                case Property.Marker when sender == wv2:
                    wv1.Recenter(wv2.Marker);
                    break;

                default:
                    break;
            };
        }

        // Helper to manage resources.
        void SetProvider(ISampleProvider prov)
        {
            // Clean up?
            if (_prov is AudioFileReader)
            {
                (_prov as AudioFileReader)!.Dispose();
            }

            _prov = prov;
            ShowWave(_prov);
            _waveOutSwapper.SetInput(_prov);
        }

        // Boilerplate helper.
        void ShowWave(ISampleProvider? prov)
        {
            if (prov is null)
            {
                return;
            }

            int tm = prov.GetTotalTime();

            // If it's stereo split into two monos, one viewer per. This is not really how to do things.
            if (prov.WaveFormat.Channels == 2) // stereo
            {
                // Data.
                prov.SetPosition(0);
                wv1.Init(new ClipSampleProvider(prov, StereoCoercion.Left));
                //wv1.SelStart = sclen / 3;
                //wv1.SelLength = sclen / 4;
                //wv1.Marker = 2 * sclen / 3;

                // Thumbnail.
                prov.SetPosition(0);
                wv2.Init(new ClipSampleProvider(prov, StereoCoercion.Right), true); // simple
                //wv2.SelStart = sclen / 4;
                //wv1.SelLength = sclen / 4;
                //wv2.Marker = 3 * sclen / 4;
            }
            else // mono
            {
                // Data.
                wv1.Init(new ClipSampleProvider(prov, StereoCoercion.None));
                //wv1.SelStart = sclen / 10;
                //wv1.SelLength = 9 * sclen / 10;
                //wv1.Marker = sclen / 4;

                // Thumbnail.
                wv2.Init(new ClipSampleProvider(Array.Empty<float>()), true); // simple);
            }

            prov.SetPosition(0);
            lblInfo.Text = prov.GetInfoString();

            timeBar.Length = new(0, 0, 0, 0, tm);
            timeBar.Marker1 = new(0, 0, 0, 0, tm / 3);
            timeBar.Marker2 = new(0, 0, 0, 0, tm / 2);
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
            wv1.Marker = (int)timeBar.Current.TotalMilliseconds;
        }

        void Timer1_Tick(object? sender, EventArgs args)
        {
            if (btnRunBars.Checked)
            {
                // Update time bar. Ticks are 100 msec.
                timeBar.IncrementCurrent(10); // not-realtime for testing
                if (timeBar.Current >= timeBar.Marker2) // done/reset
                {
                    timeBar.Current = timeBar.Marker1;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Settings_Click(object sender, EventArgs e)
        {
            _settings.Edit("howdy!", 400);
            _settings.Save();
            LogLine("You better restart!");
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
            var cases = new[] { "CONVERT" };
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

            SetProvider(new NullSampleProvider());

            _player?.Dispose();

            base.Dispose(disposing);
        }
    }

    public class TestSettings : Settings
    {
        [DisplayName("Background Color")]
        [Description("The color used for overall background.")]
        [Browsable(true)]
        [JsonConverter(typeof(JsonColorConverter))]
        public Color BackColor { get; set; } = Color.AliceBlue;

        [DisplayName("Ignore Me")]
        [Description("I do nothing.")]
        [Browsable(true)]
        public bool IgnoreMe { get; set; } = true;

        [DisplayName("Midi Settings")]
        [Description("Edit midi settings.")]
        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AudioSettings AudioSettings { get; set; } = new();
    }

}
