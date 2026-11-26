using System;
using System.Collections.Generic;
using NAudio.Wave;


namespace Ephemera.AudioLib;

/// <summary>
/// Sample provider for test waves.
/// </summary>
public class SignalGeneratorSampleProvider : ISampleProvider
{
    #region Types
    /// <summary>Flavor.</summary>
    public enum Waveform { Sin, }
    #endregion

    #region Fields
    /// <summary>Where we are now.</summary>
    double _currentPhase;

    /// <summary>Advance per sample.</summary>
    readonly double _phaseIncrement;

    /// <summary>Flavor.</summary>
    Waveform _wf; // enum

    /// <summary>samples per cycle.</summary>
    int _period;

    /// <summary>msec total.</summary>
    int _dur;
    #endregion

    #region Properties
    /// <summary>How high.</summary>
    public float Amplitude { get; set; } = 0.25f;

    /// <inheritdoc />
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibDefs.SAMPLE_RATE, 1);
    #endregion

    #region Functions
    /// <summary>
    /// Normal constructor.
    /// </summary>
    /// <param name="wf"></param>
    /// <param name="frequency"></param>
    /// <param name="dur"></param>
    public SignalGeneratorSampleProvider(Waveform wf, int frequency, int dur)
    {
        _wf = wf;
        var per = 1.0 / frequency; // 22.6757 msec/sample
        _period = (int)(per * AudioLibDefs.SAMPLE_RATE);

        switch (_wf)
        {
            case Waveform.Sin:
                _phaseIncrement = 2.0 * Math.PI * frequency / AudioLibDefs.SAMPLE_RATE;
                break;
        }
    }

    /// <inheritdoc />
    public int Read(Span<float> buffer)
    {
        int numRead = 0;

        switch (_wf)
        {
            case Waveform.Sin:
                for (int n = 0; n < buffer.Length; n++)
                {
                    buffer[n] = (float)(Amplitude * Math.Sin(_currentPhase)); // TensorPrimitives.Multiply?
                    numRead++;
                    _currentPhase += _phaseIncrement;

                    if (_currentPhase >= 2.0 * Math.PI)
                        _currentPhase -= 2.0 * Math.PI;
                }
                break;
        }

        return numRead; // 0 stops playback)
    }
    #endregion
}
