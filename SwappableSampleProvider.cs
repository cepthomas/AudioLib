using System;
using System.Collections.Generic;
using NAudio.Wave;


namespace AudioLib
{
    /// <summary>Supports hot swapping of input. Stereo or mono.</summary>
    public class SwappableSampleProvider : ISampleProvider
    {
        #region Fields
        /// <summary>The current input.</summary>
        ISampleProvider? _currentInput;

        /// <summary>The current buffer.</summary>
        float[] _sourceBuffer = Array.Empty<float>();

        /// <summary>The lock() target.</summary>
        readonly object _locker = new();
        #endregion

        #region Properties
        /// <summary>The WaveFormat of this sample provider. It's stereo even for mono inputs. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

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
                if (_sourceBuffer.Length < count)
                {
                    _sourceBuffer = new float[count];
                }

                if (_currentInput.WaveFormat.Channels == 1)
                {
                    // Convert mono into stereo. from MonoToStereoSampleProvider:
                    var sourceSamplesRequired = count / 2;
                    var outIndex = offset;
                    var sourceSamplesRead = _currentInput.Read(_sourceBuffer, 0, sourceSamplesRequired);
                    for (var n = 0; n < sourceSamplesRead; n++)
                    {
                        buffer[outIndex++] = _sourceBuffer[n];
                        buffer[outIndex++] = _sourceBuffer[n];
                    }
                    return sourceSamplesRead * 2;
                }
                else // stereo
                {
                    var sourceSamplesRequired = count;
                    int sourceSamplesRead = _currentInput.Read(_sourceBuffer, 0, count);
                    for (int i = 0; i < sourceSamplesRead; i++)
                    {
                        buffer[offset + i] = _sourceBuffer[i];
                    }
                    return sourceSamplesRead;
                }
            }
        }
    }
    #endregion
}
