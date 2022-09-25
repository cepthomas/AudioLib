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
        public WaveSelectionMode SelectionMode { get { return WaveSelectionMode.Sample; } }
        #endregion

        #region Public functions
        /// <summary>
        /// Snap sample to 10000 or 1000 or none.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public int SnapSample(int sample, SnapType snap)
        {
            int snapped = snap switch
            {
                SnapType.Coarse => MathUtils.Clamp(sample, SAMPLE_COARSE_RESOLUTION, true),
                SnapType.Fine => MathUtils.Clamp(sample, SAMPLE_FINE_RESOLUTION, true),
                _ => sample,
            };
            return snapped;
        }

        /// <summary>
        /// Parse a sample text.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int TextToSample(string val)
        {
            if (!int.TryParse(val, out int sample))
            {
                sample = -1;
            }
            return sample;
        }

        /// <summary>
        /// Human readable.
        /// </summary>
        /// <returns></returns>
        public string Format(int sample)
        {
            return $"{sample}";
        }
        #endregion
    }
}
