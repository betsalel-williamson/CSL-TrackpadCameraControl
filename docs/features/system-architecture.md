# System architecture

## End-user value

Players feel map-app-like or CAD-like trackpad control inside Cities: Skylines I without buying a mouse, while Options stay fully tunable for experimentation. Optional Assist UI chrome can drive the same camera ops for assist and pipeline validation.

## Context

```mermaid
flowchart LR
  trackpad[TrackpadHardware]
  backend[PlatformBackend]
  source[IGestureSource]
  assist[AssistUI]
  mod[CS1Mod]
  settings[ModSettings]
  cam[CameraController]

  trackpad --> backend
  backend -->|"raw primitives"| source
  source --> mod
  assist -->|"camera ops"| mod
  settings -->|"hot bindings"| mod
  settings --> assist
  mod --> cam
```

## Components

| Component            | Responsibility                                                                                          |
| -------------------- | ------------------------------------------------------------------------------------------------------- |
| Platform backend     | Capture OS trackpad contacts / gestures; emit raw primitives while the game is focused                  |
| Gesture source       | Deliver primitives into the mod — IPC helper (**dev**) or in-process capture (**deploy**); see ADR 0001 |
| Assist UI            | Optional on-screen chrome; emits the same camera ops as gestures; style follows Gesture preset          |
| CS1 mod              | CitiesHarmony-hosted C#; resolve primitives through live settings; write camera targets                 |
| ModSettings          | Single source of truth for presets, bindings, Assist UI enable, and feel; hot-applied                   |
| Unsupported backends | Same interface; report unsupported                                                                      |

Platform-specific capture details (for example the first macOS backend) live in [platform backends](./platform-backends.md) and ADR 0001 — not in this high-level picture.

## Data flow

1. Backend emits finger count, centroid delta, pinch scale, rotate delta, and modifier flags.
2. Mod maps primitives to camera ops using the live binding table.
3. Optional [Assist UI](./assist-ui-camera-chrome.md) emits the same camera ops from chrome controls.
4. Mod applies deltas to camera target position, angle, and size with settings-driven sensitivity, invert, deadzone, and smoothing.
5. One-finger pointer path is left to the game (outside Assist UI chrome).

## Constraints

- Prefer additive camera writes over heavy Harmony patches.
- Coexist with ACME; do not own zoom-limit or saved-position features.
- Fail soft if a backend is missing or fails to start.
- Interpretation stays in C# so Options never require restarting the backend for feel changes.

## Open risks

- OS-reserved multi-finger gestures vs CAD three-finger orbit (varies by platform).
- Two-finger pan overlapping system scroll.
- Backend ABI or driver differences across OS versions.
