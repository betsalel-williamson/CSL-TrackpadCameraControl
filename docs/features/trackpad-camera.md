# Trackpad camera

## Intent

Give trackpad players the same camera fluency mouse users get from middle-mouse orbit, scroll zoom, and drag pan — using multi-touch gestures instead of a three-button mouse.

## End-user outcomes

- [Pan](../glossary/pan.md), [orbit](../glossary/orbit.md), [zoom](../glossary/zoom.md), and [yaw](../glossary/yaw.md) without attaching a mouse.
- One-finger click and drag still drive build tools and UI.
- Shipped [gesture style](../glossary/gesture-style.md) is [Maps+](../glossary/maps-plus-preset.md) on AppleKit; tune [feel presets](../glossary/feel-preset.md) and [Sensitivity](../glossary/sensitivity.md) hot; values persist across quit.
- Mouse wheel still vanilla-zooms; middle-mouse drag still vanilla-orbit — see [vanilla camera suppress](./vanilla-camera-suppress.md). Trackpad two-finger pans.
- Optional [Debug panel](./debug-ui-camera-chrome.md) for the same tunables; Debug chrome buttons only when `EnableAssistChrome` is on.
- With a selection, rotate and Option-orbit follow [selection-aware gestures](./selection-aware-gestures.md).

## Gesture contract (Maps+ / AppleKit)

| Gesture                      | Camera / selection op                                                                                                                                                                                                                                                                          |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| One-finger click / drag      | Unchanged (tools / UI)                                                                                                                                                                                                                                                                         |
| Two-finger drag              | Pan (target clamped to unlocked game area)                                                                                                                                                                                                                                                     |
| Pinch                        | Zoom                                                                                                                                                                                                                                                                                           |
| Two-finger rotate            | **Rotation** (not orbit yaw): camera heading or place/relocate ghost — see [yaw](../glossary/yaw.md) / [selection-aware gestures](./selection-aware-gestures.md). Does not use the orbit velocity channel. Starting rotation clears leftover orbit coast (hard handoff).                       |
| Option (`⌥`)+two-finger drag | [Orbit](../glossary/orbit.md) from **current** look-at (**orbit yaw** + pitch via velocity), including during place/relocate (no Target re-home). Pitch follows vanilla **0°–90°** (floors at 0). No yaw angle clamp. With Option held, two-finger rotate is ignored (orbit owns the contact). |

CAD three-finger orbit is a **future** gesture style (not a v1 player choice). Schema and experimental compile flags may still retain it — see [product flags ADR](./adr/0003-feel-profiles-and-product-flags.md).

## Resolve mode and orbit latch

- [Gesture resolve mode](../glossary/gesture-resolve-mode.md) controls whether multiple camera ops can apply from one frame (default: Concurrent).
- [Orbit latch](../glossary/orbit-latch.md): once orbit engages, it holds until touch-up even if the modifier is released. While latched, orbit applies; rotation, pan, and zoom do not.
- **Rotate-owned contact** (no Option-orbit latch): after a twist starts, companion ScrollWheel must not pan or orbit for the rest of that contact — rotation only (plus pinch zoom if present).

## Apply path (orbit drag)

Option+two-finger (and Assist drag orbit) **queues** orbit yaw/pitch via `AddAngleVelocity`. Those deltas are **not** written to angles in `OnUpdate`. A Harmony postfix on `CameraController.HandleMouseEvents` flushes them into `m_angleVelocity` after vanilla inertia damp and before integrate — the same slot middle-mouse drag uses. Vanilla middle-mouse drag continues through the original `HandleMouseEvents` prefix; the postfix still merges trackpad Option-orbit pending into `m_angleVelocity`. Button chrome orbit still writes `AngleX`/`AngleY` directly.

Two-finger **rotation** writes `AngleX` (or ghost angles) directly and clears both axes of angle velocity on apply so prior orbit inertia cannot bleed into the twist. Angle writes update **only the edited axis** on `m_targetAngle` / `m_currentAngle` (no full-vector copy), so rotation cannot snap pitch the way a stale `current.y` lerp would.

## Init and readiness

- **Mod enable** (`OnEnabled`): create `ModRuntime`, patch Harmony, select capture backend — do not require the Debug panel.
- **City load** (`LoadingExtension.OnLevelLoaded`): request boot focus and **arm** capture connect once the gameplay scene is ready.
- **Each frame** (`GestureThreading`): `GesturePipeline.Tick()` connects/reconnects the backend while the mod is active and the game is focused; brief retry after load is normal.
- **Debug panel** is optional tuning UI only — factory default off; opening it is not required for gesture play.
- **Cities Harmony** is required for scroll suppress and Option-orbit velocity flush; without it pan may fight vanilla scroll-zoom and orbit may not integrate.

## Acceptance criteria (current)

- After loading a city (no Debug panel, no Options visit), pan, zoom, **rotation**, and `⌥`+two-finger orbit work within a few seconds while the game window is focused.
- With AppleKit and Maps+ defaults, pan, zoom, **rotation**, and `⌥`+two-finger orbit work in-game; pan stays within the unlocked game area; orbit pitch stays within **0°**–**90°**; rotation is not angle-clamped; starting rotation hard-handoffs leftover orbit coast.
- Selection-aware rotate / Option-orbit match [selection-aware gestures](./selection-aware-gestures.md).
- Slow / Default / Fast stay immutable; dirty edits use **New Preset** per [settings and hot configuration](./settings-and-hot-configuration.md); Sensitivity uses the slider contract (0.1×–2× factory default).
- Changing Sensitivity or pitch limits in Options or the Debug panel applies hot, stays in sync, and autosaves across quit.
- Orbit latch continues orbit after modifier release until fingers lift.
- Concurrent resolve allows pan + zoom + yaw in the same frame when not orbit-latched.
- One-finger building tools remain usable.
- [Vanilla camera suppress](./vanilla-camera-suppress.md): precise trackpad pan without vanilla zoom; mouse wheel zooms; middle-mouse orbit still vanilla; no mod camera when menus open or pointer over popups.
- Without a platform backend, the mod enables cleanly; keyboard, edge pan, and gamepad stay.
- If Cities Harmony is missing, the mod enables without crashing; pan may fight vanilla scroll-zoom.
- While product flags are off: no Contacts picker, no low-pass UI, no Debug chrome / button-step fields; no CAD / gesture-style switcher (CAD is future).
- Disable the mod to restore full vanilla camera input.

## Non-goals (v1)

- Optical [roll](../glossary/roll.md) (CS1 camera is yaw/pitch only).
- Full Windows / Linux multitouch backends (stubs only; see [platform backends](./platform-backends.md)).
- Steam Workshop packaging (after local install works).
- Synthetic middle-mouse injection as the primary path.
- Shipping CAD three-finger orbit or Contacts / Debug chrome to all players in this pass.
- Re-enabling Enable-per-op or Reverse on the product UI this pass.
