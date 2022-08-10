# AudioLib

WinForms library of various audio components and controls.

Requires VS2022 and .NET6.

Contents:
- AudioPlayer: Fairly thin wrapper around the basic NAudio wave player. Devices are limited to the ones available on your box.
- TimeBar: Elapsed time control.
- WaveViewer: Display a wave.
- TODO NAudio compatible stuff. Only 32bit fp, 44100Hz, mono. Internal buffs are float, API is double.
- Test: Fairly comprehensive example.


# External Components

- [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
