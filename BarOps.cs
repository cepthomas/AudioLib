using System;
using System.Collections.Generic;
using System.Linq;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib
{
    /// <summary>Converters for musical time. 0-based not traditional 1-based.</summary>
    public class BarOps : IConverterOps
    {
        #region Constants
        const int SUBDIVS_PER_BEAT = 100;
        const int BEATS_PER_BAR = 4;
        const int SUBDIVS_PER_BAR = SUBDIVS_PER_BEAT * BEATS_PER_BAR;
        #endregion

        #region Properties
        ///// <inheritdoc />
        //public WaveSelectionMode Mode { get { return WaveSelectionMode.Bar; } }
        #endregion

        #region Types
        /// <summary>Convenience container for internal use.</summary>
        struct BarDesc
        {
            public int bar;
            public int beat;
            public int subdiv;
            public BarDesc(int bar, int beat, int subdiv) { this.bar = bar; this.beat = beat; this.subdiv = subdiv; }
            public BarDesc() { bar = -1; beat = -1; subdiv = -1; }
            public bool Valid() { return bar >= 0 && bar < 1000 && beat >= 0 && beat < BEATS_PER_BAR && subdiv >= 0 && subdiv < SUBDIVS_PER_BEAT; }
        }
        #endregion

        #region Public functions
        /// <inheritdoc />
        public int Snap(int sample, SnapType snap)
        {
            var subdiv = SampleToSubdiv(sample);

            subdiv = snap switch
            {
                SnapType.Coarse => MathUtils.Clamp(subdiv, BEATS_PER_BAR * SUBDIVS_PER_BEAT, true),
                SnapType.Fine => MathUtils.Clamp(subdiv, SUBDIVS_PER_BEAT, true),
                _ => subdiv, // none
            };

            return SubdivToSample(subdiv);
        }

        /// <inheritdoc />
        public int Parse(string input)
        {
            int sample = -1;

            var subdiv = TextToSubdiv(input);

            if (subdiv >= 0)
            {
                sample = SubdivToSample(subdiv);
            }

            return sample;
        }

        /// <inheritdoc />
        public string Format(int sample)
        {
            var bb = SampleToBar(sample);
            return $"{bb.bar}.{bb.beat:0}.{bb.subdiv:00}";
        }
        #endregion

        #region Private functions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        int TextToSubdiv(string input)
        {
            int subdiv = -1;

            var bb = ParseBar(input);

            if (bb.Valid())
            {
                subdiv = bb.bar * SUBDIVS_PER_BAR + bb.beat * SUBDIVS_PER_BEAT + bb.subdiv;
            }

            return subdiv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subdiv"></param>
        /// <returns></returns>
        int SubdivToSample(int subdiv)
        {
            double minPerBeat = 1.0 / Globals.BPM;
            double secPerBeat = minPerBeat * 60;
            double smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            double smplPerSubdiv = smplPerBeat / SUBDIVS_PER_BEAT;
            var sample = (int)(smplPerSubdiv * subdiv);
            return sample;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        int SampleToSubdiv(int sample)
        {
            double minPerBeat = 1.0 / Globals.BPM;
            double secPerBeat = minPerBeat * 60;
            double smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            double beats = sample / smplPerBeat;
            var subdiv = (int)Math.Round(beats * SUBDIVS_PER_BEAT);
            return subdiv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        BarDesc SampleToBar(int sample)
        {
            var subdiv = SampleToSubdiv(sample);

            int bar = subdiv / SUBDIVS_PER_BAR;
            int beat = (subdiv - (bar * SUBDIVS_PER_BAR)) / SUBDIVS_PER_BEAT;
            int ssubdiv = subdiv % SUBDIVS_PER_BEAT;

            return new(bar, beat, ssubdiv);
        }

        /// <summary>
        /// Parser.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        BarDesc ParseBar(string input)
        {
            BarDesc bb = new();

            var parts = input.Split(".").ToList();
            while (parts.Count < 3) parts.Add(".0"); // pad

            if (parts.Count == 3)
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
