using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib
{
    /// <summary>Conversion options.</summary>
    public enum Conversion { ToMonoWav, SplitStereo, Resample }


    public class Hold // TODO2 only Convert()???
    {
        /// <summary>
        /// Process a file.
        /// </summary>
        /// <param name="conv">Specific operation.</param>
        /// <param name="fn">The input filename. Output file is the same with option appended.</param>
        /// <returns>Succesful conversion.</returns>
        public static bool Convert(Conversion conv, string fn)
        {
            bool converted = true;
            ISampleProvider? prov1;
            ISampleProvider? prov2 = null;
            string tag1 = "";
            string tag2 = "";

            using var afr = new AudioFileReader(fn);
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
            ISampleProvider prov = new AudioFileReader(fn);
            prov = new WdlResamplingSampleProvider(prov, AudioLibDefs.SAMPLE_RATE);
            WaveFileWriter.CreateWaveFile16(newfn, prov);
        }
    }
}    
