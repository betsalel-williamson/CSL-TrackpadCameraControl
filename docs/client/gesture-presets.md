# Gesture style (Maps+)

How fingers map to camera ops is [gesture style](../glossary/gesture-style.md). The shipped style is [Maps+](../glossary/maps-plus-preset.md) on AppleKit — chosen for map-app familiarity and lower conflict with OS three-finger system gestures.

**Sensitivity**, Slow / Default / Fast, Save as… / Load, and Reset are [feel presets](./feel-presets.md), not gesture-style seeds. Changing feel does not change Maps+ bindings.

## Maps+ (shipped)

- Two-finger drag → [pan](../glossary/pan.md)
- Pinch → [zoom](../glossary/zoom.md)
- Two-finger rotate → [yaw](../glossary/yaw.md)
- Option (`⌥`)+two-finger drag → [orbit](../glossary/orbit.md)

Once orbit starts, [orbit latch](../glossary/orbit-latch.md) holds until you lift your fingers. Orbit pitch stays within Pitch min / max (editable in Options / the Assist panel).

## CAD (not on the product surface)

[CAD](../glossary/cad-preset.md) three-finger orbit remains behind `EnableCadGestureStyle`. While that flag is off, there is no Maps+/CAD switcher in Options or the Assist panel. Prefer Maps+ for play; see [OS gesture conflicts](./os-gesture-conflicts.md) if you later enable CAD and fight system gestures.
