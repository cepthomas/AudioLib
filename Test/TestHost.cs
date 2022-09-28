using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.Json.Serialization;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;
using NBagOfTricks.PNUT;
using NBagOfUis;
using AudioLib;
using static AudioLib.Globals;


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

            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.Manual;
            Location = new(200, 10);
            Size = new(1000, 700);

            // The rest of the controls.
            txtInfo.WordWrap = true;
            txtInfo.BackColor = _settings.BackColor;
            txtInfo.MatchColors.Add("! ", Color.LightPink);
            txtInfo.MatchColors.Add("ERR", Color.LightPink);
            txtInfo.MatchColors.Add("WRN", Color.Plum);
            txtInfo.Prompt = "> ";

            cmbSelMode.Items.Add(WaveSelectionMode.Time);
            cmbSelMode.Items.Add(WaveSelectionMode.Bar);
            cmbSelMode.Items.Add(WaveSelectionMode.Sample);
            cmbSelMode.SelectedIndexChanged += (_, __) =>
            {
                switch (cmbSelMode.SelectedItem)
                {
                    case WaveSelectionMode.Time: ConverterOps = new TimeOps(); break;
                    case WaveSelectionMode.Bar: ConverterOps = new BarOps(); break;
                    case WaveSelectionMode.Sample: ConverterOps = new SampleOps(); break;
                }
                wv1.Invalidate();
                progBar.Invalidate();
            };
            cmbSelMode.SelectedItem = _settings.DefaultSelectionMode;

            sldGain.ValueChanged += (_, __) => wv1.Gain = (float)sldGain.Value;

            // Progress bar.
            progBar.CurrentChanged += (_, __) => LogLine($"Current timebar:{ConverterOps.Format(progBar.Current)}");
            progBar.ProgressColor = Color.Green;
            progBar.TextColor = Color.OrangeRed;
            progBar.BackColor = Color.Cyan;

            // Wave viewers.
            wv1.WaveColor = Color.Red;
            wv1.BackColor = Color.Cyan;
            wv1.ViewerChangeEvent += ProcessViewerChangeEvent;
            // Add to the menu.
            wv1.ContextMenuStrip.Items.Add("Just a test", null, (_, __) => LogLine("That worked"));

            wv2.WaveColor = Color.Blue;
            wv2.BackColor = Color.LightYellow;
            wv2.ViewerChangeEvent += ProcessViewerChangeEvent;

            // Static swap provider.
            _provSwap = new ClipSampleProvider(Path.Join(_testFilesDir, "test.wav"), StereoCoercion.Mono);

            // Create player.
            _waveOutSwapper = new();
            _player = new(AudioSettings.LibSettings.WavOutDevice, int.Parse(AudioSettings.LibSettings.Latency), _waveOutSwapper) { Volume = 0.5 };
            _player.PlaybackStopped += (_, __) =>
            {
                LogLine("Player finished");
                this.InvokeIfRequired(_ => chkPlay.Checked = false);
                _prov?.Rewind();
            };

            // File openers.
            foreach (var fn in new[] { "ref-stereo.wav", "one-sec.mp3", "ambi_swoosh.flac", "Tracy.m4a",
                "avTouch_sample_22050.m4a", "tri-ref.txt", "short_samples.txt", "generate.sin" })
            {
                LoadButton.DropDownItems.Add(fn, null, LoadFile_Click);
            }

            chkPlay.Click += (_, __) => Play_Click();

            btnTest.Click += (_, __) => UnitTests();

            // Go-go-go.
            timer1.Interval = 100;
            timer1.Enabled = true;
        }

        void LoadFile_Click(object? sender, EventArgs args)
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

                    default: // Create reader type.
                        if (btnAfReader.Checked)
                        {
                            prov = new AudioFileReader(fn);
                        }
                        else
                        {
                            var csp = new ClipSampleProvider(fn, StereoCoercion.Mono);
                            csp.ClipProgress += Csp_ClipProgress;
                            prov = csp;
                        }
                        break;
                }
                
                SetProvider(prov);
            }
            catch (Exception e)
            {
                LogLine("ERR " + e.Message);
            }
        }

        void ProcessViewerChangeEvent(object? sender, WaveViewer.ViewerChangeEventArgs e)
        {
            LogLine($"{(sender as WaveViewer)!.Name} change: {e.Change}");

            switch (e.Change)
            {
                case PropertyChange.Gain when sender == wv1:
                    sldGain.Value = wv1.Gain;
                    break;

                case PropertyChange.SelStart when sender == wv1:
                    progBar.SelStart = wv1.SelStart;
                    break;

                case PropertyChange.SelLength when sender == wv1:
                    progBar.SelLength = wv1.SelLength;
                    break;

                case PropertyChange.Marker when sender == wv2:
                    wv1.Recenter(wv2.Marker);
                    break;

                default:
                    break;
            };
        }

        // Helper to manage resources.
        void SetProvider(ISampleProvider prov)
        {
            // Clean up old?
            switch (_prov)
            {
                case ClipSampleProvider csp:
                    csp.ClipProgress -= Csp_ClipProgress;
                    break;

                case AudioFileReader afr:
                    afr!.Dispose();
                    break;
            }

            // New swap.
            switch (prov)
            {
                case ClipSampleProvider csp:
                    csp.ClipProgress += Csp_ClipProgress;
                    //progBar.Length = csp.SamplesPerChannel;
                    break;

                case AudioFileReader afr:
                    //progBar.Length = (int)(afr.Length / (prov.WaveFormat.BitsPerSample / 8) / prov.WaveFormat.Channels);
                    break;
            }

            _prov = prov;
            ShowWave(_prov);
            _waveOutSwapper.SetInput(_prov);
        }

        void Csp_ClipProgress(object? sender, ClipSampleProvider.ClipProgressEventArgs e)
        {
            progBar.Current = (int)e.Position;
        }

        // Boilerplate helper.
        void ShowWave(ISampleProvider? prov)
        {
            switch (prov)
            {
                case ClipSampleProvider csp:
                    {
                        csp.Rewind();
                        wv1.Init(csp);
                        csp.Rewind();

                        wv2.Init(new NullSampleProvider());

                        LogLine($"Show {csp.GetInfoString()}");
                        progBar.Length = csp.SamplesPerChannel;
                    }
                    break;
                case AudioFileReader afr:
                    {
                        // If it's stereo split into two monos. This is not really how to do things.
                        if (afr.WaveFormat.Channels == 2) // stereo
                        {
                            afr.Rewind();
                            wv1.Init(new ClipSampleProvider(afr, StereoCoercion.Left));
                            afr.Rewind();
                            wv2.Init(new ClipSampleProvider(afr, StereoCoercion.Right));
                        }
                        else // mono
                        {
                            afr.Rewind();
                            wv1.Init(new ClipSampleProvider(afr, StereoCoercion.None));
                            afr.Rewind();
                            wv2.Init(new ClipSampleProvider(afr, StereoCoercion.None));
                        }

                        afr.Rewind();
                        LogLine($"Show {afr.GetInfoString()}");
                        progBar.Length = (int)(afr.Length / (prov.WaveFormat.BitsPerSample / 8) / prov.WaveFormat.Channels);
                    }
                    break;
            }

            progBar.Current = 0;
            var thumb = wv1.RenderThumbnail(progBar.Width, progBar.Height, Color.Blue, Color.Pink, true);
            progBar.Thumbnail = thumb;
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
                LogLine($"Swapped");
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
                LogLine(AudioFileInfo.GetFileInfo(Path.Join(_testFilesDir, f), verbose));
            });

            LogLine(AudioFileInfo.GetFileInfo(@"C:\Users\cepth\OneDrive\Audio\SoundFonts\FluidR3 GM.sf2", verbose));
        }

        void Timer1_Tick(object? sender, EventArgs args)
        {
            if (btnRunBars.Checked)
            {
                // Update progress bar.
                progBar.Current += 1000; // not-realtime for testing
            }
        }

        void Settings_Click(object sender, EventArgs e)
        {
            _settings.Edit("howdy!", 400);
            _settings.Save();
            LogLine("You better restart!");
        }

        void LogLine(string s)
        {
            this.InvokeIfRequired(_ => { txtInfo.AppendLine(s); });
        }

        void UnitTests()
        {
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

        [DisplayName("Default Selection Mode")]
        [Description("Edit midi settings.")]
        [Browsable(true)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WaveSelectionMode DefaultSelectionMode { get; set; } = WaveSelectionMode.Time;

        [DisplayName("Default BPM")]
        [Browsable(true)]
        public double DefaultBPM { get; set; } = 100.0;

        [DisplayName("Midi Settings")]
        [Description("Edit midi settings.")]
        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public AudioSettings AudioSettings { get; set; } = new();
    }
}
