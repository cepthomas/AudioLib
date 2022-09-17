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

    /// <summary>Notification type.</summary>
    public enum UiChange { Gain, Marker, SelStart, SelLength }



    // public static class EXXXXX
    // {
    //     public static void Testeroooo(this TimeSpanEx prov) {  }
    // }


    // public class TimeSpanEx
    // {
    //     TimeSpan _timeSpan = new(0);

    //     public static TimeSpanEx Zero = new();

    //     public int TotalMilliseconds { get { return (int)Math.Round(_timeSpan.TotalMilliseconds); } }

    //     public TimeSpanEx() { }
    //     public TimeSpanEx(int msec) { _timeSpan = new(0, 0, 0, 0, msec); }

    //     public TimeSpanEx(float seconds) { _timeSpan = TimeSpan.FromSeconds(seconds); }

    //     public override string ToString() { return _timeSpan.ToString(AudioLibDefs.TS_FORMAT); }
    // }

    /// <summary>Container for musical time. Internally 0-based but traditional 1-based for the user.</summary>
    public class BarBeat
    {
        public const int BEAT_PARTS = 100;
        public int Bar { get; set; } = 0; // 0 to N
        public int Beat { get; set; } = 0; // 0 to 3
        public int PartBeat { get; set; } = 0; // 0 to BEAT_PARTS-1
        public override string ToString() { return $"{Bar+1}.{Beat+1}.{PartBeat:00}"; }
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
}
