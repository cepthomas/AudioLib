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
    /// Mono output only - coerces stereo input per client call. Can be used for splitting stereo files.
    /// If you need stereo, use AudioFileReader.
    /// Supplies some basic editing:
    ///   - Gain envelope.
    ///   - Gain overall.
    /// </summary>
    public class ClipSampleProvider : ISampleProvider
    {
        #region Fields
        /// <summary>The full buffer from client.</summary>
        float[] _vals = Array.Empty<float>();

        /// <summary>Make this look like a streaam.</summary>
        int _currentIndex = 0;

        /// <summary>Gain while iterating samples.</summary>
        readonly double _currentGain = 1.0f;

        /// <summary>The lock() target.</summary>
        readonly object _locker = new();

        /// <summary>Piecewise gain envelope. Key is index, value is gain. TODO all</summary>
        readonly Dictionary<int, double> _envelope = new();
        #endregion

        #region Properties
        /// <summary>The WaveFormat of this sample provider. Fixed to mono. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

        /// <summary>The associated file name. Empty if new.</summary>
        public string FileName { get; }

        /// <summary>Overall gain applied to all samples.</summary>
        public double MasterGain { get; set; } = 1.0f;

        /// <summary>Length of the clip in samples.</summary>
        public int Length { get { return _vals.Length; } }

        /// <summary>Length of the clip in seconds.</summary>
        public TimeSpan TotalTime { get { return TimeSpan.FromSeconds((double)Length / WaveFormat.SampleRate); } }

        /// <summary>Position of the simulated stream as sample index.</summary>
        public int Position
        {
            get { return _currentIndex; }
            set { lock (_locker) { _currentIndex = MathUtils.Constrain(value, 0, _vals.Length - 1); }  }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor from a sample provider. Coerces stereo to mono.
        /// </summary>
        /// <param name="source">Source provider to use.</param>
        /// <param name="mode">How to handle stereo files.</param>
        public ClipSampleProvider(ISampleProvider source, StereoCoercion mode)
        {
            FileName = "";
            ReadSource(source, mode);
        }

        /// <summary>
        /// Constructor from a buffer. Mono only.
        /// </summary>
        /// <param name="vals">The data to use.</param>
        /// <param name="fn">Maybe associated filename.</param>
        public ClipSampleProvider(float[] vals, string fn = "")
        {
            FileName = fn;
            _vals = vals;
        }

        /// <summary>
        /// Constructor from a file. Coerces stereo to mono.
        /// </summary>
        /// <param name="fn">File to use.</param>
        /// <param name="mode">How to handle stereo files.</param>
        public ClipSampleProvider(string fn, StereoCoercion mode)
        {
            FileName = fn;
            using var prov = new AudioFileReader(fn);
            ReadSource(prov, mode);

            using (var reader = new AudioFileReader(fn))
            {
                var resampler = new WdlResamplingSampleProvider(reader, 44100);

                //WaveFileWriter.CreateWaveFile16(outFile, resampler);
            }

        }
        #endregion

        #region Public functions
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
        #endregion


        public void AddGain(int sampleIndex, double gain)
        {
            // if in _envelope, update else add.
        }

        public void RemoveGain(int sampleIndex)
        {
            // if in _envelope, remove.
        }


        #region Private
        /// <summary>
        /// Common buff loader. Coerces stereo to mono per client request.
        /// </summary>
        /// <param name="source">Source provider to use.</param>
        /// <param name="mode">How to handle stereo files.</param>
        void ReadSource(ISampleProvider source, StereoCoercion mode)
        {
            source.Validate(false);

            if (source.WaveFormat.Channels == 2)
            {
                source = new StereoToMonoSampleProvider(source)
                {
                    LeftVolume = mode == StereoCoercion.Mono ? 0.5f : (mode == StereoCoercion.Left ? 1.0f : 0.0f),
                    RightVolume = mode == StereoCoercion.Mono ? 0.5f : (mode == StereoCoercion.Right ? 1.0f : 0.0f)
                };
            }

            _vals = NAudioEx.ReadAll(source);
        }
        #endregion
    }
}