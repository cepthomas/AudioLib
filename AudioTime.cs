using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NBagOfTricks;


namespace AudioLib
{
    public class AudioTime// : IComparable
    {
        #region Fields
        /// <summary>For hashing.</summary>
        readonly int _id;

        /// <summary>Increment for unique value.</summary>
        static int _all_ids = 1;
        #endregion

        #region Properties
        /// <summary>Common unit.</summary>
        public static readonly AudioTime Zero = new();

        /// <summary>Minutes part of time.</summary>
        public int Minutes { get { return TotalMilliseconds / 60000 % 60; } }

        /// <summary>Seconds part of time.</summary>
        public int Seconds { get { return TotalMilliseconds / 1000 % 60; } }

        /// <summary>Milliseconds part of time.</summary>
        public int Milliseconds { get { return TotalMilliseconds % 1000; } }
 
        /// <summary>Primary value.</summary>
        public int TotalMilliseconds { get; private set; } = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public AudioTime()
        {
            _id = _all_ids++;
        }

        /// <summary>
        /// Constructor from milliseconds.
        /// </summary>
        /// <param name="msec"></param>
        public AudioTime(float msec)
        {
            TotalMilliseconds = (int)(Math.Round(msec));
            _id = _all_ids++;
        }

        /// <summary>
        /// Constructor from discrete elements.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="sec"></param>
        /// <param name="msec"></param>
        public AudioTime(int min, int sec, int msec)
        {
            TotalMilliseconds = min * 60 * 1000 + sec * 1000 + msec;
            _id = _all_ids++;
        }

        /// <summary>
        /// Constructor from sample.
        /// </summary>
        /// <param name="sample"></param>
        public AudioTime(int sample)
        {
            double msec = 1000D * sample / AudioLibDefs.SAMPLE_RATE;
            TotalMilliseconds = (int)Math.Round(msec);
            _id = _all_ids++;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Snap to neighbor.
        /// </summary>
        /// <param name="snap"></param>
        public void Snap(SnapType snap)
        {
            TotalMilliseconds = snap switch
            {
                SnapType.Coarse => Converters.Clamp(TotalMilliseconds, 1000, true), // second
                SnapType.Fine => Converters.Clamp(TotalMilliseconds, 100, true), // tenth second
                _ => TotalMilliseconds, // none
            };
        }

        /// <summary>
        /// Convert to equivalent sample.
        /// </summary>
        /// <returns></returns>
        public int ToSample()
        {
            double sample = (double)AudioLibDefs.SAMPLE_RATE * TotalMilliseconds / 1000D;
            return (int)sample;
        }

        /// <summary>
        /// Convert to equivalent TimeSpan.
        /// </summary>
        /// <returns></returns>
        public TimeSpan ToTimeSpan()
        {
            return new(0, 0, 0, 0, TotalMilliseconds);
        }

        /// <summary>
        /// Convert from string form. mm.ss.fff
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Object or null if invalid input.</returns>
        public static AudioTime? Parse(string input)
        {
            AudioTime? tm = null;

            var parts = input.Split(".");
            if(parts.Length == 3)
            {
                int p0 = -1;
                int p1 = -1;
                int p2 = -1;

                int.TryParse(parts[0], out p0);
                int.TryParse(parts[1], out p1);
                int.TryParse(parts[2], out p2);

                if(p0 >= 0 && p0 < 60 && p1 >= 0 && p1 < 60 && p2 >= 0 && p2 < 1000)
                {
                    tm = new AudioTime(p0, p1, p2);
                }
            }

            return tm;
        }

        /// <summary>
        /// Human readable.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // TS_FORMAT = @"mm\:ss\.fff";
            return $"{Minutes:00}.{Seconds:00}.{Milliseconds:000}";
        }
        #endregion

        //#region Standard IComparable stuff
        //public override bool Equals(object? obj)
        //{
        //    return obj is not null && obj is AudioTime tm && tm.TotalMilliseconds == TotalMilliseconds;
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

        //    AudioTime? other = obj as AudioTime;
        //    if (other is not null)
        //    {
        //        return TotalMilliseconds.CompareTo(other.TotalMilliseconds);
        //    }
        //    else
        //    {
        //        throw new ArgumentException("Object is not a BarSpan");
        //    }
        //}

        //public static bool operator ==(AudioTime a, AudioTime b)
        //{
        //    return a.TotalMilliseconds == b.TotalMilliseconds;
        //}

        //public static bool operator !=(AudioTime a, AudioTime b)
        //{
        //    return !(a == b);
        //}

        //public static AudioTime operator +(AudioTime a, AudioTime b)
        //{
        //    return new AudioTime(a.TotalMilliseconds + b.TotalMilliseconds);
        //}

        //public static AudioTime operator -(AudioTime a, AudioTime b)
        //{
        //    return new AudioTime(a.TotalMilliseconds - b.TotalMilliseconds);
        //}

        //public static bool operator <(AudioTime a, AudioTime b)
        //{
        //    return a.TotalMilliseconds < b.TotalMilliseconds;
        //}

        //public static bool operator >(AudioTime a, AudioTime b)
        //{
        //    return a.TotalMilliseconds > b.TotalMilliseconds;
        //}

        //public static bool operator <=(AudioTime a, AudioTime b)
        //{
        //    return a.TotalMilliseconds <= b.TotalMilliseconds;
        //}

        //public static bool operator >=(AudioTime a, AudioTime b)
        //{
        //    return a.TotalMilliseconds >= b.TotalMilliseconds;
        //}
        //#endregion
    }
}
