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
    public static class SampleOps
    {
        #region Constants
        public const int SAMPLE_FINE_RESOLUTION = 1000;
        public const int SAMPLE_COARSE_RESOLUTION = 10000;
        #endregion

        #region Public functions
        /// <summary>
        /// Parse a sample text.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static int TextToSample(string val)
        {
            int sample;
            if (!int.TryParse(val, out sample))
            {
                sample = -1;
            }
            return sample;
        }

        /// <summary>
        /// Snap sample to 10000 or 1000 or none.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static int SnapSample(int sample, SnapType snap)
        {
            int snapped = snap switch
            {
                SnapType.Coarse => Converters.Clamp(sample, SAMPLE_COARSE_RESOLUTION, true),
                SnapType.Fine => Converters.Clamp(sample, SAMPLE_FINE_RESOLUTION, true),
                _ => sample,
            };
            return snapped;
        }
        #endregion
    }
}
