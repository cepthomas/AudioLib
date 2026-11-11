using System;
using System.Collections.Generic;
using System.Drawing;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib
{
    #region Types
    /// <summary>How to handle stereo files.</summary>
    public enum StereoCoercion { None, Left, Right, Mono }

    /// <summary>How to select.</summary>
    public enum WaveSelectionMode { Bar, Time, Sample };

    /// <summary>How to snap.</summary>
    public enum SnapType { None, Fine, Coarse };
    #endregion

    #region Definitions
    public class AudioLibDefs
    {
        /// <summary>Supported types.</summary>
        public const string AUDIO_FILE_TYPES = "*.wav;*.mp3;*.m4a;*.flac";

        /// <summary>NAudio doesn't publish this for their API.</summary>
        public const float MAX_VOLUME = 1.0f;

        /// <summary>Maximum gain.</summary>
        public const float MAX_GAIN = 5.0f;

        /// <summary>Everything internal.</summary>
        public const int SAMPLE_RATE = 44100;

        /// <summary>Stream buffer size.</summary>
        public const int READ_BUFF_SIZE = SAMPLE_RATE;

        /// <summary>Standard formatting.</summary>
        public const string TS_FORMAT = @"mm\.ss\.fff";

        /// <summary>Max clip size in minutes. Has a direct effect on memory usage.</summary>
        public const int MAX_CLIP_SIZE = 10;
    }
    #endregion
}
