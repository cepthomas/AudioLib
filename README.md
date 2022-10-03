# AudioLib

- WinForms library of various audio components and controls.
- This uses NAudio extensively but limits internal formats to IEEE 32bit fp 44100Hz.
- For usage, see [Wavicler](https://github.com/cepthomas/Wavicler) which is a fairly comprehensive example
  that documents most of the features. Also see the Test project.
- Requires VS2022 and .NET6.

# Contents

- AudioPlayer: Fairly thin wrapper around the basic NAudio wave player. Devices are limited to the ones available on your box.
- ProgressBar: Elapsed time control. Optional full wave display.
- WaveViewer: Display and navigate a wave. Adjust gain, extract selection.
- AudioFileInfo: Dumps contents of supported audio files.
- ClipSampleProvider: Sample provider that encapsulates a client supplied piece of audio.
- SwappableSampleProvider: Sample provider that supports hot swapping of input.
- PeakProvider: Customized version of NAudio's IPeakProvider family for displayiing waveforms.
- ToolStripParamEditor: UI component for editing internal values.
- NAudioEx: NAudio compatible extension methods for the providers implemented here.
- Converters: Between samples, Audio Time, milliseconds, Bar/Beat.
- SampleOps, TimeOps, BarOps: Abstractions for the different formats user can select.

# External Components

- [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
