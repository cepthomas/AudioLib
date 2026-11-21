using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Drawing.Design;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.NBagOfUis;
using Ephemera.AudioLib;

//TODO1 >>> - limits internal formats to IEEE 32bit fp 44100Hz. C:\Dev\Misc\TestAudioFiles. TODO1 expand caps.
// Type  ,File                            ,Params            
// Audio ,08 Bag Pipe Groove 01 48000.wav ,SampleRate:48000, Channels:2, BitsPerSample:16, Length:1644295
// Audio ,ambi_swoosh.flac                ,SampleRate:44100, Channels:2, BitsPerSample:16, Length:176400 
// Audio ,avTouch_sample_22050.m4a        ,SampleRate:22050, Channels:1, BitsPerSample:16, Length:450559 
// Audio ,Fat Box 01.wav                  ,SampleRate:44100, Channels:1, BitsPerSample:16, Length:211831 
// Audio ,Horns 01.wav                    ,SampleRate:44100, Channels:2, BitsPerSample:24, Length:1306721


namespace Ephemera.AudioLib.Test
{
    public partial class TestHost : Form
    {
        readonly Color _bgColor;

        /// <summary>Where the files are.</summary>
        readonly string _filesDir;

        /// <summary>The current audio provider.</summary>
        ISampleProvider _prov = new NullSampleProvider();

        /// <summary>For testing swapping providers.</summary>
        readonly ClipSampleProvider _provSwap;

        /// <summary>Input to the player.</summary>
        readonly SwappableSampleProvider _waveOutSwapper;

        /// <summary>Renders to audio.</summary>
        readonly AudioPlayer _player;

        /// <summary>Test stuff.</summary>
        readonly TestSettings _settings;

        /// <summary>Test stuff.</summary>
        IConverter _converter = Converter.Create(WaveSelectionMode.Sample);

        /// <summary>
        /// Constructor.
        /// </summary>
        public TestHost()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

            _bgColor = BackColor;

            // Get settings.
            _settings = (TestSettings)SettingsCore.Load(MiscUtils.GetSourcePath(), typeof(TestSettings));

            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.Manual;
            Location = new(200, 10);
            Size = new(1000, 700);
            Text = "AudioLib Test";

            _filesDir = Path.Join(Environment.GetEnvironmentVariable("DEV_PATH"), "Misc", "TestAudioFiles");
            if (!Path.Exists(_filesDir))
            {
                MessageBox.Show($"Test files path {_filesDir} is not valid.{Environment.NewLine}This needs DEV_PATH set, or hack source to taste.");
                Environment.Exit(1);
            }

            // The rest of the controls.
            tvInfo.WordWrap = true;
            //tvInfo.BackColor = _settings.BackColor;
            tvInfo.Prompt = "> ";
            tvInfo.Matchers =
            [
                new("! ", BgColor: Color.YellowGreen),
                new("ERR", BgColor: Color.LightPink),
                new("WRN", BgColor: Color.Plum),
            ];

            cmbSelMode.Items.Add(WaveSelectionMode.Time);
            cmbSelMode.Items.Add(WaveSelectionMode.Bar);
            cmbSelMode.Items.Add(WaveSelectionMode.Sample);
            cmbSelMode.SelectedIndexChanged += (_, __) =>
            {
                if (cmbSelMode.SelectedItem is not null)
                {
                    WaveSelectionMode mode = (WaveSelectionMode)cmbSelMode.SelectedItem;
                    wv1.SelectionMode = mode;
                    wv2.SelectionMode = mode;
                    timeBar.SelectionMode = mode;
                    _converter = Converter.Create(mode);
                }

                wv1.Invalidate();
                timeBar.Invalidate();
            };
            cmbSelMode.SelectedItem = _settings.DefaultSelectionMode;

            sldGain.ValueChanged += (_, __) => wv1.Gain = (float)sldGain.Value;

            // Progress bar.
            timeBar.CurrentChanged += (_, __) => Tell($"Current timebar:{_converter.Format(timeBar.Current)}");
            timeBar.ProgressColor = Color.Green;
            timeBar.TextColor = Color.White;
            timeBar.BackColor = Color.Cyan;

            // Wave viewers.
            wv1.WaveColor = Color.Red;
            wv1.BackColor = Color.Cyan;
            wv1.StateChange += ViewerStateChange;

            // Add stuff to the wave viewer menu.
            wv1.ContextMenuStrip!.Items.Add("Test item", null, (_, __) => Tell("Test item worked"));
            toolStripMenuItem1.Click += (_, __) => Tell($"Log it worked");

            wv2.WaveColor = Color.Blue;
            wv2.BackColor = Color.LightYellow;
            wv2.StateChange += ViewerStateChange;

            // Init controls. Typically voloume and state would be persisted per output.
            cc1.Volume = 0.8;
            cc1.State = ChannelState.Solo;
            cc1.DrawColor = _settings.DrawColor;
            cc1.SelectedColor = _settings.SelectedColor;
            cc1.InfoClick += Cc_InfoClick;

            cc2.Volume = 0.7;
            cc2.State = ChannelState.Normal;
            cc2.DrawColor = _settings.DrawColor;
            cc2.SelectedColor = _settings.SelectedColor;
            cc2.InfoClick += Cc_InfoClick;


            // Static swap provider.
            _provSwap = new ClipSampleProvider(Path.Join(_filesDir, "test.wav"), StereoCoercion.Mono);

            // Create player.
            try
            {
                _waveOutSwapper = new();
                _player = new(_settings.WavOutDevice, int.Parse(_settings.Latency), _waveOutSwapper) { Volume = 0.5f };
                _player.PlaybackStopped += (_, __) =>
                {
                    Tell("Player finished");
                    this.InvokeIfRequired(_ => chkPlay.Checked = false);
                    _prov?.Rewind();
                };

            }
            catch (Exception ex)
            {
                Tell($"ERR init failed: {ex}");
            }

            // File openers.
            var ftypes = AudioLibDefs.AUDIO_FILE_TYPES.SplitByTokens("|;*");
            Directory.GetFiles(_filesDir).ForEach(fn =>
            {
                if (ftypes.Contains(Path.GetExtension(fn)))
                {
                    btnOpen.DropDownItems.Add(fn, null, OpenFile_Click);
                }
            });

            chkPlay.Click += (_, __) => Play_Click();
            btnRewind.Click += (_, __) => _prov?.Rewind();
            btnTest.Click += (_, __) => UnitTests();

            // Go-go-go.
            timer1.Interval = 100;
            timer1.Enabled = true;
        }

        void Cc_InfoClick(object? sender, ChannelControl.InfoClickEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void OpenFile_Click(object? sender, EventArgs args)
        {
            // Reset.
            _player.Run(false);
            chkPlay.Checked = false;
            btnSwap.Checked = false;

            var ext = Path.GetExtension(sender!.ToString());

            try
            {
                ISampleProvider? prov = null;
                string fn = sender!.ToString()!;

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
                        // pick one:
                        prov = new AudioFileReader(fn);

                        //var csp = new ClipSampleProvider(fn, StereoCoercion.Mono);
                        ////csp.ClipProgress += Csp_ClipProgress;
                        //prov = csp;
                        break;
                }
                
                SetProvider(prov);
            }
            catch (Exception e)
            {
                Tell("ERR " + e.Message);
            }
        }

        /// <summary>
        /// Handle UI stuff.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ViewerStateChange(object? sender, WaveViewer.StateChangeEventArgs e)
        {
            if (sender == wv1)
            {
                switch (e.Which)
                {
                    case WaveViewer.UiChange.Gain:
                        sldGain.Value = wv1.Gain;
                        break;

                    case WaveViewer.UiChange.SelStart:
                        timeBar.SelStart = wv1.SelStart;
                        switch (_prov)
                        {
                            case ClipSampleProvider csp: csp.SelStart = wv1.SelStart; break;
                        }
                        break;

                    case WaveViewer.UiChange.SelLength:
                        timeBar.SelLength = wv1.SelLength;
                        switch (_prov)
                        {
                            case ClipSampleProvider csp: csp.SelLength = wv1.SelLength; break;
                        }
                        break;

                    default:
                        break;
                }
            }
            else if (sender == wv2)
            {
                switch (e.Which)
                {
                    case WaveViewer.UiChange.Marker:
                        wv1.Recenter(wv2.Marker);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Helper to manage resources.
        /// </summary>
        /// <param name="prov"></param>
        void SetProvider(ISampleProvider prov)
        {
            // Clean up old?
            switch (_prov)
            {
                case ClipSampleProvider csp:
                    var p = csp.SampleIndex;
                    //csp.ClipProgress -= Csp_ClipProgress;
                    break;

                case AudioFileReader afr:
                    afr!.Dispose();
                    break;
            }

            // New swap.
            switch (prov)
            {
                case ClipSampleProvider csp:
                    var p = csp.SampleIndex;
                    //csp.ClipProgress += Csp_ClipProgress;
                    //timeBar.Length = csp.SamplesPerChannel;
                    break;

                case AudioFileReader afr:
                    var p2 = afr.Position;
                    //timeBar.Length = (int)(afr.Length / (prov.WaveFormat.BitsPerSample / 8) / prov.WaveFormat.Channels);
                    break;
            }

            _prov = prov;
            ShowWave(_prov);
            _waveOutSwapper.SetInput(_prov);
        }

        /// <summary>
        /// Boilerplate helper.
        /// </summary>
        /// <param name="prov"></param>
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

                        Tell($"Show {csp.GetInfoString()}");
                        timeBar.Length = csp.SamplesPerChannel;
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
                        Tell($"Show {afr.GetInfoString()}");
                        timeBar.Length = (int)(afr.Length / (prov.WaveFormat.BitsPerSample / 8) / prov.WaveFormat.Channels);
                    }
                    break;
            }

            timeBar.Current = 0;
            var thumb = wv1.RenderThumbnail(timeBar.Width, timeBar.Height, Color.Blue, Color.Pink, true);
            timeBar.Thumbnail = thumb;
        }

        /// <summary>
        /// 
        /// </summary>
        void Play_Click()
        {
            if (_prov is null)
            {
                Tell("open a file first please");
            }
            else
            {
                if (chkPlay.Checked)
                {
                    _player.Run(true);
                    if (!_player.Playing)
                    {
                        Tell("ERR: can't run clip!");
                        chkPlay.Checked = false;
                    }
                }
                else   
                {
                    _player.Run(chkPlay.Checked);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Resample_Click(object? sender, EventArgs e)
        {
            string fn = Path.Join(_filesDir, "test.wav");
            AudioUtils.Convert(Conversion.Resample, fn);
        }

        /// <summary>
        /// Swap test for SwappableSampleProvider.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Swap_Click(object? sender, EventArgs args)
        {
            if (_prov is null)
            {
                Tell("Open a file first please");
            }
            else
            {
                var newProv = btnSwap.Checked ? _provSwap : _prov;
                _waveOutSwapper.SetInput(newProv); // For listen.
                ShowWave(newProv);
                Tell($"Swapped");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void FileInfo_Click(object? sender, EventArgs args)
        {
            // Dump all test files.
            bool verbose = false;
            Tell($"Type{AudioFileInfo.DELIM}File{AudioFileInfo.DELIM}Params");
            Directory.GetFiles(_filesDir).ForEach(f => { Tell(AudioFileInfo.GetFileInfo(f, verbose)); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Timer1_Tick(object? sender, EventArgs args)
        {
            if (btnRunBars.Checked)
            {
                // Update progress bar.
                timeBar.Current += 1000; // not-realtime for testing
            }
            else
            {
                switch (_prov)
                {
                    case ClipSampleProvider csp:
                        timeBar.Current = csp.SampleIndex;
                        break;
                    case AudioFileReader afr:
                        timeBar.Current = (int)(afr.Position / afr.WaveFormat.BitsPerSample / afr.WaveFormat.Channels * 8);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Caps_Click(object? sender, EventArgs e)
        {
            // –1 indicates the default output device, while 0 is the first output device.
            for (int id = -1; id < WaveOut.DeviceCount; id++)
            {
                var cap = WaveOut.GetCapabilities(id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Settings_Click(object sender, EventArgs e)
        {
            SettingsEditor.Edit(_settings, "howdy!!!", 400);
            _settings.Save();
            Tell("You better restart!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        void Tell(string s)
        {
            this.InvokeIfRequired(_ => { tvInfo.Append(s); });
        }

        /// <summary>
        /// 
        /// </summary>
        void UnitTests()
        {
            TestRunner runner = new(OutputFormat.Readable);
            var torun = new[] { "CONVERT" };
            runner.RunSuites(torun);
            runner.Context.OutputLines.ForEach(l => Tell(l));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
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

    // public bool Selected
    // public ChannelState State { get; set; }

    // public string Info1
    // public string Info2
    // public double Pan
    // public double Volume

    // public event EventHandler<InfoClickEventArgs>? InfoClick;
    // public event EventHandler<SelectionChangeEventArgs>? SelectionChange;
    // public event EventHandler<StateChangeEventArgs>? StateChange;

    //cc1.DrawColor = _settings.DrawColor;
    //        cc1.SelectedColor = _settings.SelectedColor;
    //        cc1.InfoClick += ChannelControl_InfoClick;

    public class TestSettings : SettingsCore
    {
        [JsonConverter(typeof(JsonColorConverter))]
        public Color SelectedColor { get; set; } = Color.AliceBlue;

        [JsonConverter(typeof(JsonColorConverter))]
        public Color DrawColor { get; set; } = Color.Green;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WaveSelectionMode DefaultSelectionMode { get; set; } = WaveSelectionMode.Time;

        [DisplayName("Default BPM")]
        [Browsable(true)]
        public double DefaultBPM { get; set; } = 100.0;

        [DisplayName("Wave Output Device")]
        [Description("How to play the audio files.")]
        [Browsable(true)]
        [Editor(typeof(WaveOutputDeviceTypeEditor), typeof(UITypeEditor))]
        public string WavOutDevice { get; set; } = "Microsoft Sound Mapper";

        [DisplayName("Latency")]
        [Description("What's the hurry?")]
        [Browsable(true)]
        [Editor(typeof(LatencyTypeEditor), typeof(UITypeEditor))]
        public string Latency { get; set; } = "200";
    }
}
