# AudioLib

WinForms library of various audio components and controls.

Requires VS2022 and .NET6.

Contents:
- AudioPlayer: Fairly thin wrapper around the basic NAudio wave player. Devices are limited to the ones available on your box.
- Settings container/editor for use by clients.
- Meter: Linear or log.
- Pot: Just like on your guitar.
- Pan: Just like on your hifi.
- Volume: Just like on your mixer. Goes from 0.0 to 2.0.
- TimeBar: Elapsed time control.
- WaveViewer: Display a wave. Has markers.
- Test: Fairly comprehensive example.

This application uses these FOSS components:
- [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).
- [NBagOfTricks](https://github.com/cepthomas/NBagOfTricks/blob/main/README.md)

