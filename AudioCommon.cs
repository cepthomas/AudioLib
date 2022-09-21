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
    public enum Property { Gain, Marker, SelStart, SelLength }
    #endregion

    #region Globals
    public static class Globals
    {
        /// <summary>Global mode.</summary>
        public static WaveSelectionMode SelectionMode { get; set; } = WaveSelectionMode.Time;

        /// <summary>Global tempo if using Beat selection mode.</summary>
        public static float BPM  { get; set; } = 100.0f;
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

        /// <summary>Everything internal.</summary>
        public const int SAMPLE_RATE = 44100;

        /// <summary>Stream buffer size.</summary>
        public const int READ_BUFF_SIZE = SAMPLE_RATE;

        /// <summary>Standard formatting.</summary>
        public const string TS_FORMAT = @"mm\.ss\.fff";

        /// <summary>Max clip size in minutes. Can be overriden in settings.</summary>
        public const int MAX_CLIP_SIZE = 10;
    }
    #endregion
}
