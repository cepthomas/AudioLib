using System;
using System.Collections.Generic;
using System.Drawing;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib
{
    /// <summary>Abstraction of selection mode. Does text parsing, formatting, snap.</summary>
    public interface IConverter
    {
        /// <summary>Snap to neighbor.</summary>
        /// <param name="sample">Test sample.</param>
        /// <param name="snap">How tight.</param>
        /// <returns>Snapped sample.</returns>
        int Snap(int sample, SnapType snap);

        /// <summary>Parse text.</summary>
        /// <param name="input">The text.</param>
        /// <returns>Corresponding sample.</returns>
        int Parse(string input);

        /// <summary>Set tempo. Only used for Bar mode.</summary>
        double BPM { get;  set; }

        /// <summary>Make a readable string.</summary>
        /// <param name="sample">Which sample.</param>
        /// <returns>The string.</returns>
        string Format(int sample);
    }

    /// <summary>Helper.</summary>
    public static class Converter
    {
        /// <summary>Make me a converter.</summary>
        /// <param name="mode">Which flavor.</param>
        /// <returns>New converter.</returns>
        public static IConverter Create(WaveSelectionMode mode)
        {
            return mode switch
            {
                WaveSelectionMode.Time => new TimeConverter(),
                WaveSelectionMode.Bar => new BarConverter(),
                WaveSelectionMode.Sample => new SampleConverter(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
