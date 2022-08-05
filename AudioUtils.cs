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
                rdr!.Position = 0; // rewind
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
            if (data is not null)
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
}
