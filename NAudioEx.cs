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
        public static (float[] vals, float max, float min) ReadAll(this ISampleProvider prov)
        {
            prov.Validate(true);

            prov.Reset();

            List<float[]> parts = new();
            float max = 0.0f;
            float min = 0.0f;

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

                // Get min/max.
                data.ForEach(v => { max = Math.Max(max, v); min = Math.Min(min, v); });
            }

            // Count.
            int i = 0;
            parts.ForEach(p => i += p.Length);
            var all = new float[i];

            // Copy.
            i = 0;
            parts.ForEach(p => { p.CopyTo(all, i); i += p.Length; } );

            return (all, max, min);
        }

        ///// <summary>
        ///// Agnostic position getter.
        ///// </summary>
        ///// <param name="prov"></param>
        ///// <returns></returns>
        //public static int GetPosition(this ISampleProvider? prov) //TODO these are a bit klunky.
        //{
        //    int pos = 0;
        //    if (prov is ClipSampleProvider csp)
        //    {
        //        pos = csp.Position;
        //    }
        //    else if (prov is AudioFileReader afr)
        //    {
        //        pos = (int)afr.Position;
        //    }
        //    return pos;
        //}

        /// <summary>
        /// Agnostic position setter.
        /// </summary>
        /// <param name="prov"></param>
        public static void Reset(this ISampleProvider prov)
        {
            if (prov is ClipSampleProvider csp)
            {
                csp.Reset();
            }
            else if (prov is WaveViewer wv)
            {
                wv.Reset();
            }
            else if (prov is AudioFileReader afr)
            {
                afr.Position = 0;
            }
        }

        ///// <summary>
        ///// Agnostic position setter.
        ///// </summary>
        ///// <param name="prov"></param>
        ///// <param name="pos"></param>
        //public static void SetPosition(this ISampleProvider prov, int pos)
        //{
        //    if (prov is ClipSampleProvider csp)
        //    {
        //        csp.Position = pos;
        //    }
        //    else if (prov is AudioFileReader afr)
        //    {
        //        afr.Position = pos;
        //    }
        //}

        /// <summary>
        /// Get provider info. Mainly for window header.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns>Info chunks.</returns>
        public static string GetInfoString(this ISampleProvider prov)
        {
            List<string> ls = new();

            GetInfo(prov).ForEach(i => ls.Add($"{i.name}:{i.val}"));

            return string.Join("  ", ls);
        }

        /// <summary>
        /// Get provider info. Mainly for window header.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns>Info chunks.</returns>
        public static List<(string name, string val)> GetInfo(this ISampleProvider prov)
        {
            // List<string> ls = new();

            //System.Collections.Generic.SortedDictionary<>

            List<(string name, string val)> info = new();

            // Simplify provider name.
            string s = prov.GetType().ToString().Replace("NAudio.Wave.", "").Replace("AudioLib.", "");
            info.Add(("Provider", s));

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
            else if (prov is WaveViewer wv)
            {
                // ?????
            }

            info.Add(("File", fn));

            if (numsamp != -1)
            {
                info.Add(("Length", numsamp.ToString()));
                info.Add(("Time", ttime.ToString(AudioLibDefs.TS_FORMAT)));
            }

            var wf = prov.WaveFormat;
            info.Add(("Encoding", wf.Encoding.ToString()));
            info.Add(("Channels", wf.Channels.ToString()));
            info.Add(("SampleRate", wf.SampleRate.ToString()));
            info.Add(("BitsPerSample", wf.BitsPerSample.ToString()));

            return info;
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

            if (wf.SampleRate != AudioLibDefs.SAMPLE_RATE)
            {
                throw new ArgumentException($"Invalid sample rate {wf.SampleRate}. Must be {AudioLibDefs.SAMPLE_RATE}");
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
        /// Resample to a new reader compatible with this application.
        /// </summary>
        /// <param name="rdr"></param>
        /// <returns>The new file reader.</returns>
        public static AudioFileReader Resample(this AudioFileReader rdr) // TODO seems clumsy
        {
            string newfn;
            string fn = rdr.FileName;
            var ext = Path.GetExtension(fn);
            newfn = fn.Replace(ext, "_resampled" + ext);
            var resampler = new WdlResamplingSampleProvider(rdr, AudioLibDefs.SAMPLE_RATE);
            WaveFileWriter.CreateWaveFile16(newfn, resampler);
            var newrdr = new AudioFileReader(newfn);

            return newrdr;
        }


        /// <summary>
        /// Export wave data to csv file.
        /// </summary>
        /// <param name="prov">Data source.</param>
        /// <param name="exportFileName"></param>
        public static void Export(this ISampleProvider prov, string exportFileName)
        {
            List<string> ls = new();
            prov.Reset();
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
