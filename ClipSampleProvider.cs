using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NBagOfTricks;


namespace AudioLib
{
    /// <summary>
    /// Provider that encapsulates a client supplied audio data subset. When constructed, it reads in the
    /// entire file. Does sample rate conversion if needed.
    /// Mono output only - coerces stereo input per client call. Can be used for splitting stereo files.
    /// </summary>
    public class ClipSampleProvider : ISampleProvider
    {
        #region Fields
        /// <summary>The full buffer from client.</summary>
        float[] _vals = Array.Empty<float>();

        /// <summary>For notifications.</summary>
        int _sampleCount = 0;
        #endregion

        #region Backing fields
        long _position = 0;
        #endregion

        #region Properties
        /// <inheritdoc />
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibDefs.SAMPLE_RATE, 1);

        /// <summary>The associated file name. Empty if new.</summary>
        public string FileName { get; }

        /// <summary>Overall gain applied to all samples.</summary>
        public float Gain { get; set; } = 1.0f;

        /// <summary>The number of samples per channel.</summary>
        public int SamplesPerChannel { get { return _vals.Length; } }

        /// <summary>The total time.</summary>
        public TimeSpan TotalTime { get { return TimeSpan.FromMilliseconds((int)(1000.0f * _vals.Length / WaveFormat.SampleRate)); } }

        /// <summary>Make this class sort of look like a stream. This is actually the index into the buffer aka sample index.</summary>
        public long Position
        {
           get { return _position; }
           set { _position = _vals.Length > 0 ? (long)MathUtils.Constrain(value, 0, _vals.Length - 1) : 0; }
        }

        /// <summary>The current time.</summary>
        public TimeSpan CurrentTime { get { return TimeSpan.FromMilliseconds((int)(1000.0f * _position / WaveFormat.SampleRate)); } }

        /// <summary>Selection start sample.</summary>
        public int SelStart { get; set; } = 0;

        /// <summary>Selection length in samples.</summary>
        public int SelLength { get; set; } = 0;

        /// <summary>Number of samples per notification.</summary>
        public int SamplesPerNotification { get; set; }
        #endregion

        #region Events
        /// <summary>Raised periodically to inform the user of play progress.</summary>
        public event EventHandler<ClipProgressEventArgs>? ClipProgress;

        public class ClipProgressEventArgs : EventArgs
        {
            public long Position { get; set; }
            public TimeSpan CurrentTime { get; set; }
        }

        // create objects up front giving GC little to do
        readonly ClipProgressEventArgs _args = new ClipProgressEventArgs() { Position = 0, CurrentTime = TimeSpan.Zero };
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor from a sample provider. Coerces stereo to mono.
        /// </summary>
        /// <param name="source">Source provider to use.</param>
        /// <param name="mode">How to handle stereo files.</param>
        /// <param name="samplesPerNotification">Number of samples between notifications.</param>
        public ClipSampleProvider(ISampleProvider source, StereoCoercion mode, int samplesPerNotification = 5000)
        {
            FileName = "";
            SamplesPerNotification = samplesPerNotification;

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
        /// Constructor from a file. Coerces stereo to client's choice.
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
        /// Reads samples from this sample provider with adjustments for gain.
        /// Honors user selection if specified.
        /// ISampleProvider implementation.
        /// </summary>
        /// <param name="buffer">Sample buffer.</param>
        /// <param name="offset">Offset into buffer.</param>
        /// <param name="count">Number of samples requested.</param>
        /// <returns>Number of samples read.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int numRead = 0;
            int end = _vals.Length;

            // Is it a specific selection?
            if(SelLength > 0)
            {
                _position = Math.Max(SelStart, _position);
                end = SelStart + SelLength;
            }

            // Read area of interest.
            long numToRead = Math.Min(count, end - _position);
            for (int n = 0; n < numToRead; n++)
            {
                buffer[n + offset] = _vals[_position] * Gain;
                numRead++;
                _position++;
                _sampleCount++;
            }

            if (_sampleCount >= SamplesPerNotification)
            {
                _args.Position = _position;
                _args.CurrentTime = CurrentTime;
                ClipProgress?.Invoke(this, _args);
                _sampleCount = 0;
            }

            return numRead;
        }
        #endregion

        #region Private functions
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
            // else mono, read as is
            _vals = source.ReadAll();
        }
        #endregion
    }
}