using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;


namespace AudioLib
{
    public class VolumeDefs
    {
        /// <summary>Corresponds to midi velocity = 0.</summary>
        public const double MIN = 0.0;

        /// <summary>Corresponds to midi velocity = 127.</summary>
        public const double MAX = 1.0;

        /// <summary>Default value.</summary>
        public const double DEFAULT = 0.8;

        /// <summary>UI control smoothness.</summary>
        public const double STEP = 0.05;

        /// <summary>Allow UI controls some more headroom.</summary>
        public const double MAX_GAIN = 2.0;
    }
}
