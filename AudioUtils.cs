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
}
