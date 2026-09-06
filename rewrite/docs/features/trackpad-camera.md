# Trackpad camera

## Intent

Give trackpad players the same camera fluency mouse users get from middle-mouse orbit, scroll zoom, and drag pan — using multi-touch gestures instead of a three-button mouse. Rewrite internals may be greenfield-simple; **Maps+ capability and dynamics stay at parity** with shipping, and Options/Debug stay at [UI parity](../glossary/ui-parity.md) — not as copied C# ([parity with shipping](./parity-with-shipping.md), [ADR 0005](./adr/0005-ux-parity-not-source-parity.md)).

## End-user outcomes

- Pan, orbit, zoom, and yaw without attaching a mouse.
- One-finger click and drag still drive build tools and UI.
- Shipped gesture style is **Maps+** (style table seed on AppKit Capture); tune feel presets and Sensitivity hot; values persist across quit.
- Mouse wheel still vanilla-zooms; middle-mouse drag still vanilla-orbit — see [vanilla camera suppress](./vanilla-camera-suppress.md). Trackpad two-finger pans.
- Optional Debug panel for the same tunables; Debug chrome buttons only when Assist is compiled on.
- With a selection, rotate and Option-orbit follow [selection-aware gestures](./selection-aware-gestures.md).

## Gesture contract (Maps+ capability parity)

Chords resolve from the **style binding table** seeded for Maps+ ([ADR 0004](./adr/0004-style-table-driven-resolve.md)) — not from a parallel hardcoded heuristic path.

| Gesture                      | Camera / selection op                                                                                                                                                                                                                                                 |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| One-finger click / drag      | Unchanged (tools / UI)                                                                                                                                                                                                                                                |
| Two-finger drag              | Pan (target clamped to unlocked game area)                                                                                                                                                                                                                            |
| Pinch                        | Zoom                                                                                                                                                                                                                                                                  |
| Two-finger rotate            | **Rotation** (not orbit yaw): camera heading or place/relocate ghost — see [selection-aware gestures](./selection-aware-gestures.md). Does not use the orbit velocity channel. Starting rotation clears leftover orbit coast (hard handoff).                          |
| Option (`⌥`)+two-finger drag | Orbit from **current** look-at (orbit yaw + pitch via velocity), including during place/relocate (no Target re-home). Pitch follows vanilla **0°–90°** (apply constant). No yaw angle clamp. With Option held, two-finger rotate is ignored (orbit owns the contact). |

CAD three-finger orbit is a **future** gesture style, present only when `EnableCadGestureStyle` is compiled on and Capture emits honest finger counts ([platform backends](./platform-backends.md)).

## Resolve mode and orbit latch

- Gesture resolve mode controls whether multiple camera ops can apply from one frame (default: Concurrent).
- Orbit latch: once orbit engages, it holds until touch-up even if the modifier is released. While latched, orbit applies; rotation, pan, and zoom do not.
- **Rotate-owned contact** (no Option-orbit latch): after a twist starts, companion scroll must not pan or orbit for the rest of that contact — rotation only (plus pinch zoom if present).

## Apply path (orbit drag)

Option+two-finger (and Assist drag orbit when compiled on) **queues** orbit yaw/pitch as angle velocity. Those deltas are **not** written to angles in the Policy/Apply tick. A Harmony postfix flushes them into the vanilla angle-velocity slot after inertia damp and before integrate — the same slot middle-mouse drag uses. Vanilla middle-mouse drag continues through the original path; the postfix still merges trackpad Option-orbit pending. Button chrome orbit (Assist only) still writes angles directly.

Two-finger **rotation** writes heading (or ghost angles) directly and clears both axes of angle velocity on apply so prior orbit inertia cannot bleed into the twist. Angle writes update **only the edited axis** so rotation cannot snap pitch from a stale lerp.

## Init and readiness

- **Mod enable:** create runtime, patch Harmony (suppress + orbit flush), select Capture backend — do not require the Debug panel.
- **City load:** request boot focus and **arm** Capture once the gameplay scene is ready.
- **Each frame:** tick Capture → Policy → Apply; reconnect Capture while the mod is active and the game is focused; brief retry after load is normal.
- **Debug panel** is optional tuning UI only — factory default off; opening it is not required for gesture play.
- **Cities Harmony** is required for scroll suppress and Option-orbit velocity flush; without it pan may fight vanilla scroll-zoom and orbit may not integrate.

## Acceptance criteria

- After loading a city (no Debug panel, no Options visit), pan, zoom, **rotation**, and `⌥`+two-finger orbit work within a few seconds while the game window is focused.
- With AppKit and Maps+ style seed, pan, zoom, **rotation**, and `⌥`+two-finger orbit match shipping dynamics; pan stays within the unlocked game area; orbit pitch stays within **0°**–**90°**; rotation is not angle-clamped; starting rotation hard-handoffs leftover orbit coast.
- Selection-aware rotate / Option-orbit match [selection-aware gestures](./selection-aware-gestures.md).
- Slow / Default / Fast stay immutable; dirty edits use **New Preset** per [settings and hot configuration](./settings-and-hot-configuration.md); Sensitivity uses the slider contract (0.1×–2× factory default).
- Changing Sensitivity in Options or the Debug panel applies hot, stays in sync, and autosaves across quit (one dirty → one flush). Orbit pitch is vanilla **0°–90°** (not Options-tunable).
- Orbit latch continues orbit after modifier release until fingers lift.
- Concurrent resolve allows pan + zoom + yaw in the same frame when not orbit-latched.
- One-finger building tools remain usable.
- [Vanilla camera suppress](./vanilla-camera-suppress.md): precise trackpad pan without vanilla zoom; mouse wheel zooms; middle-mouse orbit still vanilla; no mod camera when menus open or pointer over popups.
- Without a platform backend, the mod enables cleanly; keyboard, edge pan, and gamepad stay.
- If Cities Harmony is missing, the mod enables without crashing; pan may fight vanilla scroll-zoom.
- While ship compile flags are off: no Contacts picker, no low-pass UI, no Debug chrome / button-step fields; no CAD / gesture-style switcher — those modules are omitted from the DLL.
- Disable the mod to restore full vanilla camera input.

## Non-goals (v1)

- Optical roll (CS1 camera is yaw/pitch only).
- Full Windows / Linux multitouch backends (stubs only; see [platform backends](./platform-backends.md)).
- Synthetic middle-mouse injection as the primary path (vanilla middle-mouse orbit remains available alongside trackpad).
- Shipping **CAD** three-finger orbit, **Contacts** capture, or Assist / Debug chrome pads to players — those stay compile-gated and omitted when off.
- Re-enabling Enable-per-op or Reverse on the product UI this pass.
- Player remapping of style table rows on the ship surface (Maps+ seed is fixed for parity unless a compiled style switcher exists).
