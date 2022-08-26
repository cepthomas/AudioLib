# AudioLib

WinForms library of various audio components and controls.

- Requires VS2022 and .NET6.
- This uses NAudio extensively but limits internal formats to IEEE 32bit fp 44100Hz.
- Note that NAudio API and buffers are float, whereas this library uses double.
- Test project is a fairly comprehensive example.

Contents:
- AudioPlayer: Fairly thin wrapper around the basic NAudio wave player. Devices are limited to the ones available on your box.
- TimeBar: Elapsed time control. Now only used by the defunct ClipExplorer application.
- WaveViewer: Display and navigate a wave.
- AudioFileInfo: Dumps contents of supported audio files.
- NAudioEx: NAudio compatible extension methods for the providers implemented here.
- ClipSampleProvider: Provider that encapsulates a client supplied audio dataset.
- SwappableSampleProvider: Sample provider that supports hot swapping of input.
- PeakProvider: Customized version of NAudio's IPeakProvider family for displayiing waveforms.

# External Components

- [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
