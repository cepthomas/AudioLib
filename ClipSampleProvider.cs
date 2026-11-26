using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;

namespace Ephemera.AudioLib;

/// <summary>
/// Provider that encapsulates a client supplied audio data subset. When constructed, it reads in the
/// entire file. Does sample rate conversion if needed.
/// Mono output only - coerces stereo input per client call.
/// </summary>
public class ClipSampleProvider : ISampleProvider
{
    #region Fields
    /// <summary>The full buffer from source.</summary>
    float[] _vals = [];
    #endregion

    #region Properties
    /// <inheritdoc />
    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibDefs.SAMPLE_RATE, 1);

    /// <summary>Overall gain applied to all samples.</summary>
    public float Gain { get; set; } = 1.0f;

    /// <summary>The number of samples per channel.</summary>
    public int SamplesPerChannel { get { return _vals.Length; } }

    /// <summary>The total time.</summary>
    public TimeSpan TotalTime { get { return TimeSpan.FromMilliseconds((int)(1000.0f * _vals.Length / WaveFormat.SampleRate)); } }

    /// <summary>Where we are in the buffer.</summary>
    public int SampleIndex
    {
       get { return _sampleIndex; }
       set { _sampleIndex = _vals.Length > 0 ? MathUtils.Constrain(value, 0, _vals.Length - 1) : 0; }
    }
    int _sampleIndex = 0;

    /// <summary>The current time.</summary>
    public TimeSpan CurrentTime { get { return TimeSpan.FromMilliseconds((int)(1000.0f * _sampleIndex / WaveFormat.SampleRate)); } }

    /// <summary>Selection start sample.</summary>
    public int SelStart { get; set; } = 0;

    /// <summary>Selection length in samples. 0 means to the end.</summary>
    public int SelLength { get; set; } = 0;

    /// <summary>Original file name if available.</summary>
    public string? FileName { get; init; } = null;
    
    /// <summary>Original file modified.</summary>
    public bool Resampled { get; init; } = false;
    #endregion

    #region Events

    #endregion

    #region Constructors
    /// <summary>
    /// Constructor from a sample provider, usually AudioFileReader. Splits or merges stereo per client request.
    /// </summary>
    /// <param name="source">Source provider to use.</param>
    /// <param name="mode">How to handle stereo files.</param>
    public ClipSampleProvider(ISampleProvider source, StereoCoercion mode)
    {
        ISampleProvider prov;

        switch (source.WaveFormat.Channels)
        {
            case 1:
                // Mono, read as is.
                prov = source;
                break;

            case 2:
                // Stereo, pick an op.
                prov = new StereoToMonoSampleProvider(source)
                {
                    LeftVolume = mode == StereoCoercion.Mono ? 0.5f : (mode == StereoCoercion.Left ? 1.0f : 0.0f),
                    RightVolume = mode == StereoCoercion.Mono ? 0.5f : (mode == StereoCoercion.Right ? 1.0f : 0.0f)
                };
                break;

            default:
                throw new AudioLibException($"Unsupported number of channels: {source.WaveFormat.Channels}");
        }

        // Maybe resample.
        if (prov.WaveFormat.SampleRate != AudioLibDefs.SAMPLE_RATE)
        {
            prov = new WdlResamplingSampleProvider(prov, AudioLibDefs.SAMPLE_RATE);
            Resampled = true;
        }

        _vals = AudioUtils.ReadAll(prov);
    }

    /// <summary>
    /// Constructor from a file. Splits/merges stereo per client.
    /// </summary>
    /// <param name="fn">File to use.</param>
    /// <param name="mode">How to handle stereo files.</param>
    public ClipSampleProvider(string fn, StereoCoercion mode) : this(new AudioFileReader(fn), mode)
    {
        FileName = fn;
    }

    /// <summary>
    /// Constructor from a supplied buffer.
    /// </summary>
    /// <param name="vals">The data to use.</param>
    public ClipSampleProvider(float[] vals)
    {
        _vals = vals;
    }
    #endregion

    #region Public functions
    /// <summary>
    /// Reads samples from this sample provider. Honors user selection if specified.
    /// </summary>
    /// <param name="buffer">Sample buffer.</param>
    /// <returns>Number of samples read.</returns>
    public int Read(Span<float> buffer)
    {
        int numRead = 0;

        // Is it a specific selection? Find the end.
        int end = SelStart >= 0 && SelLength > 0 ?
            MathUtils.Constrain(SelStart + SelLength, 0, _vals.Length) :
            _vals.Length;

        // Read area of interest.
        long numToRead = Math.Min(buffer.Length, end - _sampleIndex);
        for (int n = 0; n < numToRead; n++)
        {
            buffer[n] = _vals[_sampleIndex] * Gain; // TensorPrimitives.Multiply?
            numRead++;
            _sampleIndex++;
        }

        return numRead;
    }

    /// <summary>
    /// Go back jack to start of selection.
    /// </summary>
    public void Rewind()
    {
        SampleIndex = SelStart;
    }

    /// <summary>
    /// For humans.
    /// </summary>
    public override string ToString()
    {
        var fn = FileName ?? "None";
        var rsmpl = Resampled ? "Resampled" : "";
        return $"{fn} {SamplesPerChannel} {TotalTime} {rsmpl}";
    }
    #endregion
}
