using System;
using System.Collections.Generic;
using NAudio.Wave;


namespace AudioLib
{
    /// <summary>Supports hot swapping of input.</summary>
    public class SwappableSampleProvider : ISampleProvider
    {
        #region Fields
        /// <summary>The current input.</summary>
        ISampleProvider? _currentInput;

        /// <summary>The lock() target.</summary>
        readonly object _locker = new();
        #endregion

        #region Properties
        /// <summary>The WaveFormat of this sample provider. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

        // /// <summary>If true Read always returns the number of samples requested by padding.</summary>
        // public bool ReadFully { get; set; }//TODO needed?
        #endregion

        #region Public functions
        /// <summary>
        /// Sets the input source.
        /// </summary>
        /// <param name="input">New input.</param>
        public void SetInput(ISampleProvider input)
        {
            lock (_locker)
            {
                // Sanity checks.
                AudioUtils.ValidateFormat(input.WaveFormat, false);
                // Everything is good.
                _currentInput = input;
            }
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
                throw new ArgumentException("Invalid source.");
            }

            lock (_locker)
            {
                float[] input = new float[count];
                int samplesRead = _currentInput.Read(input, 0, count);
                int outputSamples = Math.Min(samplesRead, buffer.Length - offset);
                for (int i = 0; i < outputSamples; i++)
                {
                    buffer[offset + i] = input[i];
                }
                return outputSamples;
            }
        }
    }
    #endregion
}
