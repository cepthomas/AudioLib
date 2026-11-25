using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib;

/// <summary>
/// Extensions to enhance core NAudio for this application.
/// </summary>
public static class Extensions // TODO1 combine wwith AudioUtils??
{

/*
    
    /// <summary>
    /// Make a buffer from the provider contents. Mono only.
    /// </summary>
    /// <param name="prov">The provider.</param>
    /// <returns>Values.</returns>
    public static float[] ReadAll(this ISampleProvider prov)
    {
       //prov.Rewind();

       var data = new List<float>(AudioLibDefs.READ_BUFF_SIZE);
       var buff = new float[AudioLibDefs.READ_BUFF_SIZE];
       int numRead;
       int totalRead = 0;
       int maxSamples = AudioLibDefs.MAX_CLIP_SIZE * AudioLibDefs.SAMPLE_RATE * 60;

       while ((numRead = prov.Read(buff.AsSpan(0, buff.Length))) > 0)
       {
           data.AddRange(buff.Take(numRead));

           // Test for max size.
           totalRead += numRead;
           if (totalRead > maxSamples)
           {
               throw new InvalidOperationException($"Provider/file too large");
           }
       }

       return data.ToArray();
    }

    /// <summary>
    /// Agnostic stream position setter. TODO1???
    /// </summary>
    /// <param name="prov"></param>
    public static void Rewind(this ISampleProvider prov)
    {
       switch (prov)
       {
           case ClipSampleProvider csp: csp.SampleIndex = csp.SelStart; break;
           case ResamplingAudioFileReader afr: afr.Rewind(); break;
           case WdlResamplingSampleProvider afr: afr.Rewind(); break;
           case AudioFileReader afr: afr.Rewind(); break;
           default: throw new AudioLibException("TODO1");
       }
    }

    /// <summary>
    /// Sanity check for only 32bit fp, 44100Hz, mono.
    /// </summary>
    /// <param name="prov">Format to check.</param>
    /// <param name="mono">Must be mono.</param>
    public static void Validate(this ISampleProvider prov, bool mono)
    {
        var wf = prov.WaveFormat;

        if (wf.Encoding != WaveFormatEncoding.IeeeFloat)
        {
            throw new ArgumentException($"Invalid encoding {wf.Encoding}. Must be IEEE float.");
        }

        if (wf.SampleRate != AudioLibDefs.SAMPLE_RATE)
        {
            throw new ArgumentException($"Invalid sample rate {wf.SampleRate}. Must be {AudioLibDefs.SAMPLE_RATE}.");
        }

        if (wf.BitsPerSample != 32)
        {
            throw new ArgumentException($"Invalid bits per sample {wf.BitsPerSample}. Must be 32.");
        }

        if (mono && wf.Channels != 1)
        {
            throw new ArgumentException("Only mono supported for this operation.");
        }
    }

    /// <summary>
    /// Export wave data to csv file.
    /// </summary>
    /// <param name="prov">Data source.</param>
    /// <param name="exportFileName"></param>
    public static void Export(this ISampleProvider prov, string exportFileName)
    {
        prov.Rewind();

        List<string> ls = [];
        var vals = new float[AudioLibDefs.READ_BUFF_SIZE];
        bool done = false;
        int index = 0;

        if (prov.WaveFormat.Channels == 1) // mono
        {
            ls.Add($"Index,Val");
            while (!done)
            {
                var sread = prov.Read(vals.AsSpan(0, AudioLibDefs.READ_BUFF_SIZE));
                for (int i = 0; i < sread; i++)
                {
                    ls.Add($"{index++}, {vals[i]}");
                }
                done = sread != AudioLibDefs.READ_BUFF_SIZE;
            }
        }
        else // stereo
        {
            ls.Add($"Index,Left,Right");
            while (!done)
            {
                var sread = prov.Read(vals.AsSpan(0, AudioLibDefs.READ_BUFF_SIZE));
                for (int i = 0; i < sread; i += 2)
                {
                    ls.Add($"{index++}, {vals[i]}, {vals[i + 1]}");
                }
                done = sread != AudioLibDefs.READ_BUFF_SIZE;
            }
        }

        File.WriteAllLines(exportFileName, ls);
    }


    */
}
