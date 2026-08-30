# Trackpad camera

## Intent

Give trackpad players the same camera fluency mouse users get from middle-mouse orbit, scroll zoom, and drag pan — using multi-touch gestures instead of a three-button mouse.

## End-user outcomes

- [Pan](../glossary/pan.md), [orbit](../glossary/orbit.md), [zoom](../glossary/zoom.md), and [yaw](../glossary/yaw.md) without attaching a mouse.
- One-finger click and drag still drive build tools and UI.
- Shipped [gesture style](../glossary/gesture-style.md) is [Maps+](../glossary/maps-plus-preset.md) on AppleKit; tune [feel presets](../glossary/feel-preset.md) and [Sensitivity](../glossary/sensitivity.md) hot; values persist across quit.
- Mouse wheel still zooms; trackpad two-finger pans — see [vanilla camera suppress](./vanilla-camera-suppress.md).
- Optional [Debug panel](./debug-ui-camera-chrome.md) for the same tunables; Debug chrome buttons only when `EnableAssistChrome` is on.
- With a selection, rotate and Option-orbit follow [selection-aware gestures](./selection-aware-gestures.md).

## Gesture contract (Maps+ / AppleKit)

| Gesture | Camera / selection op |
| ------- | --------------------- |
| One-finger click / drag | Unchanged (tools / UI) |
| Two-finger drag | Pan (target clamped to unlocked game area) |
| Pinch | Zoom |
| Two-finger rotate | No selection: [yaw](../glossary/yaw.md). Selection: rotate selected object — see [selection-aware gestures](./selection-aware-gestures.md). |
| Option (`⌥`)+two-finger drag | No selection: [orbit](../glossary/orbit.md) (yaw + pitch). Selection: orbit around selection. Pitch clamped to Pitch min / max (factory **7°**–**90°**). No yaw angle clamp. |

CAD three-finger orbit remains behind `EnableCadGestureStyle`.

## Resolve mode and orbit latch

- [Gesture resolve mode](../glossary/gesture-resolve-mode.md) controls whether multiple camera ops can apply from one frame (default: Concurrent).
- [Orbit latch](../glossary/orbit-latch.md): once orbit engages, it holds until touch-up even if the modifier is released. While latched, orbit applies; yaw rotate, pan, and zoom do not.

## Acceptance criteria (current)

- With AppleKit and Maps+ defaults, pan, zoom, yaw, and `⌥`+two-finger orbit work in-game; pan stays within the unlocked game area; orbit pitch stays within **7°**–**90°**; yaw is not angle-clamped.
- Selection-aware rotate / Option-orbit match [selection-aware gestures](./selection-aware-gestures.md).
- Slow / Default / Fast stay immutable; dirty edits use **New Preset** per [settings and hot configuration](./settings-and-hot-configuration.md); Sensitivity uses the slider contract (0.1×–2× factory default).
- Changing Sensitivity or pitch limits in Options or the Debug panel applies hot, stays in sync, and autosaves across quit.
- Orbit latch continues orbit after modifier release until fingers lift.
- Concurrent resolve allows pan + zoom + yaw in the same frame when not orbit-latched.
- One-finger building tools remain usable.
- [Vanilla camera suppress](./vanilla-camera-suppress.md): precise trackpad pan without vanilla zoom; mouse wheel zooms; no mod camera when menus open or pointer over popups.
- Without a platform backend, the mod enables cleanly; keyboard, edge pan, and gamepad stay.
- If Cities Harmony is missing, the mod enables without crashing; pan may fight vanilla scroll-zoom.
- While product flags are off: no CAD switcher, no Contacts picker, no low-pass UI, no Debug chrome / button-step fields.
- Disable the mod to restore full vanilla camera input.

## Non-goals (v1)

- Optical [roll](../glossary/roll.md) (CS1 camera is yaw/pitch only).
- Full Windows / Linux multitouch backends (stubs only; see [platform backends](./platform-backends.md)).
- Steam Workshop packaging (after local install works).
- Synthetic middle-mouse injection as the primary path.
- Enabling CAD / Contacts / Debug chrome for all players in this pass.
- Re-enabling Enable-per-op or Reverse on the product UI this pass.
