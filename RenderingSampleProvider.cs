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
    public class RenderingSampleProvider : ISampleProvider
    {
        #region Fields
        /// <summary>The full buffer from client. TODO or use ISampleProvider??</summary>
        readonly float[] _rawBuff = Array.Empty<float>();
        #endregion

        #region Properties
        /// <summary>The WaveFormat of this sample provider. ISampleProvider implementation.</summary>
        public WaveFormat WaveFormat { get; private set; }

        /// <summary>Overall gain. Client needs to reprocess after changing.</summary>
        public float Gain { get; set; } = 1.0f;

        /// <summary>Piecewise gain envelope. Key is index, value is gain. Client needs to reprocess after changing.</summary>
        public Dictionary<int, float> Envelope { get; set; } = new();

        ///// <summary>Piecewise gain envelope. Slopes later maybe. Client needs to reprocess after changing.</summary>
        // public List<(int loc, float gain) Envelope { get; set; } = new();
        #endregion

        #region Public
        /// <summary>
        /// Normal constructor.
        /// </summary>
        /// <param name="waveFormat">Format to use.</param>
        /// <param name="rawBuff">The data to use.</param>
        public RenderingSampleProvider(WaveFormat waveFormat, float[] rawBuff)
        {
            WaveFormat = waveFormat;
            _rawBuff = rawBuff;
        }

        /// <summary>
        /// Reads samples from this sample provider with adjustments for envelope and overall gain.
        /// ISampleProvider implementation.
        /// </summary>
        /// <param name="vals">Sample buffer.</param>
        /// <param name="offset">Offset into vals.</param>
        /// <param name="count">Number of samples required.</param>
        /// <returns>Number of samples read.</returns>
        public int Read(float[] vals, int offset, int count)
        {
            int numToRead = Math.Min(count, _rawBuff.Length - offset);

            if(Envelope.Count > 0)
            {
                // Make an ordered copy of the envelope point locations.
                List<int> envLocs = Envelope.Keys.ToList();
                envLocs.Sort();

                // Find where offset is currently.
                var loc = envLocs.Where(l => l > offset).FirstOrDefault();

                if(loc != 0)
                {
                    loc -= 1;
                }

                float envGain = Envelope[envLocs[loc]]; // default;

                for (int n = 0; n < numToRead; n++)
                {
                    if(Envelope.ContainsKey(n))
                    {
                        // Update env gain.
                        envGain = Envelope[n];
                    }
                    vals[n] = _rawBuff[n + offset] * envGain * Gain;
                }
            }
            else
            {
                // Simply adjust for gain.

                for (int n = 0; n < numToRead; n++)
                {
                    vals[n] = _rawBuff[n + offset] * Gain;
                }
            }

            return numToRead;
        }

        /// <summary>
        /// Get wave peak values for UI display.
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        public List<(float max, float min)> GetPeaks(int pixels)
        {
            int samplesPerPixel = _rawBuff.Length / pixels; // TODO accumulated fractional part? Or client's problem.
            int numVals = _rawBuff.Length;
            List<(float max, float min)> scaledVals = new();
            int offset = 0;

            var buff = new float[samplesPerPixel];

            for (int p = 0; p < pixels; p++)
            {
                // Get the vals with any gain.
                int numRead =  Read(buff, offset, samplesPerPixel);

                if(numRead > 0)
                {
                    float max = float.MinValue;
                    float min = float.MaxValue;

                    // Process the group.
                    for (int i = 0; i < numRead; i++)
                    {
                        float val = buff[i];
                        min = Math.Min(val, min);
                        max = Math.Max(val, max);
                    }
                    scaledVals.Add((max, min));
                }

                // if(numRead != samplesPerPixel)
                // {
                //     // We're done.
                // }

                offset += numRead;
            }

            return scaledVals;
        }
        #endregion
    }
}