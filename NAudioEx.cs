using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>
    /// Extensions to enhance core NAudio for this application.
    /// </summary>
    public static class NAudioEx
    {
        /// <summary>
        /// Make a buffer from the provider contents. Mono only.
        /// </summary>
        /// <param name="prov">The provider.</param>
        /// <returns>Values.</returns>
        public static float[] ReadAll(this ISampleProvider prov)
        {
            prov.Validate(true);
            prov.Rewind();

            var data = new List<float>(AudioLibDefs.READ_BUFF_SIZE);
            var buff = new float[AudioLibDefs.READ_BUFF_SIZE];
            int numRead;
            int totalRead = 0;
            int maxSamples = AudioSettings.LibSettings.MaxClipSize * AudioLibDefs.SAMPLE_RATE * 60;
            while ((numRead = prov.Read(buff, 0, buff.Length)) > 0)
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
        /// Agnostic position setter.
        /// </summary>
        /// <param name="prov"></param>
        public static void Rewind(this ISampleProvider prov)// TODO are these a bit klunky?
        {
            switch(prov)
            {
                case ClipSampleProvider csp: csp.Position = 0; break;
                case WaveViewer wv: wv.Rewind(); break;
                case AudioFileReader afr: afr.Position = 0; break;
                case SwappableSampleProvider ssp: ssp.Rewind(); break;
                default: break;
            }
        }

        // or like:
        //public static void Rewind(this ClipSampleProvider prov) { prov.Position = 0; }
        //public static void Rewind(this WaveViewer prov) { prov.Rewind(); }
        //public static void Rewind(this AudioFileReader prov) { prov.Position = 0; }
        //public static void Rewind(this SwappableSampleProvider prov) { prov.Rewind(); }


        /// <summary>
        /// Agnostic property.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns>The length or -1 if unknown.</returns>
        public static int Length(this ISampleProvider prov)
        {
            int len = -1; // default
            switch (prov)
            {
                case ClipSampleProvider csp: len = csp.Length; break;
                case WaveViewer wv: len = wv.Length; break;
                case AudioFileReader afr: len = (int)afr.Length; break;
                case SwappableSampleProvider ssp: len = 0; break;
                default: break;
            }
            return len;
        }

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
        /// Get provider info generically.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns>Info as tuple.</returns>
        public static List<(string name, string val)> GetInfo(this ISampleProvider prov)
        {
            List<(string name, string val)> info = new();

            // Common stuff.
            // Simplify provider name.
            string s = prov.GetType().ToString().Replace("NAudio.Wave.", "").Replace("AudioLib.", "");
            info.Add(("Provider", s));

            // Type specific stuff.
            switch (prov)
            {
                case ClipSampleProvider csp:
                    info.Add(("File", csp.FileName == "" ? "None" : Path.GetFileName(csp.FileName)));
                    info.Add(("Length", csp.Length.ToString()));
                    info.Add(("Time", csp.TotalTime.ToString(AudioLibDefs.TS_FORMAT)));
                    break;

                case WaveViewer wv:
                    info.Add(("Length", wv.Length.ToString()));
                    info.Add(("Time", wv.TotalTime.ToString(AudioLibDefs.TS_FORMAT)));
                    break;

                case AudioFileReader afr:
                    info.Add(("File", afr.FileName == "" ? "None" : Path.GetFileName(afr.FileName)));
                    info.Add(("Length", afr.Length.ToString()));
                    info.Add(("Time", afr.TotalTime.ToString(AudioLibDefs.TS_FORMAT)));
                    break;

                case SwappableSampleProvider ssp:// anything useful?
                    break;

                default:
                    // Don't care
                    break;
            }

            // More common stuff.
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
        /// Resample to a new file compatible with this application.
        /// </summary>
        /// <param name="fn">The current filename</param>
        /// <param name="newfn">The new filename</param>
        public static void Resample(string fn, string newfn)
        {
            using var rdr = new AudioFileReader(fn);
            var resampler = new WdlResamplingSampleProvider(rdr, AudioLibDefs.SAMPLE_RATE);
            WaveFileWriter.CreateWaveFile16(newfn, resampler);
        }

        /// <summary>
        /// Export wave data to csv file.
        /// </summary>
        /// <param name="prov">Data source.</param>
        /// <param name="exportFileName"></param>
        public static void Export(this ISampleProvider prov, string exportFileName)
        {
            List<string> ls = new();
            prov.Rewind();
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
