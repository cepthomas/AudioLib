using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using NBagOfTricks;


namespace AudioLib
{
    #region Types
    /// <summary>How to handle stereo files.</summary>
    public enum StereoCoercion { None, Left, Right, Mono }

    /// <summary>How to select.</summary>
    public enum WaveSelectionMode { Beat, Time, Sample };

    /// <summary>How to snap.</summary>
    public enum SnapType { None, Fine, Coarse };

    public class BarBeat
    {
        public const int BEAT_PARTS = 1000;
        public int Bar { get; set; } = 1; // 1 to N
        public int Beat { get; set; } = 1; // 1 to 4
        public int Subdiv { get; set; } = 0; // 0 to BEAT_SUBDIVS-1
        public override string ToString() { return $"{Bar}:{Beat}.{Subdiv:000}"; }
    }
    #endregion

    #region Definitions
    public class AudioLibDefs
    {
        /// <summary>Supported types.</summary>
        public const string AUDIO_FILE_TYPES = "*.wav;*.mp3;*.m4a;*.flac";

        /// <summary>NAudio doesn't publish this for their API.</summary>
        public const double VOLUME_MIN = 0.0;

        /// <summary>NAudio doesn't publish this for their API.</summary>
        public const double VOLUME_MAX = 1.0;

        /// <summary>Maximum gain.</summary>
        public const float MAX_GAIN = 5.0f;

        /// <summary>For viewing purposes.</summary>
        public const string TS_FORMAT = @"mm\:ss\.fff";

        /// <summary>Everything internal.</summary>
        public const int SAMPLE_RATE = 44100;

        /// <summary>Stream buffer size.</summary>
        public const int READ_BUFF_SIZE = SAMPLE_RATE;
    }
    #endregion

    #region Utilities
    public static class AudioLibUtils
    {
        /// <summary>
        /// Conversion function. Does snapping using arbitrary factors.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static int SampleToSample(int sample, SnapType snap)
        {
            int snapped = snap switch
            {
                SnapType.Fine => sample / 1000,
                SnapType.Coarse => sample / 10000,
                _ => sample,
            };
            return snapped;
        }

        /// <summary>
        /// Conversion function. Does snapping.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static TimeSpan SampleToTime(int sample, SnapType snap)
        {
            float msec = 1000.0f * sample / AudioLibDefs.SAMPLE_RATE;
            msec = snap switch
            {
                SnapType.Fine => (msec / 100) * 100,
                SnapType.Coarse => (msec / 1000) * 1000,
                _ => msec,
            };
            return new(0, 0, 0, 0, (int)msec);
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static int TimeToSample(TimeSpan ts)
        {
            double sample = ts.TotalMilliseconds * AudioLibDefs.SAMPLE_RATE / 1000;
            return (int)sample;
        }

        /// <summary>
        /// Conversion function. Does snapping.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="bpm"></param>
        /// <param name="snap"></param>
        /// <returns>BarBeat</returns>
        public static BarBeat SampleToBarBeat(int sample, float bpm, SnapType snap)
        {
            float minPerBeat = 1.0f / bpm;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            int totalBeats = (int)(sample / smplPerBeat);
            int partBeats = (int)(BarBeat.BEAT_PARTS * sample / smplPerBeat);
            int bar = totalBeats / 4;
            int beat = totalBeats % 4;
            int partBeat = partBeats % BarBeat.BEAT_PARTS;

            BarBeat bb = new()
            {
                Bar = bar + 1,
                Beat = beat + 1,
                Subdiv = snap == SnapType.None ? partBeat : 0, // TODO1 should round
            };
            return bb;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="bb">BarBeat</param>
        /// <param name="bpm"></param>
        /// <returns>Sample</returns>
        public static int BarBeatToSample(BarBeat bb, float bpm)
        {
            int partBeats = (4 * bb.Bar * BarBeat.BEAT_PARTS) + (bb.Beat * BarBeat.BEAT_PARTS) + bb.Subdiv;
            float minPerBeat = 1.0f / bpm;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            return (int)(partBeats * smplPerBeat / BarBeat.BEAT_PARTS);
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="msec"></param>
        /// <returns></returns>
        public static int MsecToSample(float msec)
        {
            float sample = msec * AudioLibDefs.SAMPLE_RATE / 1000;
            return (int)sample;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static float SampleToMsec(int sample)
        {
            float msec = 1000.0f * sample / AudioLibDefs.SAMPLE_RATE;
            return msec;
        }
    }
    #endregion
}
