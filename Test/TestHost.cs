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


namespace Ephemera.AudioLib.Test
{
    public partial class TestHost : Form
    {
        /// <summary>Where the files are.</summary>
        readonly string _filesDir;

        /// <summary>The current audio provider.</summary>
        //ISampleProvider _prov = new NullSampleProvider();



        /// <summary>Input to the player.</summary>
        readonly SwappableSampleProvider _provSwap; // _waveOutSwapper;


        ClipSampleProvider? _provClip = null;



        /// <summary>Renders to audio.</summary>
        /*readonly*/ AudioPlayer? _player;

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
                    _converter = Converter.Create(mode);
                }

                wv1.Invalidate();
                timeBar.Invalidate();
            };
            cmbSelMode.SelectedItem = _settings.DefaultSelectionMode;

            // Gain control.
            sldGain.ValueChanged += (_, __) => wv1.Gain = (float)sldGain.Value;

            // Progress bar.
            timeBar.CurrentChanged += (_, __) => Tell($"Current timebar:{_converter.Format(timeBar.Current)}");
            timeBar.ProgressColor = Color.Green;
            timeBar.TextColor = Color.DarkRed;
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
            cc2.Info1 = "We can always carry this a step further.";
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
            cc2.Info2 = "There's really no end to this.";
            cc2.State = ChannelState.Normal;
            cc2.Selected = true;
            cc2.InfoClick += Cc_InfoClick;
            cc2.StateChange += Cc_StateChange;
            cc2.SelectChange += Cc_SelectChange;



 //           _provSwap = new ClipSampleProvider(Path.Join(_filesDir, "test.wav"), StereoCoercion.Mono);

            // Create player.
            try
            {
                // Static swap provider.
                _provSwap = new SwappableSampleProvider();

                _player = new(_settings.WavOutDevice, int.Parse(_settings.Latency), _provSwap) { Volume = 0.5f };
                _player?.PlaybackStopped += (_, __) =>
                {
                    Tell("Player finished");
                    this.InvokeIfRequired(_ => chkPlay.Checked = false);
//                    _prov?.Rewind();
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

            // Transport control etc.
            chkPlay.Click += (_, __) => Play_Click();
            //TODO1 these:
            //btnRewind.Click += (_, __) => _prov?.Rewind();
            //btnResample.Click += Resample_Click;
            //btnSwap.Click += Swap_Click;

            btnSettings.Click += Settings_Click;
            btnFileInfo.Click += FileInfo_Click;
            btnCaps.Click += Caps_Click;
            btnTest.Click += (_, __) => UnitTests();

            // Go-go-go.
            timer1.Interval = 100;
            timer1.Tick += Timer1_Tick;
            timer1.Enabled = true;
        }


        void Hacko()
        {
            var afr = new AudioFileReader("DooDaa");

        }


/////////////////////////// TODO1 these /////////////////////////////////////////////////////////
        void Cc_SelectChange(object? sender, ChannelControl.SelectChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Cc_StateChange(object? sender, ChannelControl.StateChangeEventArgs e)
        {
            throw new NotImplementedException();
        }

        void Cc_InfoClick(object? sender, ChannelControl.InfoClickEventArgs e)
        {
            throw new NotImplementedException();
        }
////////////////////////////////////////////////////////////////////////////////////



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
            btnSwap.Checked = false;

            try
            {
                var ext = Path.GetExtension(fn).ToLower();
                ClipSampleProvider? prov = null;

                if (ext == ".txt") // Assume wave from text file. If not it will throw.
                {
                    var sdata = File.ReadAllLines(fn);
                    var tdata = new float[sdata.Length];
                    for (int i = 0; i < sdata.Length; i++) { tdata[i] = float.Parse(sdata[i]); }
                    prov = new ClipSampleProvider(tdata);
                }
                else if (AudioLibDefs.AUDIO_FILE_TYPES.Contains($"*{ext}")) // Assume valid audio file type.
                {
                    // Create reader type.
                    prov = new ClipSampleProvider(fn, StereoCoercion.Mono);
                }
                else
                {
                    prov = null;
                    throw new AudioLibException($"Unsupported file type [{fn}]");
                }


                // switch (ext)
                // {
                //     case ".txt": // Assume wave from text file. If not it will throw.
                //         var sdata = File.ReadAllLines(fn);
                //         var tdata = new float[sdata.Length];
                //         for (int i = 0; i < sdata.Length; i++) { tdata[i] = float.Parse(sdata[i]); }
                //         prov = new ClipSampleProvider(tdata);
                //         break;
                //
                //     default: // Assume valid audio file type.
                //         if (AudioLibDefs.AUDIO_FILE_TYPES.Contains($"*{ext}"))
                //         {
                //             // Create reader type.
                //             var csp = new ClipSampleProvider(fn, StereoCoercion.Mono);
                //             //csp.ClipProgress += Csp_ClipProgress;
                //             prov = csp;
                //         }
                //         else
                //         {
                //             throw new AudioLibException($"Unsupported file type [{fn}]");
                //         }
                //         break;
                // }

                SetProvider(prov);

                /////// ShowWave(prov);
                //_provClip.Rewind();
                //wv1.Init(_provClip);
                //wv2.Init(new NullSampleProvider());
                //Tell($"Show {_provClip.GetInfo()}");
                //timeBar.Length = _provClip.SamplesPerChannel;
            }
            catch (Exception e)
            {
                Tell("ERR " + e.Message);
            }
        }


        /// <summary>
        /// Helper to manage resources.
        /// </summary>
        /// <param name="prov"></param>
        void SetProvider(ClipSampleProvider prov)
        {
            // called in OpenFile >>> SetProvider(prov);

            // Clean up old?
//            var pold = _provClip.SampleIndex;
            // New swap.
//            var pnew = prov.SampleIndex;
            //timeBar.Length = prov.SamplesPerChannel;

            _provClip = prov;


            ///// ShowWave(_prov);
            _provClip.Rewind();
            wv1.Init(_provClip);
            _provClip.Rewind();
            wv2.Init(new NullSampleProvider());
            Tell($"Show {_provClip.GetInfo()}");
            timeBar.Length = _provClip.SamplesPerChannel;

            timeBar.Current = 0;
            var thumb = wv1.RenderThumbnail(timeBar.Width, timeBar.Height, Color.Blue, Color.Pink, true);
            timeBar.Thumbnail = thumb;


            //? _waveOutSwapper.SetInput(_prov);
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
                        //switch (_prov)
                        //{
                        //    case ClipSampleProvider csp: csp.SelStart = wv1.SelStart; break;
                        //}
                        break;

                    case WaveViewer.UiChange.SelLength:
                        if (_provClip is not null)
                        {
                            timeBar.SelLength = wv1.SelLength;
                            _provClip.SelLength = wv1.SelLength;
                        }
                        //switch (_prov)
                        //{
                        //    case ClipSampleProvider csp: csp.SelLength = wv1.SelLength; break;
                        //}
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

        ///// <summary>
        ///// Helper to manage resources.
        ///// </summary>
        ///// <param name="prov"></param>
        //void SetProvider(ISampleProvider prov)
        //{
        //    // Clean up old?
        //    switch (_prov)
        //    {
        //        case ClipSampleProvider csp:
        //            var p = csp.SampleIndex;
        //            //csp.ClipProgress -= Csp_ClipProgress;
        //            break;

        //        case ResamplingAudioFileReader afr:
        //            afr!.Dispose();
        //            break;

        //        case AudioFileReader afr:
        //            afr!.Dispose();
        //            break;
        //    }

        //    // New swap.
        //    switch (prov)
        //    {
        //        case ClipSampleProvider csp:
        //            var p = csp.SampleIndex;
        //            //csp.ClipProgress += Csp_ClipProgress;
        //            //timeBar.Length = csp.SamplesPerChannel;
        //            break;

        //        case ResamplingAudioFileReader afr:
        //            var p2 = afr.Position;
        //            //timeBar.Length = (int)(afr.Length / (prov.WaveFormat.BitsPerSample / 8) / prov.WaveFormat.Channels);
        //            break;

        //        case AudioFileReader afr:
        //            var p2 = afr.Position;
        //            //timeBar.Length = (int)(afr.Length / (prov.WaveFormat.BitsPerSample / 8) / prov.WaveFormat.Channels);
        //            break;
        //    }

        //    _prov = prov;
        //    ShowWave(_prov);
        //    _waveOutSwapper.SetInput(_prov);
        //}

        ///// <summary>
        ///// Boilerplate helper.
        ///// </summary>
        ///// <param name="prov"></param>
        //void ShowWave(ISampleProvider? prov)
        //{
        //    switch (prov)
        //    {
        //        case ClipSampleProvider csp:
        //            {
        //                csp.Rewind();
        //                wv1.Init(csp);
        //                wv2.Init(new NullSampleProvider());

        //                Tell($"Show {csp.GetInfo()}");
        //                timeBar.Length = csp.SamplesPerChannel;
        //                break;
        //            }

        //        case NullSampleProvider nsp:
        //            {
        //                wv1.Init(new NullSampleProvider());
        //                wv2.Init(new NullSampleProvider());
        //                timeBar.Length = 0;
        //                break;
        //            }

        //        default:
        //            throw new AudioLibException($"Invalid provider [{prov}]");


        //            //case AudioFileReader afr:  //case ResamplingAudioFileReader afr:
        //            //    {
        //            //        // If it's stereo split into two monos. This is not really how to do things.
        //            //        if (afr.WaveFormat.Channels == 2) // stereo
        //            //        {
        //            //            afr.Rewind();
        //            //            wv1.Init(new ClipSampleProvider(afr, StereoCoercion.Left));
        //            //            afr.Rewind();
        //            //            wv2.Init(new ClipSampleProvider(afr, StereoCoercion.Right));
        //            //        }
        //            //        else // mono
        //            //        {
        //            //            afr.Rewind();
        //            //            wv1.Init(new ClipSampleProvider(afr, StereoCoercion.None));
        //            //            afr.Rewind();
        //            //            wv2.Init(new ClipSampleProvider(afr, StereoCoercion.None));
        //            //        }
        //            //
        //            //        afr.Rewind();
        //            //        Tell($"Show {afr.GetInfo()}");
        //            //        timeBar.Length = (int)(afr.Length / (prov.WaveFormat.BitsPerSample / 8) / prov.WaveFormat.Channels);
        //            //        break;
        //            //    }
        //    }

        //    timeBar.Current = 0;
        //    var thumb = wv1.RenderThumbnail(timeBar.Width, timeBar.Height, Color.Blue, Color.Pink, true);
        //    timeBar.Thumbnail = thumb;
        //}

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
             //       _provClip.Rewind();
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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //void Resample_Click(object? sender, EventArgs e)
        //{
        //    string fn = Path.Join(_filesDir, "test.wav");
        //    AudioUtils.Convert(Conversion.Resample, fn);
        //}

        ///// <summary>
        ///// Swap test for SwappableSampleProvider.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="args"></param>
        //void Swap_Click(object? sender, EventArgs args)
        //{
        //    if (_prov is null)
        //    {
        //        Tell("Open a file first please");
        //    }
        //    else
        //    {
        //        var newProv = btnSwap.Checked ? _provSwap : _prov;
        //        _waveOutSwapper.SetInput(newProv); // For listen.
        //        ShowWave(newProv);
        //        Tell($"Swapped");
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void FileInfo_Click(object? sender, EventArgs args)
        {
            //OpenFile(@"C:\Dev\Misc\TestAudioFiles\short_samples.txt");
            //return;


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
                if (_provClip is not null)
                {
                    timeBar.Current = _provClip.SampleIndex;
                }
                else
                {
                    timeBar.Current = 0;
                }


                //switch (_prov)
                //{
                //    case ClipSampleProvider csp:
                //        timeBar.Current = csp.SampleIndex;
                //        break;

                //    case NullSampleProvider nsp:
                //        timeBar.Current = 0;
                //        break;

                //    //case ResamplingAudioFileReader afr:
                //    //    timeBar.Current = (int)(afr.Position / afr.WaveFormat.BitsPerSample / afr.WaveFormat.Channels * 8);
                //    //    break;

                //    default:
                //        throw new AudioLibException($"Invalid provider [{_prov}]");
                //}
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Settings_Click(object? sender, EventArgs e)
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
            _player?.Dispose();
            _player = null;

            base.Dispose(disposing);
        }
    }
}
