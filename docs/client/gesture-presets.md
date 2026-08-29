# Gesture presets

Presets seed bindings for [pan](../glossary/pan.md), [zoom](../glossary/zoom.md), [yaw](../glossary/yaw.md), and [orbit](../glossary/orbit.md). Options UI will expose a **Gesture preset** dropdown later; until then, Maps+ is the default seed and CAD is applied via preset seed (`ApplyPreset`).

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

Any manual change after applying a preset. Use **Reset** (when Options UI ships) to restore preset or factory defaults.
