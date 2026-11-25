using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib;

public class AudioUtils
{
    /// <summary>
    /// Make a buffer from the provider contents. Mono only.
    /// </summary>
    /// <param name="prov">The provider.</param>
    /// <returns>Values.</returns>
    public static float[] ReadAll(ISampleProvider prov)
    {
        //AudioUtils.Rewind(prov);

        // Accumulate chunks from provider.
        var buffs = new List<(float[] d, int n)>();
        int totalRead = 0;
        do
        {
            var buff = new float[AudioLibDefs.READ_BUFF_SIZE];
            int numRead = prov.Read(buff.AsSpan(0, buff.Length));
            if (numRead == 0) break;
            // else
            totalRead += numRead;
            buffs.Add((buff, numRead));
        }
        while (true);

        // Concat and store all.
        var vals = new float[totalRead];
        int ind = 0;
        buffs.ForEach(buff =>
        {
            var src = buff.d.AsSpan(0, buff.n);
            var dest = vals.AsSpan(ind, buff.n);
            src.CopyTo(dest);
            ind += src.Length;  // buff.n;
        });

        return vals;
    }

    /*

/// <summary>
/// Make a buffer from the provider contents. Mono only.
/// </summary>
/// <param name="prov">The provider.</param>
/// <returns>Values.</returns>
public static float[] ReadAll(ISampleProvider prov) <<<< original version
{
    //AudioUtils.Rewind(prov);

    var data = new List<float>(AudioLibDefs.READ_BUFF_SIZE);
    var buff = new float[AudioLibDefs.READ_BUFF_SIZE];
    int numRead;
    int totalRead = 0;
    //int maxSamples = AudioLibDefs.MAX_CLIP_SIZE * AudioLibDefs.SAMPLE_RATE * 60;

    while ((numRead = prov.Read(buff.AsSpan(0, buff.Length))) > 0)
    {
        data.AddRange(buff.Take(numRead));

        // Test for max size.
        totalRead += numRead;
        if (totalRead > AudioLibDefs.MAX_SAMPLES)
        {
            throw new InvalidOperationException($"Provider/file too large");
        }
    }

    return [.. data];
}




/// <summary>Conversion options.</summary>
public enum Conversion { ToMonoWav, SplitStereo, Resample }



    /// <summary>
    /// Agnostic stream position setter. TODO1??? what exactly needs to be rewound? def AFRx2
    /// </summary>
    /// <param name="prov"></param>
    public static void Rewind(ISampleProvider prov)
    {
        switch (prov)
        {
            case ClipSampleProvider csp: csp.SampleIndex = csp.SelStart; break;  //ok
            case ResamplingAudioFileReader afr: afr.Rewind(); break;
            case WdlResamplingSampleProvider afr: break; // afr.Rewind(); break;
            case AudioFileReader afr: afr.Position = 0; break; // afr.Rewind(); break;
            default: throw new AudioLibException("Hey!!!!");
        }
    }
}


/// <summary>NAudio extensions.</summary>
public class AudioUtils
{
    /// <summary>
    /// Convert a file.
    /// </summary>
    /// <param name="conv">Specific operation.</param>
    /// <param name="fn">The input filename. Output file is the same with option appended.</param>
    /// <returns>Succesful conversion.</returns>
    public static bool Convert(Conversion conv, string fn) // used by Wavicler
    {
        bool converted = true;
        ISampleProvider? prov1;
        ISampleProvider? prov2 = null;
        string tag1 = "";
        string tag2 = "";

        using var afr = new AudioFileReaderXX(fn);
        prov1 = afr;

        switch (conv)
        {
            case Conversion.ToMonoWav:
                if (prov1.WaveFormat.Channels == 2)
                {
                    prov1 = new StereoToMonoSampleProvider(prov1) { LeftVolume = 0.5f, RightVolume = 0.5f };
                }
                // else leave mono as is.
                tag1 = "mono";
                break;

            case Conversion.SplitStereo:
                if (prov1.WaveFormat.Channels == 2)
                {
                    prov2 = new StereoToMonoSampleProvider(prov1) { LeftVolume = 0.0f, RightVolume = 1.0f };
                    prov1 = new StereoToMonoSampleProvider(prov1) { LeftVolume = 1.0f, RightVolume = 0.0f };
                    tag1 = "left";
                    tag2 = "right";
                }
                else
                {
                    prov1 = null;
                    converted = false;
                }
                break;

            case Conversion.Resample:
                if (prov1.WaveFormat.SampleRate != AudioLibDefs.SAMPLE_RATE)
                {
                    prov1 = new WdlResamplingSampleProvider(prov1, AudioLibDefs.SAMPLE_RATE);
                    tag1 = "resampled";
                }
                else
                {
                    // Already correct rate.
                    prov1 = null;
                    converted = false;
                }
                break;
        }

        if (prov1 is not null)
        {
            var ext = Path.GetExtension(fn);
            string newfn = fn.Replace(ext, $"_{tag1}.wav");
            WaveFileWriter.CreateWaveFile16(newfn, prov1);
        }

        if (prov2 is not null)
        {
            var ext = Path.GetExtension(fn);
            string newfn = fn.Replace(ext, $"_{tag2}.wav");
            WaveFileWriter.CreateWaveFile16(newfn, prov2);
        }

        return converted;
    }

    /// <summary>
    /// For client convenience.
    /// </summary>
    /// <param name="fn"></param>
    /// <param name="newfn"></param>
    public static void Resample(string fn, string newfn)
    {
        ISampleProvider prov = new AudioFileReaderXX(fn);
        prov = new WdlResamplingSampleProvider(prov, AudioLibDefs.SAMPLE_RATE);
        WaveFileWriter.CreateWaveFile16(newfn, prov);
    }
*/
}
