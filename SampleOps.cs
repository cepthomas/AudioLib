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
    /// <summary>Converters for samples.</summary>
    public class SampleOps : IConverterOps
    {
        #region Constants
        public const int SAMPLE_FINE_RESOLUTION = 1000;
        public const int SAMPLE_COARSE_RESOLUTION = 10000;
        #endregion

        #region Properties
        ///// <inheritdoc />
        //public WaveSelectionMode Mode { get { return WaveSelectionMode.Sample; } }
        #endregion

        #region Public functions
        /// <inheritdoc />
        public int Snap(int sample, SnapType snap)
        {
            int snapped = snap switch
            {
                SnapType.Coarse => MathUtils.Clamp(sample, SAMPLE_COARSE_RESOLUTION, true),
                SnapType.Fine => MathUtils.Clamp(sample, SAMPLE_FINE_RESOLUTION, true),
                _ => sample,
            };
            return snapped;
        }

        /// <inheritdoc />
        public int Parse(string val)
        {
            if (!int.TryParse(val, out int sample))
            {
                sample = -1;
            }
            return sample;
        }

        /// <inheritdoc />
        public string Format(int sample)
        {
            return $"{sample}";
        }
        #endregion
    }
}
