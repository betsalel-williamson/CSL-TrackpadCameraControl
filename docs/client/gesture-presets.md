# Gesture presets

Presets seed bindings for [pan](../glossary/pan.md), [zoom](../glossary/zoom.md), [yaw](../glossary/yaw.md), and [orbit](../glossary/orbit.md). Choose **Maps+** or **CAD** in the in-game Assist / tuning panel or in Options → Trackpad Camera Control.

Applying a built-in seed sets the orbit trigger for that style. It does **not** wipe your custom drag scales, button steps, or low-pass settings. Use **Reset to factory default** to restore schema defaults. Named Save as… / Load user presets come later.

## Maps+ (default)

Aligned with common map-app trackpad use and lower conflict with OS three-finger system gestures:

- Two-finger drag → pan
- Pinch → zoom
- Two-finger rotate → yaw
- Modifier + two-finger drag → orbit (Option on macOS)

Once orbit starts, [orbit latch](../glossary/orbit-latch.md) holds until you lift your fingers.

## CAD

Aligned with Blender / Fusion-style viewport control:

- Same pan / pinch / yaw as Maps+
- Three-finger drag → orbit (same latch until fingers lift)

Three-finger orbit may fight OS system gestures. See [OS gesture conflicts](./os-gesture-conflicts.md).

## Custom

Any manual change after applying a preset. Reset to factory restores defaults; re-apply Maps+ or CAD only to re-seed the orbit trigger.
