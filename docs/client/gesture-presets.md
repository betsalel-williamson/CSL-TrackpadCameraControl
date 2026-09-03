# Gesture style (Maps+)

How fingers map to camera ops is [gesture style](../glossary/gesture-style.md). v1 ships **[Maps+](../glossary/maps-plus-preset.md)** on AppleKit only — map-app familiarity and lower conflict with OS three-finger system gestures.

**Sensitivity**, Slow / Default / Fast, **New Preset**, Save as…, and Reset are [feel presets](./feel-presets.md), not gesture-style seeds. Changing feel does not change Maps+ bindings.

## Maps+ (shipped)

- Two-finger drag → [pan](../glossary/pan.md) (clamped to city bounds)
- Pinch → [zoom](../glossary/zoom.md)
- Two-finger rotate → [rotate](../glossary/yaw.md) the camera, or rotate a **new/relocate ghost** while placing
- Option (`⌥`)+two-finger drag → [orbit](../glossary/orbit.md) from current look-at (including during place/relocate)

Once orbit starts, [orbit latch](../glossary/orbit-latch.md) holds until you lift your fingers. Orbit pitch stays within vanilla pitch range **0–90°** (same as the game; floors at 0 so free-cam cannot go negative).

## Future — CAD

[CAD](../glossary/cad-preset.md) three-finger orbit is a **future** gesture style (not available to players in v1). There is no Maps+/CAD switcher in Options or the Debug panel. See [OS gesture conflicts](./os-gesture-conflicts.md) for why three-finger orbit conflicts with many Mac system gestures.
