# Trackpad camera

## Intent

Give trackpad players the same camera fluency mouse users get from middle-mouse orbit, scroll zoom, and drag pan — using multi-touch gestures instead of a three-button mouse.

## End-user outcomes

- [Pan](../glossary/pan.md), [orbit](../glossary/orbit.md), [zoom](../glossary/zoom.md), and [yaw](../glossary/yaw.md) without attaching a mouse.
- One-finger click and drag still drive build tools and UI.
- Choose [Maps+](../glossary/maps-plus-preset.md) or [CAD](../glossary/cad-preset.md) presets (seeds via `ApplyPreset` today; Options UI later), then tune every binding and feel value hot (no restart).
- Optionally enable [Assist UI](../glossary/assist-ui.md) chrome for the same camera ops (and to validate that path without Multitouch) — Assist UI wiring ships in a later phase.

## Gesture contract (preset seeds)

Presets seed bindings. Users may override any row; that becomes Custom. Modifier keys are described in platform-neutral terms; OS-specific key names appear in the client platform notes.

### Maps+ (default seed)

| Gesture                    | Camera op              |
| -------------------------- | ---------------------- |
| One-finger click / drag    | Unchanged (tools / UI) |
| Two-finger drag            | Pan                    |
| Pinch                      | Zoom                   |
| Two-finger rotate          | Yaw                    |
| Modifier + two-finger drag | Orbit (yaw + pitch)    |

### CAD seed

| Gesture                 | Camera op           |
| ----------------------- | ------------------- |
| One-finger click / drag | Unchanged           |
| Two-finger drag         | Pan                 |
| Pinch                   | Zoom                |
| Two-finger rotate       | Yaw                 |
| Three-finger drag       | Orbit (yaw + pitch) |

## Resolve mode and orbit latch

- [Gesture resolve mode](../glossary/gesture-resolve-mode.md) controls whether multiple camera ops can apply from one frame (default: Concurrent).
- [Orbit latch](../glossary/orbit-latch.md): once orbit engages, it holds until touch-up even if the modifier is released. While latched, orbit and yaw rotate apply; pan and zoom do not.

## Acceptance criteria (current)

- With a supported trackpad backend and Maps+ defaults, pan, zoom, yaw, and modifier+two-finger orbit work in-game.
- Calling `ApplyPreset(CAD)` (or switching preset when Options UI lands) makes three-finger orbit take effect on the next gesture (no restart).
- Changing any sensitivity, invert, enable, deadzone, orbit trigger, or resolve mode applies hot via live ModSettings.
- Orbit latch continues orbit after modifier release until fingers lift; pan and zoom stay suppressed while latched.
- Concurrent resolve allows pan + zoom + yaw in the same frame when not orbit-latched.
- One-finger building tools remain usable.
- Without a platform backend (unsupported OS or missing bridge), the mod enables cleanly and does not break vanilla input.
- No Options UI required for this slice (in-memory settings defaults and `ApplyPreset`).

## Acceptance criteria (later phases)

- Options UI exposes preset, resolve mode, and all tunables.
- With Assist UI enabled, corner chrome can drive the same pan / zoom / yaw / orbit ops through the shared apply path.
- Optional Harmony gate: single checkbox to enable or disable vanilla in-game camera controls so trackpad and vanilla do not fight.

## Non-goals (v1)

- Optical [roll](../glossary/roll.md) (CS1 camera is yaw/pitch only).
- Full Windows / Linux multitouch backends (stubs only; see [platform backends](./platform-backends.md)).
- Steam Workshop packaging (after local install works).
- Synthetic middle-mouse injection as the primary path.
