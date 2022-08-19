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
    /// Provider that encapsulates a client supplied audio data subset. When constructed, it reads in the
    /// entire file. Does sample rate conversion if needed.
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

        /// <summary>Make this class look like a streaam.</summary>
        int _position = 0;

        /// <summary>Gain while iterating samples.</summary>
        float _currentGain = 1.0f;

        /// <summary>The lock() target.</summary>
        readonly object _locker = new();

        /// <summary>Piecewise gain envelope. Key is index, value is gain.</summary>
        readonly Dictionary<int, float> _envelope = new();
        #endregion

        #region Properties
        /// <summary>The WaveFormat of this sample provider. Fixed to mono. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibDefs.SAMPLE_RATE, 1);

        /// <summary>The associated file name. Empty if new.</summary>
        public string FileName { get; }

        /// <summary>Overall gain applied to all samples.</summary>
        public float MasterGain { get; set; } = 1.0f;

        /// <summary>Length of the clip in samples.</summary>
        public int Length { get { return _vals.Length; } }

        /// <summary>Length of the clip in seconds.</summary>
        public TimeSpan TotalTime { get { return TimeSpan.FromSeconds((double)Length / WaveFormat.SampleRate); } }

        /// <summary>Position of the simulated stream as sample index.</summary>
        public int Position
        {
            get { return _position; }
            set { lock (_locker) { _position = MathUtils.Constrain(value, 0, _vals.Length - 1); GetEnvelopeGain(_position); } }
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
        public ClipSampleProvider(float[] vals)
        {
            FileName = "";
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
                int numToRead = Math.Min(count, _vals.Length - _position);

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
                        buffer[n + offset] = (float)(_vals[_position] * MasterGain);
                        _position++;
                        numRead++;
                    }
                }
            }

            return numRead;
        }

        /// <summary>
        /// If in _envelope, update else add. If 0, remove.
        /// </summary>
        /// <param name="sampleIndex">Inflection.</param>
        /// <param name="gain">Gain.</param>
        public void SetGain(int sampleIndex, float gain)
        {
            if(_envelope.ContainsKey(sampleIndex))
            {
                if(gain == 0)
                {
                    _envelope.Remove(sampleIndex);
                }
                else
                {
                    _envelope[sampleIndex] = gain;
                }
            }
            else
            {
                _envelope[sampleIndex] = gain;
            }
        }
        #endregion

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

            _vals = source.ReadAll();
        }

        /// <summary>
        /// Get the envelope in effect at the position.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns>The envelope gain at pos.</returns>
        void GetEnvelopeGain(int pos)
        {
            _currentGain = 1.0f; // default

            if (_envelope.Count > 0)
            {
                // Find where offset is currently.
                //_envelope.OrderBy(e => e.Key)

                // Make an ordered copy of the _envelope point locations.
                List<int> envLocs = _envelope.Keys.ToList();
                envLocs.Sort();

                int tempEnv = 0;
                for (int i = 0; i < envLocs.Count; i++)
                {
                    if (envLocs[i] > pos)
                    {
                        // Found the transition after.
                        _currentGain = _envelope[tempEnv];
                        break;
                    }
                    else
                    {
                        // Next.
                        tempEnv = i;
                    }
                }
            }
        }
        #endregion
    }
}