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
    }

    /// <summary>How to handle stereo files.</summary>
    public enum StereoCoercion { Left, Right, Mono }
}
