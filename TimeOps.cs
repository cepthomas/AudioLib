using System;
using System.Collections.Generic;


namespace AudioLib
{
    /// <summary>Converters for audio time.</summary>
    public static class TimeOps
    {
        #region Constants
        internal const int MSEC_PER_SECOND = 1000;
        internal const int SEC_PER_MINUTE = 60;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msec"></param>
        /// <returns></returns>
        public static int MsecToSample(float msec)
        {
            double sample = (double)AudioLibDefs.SAMPLE_RATE * msec / MSEC_PER_SECOND;
            return (int)sample;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static int SampleToMsec(int sample)
        {
            double msec = (double)MSEC_PER_SECOND * sample / AudioLibDefs.SAMPLE_RATE;
            return (int)Math.Round(msec);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static int SnapSample(int sample, SnapType snap)
        {
            var tmsec = SampleToMsec(sample);

            tmsec = snap switch
            {
                SnapType.Coarse => Converters.Clamp(tmsec, MSEC_PER_SECOND, true), // second
                SnapType.Fine => Converters.Clamp(tmsec, MSEC_PER_SECOND / 10, true), // tenth second
                _ => tmsec, // none
            };

            return MsecToSample(tmsec);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int TextToSample(string input)
        {
            int sample = -1;

            int msec = TextToMsec(input);

            if (msec >= 0)
            {
                sample = MsecToSample(msec);
            }

            return sample;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int TextToMsec(string input)
        {
            int tmsec = -1;

            var tm = TextToTime(input);

            if (tm.Valid())
            {
                tmsec = tm.min * SEC_PER_MINUTE * MSEC_PER_SECOND + tm.sec * MSEC_PER_SECOND + tm.msec;
            }

            return tmsec;
        }

        /// <summary>
        /// Human readable.
        /// </summary>
        /// <returns></returns>
        public static string Format(int sample)
        {
            var tm = SampleToTime(sample);
            return $"{tm.min:00}.{tm.sec:00}.{tm.msec:000}";
        }
        #endregion

        #region Private functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        static TimeDesc SampleToTime(int sample)
        {
            var tmsec = SampleToMsec(sample);
            return new(tmsec / (SEC_PER_MINUTE * MSEC_PER_SECOND) % SEC_PER_MINUTE, tmsec / MSEC_PER_SECOND % SEC_PER_MINUTE, tmsec % MSEC_PER_SECOND);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static TimeDesc TextToTime(string input)
        {
            TimeDesc tm = new();

            var parts = input.Split(".");
            if (parts.Length == 3)
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
