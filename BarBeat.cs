using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>Container for musical time. Internally 0-based but traditional 1-based for the user.</summary>
    public class BarBeat// : IComparable
    {
        #region Fields
        /// <summary>For hashing.</summary>
        readonly int _id;

        /// <summary>Increment for unique value.</summary>
        static int _all_ids = 1;

        /// <summary>Constant.</summary>
        const int SUBDIVS_PER_BEAT = 100;

        /// <summary>Constant.</summary>
        const int BEATS_PER_BAR = 4;

        /// <summary>Constant.</summary>
        const int SUBDIVS_PER_BAR = SUBDIVS_PER_BEAT * BEATS_PER_BAR;
        #endregion

        #region Properties
        /// <summary>Common unit.</summary>
        public static readonly BarBeat Zero = new();

        /// <summary>0 to N.</summary>
        public int Bar { get { return TotalSubdivs / SUBDIVS_PER_BAR; } }

        /// <summary>0 to 3.</summary>
        public int Beat { get { return (TotalSubdivs - Bar * SUBDIVS_PER_BAR) / SUBDIVS_PER_BEAT; } }

        /// <summary>0 to BEAT_PARTS-1.</summary>
        public int Subdiv { get { return TotalSubdivs % SUBDIVS_PER_BEAT; } }
 
        /// <summary>Primary value.</summary>
        public int TotalSubdivs { get; private set; } = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public BarBeat()
        {
            TotalSubdivs = 0;
            _id = _all_ids++;
        }

        /// <summary>
        /// Constructor from total subdivs.
        /// </summary>
        /// <param name="subdivs">Total subdivs.</param>
        public BarBeat(int subdivs)
        {
            TotalSubdivs = subdivs;
            _id = _all_ids++;
        }

        /// <summary>
        /// Constructor from discrete elements. 0-based.
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="beat"></param>
        /// <param name="subdiv"></param>
        public BarBeat(int bar, int beat, int subdiv)
        {
            TotalSubdivs = bar * SUBDIVS_PER_BAR + beat * SUBDIVS_PER_BEAT + subdiv;
            _id = _all_ids++;
        }

        /// <summary>
        /// Constructor from samples.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="bpm"></param>
        public BarBeat(int sample, float bpm)
        {
            // Convert then snap.
            float minPerBeat = 1.0f / bpm;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            float beats = sample / smplPerBeat;

            TotalSubdivs = (int)Math.Round(beats * SUBDIVS_PER_BEAT);
            _id = _all_ids++;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Snap bar or beat or none.
        /// </summary>
        /// <param name="snap"></param>
        public void Snap(SnapType snap)
        {
            switch (snap)
            {
                case SnapType.None:
                    // no adjust
                    break;

                case SnapType.Coarse: // at bar
                    TotalSubdivs = Converters.Clamp(TotalSubdivs, BEATS_PER_BAR * SUBDIVS_PER_BEAT, true);
                    break;

                case SnapType.Fine: // at beat
                    TotalSubdivs = Converters.Clamp(TotalSubdivs, SUBDIVS_PER_BEAT, true);
                    break;
            }
        }

        /// <summary>
        /// Convert to samples
        /// </summary>
        /// <param name="bpm"></param>
        /// <returns></returns>
        public int ToSample(float bpm)
        {
            float minPerBeat = 1.0f / bpm;
            float secPerBeat = minPerBeat * 60;
            float smplPerBeat = AudioLibDefs.SAMPLE_RATE * secPerBeat;
            float smplPerSubdiv = smplPerBeat / SUBDIVS_PER_BEAT;
            var res = (int)(smplPerSubdiv * TotalSubdivs);
            return res;
        }

        /// <summary>
        /// Convert from string form. 123.3.99
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Object or null if invalid input.</returns>
        public static BarBeat? Parse(string input)
        {
            BarBeat? bb = null;

            var parts = input.Split(".");
            if (parts.Length == 3)
            {
                int p0 = -1;
                int p1 = -1;
                int p2 = -1;

                int.TryParse(parts[0], out p0);
                int.TryParse(parts[1], out p1);
                int.TryParse(parts[2], out p2);

                if (p0 >= 0 && p1 >= 0 && p1 < 4 && p2 >= 0 && p2 < 100)
                {
                    bb = new BarBeat(p0, p1, p2);
                }
            }

            return bb;
        }

        /// <summary>
        /// Make readable. 1-based.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // 123.3.99
            return $"{Bar}.{Beat}.{Subdiv:00}";
            //return $"{Bar + 1}.{Beat + 1}.{Subdiv:00}";
        }
        #endregion

        //#region Standard IComparable stuff
        //public override bool Equals(object? obj)
        //{
        //    return obj is not null && obj is BarBeat tm && tm.TotalSubdivs == TotalSubdivs;
        //}

        //public override int GetHashCode()
        //{
        //    return _id;
        //}

        //public int CompareTo(object? obj)
        //{
        //    if (obj is null)
        //    {
        //        throw new ArgumentException("Object is null");
        //    }

        //    BarBeat? other = obj as BarBeat;
        //    if (other is not null)
        //    {
        //        return TotalSubdivs.CompareTo(other.TotalSubdivs);
        //    }
        //    else
        //    {
        //        throw new ArgumentException("Object is not a BarSpan");
        //    }
        //}

        //public static bool operator ==(BarBeat a, BarBeat b)
        //{
        //    return a.TotalSubdivs == b.TotalSubdivs;
        //}

        //public static bool operator !=(BarBeat a, BarBeat b)
        //{
        //    return !(a == b);
        //}

        //public static BarBeat operator +(BarBeat a, BarBeat b)
        //{
        //    return new BarBeat(a.TotalSubdivs + b.TotalSubdivs);
        //}

        //public static BarBeat operator -(BarBeat a, BarBeat b)
        //{
        //    return new BarBeat(a.TotalSubdivs - b.TotalSubdivs);
        //}

        //public static bool operator <(BarBeat a, BarBeat b)
        //{
        //    return a.TotalSubdivs < b.TotalSubdivs;
        //}

        //public static bool operator >(BarBeat a, BarBeat b)
        //{
        //    return a.TotalSubdivs > b.TotalSubdivs;
        //}

        //public static bool operator <=(BarBeat a, BarBeat b)
        //{
        //    return a.TotalSubdivs <= b.TotalSubdivs;
        //}

        //public static bool operator >=(BarBeat a, BarBeat b)
        //{
        //    return a.TotalSubdivs >= b.TotalSubdivs;
        //}
        //#endregion
    }
}
