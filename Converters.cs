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
        /// <summary>
        /// Snap to closest neighbor.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="granularity">The neighbors increment.</param>
        /// <param name="round">Round or truncate.</param>
        /// <returns></returns>
        public static int Clamp(int val, int granularity, bool round) // TODO put in nbot.
        {
            int res = (val / granularity) * granularity;
            if (round && val % granularity > granularity / 2)
            {
                res += granularity;
            }
            return res;
        }
    }
}
