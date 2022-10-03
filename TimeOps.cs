using NBagOfTricks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioLib
{
    /// <summary>Converters for audio time.</summary>
    public class TimeOps : IConverterOps
    {
        #region Constants
        internal const int MSEC_PER_SECOND = 1000;
        internal const int SEC_PER_MINUTE = 60;
        #endregion

        #region Properties
        ///// <inheritdoc />
        //public WaveSelectionMode Mode { get { return WaveSelectionMode.Time; } }
        #endregion

        #region Types
        /// <summary>Convenience container for internal use.</summary>
        struct TimeDesc
        {
            public int min;
            public int sec;
            public int msec;
            public TimeDesc(int min, int sec, int msec) { this.min = min; this.sec = sec; this.msec = msec; }
            public TimeDesc() { min = -1; sec = -1; msec = -1; }
            public bool Valid() { return min >= 0 && min < AudioLibDefs.MAX_CLIP_SIZE && sec >= 0 && sec < TimeOps.SEC_PER_MINUTE && msec >= 0 && msec < TimeOps.MSEC_PER_SECOND; }
        }
        #endregion

        #region Public functions
        /// <inheritdoc />
        public int Snap(int sample, SnapType snap)
        {
            var tmsec = SampleToMsec(sample);

            tmsec = snap switch
            {
                SnapType.Coarse => MathUtils.Clamp(tmsec, MSEC_PER_SECOND, true), // second
                SnapType.Fine => MathUtils.Clamp(tmsec, MSEC_PER_SECOND / 10, true), // tenth second
                _ => tmsec, // none
            };

            return MsecToSample(tmsec);
        }

        /// <inheritdoc />
        public int Parse(string input)
        {
            int sample = -1;

            int msec = ParseMsec(input);

            if (msec >= 0)
            {
                sample = MsecToSample(msec);
            }

            return sample;
        }

        /// <inheritdoc />
        public string Format(int sample)
        {
            var tm = SampleToTime(sample);
            return $"{tm.min:00}.{tm.sec:00}.{tm.msec:000}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msec"></param>
        /// <returns></returns>
        public int MsecToSample(double msec)
        {
            double sample = (double)AudioLibDefs.SAMPLE_RATE * msec / MSEC_PER_SECOND;
            return (int)sample;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public int SampleToMsec(int sample)
        {
            double msec = (double)MSEC_PER_SECOND * sample / AudioLibDefs.SAMPLE_RATE;
            return (int)Math.Round(msec);
        }
        #endregion

        #region Private functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        TimeDesc SampleToTime(int sample)
        {
            var tmsec = SampleToMsec(sample);
            return new(tmsec / (SEC_PER_MINUTE * MSEC_PER_SECOND) % SEC_PER_MINUTE, tmsec / MSEC_PER_SECOND % SEC_PER_MINUTE, tmsec % MSEC_PER_SECOND);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        int ParseMsec(string input)
        {
            int tmsec = -1;

            var tm = ParseTime(input);

            if (tm.Valid())
            {
                tmsec = tm.min * SEC_PER_MINUTE * MSEC_PER_SECOND + tm.sec * MSEC_PER_SECOND + tm.msec;
            }

            return tmsec;
        }

        /// <summary>
        /// Parser.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        TimeDesc ParseTime(string input)
        {
            TimeDesc tm = new();

            var parts = input.Split(".").ToList();
            while (parts.Count < 3) parts.Add(".0"); // pad

            if (parts.Count == 3)
            {
                if (int.TryParse(parts[0], out int min)) tm.min = min;
                if (int.TryParse(parts[1], out int sec)) tm.sec = sec;
                if (int.TryParse(parts[2], out int msec)) tm.msec = msec;
            }

            return tm;
        }
        #endregion
    }
}
