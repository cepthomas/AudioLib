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
    public class AudioTime : IComparable
    {
        #region Fields
        /// <summary>For hashing.</summary>
        readonly int _id;

        /// <summary>Increment for unique value.</summary>
        static int _all_ids = 1;

        /// <summary>The behind object. TODO1?</summary>
        TimeSpan _timeSpan = new(0);
        #endregion

        #region Properties
        public static readonly AudioTime Zero = new();
        public int TotalMilliseconds { get { return (int)Math.Round(_timeSpan.TotalMilliseconds); } }
        #endregion

        public AudioTime()
        {
            _id = _all_ids++;
        }

        public AudioTime(int msec)
        {
            _timeSpan = new(0, 0, 0, 0, msec);
            _id = _all_ids++;
        }

        public AudioTime(float seconds)
        {
            _timeSpan = TimeSpan.FromSeconds(seconds);
            _id = _all_ids++;
        }

        public override string ToString()
        {
            return _timeSpan.ToString(AudioLibDefs.TS_FORMAT);
        }

        // TODO1 AudioTime.TryParse(string? input);


        #region Standard IComparable stuff
        public override bool Equals(object? obj)
        {
            return obj is not null && obj is AudioTime tm && tm.TotalMilliseconds == TotalMilliseconds;
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                throw new ArgumentException("Object is null");
            }

            AudioTime? other = obj as AudioTime;
            if (other is not null)
            {
                return TotalMilliseconds.CompareTo(other.TotalMilliseconds);
            }
            else
            {
                throw new ArgumentException("Object is not a BarSpan");
            }
        }

        public static bool operator ==(AudioTime a, AudioTime b)
        {
            return a.TotalMilliseconds == b.TotalMilliseconds;
        }

        public static bool operator !=(AudioTime a, AudioTime b)
        {
            return !(a == b);
        }

        public static AudioTime operator +(AudioTime a, AudioTime b)
        {
            return new AudioTime(a.TotalMilliseconds + b.TotalMilliseconds);
        }

        public static AudioTime operator -(AudioTime a, AudioTime b)
        {
            return new AudioTime(a.TotalMilliseconds - b.TotalMilliseconds);
        }

        public static bool operator <(AudioTime a, AudioTime b)
        {
            return a.TotalMilliseconds < b.TotalMilliseconds;
        }

        public static bool operator >(AudioTime a, AudioTime b)
        {
            return a.TotalMilliseconds > b.TotalMilliseconds;
        }

        public static bool operator <=(AudioTime a, AudioTime b)
        {
            return a.TotalMilliseconds <= b.TotalMilliseconds;
        }

        public static bool operator >=(AudioTime a, AudioTime b)
        {
            return a.TotalMilliseconds >= b.TotalMilliseconds;
        }
        #endregion
    }
}
