# Trackpad camera

## Intent

Give trackpad players the same camera fluency mouse users get from middle-mouse orbit, scroll zoom, and drag pan — using multi-touch gestures instead of a three-button mouse.

## End-user outcomes

- [Pan](../glossary/pan.md), [orbit](../glossary/orbit.md), [zoom](../glossary/zoom.md), and [yaw](../glossary/yaw.md) without attaching a mouse.
- One-finger click and drag still drive build tools and UI.
- Choose [Maps+](../glossary/maps-plus-preset.md) or [CAD](../glossary/cad-preset.md) presets in Options, then tune every binding and feel value hot (no restart).
- Optionally enable [Assist UI](../glossary/assist-ui.md) chrome for the same camera ops (and to validate that path without Multitouch).

## Gesture contract (preset seeds)

Presets seed Options. Users may override any row; that becomes Custom. Modifier keys are described in platform-neutral terms; OS-specific key names appear in the client platform notes.

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

## Acceptance criteria (MVP)

Proof slice before full v1 presets and Options:

- On macOS with the TrackpadBridge (dev path) connected, trackpad **pinch** changes camera **zoom** in-game.
- One-finger building tools remain usable.
- Without a backend or if the bridge is missing/disconnected, the mod enables cleanly and does not break vanilla input.
- No Options UI required for this slice (in-memory settings defaults).

## Acceptance criteria (v1)

- With a supported trackpad backend and Maps+ defaults, pan, zoom, yaw, and modifier+two-finger orbit work in-game.
- Switching to CAD in Options makes three-finger orbit take effect on the next gesture (no restart).
- Changing any sensitivity, invert, enable, deadzone, or orbit trigger applies hot.
- With Assist UI enabled, corner chrome can drive the same pan / zoom / yaw / orbit ops through the shared apply path.
- One-finger building tools remain usable.
- Without a platform backend (unsupported OS or missing bridge), the mod enables cleanly and does not break vanilla input.
- Does not reimplement ACME camera-suite features (saved positions, zoom limits, free-cam).

## Non-goals (v1)

- Optical [roll](../glossary/roll.md) (CS1 camera is yaw/pitch only).
- Full Windows / Linux multitouch backends (stubs only; see [platform backends](./platform-backends.md)).
- Steam Workshop packaging (after local install works).
- Synthetic middle-mouse injection as the primary path.
