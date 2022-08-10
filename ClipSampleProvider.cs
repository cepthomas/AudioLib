using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>
    /// Provider that encapsulates a client supplied audio data subset.
    /// Supplies some basic editing:
    ///   - Gain envelope.
    ///   - Gain overall.
    /// </summary>
    public sealed class ClipSampleProvider : ISampleProvider
    {
        /// <summary>How to handle stereo files.</summary>
        public enum StereoCoerce { Left, Right, Mono }

        #region Fields
        /// <summary>The full buffer from client.</summary>
        readonly float[] _vals = Array.Empty<float>();

        /// <summary>Make this look like a streaam.</summary>
        int _currentIndex = 0;

        /// <summary>Gain while iterating samples.</summary>
        readonly double _currentGain = 1.0f;

        /// <summary>The lock() target.</summary>
        readonly object _locker = new();

        /// <summary>Piecewise gain envelope. Key is index, value is gain.</summary>
        readonly Dictionary<int, double> _envelope = new();
        #endregion

        #region Properties
        /// <summary>The WaveFormat of this sample provider. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

        /// <summary>The associated file name. Empty if new.</summary>
        public string FileName { get; }

        /// <summary>Overall gain applied to all samples.</summary>
        public double MasterGain { get; set; } = 1.0f;

        /// <summary>Length of the clip in samples.</summary>
        public int Length { get { return _vals.Length; } }

        ///// <summary>Length of the clip in seconds.</summary>
        //public Duration { get; init; } = 123; // TODO

        /// <summary>Position of the simulated stream as sample index.</summary>
        public int Position
        {
            get { return _currentIndex; }
            set { lock (_locker) { _currentIndex = MathUtils.Constrain(value, 0, _vals.Length - 1); }  }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor from a sample provider.
        /// </summary>
        /// <param name="provider">Format to use.</param>
        public ClipSampleProvider(ISampleProvider provider)
        {
            AudioUtils.ValidateFormat(provider.WaveFormat, true);
            FileName = "";
            _vals = AudioUtils.ReadAll(provider);
        }

        /// <summary>
        /// Constructor from a buffer.
        /// </summary>
        /// <param name="waveFormat">Format to use.</param>
        /// <param name="vals">The data to use.</param>
        /// <param name="fn">Maybe associated filename.</param>
        public ClipSampleProvider(WaveFormat waveFormat, float[] vals, string fn = "")
        {
            AudioUtils.ValidateFormat(waveFormat, true);
            FileName = fn;
            _vals = vals;
        }

        /// <summary>
        /// Constructor from a file. Deals with stereo files.
        /// </summary>
        /// <param name="fn">File to use.</param>
        /// <param name="mode">How to handle stereo files.</param>
        public ClipSampleProvider(string fn, StereoCoerce mode = StereoCoerce.Mono)
        {
            FileName = fn;

            using var reader = new AudioFileReader(fn);
            ISampleProvider prov = new AudioFileReader(fn);

            if (prov.WaveFormat.Channels == 2)
            {
                prov = new StereoToMonoSampleProvider(prov)
                {
                    LeftVolume = mode == StereoCoerce.Mono ? 0.5f : (mode == StereoCoerce.Left ? 1.0f : 0.0f),
                    RightVolume = mode == StereoCoerce.Mono ? 0.5f : (mode == StereoCoerce.Right ? 1.0f : 0.0f)
                };
            }

            _vals = AudioUtils.ReadAll(prov);
        }
        #endregion

        #region Public
        /// <summary>
        /// Reads samples from this sample provider with adjustments for envelope and overall gain.
        /// ISampleProvider implementation.
        /// </summary>
        /// <param name="buffer">Sample buffer.</param>
        /// <param name="offset">Offset into buffer.</param>
        /// <param name="count">Number of samples required.</param>
        /// <returns>Number of samples read.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int numRead = 0;

            // Get the source vals.
            lock (_locker)
            {
                int numToRead = Math.Min(count, _vals.Length - _currentIndex);

                if(_envelope.Count > 0)
                {
                    // Make an ordered copy of the _envelope point locations.
                    List<int> envLocs = _envelope.Keys.ToList();
                    envLocs.Sort();

                    // Find where offset is currently.
                    var loc = envLocs.Where(l => l > offset).FirstOrDefault();

                    if(loc != 0)
                    {
                        loc -= 1;
                    }

                    double envGain = _envelope[envLocs[loc]]; // default;

                    for (int n = 0; n < numToRead; n++)
                    {
                        if(_envelope.ContainsKey(n))
                        {
                            // Update env gain.
                            envGain = _envelope[n];
                        }
                        buffer[n + offset] = (float)(_vals[n] * envGain * MasterGain);
                    }
                }
                else
                {
                    // Simply adjust for master gain.
                    for (int n = 0; n < numToRead; n++)
                    {
                        buffer[n + offset] = (float)(_vals[_currentIndex] * MasterGain);
                        _currentIndex++;
                        numRead++;
                    }
                }
            }

            return numRead;
        }

        public void AddGain(int sampleIndex, double gain)
        {
            // if in _envelope, update else add.
        }

        public void RemoveGain(int sampleIndex)
        {
            // if in _envelope, remove.
        }
        #endregion
    }
}