# AudioLib

- WinForms library of various audio components and controls.
- This uses NAudio3 extensively but limits internal formats to IEEE 32bit fp 44100Hz. TODO1 expand caps.
- For usage, see [Wavicler](https://github.com/cepthomas/Wavicler) which is a fairly comprehensive example
  that documents most of the features. Also see the Test project.
- Requires VS2026 and .NET10.

# Contents

- AudioPlayer: Fairly thin wrapper around the basic NAudio wave player. Devices are limited to the ones available on your box.
- ClipSampleProvider: Sample provider that encapsulates a client supplied piece of audio.
- SwappableSampleProvider: Sample provider that supports hot swapping of input.
- PeakProvider: Customized version of NAudio's IPeakProvider family for displayiing waveforms.
- SavingWaveProvider.cs: Render to file.
- NullSampleProvider.cs: Dummy.

UI:
- TimeBar: Elapsed time control. Optional full wave display.
- WaveViewer: Display and navigate a wave. Adjust gain, extract selection.
- UiEditors.cs (2k)

Utility:
- AudioFileInfo: Dumps contents of supported audio files.
- *Converter: Between samples, Audio Time, Bar/Beat.
- AudioUtils.cs: Conversion, resampling.
- Extensions: NAudio compatible extension methods for the providers implemented here.


# External Components

- [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
