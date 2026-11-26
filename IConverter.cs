using System;
using System.Collections.Generic;
using System.Drawing;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib;

/// <summary>Abstraction based on selection mode. Does text parsing, formatting, snap.</summary>
public interface ITranslator
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
public static class Translator
{
    /// <summary>Make me a translator.</summary>
    /// <param name="mode">Which flavor.</param>
    /// <returns>New translator.</returns>
    public static ITranslator Create(WaveSelectionMode mode)
    {
        return mode switch
        {
            WaveSelectionMode.Time => new TimeTranslator(),
            WaveSelectionMode.Bar => new BarTranslator(),
            WaveSelectionMode.Sample => new SampleTranslator(),
            _ => throw new NotImplementedException(),
        };
    }
}
