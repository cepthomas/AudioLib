using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Drawing.Design;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.NBagOfUis;
using Ephemera.AudioLib;


namespace Ephemera.AudioLib.Test
{
    public class TestSettings : SettingsCore
    {
        [JsonConverter(typeof(JsonColorConverter))]
        public Color SelectedColor { get; set; } = Color.AliceBlue;

        [JsonConverter(typeof(JsonColorConverter))]
        public Color DrawColor { get; set; } = Color.Green;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WaveSelectionMode DefaultSelectionMode { get; set; } = WaveSelectionMode.Time;

        [DisplayName("Default BPM")]
        [Browsable(true)]
        public double DefaultBPM { get; set; } = 100.0;

        [DisplayName("Wave Output Device")]
        [Description("How to play the audio files.")]
        [Browsable(true)]
        [Editor(typeof(WaveOutputDeviceTypeEditor), typeof(UITypeEditor))]
        public string WavOutDevice { get; set; } = "Microsoft Sound Mapper";

        [DisplayName("Latency")]
        [Description("What's the hurry?")]
        [Browsable(true)]
        [Editor(typeof(LatencyTypeEditor), typeof(UITypeEditor))]
        public string Latency { get; set; } = "200";
    }
}
