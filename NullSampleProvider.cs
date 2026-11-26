using System;
using System.Collections.Generic;
using NAudio.Wave;


namespace Ephemera.AudioLib;

/// <summary>
/// Sample provider that does nothing.
/// </summary>
public class NullSampleProvider : ISampleProvider
{
    /// <inheritdoc />
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibConfig.SampleRate, 1);

    /// <inheritdoc />
    public int Read(Span<float> buffer)
    {
        return 0;
    }
}
