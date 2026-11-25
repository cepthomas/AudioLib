using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib;

public class ResamplingAudioFileReader_XX : ISampleProvider, IDisposable // TODO1 delete me
{

    AudioFileReader _afr;

    ISampleProvider _prov;

    public WaveFormat WaveFormat { get { return _afr.WaveFormat; } }  // = WaveFormat.CreateIeeeFloatWaveFormat(AudioLibDefs.SAMPLE_RATE, 2);

    /// <summary>
    /// Initializes a new instance of ResamplingAudioFileReader
    /// </summary>
    /// <param name="fileName">The file to open</param>
    /// <param name="sampleRate">Ensure sample rate</param>
    public ResamplingAudioFileReader_XX(string fileName, int sampleRate)
    {
        _afr = new AudioFileReader(fileName);

        if (_afr.WaveFormat.SampleRate != sampleRate)
        {
            _prov = new WdlResamplingSampleProvider(_afr, sampleRate);
            //prov.Dispose();
        }
        else
        {
            _prov = _afr;
        }
    }

    public int Read(Span<float> buffer)
    {
        return _prov.Read(buffer);
    }

    public void Rewind()
    {
        _afr.Position = 0;
    }

    public void Dispose()
    {
       //TODO1 throw new NotImplementedException();
    }



    /// <summary>
    /// Get provider info. Mainly for UI display.
    /// </summary>
    /// <returns>Info chunks.</returns>
    public string GetInfo()
    {
        List<string> ls = [];

        // Common stuff.
        ls.Add($"Provider:{_prov.GetType().ToString().Replace("NAudio.Wave.", "").Replace("AudioLib.", "")}");

        //ls.Add($"File:{afr.FileName}");
        //ls.Add($"Length:{afr.Length}");
        //ls.Add($"Time:{afr.TotalTime}");

        // More common stuff.
        ls.Add($"Encoding:{_prov.WaveFormat.Encoding}");
        ls.Add($"Channels:{_prov.WaveFormat.Channels}");
        ls.Add($"SampleRate:{_prov.WaveFormat.SampleRate}");
        ls.Add($"BitsPerSample:{_prov.WaveFormat.BitsPerSample}");

        return string.Join("  ", ls);
    }

}
