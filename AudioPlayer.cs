using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;



namespace AudioLib
{
    public partial class AudioPlayer : IDisposable
    {
        #region Fields
        /// <summary>Wave output play device.</summary>
        WaveOut? _waveOut = null;

        /// <summary>Current state.</summary>
        AudioState _state = AudioState.Stopped;
            
        /// <summary>Stream read chunk.</summary>
        const int READ_BUFF_SIZE = 1000000;

        /// <summary>The volume.</summary>
        double _volume = VolumeDefs.DEFAULT;
        #endregion

        #region Events

        public event EventHandler? PlaybackCompleted;//TODOX

        #endregion

        #region Properties
        /// <inheritdoc />
        public double Volume
        {
            get { return _volume; }
            set { _volume = MathUtils.Constrain(value, VolumeDefs.MIN, VolumeDefs.MAX); if (_waveOut != null) _waveOut.Volume = (float)_volume; }
        }

        /// <inheritdoc />
        public AudioState State
        {
            get { return _state; }
            set { _state = value; Run(_state == AudioState.Playing); }
        }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Normal constructor.
        /// </summary>
        /// <param name="wavOutDevice">Device name.</param>
        /// <param name="latency">How slow.</param>
        public AudioPlayer(string wavOutDevice, int latency)
        {
            // Create output device.
            for (int id = -1; id < WaveOut.DeviceCount; id++)
            {
                var cap = WaveOut.GetCapabilities(id);
                if (wavOutDevice == cap.ProductName)
                {
                    _waveOut = new WaveOut
                    {
                        DeviceNumber = id,
                        DesiredLatency = latency
                    };
                    _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                    break;
                }
            }
        }

        /// <summary>
        /// Empty constructor to satisfy nullability.
        /// </summary>
        public AudioPlayer()
        {
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
            _waveOut = null;
        }
        #endregion

        /// <summary>
        /// Bind the source to output.
        /// </summary>
        /// <param name="smpl"></param>
        /// <returns></returns>
        public bool Init(ISampleProvider smpl)
        {
            bool ok = true;
            _waveOut!.Init(smpl);
            _waveOut!.Volume = (float)Volume;
            return ok;
        }

        /// <summary>
        /// Start/stop everything.
        /// </summary>
        /// <param name="go">Or no.</param>
        public void Run(bool go)
        {
            if (go)
            {
                _waveOut!.Play();
                _state = AudioState.Playing;
            }
            else
            {
                _waveOut!.Pause(); // or Stop?
                //ResetMeters();
                _state = AudioState.Stopped;
            }
        }

        /// <summary>
        /// Go back jack.
        /// </summary>
        public void Rewind()
        {
            // Nothing to do.
        }

        /// <summary>
        /// Export wave data to text file.
        /// </summary>
        /// <param name="exportFileName"></param>
        /// <param name="rdr">Data source.</param>
        public void Export(string exportFileName, AudioFileReader rdr)
        {
            List<string> ret = new();

            if (rdr is not null)
            {
                rdr!.Position = 0; // rewind
                var sampleChannel = new SampleChannel(rdr, false);

                // Read all data.
                long len = rdr.Length / (rdr.WaveFormat.BitsPerSample / 8);
                var data = new float[len];
                int offset = 0;
                int num = -1;

                while (num != 0)
                {
                    try // see OpenFile().
                    {
                        num = rdr.Read(data, offset, READ_BUFF_SIZE);
                        offset += num;
                    }
                    catch (Exception)
                    {
                    }
                }

                // Make a csv file of data for external processing.
                if (sampleChannel.WaveFormat.Channels == 2) // stereo
                {
                    ret.Add($"Index,Left,Right");
                    long stlen = len / 2;

                    for (long i = 0; i < stlen; i++)
                    {
                        ret.Add($"{i + 1}, {data[i * 2]}, {data[i * 2 + 1]}");
                    }
                }
                else // mono
                {
                    ret.Add($"Index,Val");
                    for (int i = 0; i < data.Length; i++)
                    {
                        ret.Add($"{i + 1}, {data[i]}");
                    }
                }

                File.WriteAllLines(exportFileName, ret);
            }
            else
            {
                throw new InvalidOperationException("Audio file not open");
            }
        }

        /// <summary>
        /// Usually end of file but could be error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception is not null)
            {
                // TODOX? LogMessage("ERR", e.Exception.Message);
            }

            PlaybackCompleted?.Invoke(this, new EventArgs());
            _state = AudioState.Complete;
        }
    }
}
