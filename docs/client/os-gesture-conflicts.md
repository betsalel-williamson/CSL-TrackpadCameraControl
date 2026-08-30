# OS gesture conflicts

Multi-finger gestures are often reserved by the operating system (mission control, virtual desktops, window drag, scroll). This mod cannot override every OS policy; staying on [Maps+](../glossary/maps-plus-preset.md) keeps orbit on Option (`⌥`)+two-finger so three-finger system gestures stay free.

In-game, vanilla scroll-zoom is a separate concern: while the mod is enabled, [vanilla camera suppress](../glossary/vanilla-camera-suppress.md) suppresses **precise trackpad** scroll for pan, and leaves a real **mouse wheel** free to zoom (Cities Harmony required). OS-level reservations still apply.

## General guidance

- Prefer **Maps+** (shipped) — orbit uses `⌥`+two-finger instead of three fingers.
- CAD three-finger orbit is not on the product surface while `EnableCadGestureStyle` is off. If you enable it later, remap or disable conflicting OS three-finger gestures.
- If two-finger pan still vanilla-zooms, confirm Cities Harmony is subscribed and the mod is enabled — then disable the mod to restore full vanilla scroll-zoom.
- When Options/menus are open or the cursor is over a popup, two-finger scroll belongs to the UI, not the city camera — see [Options and hot tuning](./options-and-hot-tuning.md).

## Platform notes

- [macOS notes](./macos-gesture-conflicts.md)
- Windows / Linux: add notes when those backends ship
