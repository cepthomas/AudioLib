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
    public enum WaveSelectionMode { Bar, Time, Sample };

    /// <summary>How to snap.</summary>
    public enum SnapType { Off, Fine, Coarse };

    /// <summary>Notification of param change.</summary>
    public enum ParamChange { None, Gain, Marker, SelStart, SelLength }

    /// <summary>Abstraction of selection mode. Does text parsing, formatting, snap.</summary>
    public interface IConverterOps
    {
        /// <summary>Snap to neighbor.</summary>
        /// <param name="sample">Test sample.</param>
        /// <param name="snap">How tight.</param>
        /// <returns>Snapped sample.</returns>
        int Snap(int sample, SnapType snap);

        /// <summary>Parse text.</summary>
        /// <param name="input">The text.</param>
        /// <returns>Corresponding sample.</returns>
        int Parse(string input);

        /// <summary>Make a readable string.</summary>
        /// <param name="sample">Which sample.</param>
        /// <returns>The string.</returns>
        string Format(int sample);
    }
    #endregion

    #region Globals
    public static class Globals
    {
        /// <summary>This abstracts the conversions for the different WaveSelectionModes.</summary>
        public static IConverterOps ConverterOps { get; set; } = new SampleOps();

        /// <summary>Global tempo if using Beat selection mode.</summary>
        public static double BPM { get; set; } = 100.0f;

        /// <summary>Colors.</summary>
        public static Color ControlColor { get; set; } = Color.MediumOrchid;
        public static Color WaveColor { get; set; } = Color.ForestGreen;
        public static Color GridColor { get; set; } = Color.LightGray;
        public static Color MarkColor { get; set; } = Color.Red;
        public static Color TextColor { get; set; } = Color.DimGray;
    }
    #endregion

    #region Definitions
    public class AudioLibDefs
    {
        /// <summary>Supported types.</summary>
        public const string AUDIO_FILE_TYPES = "*.wav;*.mp3;*.m4a;*.flac";

        /// <summary>NAudio doesn't publish this for their API.</summary>
        public const float VOLUME_MIN = 0.0f;

        /// <summary>NAudio doesn't publish this for their API.</summary>
        public const float VOLUME_MAX = 1.0f;

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
