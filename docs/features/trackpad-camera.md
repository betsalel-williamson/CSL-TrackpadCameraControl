# Trackpad camera

## Intent

Give trackpad players the same camera fluency mouse users get from middle-mouse orbit, scroll zoom, and drag pan — using multi-touch gestures instead of a three-button mouse.

## End-user outcomes

- [Pan](../glossary/pan.md), [orbit](../glossary/orbit.md), [zoom](../glossary/zoom.md), and [yaw](../glossary/yaw.md) without attaching a mouse.
- One-finger click and drag still drive build tools and UI.
- Shipped [gesture style](../glossary/gesture-style.md) is [Maps+](../glossary/maps-plus-preset.md) on AppleKit; tune [feel presets](../glossary/feel-preset.md) and [Sensitivity](../glossary/sensitivity.md) hot; values persist across quit.
- Mouse wheel still zooms; trackpad two-finger pans — see [vanilla camera suppress](./vanilla-camera-suppress.md).
- Optional Assist panel for the same tunables; Assist chrome buttons only when `EnableAssistChrome` is on.

## Gesture contract (Maps+ / AppleKit)

| Gesture | Camera op |
| ------- | --------- |
| One-finger click / drag | Unchanged (tools / UI) |
| Two-finger drag | Pan |
| Pinch | Zoom |
| Two-finger rotate | Yaw |
| Option (`⌥`)+two-finger drag | Orbit (yaw + pitch), pitch clamped to Pitch min / max |

CAD three-finger orbit remains behind `EnableCadGestureStyle`.

## Resolve mode and orbit latch

- [Gesture resolve mode](../glossary/gesture-resolve-mode.md) controls whether multiple camera ops can apply from one frame (default: Concurrent).
- [Orbit latch](../glossary/orbit-latch.md): once orbit engages, it holds until touch-up even if the modifier is released. While latched, orbit and yaw rotate apply; pan and zoom do not.

## Acceptance criteria (current)

- With AppleKit and Maps+ defaults, pan, zoom, yaw, and `⌥`+two-finger orbit work in-game; orbit pitch stays within Pitch min / max.
- Slow / Default / Fast and Save as… / Load / Reset match the [feel preset](../glossary/feel-preset.md) contract; Sensitivity values use two decimals and are **> 0**.
- Changing Sensitivity, reverse, or pitch limits in Options or the in-game panel applies hot and persists across quit.
- Orbit latch continues orbit after modifier release until fingers lift.
- Concurrent resolve allows pan + zoom + yaw in the same frame when not orbit-latched.
- One-finger building tools remain usable.
- [Vanilla camera suppress](./vanilla-camera-suppress.md): precise trackpad pan without vanilla zoom; mouse wheel zooms; no mod camera when menus open or pointer over popups.
- Without a platform backend, the mod enables cleanly; keyboard, edge pan, and gamepad stay.
- If Cities Harmony is missing, the mod enables without crashing; pan may fight vanilla scroll-zoom.
- While product flags are off: no CAD switcher, no Contacts picker, no low-pass UI, no Assist chrome / button-step fields.
- Disable the mod to restore full vanilla camera input.

## Non-goals (v1)

- Optical [roll](../glossary/roll.md) (CS1 camera is yaw/pitch only).
- Full Windows / Linux multitouch backends (stubs only; see [platform backends](./platform-backends.md)).
- Steam Workshop packaging (after local install works).
- Synthetic middle-mouse injection as the primary path.
- Enabling CAD / Contacts / Assist chrome for all players in this pass.
