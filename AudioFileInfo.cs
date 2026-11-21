using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Wave;
using NAudio.SoundFont;
using Ephemera.NBagOfTricks;
using System.Linq;


namespace Ephemera.AudioLib
{
    public class AudioFileInfo
    {
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static string DELIM = ",";
#pragma warning restore

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
                ".wav"  => GetInfoMfr(fileName, verbose),
                ".mp3" => GetInfoMfr(fileName, verbose),
                ".sf2" => GetInfoSf(fileName, verbose),
                ".m4a" => GetInfoMfr(fileName, verbose),
                ".flac" => GetInfoMfr(fileName, verbose),
                ".mid" => GetInfoMidi(fileName, verbose),
                ".prs" => GetInfoMidi(fileName, verbose),
                ".sty" => GetInfoMidi(fileName, verbose),
                ".pcs" => GetInfoMidi(fileName, verbose),
                ".sst" => GetInfoMidi(fileName, verbose),
                _ => GetInfoOther(fileName, verbose),
            };
            return sinfo;
        }

        /// <summary>
        /// Get info for file using MediaFoundationReader.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        static string GetInfoMfr(string fileName, bool verbose)
        {
            List<string> ls;

            try
            {
                // Class for reading any file that Media Foundation can play. Automatically converts to PCM.
                MediaFoundationReader mfr = new(fileName);
                var wf = mfr.WaveFormat;
                ls =
                [
                    "Audio",
                    fileName,
                    //$"Encoding:{wf.Encoding}",
                    $"SampleRate:{wf.SampleRate}",
                    $"Channels:{wf.Channels}",
                    $"BitsPerSample:{wf.BitsPerSample}",
                    $"Length:{mfr.Length}",
                ];

                // verbose?
                //$"AverageBytesPerSecond:{wf.AverageBytesPerSecond}"
                //$"TotalTime:{mfr.TotalTime}"
            }
            catch (Exception ex)
            {
                ls =
                [
                    "SoundFont",
                    fileName,
                    $"Error{ex}",
                ];
            }

            return string.Join(DELIM, ls);
        }

        /// <summary>
        /// Get info for midi (incl style) file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        static string GetInfoMidi(string fileName, bool verbose)
        {
            List<string> ls;

            try
            {
                var mfile = new NAudio.Midi.MidiFile(fileName, true);
                ls =
                [
                    "Midi",
                    fileName,
                    $"FileFormat:{mfile.FileFormat}",
                    $"Tracks:{mfile.Tracks}",
                    $"Events:{mfile.Events.ToList().Count}",
                ];

                // verbose?
            }
            catch (Exception ex)
            {
                ls =
                [
                    "SoundFont",
                    fileName,
                    $"Error{ex}",
                ];
            }

            return string.Join(DELIM, ls);
        }

        /// <summary>
        /// Get info for soundfonts.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        static string GetInfoSf(string fileName, bool verbose)
        {
            List<string> ls;

            try
            {
                SoundFont sf = new(fileName);
                var fi = sf.FileInfo;
                // SoundFont,C:\Dev\Misc\TestAudioFiles\4XW6-Perc.sf2,
                //
                // //Bank Name:User Bank,Sound Engine:E-mu 10K1,SoundFont Version:NAudio.SoundFont.SFVersion

                ls =
                [
                    "SoundFont",
                    fileName,
                    $"BankName:{fi.BankName}",
                    $"SoundEngine:{fi.WaveTableSoundEngine}",
                    $"Version:{fi.SoundFontVersion}",
                ];

                // verbose?
                // public SFVersion ROMVersion { get; set; }
                // public string Author { get; set; }
                // public string Comments { get; set; }
                // public string Copyright { get; set; }
                // public string CreationDate { get; set; }
                // public string DataROM { get; set; }
                // public string TargetProduct { get; set; }
                // public string Tools { get; set; }

                if (verbose)
                {
                    List<string> lsv = [];

                    foreach (Preset p in sf.Presets)
                    {
                        lsv.Add($"Preset:{p}");
                        p.Zones.ForEach(z =>
                        {
                            lsv.Add($"  Zone:{z}");
                            z.Generators.ForEach(g => lsv.Add($"    Generator:{g}"));
                            z.Modulators.ForEach(m => lsv.Add($"    Modulator:{m}"));
                        });
                    }

                    foreach (Instrument i in sf.Instruments)
                    {
                        lsv.Add($"Instrument:{i}");
                        i.Zones.ForEach(z =>
                        {
                            lsv.Add($"  Zone:{z}");
                            z.Generators.ForEach(g => lsv.Add($"    Generator:{g}"));
                            z.Modulators.ForEach(m => lsv.Add($"    Modulator:{m}"));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                ls =
                [
                    "SoundFont",
                    fileName,
                    $"Error{ex}",
                ];
            }

            return string.Join(DELIM, ls);
        }

        /// <summary>
        /// Get info for other file types. Maybe.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        static string GetInfoOther(string fileName, bool verbose)
        {
            return $"Other{DELIM}{fileName}";
        }
    }
}    