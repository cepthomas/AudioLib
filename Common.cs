using System;
using System.Collections.Generic;
using System.Drawing;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib;

/// <summary>How to handle stereo files.</summary>
public enum StereoCoercion { None, Left, Right, Mono }

/// <summary>How to select.</summary>
public enum WaveSelectionMode { Bar, Time, Sample };

/// <summary>How to snap.</summary>
public enum SnapType { None, Fine, Coarse };

/// <summary>Library error.</summary>
public class AudioLibException(string message) : Exception(message) { }


/// <summary>Library options. Client can adjust, carefully.</summary>
public class AudioLibConfig
{
    /// <summary>Supported types. Standard windows file type format.</summary>
    public static string AudioFileTypes { get; set; } = "*.wav;*.mp3;*.m4a;*.flac;*.txt";

    /// <summary>Everything internal.</summary>
    public static int SampleRate { get; set; } = 44100;

    /// <summary>NAudio doesn't publish this for their API.</summary>
    public static float MaxVolume { get; set; } = 1.0f;

    /// <summary>Maximum gain.</summary>
    public static float MaxGain { get; set; } = 5.0f;

    /// <summary>Standard formatting.</summary>
    public static string TimestampFormat { get; set; } = @"mm\.ss\.fff";

    /// <summary>Max clip size in minutes. Has a direct effect on memory usage.</summary>
    public static int MaxClipMinutes { get; set; } = 10;

    /// <summary>Stream buffer size. Readonly.</summary>
    public static int ReadBuffSize { get { return SampleRate; } }

    /// <summary>Resolution of UI. Readonly.</summary>
    public static float VolumeResolution { get { return MaxVolume / 50; } }

    /// <summary>Max clip size in samples. Has a direct effect on memory usage. Readonly.</summary>
    public static int MaxSamples { get { return MaxClipMinutes * 60 * SampleRate; } }
}
