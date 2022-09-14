using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using NBagOfTricks;

// Functons to convert between the various time representation.

namespace AudioLib
{
    public static class Converters
    {
        /// <summary>
        /// Snap to closest neighbor.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="granularity">Where the neighbors are.</param>
        /// <returns></returns>
        static int Clamp(int val, int granularity) // TODO put in nbot.
        {
            int res = (val / granularity) * granularity;
            if (val % granularity > granularity / 2)
            {
                res += granularity;
            }
            return res;
        }

        /// <summary>
        /// Snap sample to 10000 or 1000 or none.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static int SnapSample(int sample, SnapType snap)
        {
            int snapped = snap switch
            {
                SnapType.Coarse => Clamp(sample, 10000),
                SnapType.Fine => Clamp(sample, 1000),
                _ => sample,
            };
            return snapped;
        }

        /// <summary>
        /// Snap time to second or tenth second or none.
        /// </summary>
        /// <param name="ts"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static TimeSpan SnapTime(TimeSpan ts, SnapType snap)
        {
            int msec = (int)ts.TotalMilliseconds;
            msec = snap switch
            {
                SnapType.Coarse => Clamp(msec, 1000), // second
                SnapType.Fine => Clamp(msec, 100), // tenth second
                _ => msec, // none
            };
            return new(0, 0, 0, 0, msec);
        }

        /// <summary>
        /// Snap BarBeat to bar or beat or none.
        /// </summary>
        /// <param name="bb"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static BarBeat SnapBarBeat(BarBeat bb, SnapType snap)
        {
            switch (snap)
            {
                case SnapType.None:
                    // no adjust
                    break;

                case SnapType.Coarse: // bar
                    if (bb.PartBeat > BarBeat.BEAT_PARTS / 2)
                    {
                        bb.Beat++;
                    }
                    if (bb.Beat >= 2)
                    {
                        bb.Bar++;
                    }
                    bb.Beat = 0;
                    bb.PartBeat = 0;
                    break;


                case SnapType.Fine: // beat
                    if (bb.PartBeat > BarBeat.BEAT_PARTS / 2)
                    {
                        bb.Beat++;
                    }
                    if (bb.Beat >= 4)
                    {
                        bb.Bar++;
                        bb.Beat = 0;
                    }
                    bb.PartBeat = 0;
                    break;
            }

            return bb;
        }

        /// <summary>
        /// Convert sample to time with snapping.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static TimeSpan SampleToTime(int sample, SnapType snap)
        {
            double msec = 1000D * sample / AudioLibDefs.SAMPLE_RATE;
            msec = Math.Round(msec);
            return SnapTime(new(0, 0, 0, 0, (int)msec), snap);
        }

        /// <summary>
        /// Convert time to sample.
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static int TimeToSample(TimeSpan ts)
        {
            double sample = (double)AudioLibDefs.SAMPLE_RATE * ts.TotalMilliseconds / 1000D;
            return (int)sample;
        }

        /// <summary>
        /// Convert sample to barbeat with snapping.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="bpm"></param>
        /// <param name="snap"></param>
        /// <returns>BarBeat</returns>
        public static BarBeat SampleToBarBeat(int sample, float bpm, SnapType snap)
        {
            // Convert then snap.
            float minPerBeat = 1.0f / bpm;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            int totalBeats = (int)(sample / smplPerBeat);
            double partBeats = (double)sample * BarBeat.BEAT_PARTS / smplPerBeat;
            partBeats = Math.Round(partBeats);

            BarBeat bb = new()
            {
                Bar = totalBeats / 4,
                Beat = totalBeats % 4,
                PartBeat = (int)partBeats % BarBeat.BEAT_PARTS
            };

            return SnapBarBeat(bb, snap);
        }

        /// <summary>
        /// Convert barbeat to sample.
        /// </summary>
        /// <param name="bb">BarBeat</param>
        /// <param name="bpm"></param>
        /// <returns>Sample</returns>
        public static int BarBeatToSample(BarBeat bb, float bpm)
        {
            int partBeats = (4 * bb.Bar * BarBeat.BEAT_PARTS) + (bb.Beat * BarBeat.BEAT_PARTS) + bb.PartBeat;
            float minPerBeat = 1.0f / bpm;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;

            return (int)(partBeats * smplPerBeat / BarBeat.BEAT_PARTS);
        }

        /// <summary>
        /// Convert sample to milliseconds.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static float SampleToMsec(int sample)
        {
            double msec = 1000D * sample / AudioLibDefs.SAMPLE_RATE;
            return (float)msec;
        }

        /// <summary>
        /// Convert milliseconds to sample.
        /// </summary>
        /// <param name="msec"></param>
        /// <returns></returns>
        public static int MsecToSample(float msec)
        {
            double sample = (double)msec * AudioLibDefs.SAMPLE_RATE / 1000D;
            return (int)sample;
        }
    }
}
