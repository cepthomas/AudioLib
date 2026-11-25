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
/// Mono output only - coerces stereo input per client call. Can be used for splitting stereo files.
/// </summary>
public class ClipSampleProvider : ISampleProvider
{
    #region Fields
    /// <summary>The full buffer from source.</summary>
    float[] _vals = [];
    #endregion

    //original file name
    string _fileName = "";
    bool _resampled = false;


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
    #endregion

    #region Events

    #endregion

    #region Constructors
    /// <summary>
    /// Constructor from a sample provider. Splits or merges stereo per client request.
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
            _resampled = true;
        }

        _vals = AudioUtils.ReadAll(prov);

        //// Accumulate chunks from provider.
        //var buffs = new List<(float[] d, int n)>();
        //int totalRead = 0;
        //do
        //{
        //    var buff = new float[AudioLibDefs.READ_BUFF_SIZE];
        //    int numRead = prov.Read(buff.AsSpan(0, buff.Length));
        //    if (numRead == 0) break;
        //    // else
        //    totalRead += numRead;
        //    buffs.Add((buff, numRead));
        //}
        //while (true);

        //// Concat and store all.
        //_vals = new float[totalRead];
        //int ind = 0;
        //buffs.ForEach(buff =>
        //{
        //    var src = buff.d.AsSpan(0, buff.n);
        //    var dest = _vals.AsSpan(ind, buff.n);
        //    src.CopyTo(dest);
        //    ind += src.Length;  // buff.n;
        //});
    }

    /// <summary>
    /// Constructor from a file. Splits/merges stereo per client.
    /// </summary>
    /// <param name="fn">File to use.</param>
    /// <param name="mode">How to handle stereo files.</param>
    public ClipSampleProvider(string fn, StereoCoercion mode) : this(new AudioFileReader(fn), mode)
    {
        _fileName = fn;
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
            buffer[n] = _vals[_sampleIndex] * Gain;
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
    /// Get provider info. Mainly for UI display.
    /// </summary>
    /// <returns>Info chunks.</returns>
    public string GetInfo()
    {
        List<string> ls = new();

        // Common stuff.
        ls.Add($"Provider:{GetType().ToString().Replace("NAudio.Wave.", "").Replace("AudioLib.", "")}");

        //var fn = csp.FileName == "" ? "None" : csp.FileName;
        //ls.Add($"File:{fn}");
        ls.Add($"Time:{TotalTime}");
        ls.Add($"SamplesPerChannel:{SamplesPerChannel}");

        // More common stuff.
        ls.Add($"Encoding:{WaveFormat.Encoding}");
        ls.Add($"Channels:{WaveFormat.Channels}");
        ls.Add($"SampleRate:{WaveFormat.SampleRate}");
        ls.Add($"BitsPerSample:{WaveFormat.BitsPerSample}");

        return string.Join("  ", ls);
    }
    #endregion

    #region Private functions
    ///// <summary>
    ///// Common buff loader.
    ///// </summary>
    ///// <param name="source">Source provider to use.</param>
    ///// <param name="mode">How to handle stereo files.</param>
    //void ReadSource(ISampleProvider source, StereoCoercion mode)
    //{
    //    ISampleProvider prov;

    //    if (source.WaveFormat.Channels == 2)
    //    {
    //        prov = new StereoToMonoSampleProvider(source)
    //        {
    //            LeftVolume = mode == StereoCoercion.Mono ? 0.5f : (mode == StereoCoercion.Left ? 1.0f : 0.0f),
    //            RightVolume = mode == StereoCoercion.Mono ? 0.5f : (mode == StereoCoercion.Right ? 1.0f : 0.0f)
    //        };
    //    }
    //    else
    //    {
    //        // Mono, read as is.
    //        prov = source;
    //    }

    //    _vals = AudioUtils.ReadAll(prov);
    //}
    #endregion
}
