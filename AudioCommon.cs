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
    public class AudioLibDefs
    {
        /// <summary>Supported types.</summary>
        public const string AUDIO_FILE_TYPES = "*.wav;*.mp3;*.m4a;*.flac";

        /// <summary>NAudio doesn't publish this for their API.</summary>
        public const double VOLUME_MIN = 0.0;

        /// <summary>NAudio doesn't publish this for their API.</summary>
        public const double VOLUME_MAX = 1.0;

        /// <summary>Internal buffer size.</summary>
        public const int READ_BUFF_SIZE = 1000000;

        /// <summary>For viewing purposes.</summary>
        public const string TS_FORMAT = @"mm\:ss\.fff";

        /// <summary>Everything internal.</summary>
        public const int SAMPLE_RATE = 44100;
    }

    /// <summary>How to handle stereo files.</summary>
    public enum StereoCoercion { Left, Right, Mono }

    public static class AudioLibUtils
    {
        // 44.1 sample/msec <-> 0.0226757369614512 msec/sample

        public static int TimeToSample(TimeSpan ts)
        {
            double sample = ts.TotalMilliseconds * AudioLibDefs.SAMPLE_RATE / 1000;
            return (int)sample;
        }

        public static int MsecToSample(float msec)
        {
            float sample = msec * AudioLibDefs.SAMPLE_RATE / 1000;
            return (int)sample;
        }

        public static TimeSpan SampleToTime(int sample)
        {
            float msec = 1000.0f * sample / AudioLibDefs.SAMPLE_RATE;
            TimeSpan ts = new(0, 0, 0, 0, (int)msec);
            return ts;
        }

        public static float SampleToMsec(int sample)
        {
            float msec = 1000.0f * sample / AudioLibDefs.SAMPLE_RATE;
            return msec;
        }
    }
}
