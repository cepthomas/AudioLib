using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    #region Types
    /// <summary>Player state.</summary>
    public enum AudioState { Stopped = 0, Playing = 1, Complete = 2 }

    public class VolumeDefs
    {
        public const double MIN = 0.0;
        public const double MAX = 2.0;
        public const double DEFAULT = 0.8;
        public const double RESOLUTION = 0.1;
    }
    #endregion
}
