using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;
using NAudio.SoundFont;


namespace AudioLib
{
    /// <summary>Borrowed from NAudio.</summary>
    public class AudioFileInfo
    {
        /// <summary>
        /// Top level function to get file info.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string GetFileInfo(string fileName, bool verbose)
        {
            FileInfo fi = new(fileName);
            string sinfo = fi.Extension.ToLower() switch
            {
                ".wav"  => GetInfoWav(fileName, verbose),
                ".m4a"  => GetInfoWav(fileName, verbose),
                ".flac" => GetInfoWav(fileName, verbose),
                ".mp3"  => GetInfoMp3(fileName, verbose),
                ".sf2"  => GetInfoSf(fileName, verbose),
                _ => GetInfoOther(fileName, verbose),
            };
            return sinfo;
        }

        /// <summary>
        /// Get info for mp3 file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string GetInfoMp3(string fileName, bool verbose)
        {
            var ls = new List<string>
            {
                $"======== mp3 =========== File:{fileName} ================="
            };

            using (var rd = new Mp3FileReader(fileName))
            {
                var wf = rd.Mp3WaveFormat;
                ls.AddRange(FormatWaveFormat(wf));
                ls.Add($"Length:{rd.Length}");
                ls.Add($"TotalTime:{rd.TotalTime}");

                if (verbose)
                {
                    ls.Add($"Extra Size:{wf.ExtraSize} Block Align:{wf.BlockAlign}");
                    ls.Add($"ID:{wf.id} Flags:{wf.flags} BlockSize:{wf.blockSize} FramesPerBlock:{wf.framesPerBlock}");
                    ls.Add($"ID3v1Tag:{rd.Id3v1Tag} ID3v2Tag:{rd.Id3v2Tag}");

                    Mp3Frame frame;
                    while ((frame = rd.ReadNextFrame()) != null)
                    {
                        ls.Add($"Frame:{frame.MpegVersion},{frame.MpegLayer},{frame.SampleRate}Hz,{frame.ChannelMode},{frame.BitRate}bps, length {frame.FrameLength}");
                    }
                }
            }

            return string.Join(Environment.NewLine, ls);
        }

        /// <summary>
        /// Get info for wav file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string GetInfoWav(string fileName, bool verbose)
        {
            var ls = new List<string>
            {
                $"======== wav =========== File:{fileName} ================="
            };

            using (var rd = new WaveFileReader(fileName))
            {
                var wf = rd.WaveFormat;
                ls.AddRange(FormatWaveFormat(wf));
                ls.Add($"Length:{rd.Length}");
                ls.Add($"TotalTime:{rd.TotalTime}");

                if (verbose)
                {
                    ls.Add($"ExtraSize:{wf.ExtraSize} BlockAlign:{wf.BlockAlign}");

                    foreach (RiffChunk chunk in rd.ExtraChunks)
                    {
                        ls.Add($"Chunk:{chunk.IdentifierAsString} Length:{chunk.Length}");
                        //byte[] data = rd.GetChunkData(chunk);
                        //DescribeChunk(chunk, sb, data);
                    }
                }
            }

            return string.Join(Environment.NewLine, ls);
        }

        /// <summary>
        /// Get info for soundfonts.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string GetInfoSf(string fileName, bool verbose)
        {
            var ls = new List<string>
            {
                $"======== sf2 =========== File:{fileName} ================="
            };

            SoundFont sf = new(fileName);
            ls.Add($"{sf.FileInfo}");

            if (verbose)
            {
                foreach (Preset p in sf.Presets)
                {
                    ls.Add($"Preset:{p}");
                    p.Zones.ForEach(z => 
                    {
                        ls.Add($"  Zone:{z}");
                        z.Generators.ForEach(g => ls.Add($"    Generator:{g}"));
                        z.Modulators.ForEach(m => ls.Add($"    Modulator:{m}"));
                    });
                }

                foreach (Instrument i in sf.Instruments)
                {
                    ls.Add($"Instrument:{i}");
                    i.Zones.ForEach(z => 
                    {
                        ls.Add($"  Zone:{z}");
                        z.Generators.ForEach(g => ls.Add($"    Generator:{g}"));
                        z.Modulators.ForEach(m => ls.Add($"    Modulator:{m}"));
                    });
                }
            }

            return string.Join(Environment.NewLine, ls);
        }

        /// <summary>
        /// Get info for other file types. Maybe.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string GetInfoOther(string fileName, bool verbose)
        {
            var ls = new List<string>
            {
                $"======== mfr =========== File:{fileName} ================="
            };

            MediaFoundationReader mfr = new(fileName);

            var wf = mfr.WaveFormat;
            ls.AddRange(FormatWaveFormat(wf));
            ls.Add($"Length:{mfr.Length}");
            ls.Add($"TotalTime:{mfr.TotalTime}");

            if (verbose)
            {

            }

            return string.Join(Environment.NewLine, ls);
        }

        /// <summary>
        /// Helper.
        /// </summary>
        /// <param name="wf"></param>
        /// <returns></returns>
        static List<string> FormatWaveFormat(WaveFormat wf)
        {
            var ls = new List<string>
            {
                $"Encoding:{wf.Encoding}",
                $"SampleRate:{wf.SampleRate}",
                $"Channels:{wf.Channels}",
                $"BitsPerSample:{wf.BitsPerSample}",
                $"AverageBytesPerSecond:{wf.AverageBytesPerSecond}"
            };
            return ls;
        }
    }
}    