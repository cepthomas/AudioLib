using System;
using System.Collections.Generic;


namespace AudioLib
{
    /// <summary>Converters for musical time. 0-based not traditional 1-based.</summary>
    public static class BarBeatOps
    {
        #region Constants
        const int SUBDIVS_PER_BEAT = 100;
        const int BEATS_PER_BAR = 4;
        const int SUBDIVS_PER_BAR = SUBDIVS_PER_BEAT * BEATS_PER_BAR;
        #endregion

        #region Constants
        /// <summary>User sets this for calculations.</summary>
        public static float BPM { get; set; }
        #endregion

        #region Types
        /// <summary>Convenience container for internal use.</summary>
        struct BarBeatDesc
        {
            public int bar;
            public int beat;
            public int subdiv;
            public BarBeatDesc(int bar, int beat, int subdiv) { this.bar = bar; this.beat = beat; this.subdiv = subdiv; }
            public BarBeatDesc() { bar = -1; beat = -1; subdiv = -1; }
            public bool Valid() { return bar >= 0 && bar < 1000 && beat >= 0 && beat < BEATS_PER_BAR && subdiv >= 0 && subdiv < SUBDIVS_PER_BEAT; }
        }
        #endregion

        #region Public functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subdiv"></param>
        /// <returns></returns>
        public static int SubdivToSample(int subdiv)
        {
            float minPerBeat = 1.0f / BPM;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            float smplPerSubdiv = smplPerBeat / SUBDIVS_PER_BEAT;
            var sample = (int)(smplPerSubdiv * subdiv);
            return sample;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static int SampleToSubdiv(int sample)
        {
            float minPerBeat = 1.0f / BPM;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            float beats = sample / smplPerBeat;
            var subdiv = (int)Math.Round(beats * SUBDIVS_PER_BEAT);
            return subdiv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        public static int SnapSample(int sample, SnapType snap)
        {
            var subdiv = SampleToSubdiv(sample);

            subdiv = snap switch
            {
                SnapType.Coarse => Converters.Clamp(subdiv, BEATS_PER_BAR * SUBDIVS_PER_BEAT, true),
                SnapType.Fine => Converters.Clamp(subdiv, SUBDIVS_PER_BEAT, true),
                _ => subdiv, // none
            };

            return SubdivToSample(subdiv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int TextToSample(string input)
        {
            int sample = -1;

            var subdiv = TextToSubdiv(input);

            if (subdiv >= 0)
            {
                sample = SubdivToSample(subdiv);
            }

            return sample;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int TextToSubdiv(string input)
        {
            int subdiv = -1;

            var bb = TextToBarBeat(input);

            if (bb.Valid())
            {
                subdiv = bb.bar * SUBDIVS_PER_BAR + bb.beat * SUBDIVS_PER_BEAT + bb.subdiv;
            }

            return subdiv;
        }

        /// <summary>
        /// Human readable.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public static string Format(int sample)
        {
            var bb = SampleToBarBeat(sample);
            return $"{bb.bar}.{bb.beat:0}.{bb.subdiv:00}";
        }
        #endregion

        #region Private functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        static BarBeatDesc SampleToBarBeat(int sample)
        {
            var subdiv = SampleToSubdiv(sample);

            int bar = subdiv / SUBDIVS_PER_BAR;
            int beat = (subdiv - (bar * SUBDIVS_PER_BAR)) / SUBDIVS_PER_BEAT;
            int ssubdiv = subdiv % SUBDIVS_PER_BEAT;

            return new(bar, beat, ssubdiv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static BarBeatDesc TextToBarBeat(string input)
        {
            BarBeatDesc bb = new();

            var parts = input.Split(".");
            if (parts.Length == 3)
            {
                if (int.TryParse(parts[0], out int bar)) bb.bar = bar;
                if (int.TryParse(parts[1], out int beat)) bb.beat = beat;
                if (int.TryParse(parts[2], out int subdiv)) bb.subdiv = subdiv;
            }

            return bb;
        }
        #endregion
    }
}
