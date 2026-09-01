# Gesture style (Maps+)

How fingers map to camera ops is [gesture style](../glossary/gesture-style.md). The shipped style is [Maps+](../glossary/maps-plus-preset.md) on AppleKit — chosen for map-app familiarity and lower conflict with OS three-finger system gestures.

**Sensitivity**, Slow / Default / Fast, **New Preset**, Save as…, and Reset are [feel presets](./feel-presets.md), not gesture-style seeds. Changing feel does not change Maps+ bindings.

## Maps+ (shipped)

- Two-finger drag → [pan](../glossary/pan.md) (clamped to city bounds)
- Pinch → [zoom](../glossary/zoom.md)
- Two-finger rotate → [yaw](../glossary/yaw.md) the camera, or rotate a **new/relocate ghost** while placing
- Option (`⌥`)+two-finger drag → [orbit](../glossary/orbit.md) from current look-at (including during place/relocate)

Once orbit starts, [orbit latch](../glossary/orbit-latch.md) holds until you lift your fingers. Orbit pitch stays within vanilla pitch range **0–90°** (same as the game; floors at 0 so free-cam cannot go negative).

## CAD (not on the product surface)

[CAD](../glossary/cad-preset.md) three-finger orbit remains behind `EnableCadGestureStyle`. While that flag is off, there is no Maps+/CAD switcher in Options or the Debug panel. Prefer Maps+ for play; see [OS gesture conflicts](./os-gesture-conflicts.md) if you later enable CAD and fight system gestures.
