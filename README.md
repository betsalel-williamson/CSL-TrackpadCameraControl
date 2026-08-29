# Trackpad Camera Control

**Cities: Skylines I** mod for **trackpad** camera control — pan, orbit, and zoom via multitouch (pinch, two-finger, three-finger), with hot-configurable Options.

> Status: **Phase 1 — documentation and design.** No gameplay DLL yet.
>
> **Implementation status:** macOS trackpad backend first; Windows / Linux backends are stubs for contributors. High-level design and Options are platform-neutral.

## Why this exists

CS1 camera orbit expects a middle mouse button. Trackpad players have asked for map-app-style gestures for years; no Workshop mod ships true multitouch camera control. Vanilla workarounds (Rotate Camera Modifier + OS middle-click tools) are partial. This project fills that gap.

## Search keywords

`trackpad` · `touchpad` · `multitouch` · `pinch` · `camera` · `orbit` · `pan` · `zoom` · `Cities Skylines` · `CSL` · `Mac` · `Windows`

## Naming

| Surface | Name |
| --- | --- |
| Display | **Trackpad Camera Control** |
| Repository | [`CSL-TrackpadCameraControl`](https://github.com/betsalel-williamson/CSL-TrackpadCameraControl) |
| Parallel | Named like [Joystick Camera Control](https://github.com/RenaKunisaki/CSL-JoystickCameraControl) |

## Gesture presets (Options)

| Preset | Orbit |
| --- | --- |
| **Maps+** (default) | Modifier + two-finger drag (Option on macOS) |
| **CAD** | Three-finger drag |

Both: two-finger drag = pan, pinch = zoom, two-finger rotate = yaw. Every sensitivity and binding is hot-editable — nothing is hardcoded.

## Docs

Sharded docs via [MDCP](https://github.com/betsalel-williamson/mdcp):

```bash
npm install
npm run docs          # compile + check (lint required)
npm run format:check  # csharpier + clang-format (see docs/developer/lint-and-format.md)
```

| Guide | Path |
| --- | --- |
| Features / architecture | [`docs/features/`](docs/features/) |
| Player guide | [`docs/client/`](docs/client/) |
| Contributor guide | [`docs/developer/`](docs/developer/) |
| Glossary | [`docs/glossary/`](docs/glossary/) |

## Prior art

Coexists with [ACME](https://steamcommunity.com/sharedfiles/filedetails/?id=2778750497) (camera suite). Learns continuous-input patterns from Joystick Camera Control. Does **not** reimplement ACME features.

## Platform

- **Design / Options:** platform-neutral (shared IPC primitives + settings)
- **v1 backend:** macOS trackpad (native multitouch bridge)
- **Stubs:** Windows / Linux — same interface; contributions welcome

## License

MIT — see [LICENSE](LICENSE).

## Disclaimers and trademarks

This is an unofficial, fan-made project. It is **not** affiliated with, endorsed by, or sponsored by Paradox Interactive, Colossal Order, Apple Inc., Microsoft Corporation, or any other trademark holder.

- **Cities: Skylines** and related marks are trademarks or registered trademarks of Paradox Interactive AB and/or Colossal Order Ltd.
- **Mac**, **macOS**, **Magic Trackpad**, and related marks are trademarks of Apple Inc.
- **Windows** and related marks are trademarks of Microsoft Corporation.
- Other product names mentioned (for example ACME, Cities Harmony, Blender, Fusion) remain the property of their respective owners and are used only for identification or comparison.

Use of these names does not imply any relationship with or endorsement by the trademark holders. Cities: Skylines is required to use this mod and must be obtained separately through legitimate channels.
