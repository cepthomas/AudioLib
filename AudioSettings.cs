using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using NAudio.Wave;
using NBagOfTricks;


namespace AudioLib
{
    [Serializable]
    public class AudioSettings
    {
        /// <summary>Current audio settings. Client must set this before accessing!</summary>
        [Browsable(false)]
        public static AudioSettings LibSettings
        {
            get { if (_settings is null) throw new InvalidOperationException("Client must set this property before accessing"); return _settings; }
            set { _settings = value; }
        }
        static AudioSettings? _settings = null;

        #region Persisted editable properties
        [DisplayName("Wave Output Device")]
        [Description("How to play the audio files.")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string WavOutDevice { get; set; } = "Microsoft Sound Mapper";

        [DisplayName("Latency")]
        [Description("What's the hurry?")]
        [Browsable(true)]
        [TypeConverter(typeof(FixedListTypeConverter))]
        public string Latency { get; set; } = "200";

        [DisplayName("Snap To Grid")]
        [Description("Snap to msec.")]
        [Browsable(true)]
        public int SnapMsec { get; set; } = 100;
        #endregion
    }

    /// <summary>Converter for selecting property value from known lists.</summary>
    public class FixedListTypeConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) { return true; }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) { return true; }

        // Get the specific list based on the property name.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
        {
            List<string>? rec = null;

            switch (context!.PropertyDescriptor.Name)
            {
                case "Latency":
                    rec = new List<string>()
                    {
                        "25", "50", "100", "150", "200", "300", "400", "500"
                    };
                    break;

                case "WavOutDevice":
                    rec = new List<string>();
                    for (int id = -1; id < WaveOut.DeviceCount; id++) // –1 indicates the default output device, while 0 is the first output device.
                    {
                        var cap = WaveOut.GetCapabilities(id);
                        rec.Add(cap.ProductName);
                    }
                    break;
            }

            return new StandardValuesCollection(rec);
        }
    }
}
