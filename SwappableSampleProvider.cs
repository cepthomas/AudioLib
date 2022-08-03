using System;
using System.Collections.Generic;
using NAudio.Wave;


namespace AudioLib
{
    /// <summary>Supports hot swapping of input to output.</summary>
    public class SwappableSampleProvider : ISampleProvider
    {
        #region Fields
        /// <summary>The current input.</summary>
        ISampleProvider? _currentInput;

        /// <summary>The input values.</summary>
        float[] _inputBuffer = Array.Empty<float>();

        /// <summary>The lock() target.</summary>
        readonly object _locker = new();
        #endregion

        #region Properties
        /// <summary>The WaveFormat of this sample provider. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; private set; }

        /// <summary>
        /// When set to true, the Read method always returns the number of samples requested, even if there are
        /// no inputs, or if the current inputs reach their end.
        /// Setting this to true effectively makes this a never-ending sample provider, so take care if you plan to write it out to a file.
        /// </summary>
        public bool ReadFully { get; set; }
        #endregion

        #region Public functions.
        public SwappableSampleProvider(WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
        }

        /// <summary>
        /// Sets the input source.
        /// </summary>
        /// <param name="input">New input.</param>
        public void SetInput(ISampleProvider input)
        {
            if(input is null)
            {
                throw new ArgumentException("Invalid input.");
            }

            lock (_locker)
            {
                // Sanity checks.
                if (WaveFormat is null)
                {
                    WaveFormat = input.WaveFormat;
                }
                else
                {
                    if (WaveFormat.SampleRate != input.WaveFormat.SampleRate || WaveFormat.Channels != input.WaveFormat.Channels)
                    {
                        _currentInput = null;
                        throw new ArgumentException("Mis-matched WaveFormat.");
                    }
                    // Everything is good.
                    _currentInput = input;
                }
            }
        }

        /// <summary>
        /// Reads samples from this sample provider. ISampleProvider implementation.
        /// </summary>
        /// <param name="buffer">Sample buffer.</param>
        /// <param name="offset">Offset into sample buffer.</param>
        /// <param name="count">Number of samples required.</param>
        /// <returns>Number of samples read.</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            if (_currentInput is null)
            {
                throw new ArgumentException("Invalid _source.");
            }

            if (_inputBuffer.Length < count)
            {
                _inputBuffer = new float[count];
            }

            int outputSamples = 0;

            lock (_locker)
            {
                int samplesRead = _currentInput.Read(_inputBuffer, 0, count);
                int outIndex = offset;
                for (int n = 0; n < samplesRead; n++)
                {
                    if (n >= outputSamples)
                    {
                        buffer[outIndex++] = _inputBuffer[n];
                    }
                    else
                    {
                        buffer[outIndex++] += _inputBuffer[n];
                    }
                }
                outputSamples = Math.Max(samplesRead, outputSamples);

                if (samplesRead < count)
                {
                   // InputEnded?.Invoke(this, new SampleProviderEventArgs(source));
                }
            }

            // Optionally ensure we return a full buffer.
            if (ReadFully && outputSamples < count)
            {
                int outputIndex = offset + outputSamples;
                while (outputIndex < offset + count)
                {
                    buffer[outputIndex++] = 0;
                }
                outputSamples = count;
            }

            return outputSamples;
        }
    }
    #endregion
}
