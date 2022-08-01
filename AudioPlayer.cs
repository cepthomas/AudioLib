using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
    public sealed class AudioPlayer : IDisposable
    {
        #region Fields
        /// <summary>Wave output play device.</summary>
        readonly WaveOut? _waveOut = null;

        /// <summary>The volume.</summary>
        double _volume = 0.6;
        #endregion

        #region Events
        /// <summary>Wave playing done.</summary>
        public event EventHandler<StoppedEventArgs>? PlaybackStopped;
        #endregion

        #region Properties
        /// <summary>Are we ok?</summary>
        public bool Valid { get { return _waveOut is not null; } }

        /// <summary>Volume.</summary>
        public double Volume
        {
            get
            {
                return _volume;
            }
            set 
            {
                _volume = MathUtils.Constrain(value, AudioLibDefs.VOLUME_MIN, AudioLibDefs.VOLUME_MAX);
                if (_waveOut is not null) { _waveOut.Volume = (float)_volume; }
            }
        }

        /// <summary>State.</summary>
        public bool Playing { get; private set; }
        #endregion

        #region Lifecycle
        /// <summary>
        /// Normal constructor.
        /// </summary>
        /// <param name="wavOutDevice">Device name.</param>
        /// <param name="latency">How slow.</param>
        public AudioPlayer(string wavOutDevice, int latency)
        {
            // Create output device. –1 indicates the default output device, while 0 is the first output device.
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
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();
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
            bool ok = false;
            if (_waveOut is not null)
            {
                _waveOut.Init(smpl); // TODO sometimes calling this more than once seems to unhook the event callback.
                _waveOut.Volume = (float)Volume;
                ok = true;
            }
            Playing = false;
            return ok;
        }

        /// <summary>
        /// Start/stop everything.
        /// </summary>
        /// <param name="go">Or no.</param>
        public void Run(bool go)
        {
            if (_waveOut is not null)
            {
                if (go)
                {
                    _waveOut.Play();
                    Playing = _waveOut.PlaybackState == PlaybackState.Playing;
                }
                else
                {
                    _waveOut.Pause(); // or Stop?
                    Playing = false;
                }
            }
            else
            {
                Playing = false;
            }
        }

        /// <summary>
        /// Go back jack.
        /// </summary>
        public void Rewind()
        {
            // Nothing to do - client owns the reader.
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
            //Debug.WriteLine($"WaveOut_PlaybackStopped()");
            Playing = false;
            PlaybackStopped?.Invoke(this, e);
        }
        #endregion
    }
}
