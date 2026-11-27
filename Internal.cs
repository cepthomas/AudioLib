using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib;

/// <summary>Just for me.</summary>
internal class Internal
{

    // ctrl: 0 -> Width
    // data: sel1 - sel2
    // shift, scale/zoom    

    /// <summary>Convert X pos to sample index.</summary>
    /// <param name="pixel">Control pixel X.</param>
    /// <param name="width">Control width.</param>
    /// <param name="sampleMin"></param>
    /// <param name="sampleMax"></param>
    /// <returns>Corresponding sample or -1 if not visible.</returns>
    internal static int PixelToSample(int pixel, int width, int sampleMin, int sampleMax)
    {
        int sample = -1;

        if (pixel >= 0 && pixel < width && sampleMax > sampleMin)
        {
            sample = MathUtils.Map(pixel, 0, width, sampleMin, sampleMax);
        }

        // if (pixel >= 0 && pixel < width)
        // {
        //     sample = pixel * _samplesPerPixel + _visibleStart;
        // }

        return sample;
    }

    /// <summary>Find sample visible location.</summary>
    /// <param name="sample"></param>
    /// <returns>The pixel or -1 if not visible.</returns>
    /// <param name="width">Control width.</param>
    /// <param name="sampleMin"></param>
    /// <param name="sampleMax"></param>
    internal static int SampleToPixel(int sample, int width, int sampleMin, int sampleMax)
    {
        int pixel = -1;

        if (sample >= 0 && sampleMax > sampleMin)
        {
            pixel = MathUtils.Map(sample, sampleMin, sampleMax, 0, width);
        }



        // if (_samplesPerPixel > 0)
        // {
        //     int offset = sample - _visibleStart;
        //     pixel = offset / _samplesPerPixel;
        //     if (pixel < 0 || pixel >= Width)
        //     {
        //         pixel = -1;
        //     }
        // }

        return pixel;
    }


#if _OLD_CONV

    /// <summary>
    /// Convert X pos to sample index.
    /// </summary>
    /// <param name="pixel">Control pixel X.</param>
    /// <returns>The sample or -1 if not visible.</returns>
    int PixelToSample_TimeBar(int pixel)
    {
        int sample = -1;

        if (pixel >= 0 && pixel < Width)
        {
            sample = pixel * _samplesPerPixel + _visibleStart;
        }

        return sample;
    }


    /// <summary>
    /// Convert X pos to sample index.
    /// </summary>
    /// <param name="pixel">Control pixel X.</param>
    /// <returns>The sample or -1 if not visible.</returns>
    int PixelToSample_WaveViewer(int pixel)
    {
        int sample = -1;

        if (pixel >= 0 && pixel < Width)
        {
            sample = pixel * _samplesPerPixel + _visibleStart;
        }

        return sample;
    }

    /// <summary>
    /// Find sample visible location.
    /// </summary>
    /// <param name="sample"></param>
    /// <returns>The pixel or -1 if not visible.</returns>
    int SampleToPixel_TimeBar(int sample)
    {
        int pixel = -1;

        if (_samplesPerPixel > 0)
        {
            int offset = sample - _visibleStart;
            pixel = offset / _samplesPerPixel;

            if (pixel < 0 || pixel >= Width)
            {
                pixel = -1;
            }

            // old version pixel = MathUtils.Map(sample, 0, _length, 0, Width);
        }

        return pixel;
    }

    /// <summary>
    /// Find sample visible location.
    /// </summary>
    /// <param name="sample"></param>
    /// <returns>The pixel or -1 if not visible.</returns>
    int SampleToPixel_WaveViewer(int sample)
    {
        int pixel = -1;

        if (_samplesPerPixel > 0)
        {
            int offset = sample - _visibleStart;
            pixel = offset / _samplesPerPixel;

            if (pixel < 0 || pixel >= Width)
            {
                pixel = -1;
            }
        }

        return pixel;
    }

#endif

}
