using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;


namespace AudioLib
{
    public class AudioUtils
    {
        /// <summary>
        /// Make a buffer out of the provider contents.
        /// </summary>
        /// <param name="sprov">The provider.</param>
        /// <param name="len">Optional known length. TODO?</param>
        /// <returns></returns>
        public static float[] ReadAll(ISampleProvider sprov, int len = 0)
        {
            List<float[]> parts = new();

            bool done = false;
            while (!done)
            {
                // Get a chunk.
                int toRead = 20000;//TODO fix.
                var data = new float[toRead];

                int numRead = sprov.Read(data, 0, toRead);

                if(numRead != toRead)
                {
                    Array.Resize(ref data, numRead);
                    done = true; // last bunch
                }
                parts.Add(data);
            }

            // Count.
            int i = 0;
            parts.ForEach(p => i += p.Length);
            var all = new float[i];

            // Copy.
            i = 0;
            parts.ForEach(p => { p.CopyTo(all, i); i += p.Length; } );

            return all;
        }

        /// <summary>
        /// Sanity check for only 32bit fp, 44100Hz, mono.
        /// </summary>
        /// <param name="waveFormat">Format to check.</param>
        public static void ValidateFormat(WaveFormat waveFormat)
        {
            if (waveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
               throw new ArgumentException("Wave format must be IEEE float");
            }

            if (waveFormat.SampleRate != 44100)
            {
                throw new ArgumentException("Mismatched sample rate.");
            }

            if (waveFormat.Channels != 1)
            {
                throw new ArgumentException("Only mono please");
            }
        }

        /// <summary>
        /// Export wave data to text file.
        /// </summary>
        /// <param name="exportFileName"></param>
        /// <param name="rdr">Data source.</param>
        public static void Export(string exportFileName, AudioFileReader rdr)
        {
            List<string> ret = new();
            const int READ_BUFF_SIZE = 1000000;

            if (rdr is not null)
            {
                rdr.Position = 0; // rewind
                var sampleChannel = new SampleChannel(rdr, false);

                // Read all data.
                long len = rdr.Length / (rdr.WaveFormat.BitsPerSample / 8);
                var data = new float[len];
                int offset = 0;
                int num = -1;

                while (num != 0)
                {
                    try // see OpenFile().
                    {
                        num = rdr.Read(data, offset, READ_BUFF_SIZE);
                        offset += num;
                    }
                    catch (Exception)
                    {
                    }
                }

                // Make a csv file of data for external processing.
                if (sampleChannel.WaveFormat.Channels == 2) // stereo
                {
                    ret.Add($"Index,Left,Right");
                    long stlen = len / 2;

                    for (long i = 0; i < stlen; i++)
                    {
                        ret.Add($"{i + 1}, {data[i * 2]}, {data[i * 2 + 1]}");
                    }
                }
                else // mono
                {
                    ret.Add($"Index,Val");
                    for (int i = 0; i < data.Length; i++)
                    {
                        ret.Add($"{i + 1}, {data[i]}");
                    }
                }

                File.WriteAllLines(exportFileName, ret);
            }
            else
            {
                throw new InvalidOperationException("Audio file not open");
            }
        }

        /// <summary>
        /// Simple dump utility.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fn"></param>
        public static void Dump(float[] data, string fn)
        {
            List<string> ss = new();
            for (int i = 0; i < data.Length; i++)
            {
                ss.Add($"{i + 1}, {data[i]}");
            }
            File.WriteAllLines(fn, ss);
        }
    }
}
