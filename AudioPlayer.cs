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


//TODO Snipping, editing, etc.


namespace AudioLib
{
    /// <summary>
    /// A simple audio file player.
    /// </summary>
    public class AudioPlayer : IDisposable
    {
        #region Fields
        /// <summary>Wave output play device.</summary>
        readonly WaveOut _waveOut;

        /// <summary>Current state.</summary>
        AudioState _state = AudioState.Stopped;
            
        /// <summary>Stream read chunk.</summary>
        const int READ_BUFF_SIZE = 1000000;

        /// <summary>The volume.</summary>
        double _volume = VolumeDefs.DEFAULT;
        #endregion

        #region Events
        /// <summary>Wave playing done.</summary>
        public event EventHandler<StoppedEventArgs>? PlaybackStopped;
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
            // Create output device. –1 indicates the default output device, while 0 is the first output device
            for (int i = -1; i < WaveOut.DeviceCount; i++)
            {
                var cap = WaveOut.GetCapabilities(i);
                if (wavOutDevice == cap.ProductName)
                {
                    _waveOut = new WaveOut
                    {
                        DeviceNumber = i,
                        DesiredLatency = latency
                    };
                    _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                    break;
                }
            }

            if (_waveOut is null)
            {
                throw new ArgumentException($"Invalid midi device: {wavOutDevice}");
            }
        }

        /// <summary>
        /// Empty constructor to satisfy nullability.
        /// </summary>
        public AudioPlayer()
        {
            _waveOut = new WaveOut
            {
                DeviceNumber = -1,
                DesiredLatency = 500
            };
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            _waveOut.Stop();
            _waveOut.Dispose();
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Bind the source to output.
        /// </summary>
        /// <param name="smpl"></param>
        /// <returns></returns>
        public bool Init(ISampleProvider smpl)
        {
            bool ok = true;
            _waveOut.Init(smpl);
            _waveOut.Volume = (float)Volume;
            _state = AudioState.Stopped;
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
                _waveOut.Play();

                if (_waveOut.PlaybackState == PlaybackState.Playing)
                {
                    _state = AudioState.Playing;
                }
            }
            else
            {
                _waveOut.Pause(); // or Stop?
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
        /// Export wave data to text file. TODO This doesn't really belong here, but ok for now.
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
        #endregion

        #region Private functions
        /// <summary>
        /// Usually end of file but could be error. Client can handle.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            var ss = (WaveOut)sender;
            Debug.WriteLine($"WaveOut S:{ss.PlaybackState} P:{ss.GetPosition()}");


            PlaybackStopped?.Invoke(this, e);
            _state = AudioState.Complete;
        }
        #endregion
    }
}
