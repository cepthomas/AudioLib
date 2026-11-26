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

    /// <summary>
    /// Export wave data to csv file.
    /// </summary>
    /// <param name="prov">Data source.</param>
    /// <param name="exportFileName"></param>
    public static void Export(ISampleProvider prov, string exportFileName)
    {
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

    /// <summary>
    /// Get wave peak values for UI display.
    /// App-specific version of NAudio's IPeakProvider -> https://github.com/naudio/NAudio.WaveFormRenderer/tree/master
    /// </summary>
    /// <param name="vals">Generate peaks from this.</param>
    /// <param name="startIndex">Where to start in source.</param>
    /// <param name="samplesPerPixel">UI resolution.</param>
    /// <param name="totalPixels">Where to stop.</param>
    /// <returns></returns>
    public static List<(float max, float min)> GetPeaks(float[] vals, int startIndex, int samplesPerPixel, int totalPixels)
    {
        if (samplesPerPixel == 0)
        {
            throw new ArgumentException($"samplesPerPixel must be > 0");
        }

        List<(float max, float min)> scaledVals = new();
        int pixelCount = 0;

        if (samplesPerPixel > 0)
        {
            // Uses buckets with min/max. Could also implement average, rms, max/min, ...
            for (int srcIndex = startIndex;
                srcIndex < vals.Length && pixelCount < totalPixels;
                srcIndex += samplesPerPixel, pixelCount++)
            {
                // Get the vals with any gain.
                int numSamples = Math.Min(samplesPerPixel, vals.Length - srcIndex);

                if (numSamples > 0)
                {
                    float max = float.MinValue;
                    float min = float.MaxValue;

                    // Process the group.
                    for (int i = 0; i < numSamples; i++)
                    {
                        float val = vals[srcIndex + i];
                        min = Math.Min(val, min);
                        max = Math.Max(val, max);
                    }

                    scaledVals.Add((max, min));
                }
            }
        }

        return scaledVals;
    }


}
