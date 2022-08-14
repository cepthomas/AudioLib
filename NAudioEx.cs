using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>
    /// Extensions to enhance or extend core NAudio.
    /// </summary>
    public static class NAudioEx
    {
        /// <summary>
        /// Make a buffer from the provider contents. Mono only.
        /// </summary>
        /// <param name="prov">The provider.</param>
        /// <returns></returns>
        public static float[] ReadAll(this ISampleProvider prov)
        {
            prov.Validate(true);

            prov.SetPosition(0);

            List<float[]> parts = new();

            bool done = false;
            while (!done)
            {
                // Get a chunk.
                int toRead = AudioLibDefs.READ_BUFF_SIZE;
                var data = new float[toRead];

                int numRead = prov.Read(data, 0, toRead);

                if(numRead != toRead)
                {
                    // last bunch
                    Array.Resize(ref data, numRead);
                    done = true;
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
        /// Agnostic position getter.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns></returns>
        public static int GetPosition(this ISampleProvider? prov)
        {
            int pos = 0;
            if (prov is ClipSampleProvider csp)
            {
                pos = csp.Position;
            }
            else if (prov is AudioFileReader afr)
            {
                pos = (int)afr.Position;
            }
            return pos;
        }

        /// <summary>
        /// Agnostic position setter.
        /// </summary>
        /// <param name="prov"></param>
        /// <param name="pos"></param>
        public static void SetPosition(this ISampleProvider prov, int pos)
        {
            if (prov is ClipSampleProvider csp)
            {
                csp.Position = pos;
            }
            else if (prov is AudioFileReader afr)
            {
                afr.Position = pos;
            }
        }

        /// <summary>
        /// Get provider info. Mainly for window header.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns></returns>
        public static string GetInfo(this ISampleProvider prov)
        {
            List<string> ls = new();

            string s = prov.GetType().ToString().Replace("NAudio.Wave.", "");
            ls.Add($"Provider:{s}");

            string fn = "None";
            int numsamp = -1;
            TimeSpan ttime = new(); 
            if (prov is ClipSampleProvider csp)
            {
                fn = csp.FileName == "" ? "None" : Path.GetFileName(csp.FileName);
                numsamp = csp.Length * 4 / csp.WaveFormat.BitsPerSample;
                ttime = csp.TotalTime;
            }
            else if (prov is AudioFileReader afr)
            {
                fn = afr.FileName == "" ? "None" : Path.GetFileName(afr.FileName);
                numsamp = (int)(afr.Length * 4 / afr.WaveFormat.BitsPerSample);
                ttime = afr.TotalTime;
            }

            ls.Add($"File:{fn}");

            if(numsamp != -1)
            {
                ls.Add($"Length:{numsamp}");
                ls.Add($"Time:{ttime.ToString(AudioLibDefs.TS_FORMAT)}");
            }

            var wf = prov.WaveFormat;
            ls.Add($"Encoding:{wf.Encoding}");
            ls.Add($"Channels:{wf.Channels}");
            ls.Add($"SampleRate:{wf.SampleRate}");
            ls.Add($"BitsPerSample:{wf.BitsPerSample}");

            return string.Join("   ", ls);
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
               throw new ArgumentException($"Invalid encoding {wf.Encoding}. Must be IEEE float");
            }

            if (wf.SampleRate != 44100)
            {
                throw new ArgumentException($"Invalid sample rate {wf.SampleRate}. Must be 44100");
            }

            if (wf.BitsPerSample != 32)
            {
                throw new ArgumentException($"Invalid bits per sample {wf.BitsPerSample}. Must be 32");
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
            List<string> ls = new();
            prov.SetPosition(0); // rewind
            var vals = new float[AudioLibDefs.READ_BUFF_SIZE];
            bool done = false;
            int index = 0;

            if (prov.WaveFormat.Channels == 1) // mono
            {
                ls.Add($"Index,Val");
                while(!done)
                {
                    var sread = prov.Read(vals, 0, AudioLibDefs.READ_BUFF_SIZE);
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
                    var sread = prov.Read(vals, 0, AudioLibDefs.READ_BUFF_SIZE);
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
}
