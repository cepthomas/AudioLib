# AudioLib

- WinForms library of various audio components and controls.
- This uses NAudio3 extensively but limits internal formats to IEEE 32bit fp 44100Hz.
- For usage, see [Wavicler](https://github.com/cepthomas/Wavicler) which is a fairly comprehensive example
  that documents most of the features. Also see the Test project.
- Requires VS2026 and .NET10.

# Contents

- AudioPlayer: Fairly thin wrapper around the basic NAudio wave player. Devices are limited to the ones available on your box.
- ClipSampleProvider: Sample provider that encapsulates a client supplied piece of audio.
- SwappableSampleProvider: Sample provider that supports hot swapping of input.
- SavingWaveProvider.cs: Render to file.
- NullSampleProvider.cs: Dummy.

UI:
- ChannelControl: For controlling the channel. TBD.
- TimeBar: Elapsed time control. Optional full wave display.
- WaveViewer: Display and navigate a wave. Adjust gain, extract selection.
- UiEditors: For audio config.

Utility:
- AudioFileInfo: Dumps contents of supported audio files.
- *Translator: Between samples, Audio Time, Bar/Beat.
- AudioUtils.cs: Conversion, resampling, peaks.
- Extensions: NAudio compatible extension methods for the providers implemented here.


# External Components

- [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
