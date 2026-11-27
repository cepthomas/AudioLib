using System;
using System.Collections.Generic;
using System.ComponentModel;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib;

/// <summary>A simple audio player.</summary>
public sealed class AudioPlayer : IDisposable
{
    #region Fields
    /// <summary>Wave output play device.</summary>
    readonly WaveOut _waveOut;
    #endregion

    #region Events
    /// <summary>Wave playing done.</summary>
    public event EventHandler<StoppedEventArgs>? PlaybackStopped;
    #endregion

    #region Properties
    /// <summary>Are we ok?</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool Valid { get { return _waveOut is not null; } }

    /// <summary>Volume.</summary>
    public float Volume
    {
        get { return _volume; }
        set { _volume = (float)MathUtils.Constrain(value, 0.0f, AudioLibConfig.MaxVolume); _waveOut?.Volume = _volume; }
    }
    float _volume = AudioLibConfig.MaxVolume * 0.7f;

    /// <summary>State.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
    public bool Playing { get; private set; }
    #endregion

    #region Lifecycle
    /// <summary>
    /// Normal constructor.
    /// </summary>
    /// <param name="wavOutDevice">Device name.</param>
    /// <param name="latency">How slow.</param>
    /// <param name="prov">Provider.</param>
    public AudioPlayer(string wavOutDevice, int latency, ISampleProvider prov)
    {
        // NAudio3 changes:
        // DesiredLatency is replaced by BufferMilliseconds. This is a compile break with no shim, and the meaning changed:
        // DesiredLatency was the total across all buffers, whereas BufferMilliseconds (default 100) sizes each individual buffer.
        // With the default NumberOfBuffers = 2, DesiredLatency = 300 becomes BufferMilliseconds = 150.

        // Create output device. –1 indicates the default output device, while 0 is the first output device.
        for (int i = -1; i < WaveOut.DeviceCount; i++)
        {
            var cap = WaveOut.GetCapabilities(i);
            if (wavOutDevice == cap.ProductName)
            {
                _waveOut = new WaveOut
                {
                    DeviceNumber = i,
                    NumberOfBuffers = 2,
                    BufferMilliseconds = latency / 2,
                    Volume = Volume
                };

                // Usually end of file but could be error. Client can handle.
                _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;

                _waveOut.Init(prov);
                break;
            }
        }

        if (_waveOut is null)
        {
            throw new AudioLibException($"Invalid device: {wavOutDevice}");
        }
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    public void Dispose()
    {
        _waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
        _waveOut.Stop();
        _waveOut.Dispose();
    }
    #endregion

    #region Public functions
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
    #endregion

    #region Private functions
    /// <summary>
    /// Usually end of file but could be error. Client can handle.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void WaveOut_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        Playing = false;
        PlaybackStopped?.Invoke(this, e);
    }
    #endregion
}
