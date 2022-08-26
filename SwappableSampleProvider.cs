using System;
using System.Collections.Generic;
using NAudio.Wave;


namespace AudioLib
{
    /// <summary>
    /// Sample provider that supports hot swapping of input. Mainly used to supply input to
    /// WaveOut which doesn't like having its input switched.
    /// Takes stereo or mono input, output is always stereo.
    /// </summary>
    public class SwappableSampleProvider : ISampleProvider
    {
        #region Fields
        /// <summary>The current input.</summary>
        ISampleProvider? _currentInput;
        #endregion

        #region Properties
        /// <summary>The fixed stereo WaveFormat of this sample provider. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibDefs.SAMPLE_RATE, 2);

        ///// <summary>Position of the simulated stream as sample index.</summary>
        //public int Position
        //{
        //    get { return _currentInput.GetPosition(); }
        //    set { if(_currentInput is not null) _currentInput.SetPosition(value); }
        //}
        #endregion

        #region Public functions
        /// <summary>
        /// Sets the input source.
        /// </summary>
        /// <param name="input">New input or null to disable.</param>
        public void SetInput(ISampleProvider? input)
        {
            // Sanity checks.
            input?.Validate(false);
            // Everything is good.
            _currentInput = input;
            Rewind();
        }

        /// <summary>
        /// Rewind.
        /// </summary>
        public void Rewind()
        {
            _currentInput?.Rewind();
        }

        /// <summary>
        /// Reads samples from this sample provider. ISampleProvider implementation.
        /// </summary>
        /// <param name="buffer">Sample buffer.</param>
        /// <param name="offset">Offset into buffer.</param>
        /// <param name="count">Number of samples required.</param>
        /// <returns>Number of samples read.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            if (_currentInput is null)
            {
                //throw new ArgumentException("Invalid source.");
                return 0;
            }

            var readBuffer = new float[count];

            if (_currentInput.WaveFormat.Channels == 1)
            {
                // Convert mono into stereo. Borrowed from MonoToStereoSampleProvider:
                var req = count / 2;
                var index = offset;
                var sread = _currentInput.Read(readBuffer, 0, req);
                for (var n = 0; n < sread; n++)
                {
                    buffer[index++] = readBuffer[n]; // L
                    buffer[index++] = readBuffer[n]; // R
                }
                return sread * 2;
            }
            else // Stereo - as is.
            {
                var req = count;
                var index = offset;
                int sread = _currentInput.Read(readBuffer, 0, req);
                for (int i = 0; i < sread; i++)
                {
                    buffer[index++] = readBuffer[i];
                }
                return sread;
            }
        }
    }
    #endregion
}
