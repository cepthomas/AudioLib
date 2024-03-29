using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib
{
    /// <summary>
    /// Customized version of NAudio's IPeakProvider family.
    /// </summary>
    public class PeakProvider
    {
        /// <summary>
        /// Get wave peak values for UI display.
        /// </summary>
        /// <param name="vals">Generate peaks from this.</param>
        /// <param name="startIndex">Where to start in source.</param>
        /// <param name="samplesPerPixel">UI resolution.</param>
        /// <param name="totalPixels">Where to stop.</param>
        /// <returns></returns>
        public static List<(float max, float min)> GetPeaks(float[] vals, int startIndex, int samplesPerPixel, int totalPixels)
        {
            if (samplesPerPixel == 0)
            {
                throw new ArgumentException($"samplesPerPixel must be > 0");
            }

            List<(float max, float min)> scaledVals = new();
            int pixelCount = 0;

            if(samplesPerPixel > 0)
            {
                // Uses buckets with min/max. Could also implement average, rms, max/min, ...
                for(int srcIndex = startIndex;
                    srcIndex < vals.Length && pixelCount < totalPixels;
                    srcIndex += samplesPerPixel, pixelCount++)
                {
                    // Get the vals with any gain.
                    int numSamples = Math.Min(samplesPerPixel, vals.Length - srcIndex);

                    if (numSamples > 0)
                    {
                        float max = float.MinValue;
                        float min = float.MaxValue;

                        // Process the group.
                        for (int i = 0; i < numSamples; i++)
                        {
                            float val = vals[srcIndex + i];
                            min = Math.Min(val, min);
                            max = Math.Max(val, max);
                        }

                        scaledVals.Add((max, min));
                    }
                }
            }

            return scaledVals;
        }
    }
}
