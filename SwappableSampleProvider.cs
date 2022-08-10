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

        /// <summary>The input values.</summary>
        float[] _buffer = Array.Empty<float>();

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
        // /// <summary>
        // /// Normal constructor.
        // /// </summary>
        // /// <param name="waveFormat">Format to use. All ins and outs must be the same.</param>
        // public SwappableSampleProvider(WaveFormat waveFormat)
        // {
        //     WaveFormat = waveFormat;
        // }

        /// <summary>
        /// Sets the input source.
        /// </summary>
        /// <param name="input">New input.</param>
        public void SetInput(ISampleProvider input)
        {
            lock (_locker)
            {
                // Sanity checks.
                AudioUtils.ValidateFormat(input.WaveFormat);
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

            if (_buffer.Length < count)
            {
                _buffer = new float[count];
            }

            lock (_locker)
            {
                int samplesRead = _currentInput.Read(_buffer, 0, count);
                int outIndex = offset;
                int outputSamples = Math.Min(samplesRead, _buffer.Length - offset);
                Array.Copy(_buffer, 0, buffer, 0, outputSamples);
                return outputSamples;
            }
        }
    }
    #endregion
}
