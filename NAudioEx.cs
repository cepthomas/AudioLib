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
    /// TODO2 A lot of these are kind of clunky but the alternative is to add some new functionality
    /// to ISampleprovider. Maybe I'll branch NAudio some day.
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
        /// Resample to a new wav file.
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
            prov.Rewind();

            List<string> ls = new();
            var vals = new float[AudioLibDefs.READ_BUFF_SIZE];
            bool done = false;
            int index = 0;

            if (prov.WaveFormat.Channels == 1) // mono
            {
                ls.Add($"Index,Val");
                while (!done)
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

        /// <summary>
        /// Agnostic stream position setter.
        /// </summary>
        /// <param name="prov"></param>
        public static void Rewind(this ISampleProvider prov)
        {
            switch (prov)
            {
                case ClipSampleProvider csp: csp.SampleIndex = csp.SelStart; break;
                case AudioFileReader afr: afr.Position = 0; break;
            }
        }

        /// <summary>
        /// Get provider info. Mainly for UI display.
        /// </summary>
        /// <param name="prov"></param>
        /// <returns>Info chunks.</returns>
        public static string GetInfoString(this ISampleProvider prov)
        {
            List<string> ls = new();

            // Common stuff.
            ls.Add($"Provider:{prov.GetType().ToString().Replace("NAudio.Wave.", "").Replace("AudioLib.", "")}");

            // Type specific stuff.
            switch (prov)
            {
                case ClipSampleProvider csp:
                    var fn = csp.FileName == "" ? "None" : csp.FileName;
                    ls.Add($"File:{fn}");
                    ls.Add($"Time:{csp.TotalTime}");
                    ls.Add($"SamplesPerChannel:{csp.SamplesPerChannel}");
                    break;
                case AudioFileReader afr:
                    ls.Add($"File:{afr.FileName}");
                    ls.Add($"Length:{afr.Length}");
                    ls.Add($"Time:{afr.TotalTime}");
                    break;
                default: throw new InvalidOperationException($"Unsupported provider.");
            }

            // More common stuff.
            ls.Add($"Encoding:{prov.WaveFormat.Encoding}");
            ls.Add($"Channels:{prov.WaveFormat.Channels}");
            ls.Add($"SampleRate:{prov.WaveFormat.SampleRate}");
            ls.Add($"BitsPerSample:{prov.WaveFormat.BitsPerSample}");

            return string.Join("  ", ls);
        }
    }
}
