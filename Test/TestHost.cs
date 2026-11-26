using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using NAudio.Wave;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.NBagOfUis;


namespace Ephemera.AudioLib.Test
{
    public partial class TestHost : Form
    {
        /// <summary>Where the files are.</summary>
        readonly string _filesDir;

        /// <summary>The current audio provider.</summary>
        ClipSampleProvider? _provClip = null;

        /// <summary>Input to the player.</summary>
        readonly SwappableSampleProvider _provSwap = new(); // _waveOutSwapper;

        /// <summary>Renders to audio.</summary>
        readonly AudioPlayer? _player;

        /// <summary>Test stuff.</summary>
        readonly TestSettings _settings;

        /// <summary>Test stuff.</summary>
        ITranslator _translator = Translator.Create(WaveSelectionMode.Sample);

        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public TestHost()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();

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
            tvInfo.Prompt = "> ";
            tvInfo.Matchers =
            [
                new("! ", BgColor: Color.YellowGreen),
                new("ERR", BgColor: Color.LightPink),
                new("WRN", BgColor: Color.Plum),
            ];

            // Selection mode.
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
                    _translator = Translator.Create(mode);
                }

                wv1.Invalidate();
                timeBar.Invalidate();
            };
            cmbSelMode.SelectedItem = _settings.DefaultSelectionMode;

            // Gain control.
            sldGain.ValueChanged += (_, __) => wv1.Gain = (float)sldGain.Value;

            // Progress bar.
            timeBar.CurrentChanged += (_, __) => Tell($"Current timebar:{_translator.Format(timeBar.Current)}");
            timeBar.ProgressColor = Color.Green;
            timeBar.TextColor = Color.White;
            timeBar.BackColor = Color.LightCyan;

            // Wave viewers.
            wv1.WaveColor = Color.Blue;
            wv1.BackColor = Color.LightYellow;
            wv1.StateChange += ViewerStateChange;
            wv1.ContextMenuStrip!.Items.Add("Test item", null, (_, __) => Tell("Test item worked"));

            wv2.WaveColor = Color.Blue;
            wv2.BackColor = Color.LightYellow;
            wv2.StateChange += ViewerStateChange;

            // Channel controls. Typically voloume/pan/state/selected?? would be persisted per output.
            cc1.DrawColor = _settings.DrawColor;
            cc1.SelectedColor = _settings.SelectedColor;
            cc1.Volume = 0.8;
            cc1.Pan = -0.6;
            cc1.Info1 = "Channel 111";
            cc1.Info2 = "111 We can always carry this a step further.";
            cc1.State = ChannelState.Solo;
            cc1.Selected = false;
            cc1.InfoClick += Cc_InfoClick;
            cc1.StateChange += Cc_StateChange;
            cc1.SelectChange += Cc_SelectChange;

            cc2.DrawColor = _settings.DrawColor;
            cc2.SelectedColor = _settings.SelectedColor;
            cc2.Volume = 0.7;
            cc2.Pan = 0.1;
            cc2.Info1 = "Channel 222";
            cc2.Info2 = "222 There's really no end to this.";
            cc2.State = ChannelState.Normal;
            cc2.Selected = true;
            cc2.InfoClick += Cc_InfoClick;
            cc2.StateChange += Cc_StateChange;
            cc2.SelectChange += Cc_SelectChange;

            // Create player with swappable input.
            _player = new(_settings.WavOutDevice, int.Parse(_settings.Latency), _provSwap) { Volume = 0.5f };
            _player?.PlaybackStopped += (_, __) =>
            {
                Tell("Player finished");
                this.InvokeIfRequired(_ => chkPlay.Checked = false);
                _provClip?.Rewind();
            };

            // File openers.
            var ftypes = AudioLibConfig.AudioFileTypes.SplitByTokens("|;*");

            Directory.GetFiles(_filesDir).
                Where(fn => ftypes.Contains(Path.GetExtension(fn))).
                ForEach(fn => btnOpen.DropDownItems.Add(fn, null, OpenFile_Click));

            // Control etc.
            chkPlay.Click += (_, __) => Play_Click();
            btnRewind.Click += (_, __) => _provClip?.Rewind();
            btnSettings.Click += (_, __) => DoSettings();

            // What do we want to do?
            btnTest.Text = "unit test";
            btnTest.Click += (_, __) => DoUnitTests();

            //btnTest.Text = "capabilities";
            //btnTest.Click += (_, __) => DoCaps();

            //btnTest.Text = "file info";
            //btnTest.Click += (_, __) => DoFileInfo();

            // Go-go-go.
            timer1.Interval = 100;
            timer1.Tick += Timer1_Tick;
            timer1.Enabled = true;
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
                _player?.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Core
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Cc_SelectChange(object? sender, ChannelControl.SelectChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Cc_StateChange(object? sender, ChannelControl.StateChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            string fn = sender!.ToString()!;
            OpenFile(fn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn"></param>
        void OpenFile(string fn)
        {
            // Reset.
            _player?.Run(false);
            chkPlay.Checked = false;

            try
            {
                var ext = Path.GetExtension(fn).ToLower();

                if (ext == ".txt") // Assume wave from text file. If not it will throw.
                {
                    List<float> vals = [];
                    File.ReadAllLines(fn).ForEach(l =>  vals.Add(float.Parse(l)));
                    _provClip = new ClipSampleProvider(vals);
                }
                else if (AudioLibConfig.AudioFileTypes.Contains($"*{ext}")) // Assume valid audio file type.
                {
                    // Create reader type.
                    _provClip = new ClipSampleProvider(fn, StereoCoercion.Mono);
                }
                else
                {
                    throw new AudioLibException($"Unsupported file type [{fn}]");
                }

                Tell($"OpenFile {_provClip}");
                _provSwap.SetInput(_provClip);

                // Configure UI components.
                wv1.Init(_provClip);
                wv2.Init(new NullSampleProvider());

                timeBar.Length = _provClip.SamplesPerChannel;
                timeBar.Thumbnail = wv1.RenderThumbnail(timeBar.Width, timeBar.Height, Color.Blue, Color.Pink, true);
                timeBar.Current = 0;
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
                        if (_provClip is not null)
                        {
                            timeBar.SelStart = wv1.SelStart;
                            _provClip.SelStart = wv1.SelStart;
                        }
                        break;

                    case WaveViewer.UiChange.SelLength:
                        if (_provClip is not null)
                        {
                            timeBar.SelLength = wv1.SelLength;
                            _provClip.SelLength = wv1.SelLength;
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
        /// 
        /// </summary>
        void Play_Click()
        {
            if (_provClip is null)
            {
                Tell("ERR open a file first please");
            }
            else if (_player is null)
            {
                Tell("ERR dead player");
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
                timeBar.Current = _provClip is not null ? _provClip.SampleIndex : 0;
            }
        }
        #endregion

        #region Test Workers
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DoSettings()
        {
            SettingsEditor.Edit(_settings, "howdy!!!", 400);
            _settings.Save();
            Tell("You better restart!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void DoFileInfo()
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
        /// <param name="e"></param>
        void DoCaps()
        {
            // –1 indicates the default output device, while 0 is the first output device.
            for (int id = -1; id < WaveOut.DeviceCount; id++)
            {
                var cap = WaveOut.GetCapabilities(id);
                Tell($"dev out {id}: [{cap.ProductName}]({cap.Channels})");
            }

            for (int id = -1; id < WaveIn.DeviceCount; id++)
            {
                var cap = WaveIn.GetCapabilities(id);
                Tell($"dev in {id}: [{cap.ProductName}]({cap.Channels})");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DoUnitTests()
        {
            TestRunner runner = new(OutputFormat.Readable);
            var torun = new[] { "CONVERT" };
            runner.RunSuites(torun);
            runner.Context.OutputLines.ForEach(l => Tell(l));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        void Tell(string s)
        {
            this.InvokeIfRequired(_ => { tvInfo.Append(s); });
        }
        #endregion
    }
}
