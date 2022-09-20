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
    /// <summary>
    /// Functions to convert between the various time representation.
    /// There will be a certain inaccuracy with these conversions. Small-ish for AudioTime and msec conversions,
    /// worse for BarBeat. Such is the nature of things.
    /// 44.1 samples per msec == 0.0227 msec per sample
    /// </summary>
    public static class Converters
    {
        public const int SAMPLE_FINE_RESOLUTION = 1000;
        public const int SAMPLE_COARSE_RESOLUTION = 10000;

        /// <summary>
        /// Snap to closest neighbor.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="granularity">The neighbors increment.</param>
        /// <param name="round">Round or truncate.</param>
        /// <returns></returns>
        public static int Clamp(int val, int granularity, bool round) // TODO1 put in nbot.
        {
            int res = (val / granularity) * granularity;
            if (round && val % granularity > granularity / 2)
            {
                res += granularity;
            }
            return res;
        }

        /// <summary>
        /// Snap sample to 10000 or 1000 or none.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static int SnapSample(int sample, SnapType snap) // TODO1 prob just use Clamp()
        {
            int snapped = snap switch
            {
                SnapType.Coarse => Converters.Clamp(sample, SAMPLE_COARSE_RESOLUTION, true),
                SnapType.Fine => Converters.Clamp(sample, SAMPLE_FINE_RESOLUTION, true),
                _ => sample,
            };
            return snapped;
        }
    }
}
