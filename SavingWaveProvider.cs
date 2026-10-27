using System;
using NAudio.Wave;
using Ephemera.NBagOfTricks;


namespace Ephemera.AudioLib
{
    public class SavingWaveProvider(IWaveProvider sourceWaveProvider, string wavFilePath) : IWaveProvider, IDisposable
    {
        public WaveFormat WaveFormat { get { return sourceWaveProvider.WaveFormat; } }

        private readonly IWaveProvider sourceWaveProvider = sourceWaveProvider;
        private readonly WaveFileWriter writer = new(wavFilePath, sourceWaveProvider.WaveFormat);
        private bool isWriterDisposed;

        public int Read(Span<byte> buffer)
        {
            var read = sourceWaveProvider.Read(buffer);
            if (buffer.Length > 0 && !isWriterDisposed)
            {
                writer.Write(buffer);
            }
            if (buffer.Length == 0)
            {
                Dispose(); // auto-dispose in case users forget
            }
            return read;
        }

        public void Dispose()
        {
            if (!isWriterDisposed)
            {
                isWriterDisposed = true;
                writer.Dispose();
            }
        }
    }
}
