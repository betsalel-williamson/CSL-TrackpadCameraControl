# Selection-aware gestures

## Intent

While **placing a new** building/prop or **relocating** one, Maps+ two-finger rotate turns the ghost and Option-orbit may pivot on that ghost. Otherwise Option-orbit turns around the **current camera look-at** (no snap back to a previous pivot). Clicking an already-placed object must not steal two-finger rotate.

## Gesture contract

| Condition | Gesture | Result |
| --------- | ------- | ------ |
| Relocate or new placement ghost | Two-finger rotate | Rotate the **ghost** (`m_angle`) — not the old-cell buffer during relocate |
| Click-selected placed object only | Two-finger rotate | Camera [yaw](../glossary/yaw.md) (no object spin) |
| Relocate or new placement ghost | Option (`⌥`)+two-finger drag | Camera **orbit around** the ghost pivot (pitch still clamped) |
| No place/relocate ghost | Option (`⌥`)+two-finger drag | Camera [orbit](../glossary/orbit.md) from **current** look-at (do not re-home Target) |
| No selection / no placement | Two-finger rotate | Camera [yaw](../glossary/yaw.md) |

Base Maps+ pan / pinch zoom and [orbit latch](../glossary/orbit-latch.md) remain as in [trackpad camera](./trackpad-camera.md). Selection does not change pinch zoom or two-finger pan.

## Acceptance criteria

- During new place or relocate, two-finger rotate turns the ghost; the camera does not yaw from that gesture.
- During relocate, Escape cancel must not leave the original building spun (ghost-only yaw).
- Clicking a placed object does **not** enable object rotate; two-finger rotate yaws the camera.
- After panning away, Option-orbit must not snap the look-at back to a prior orbit/selection pivot.
- During place/relocate, `⌥`+two-finger may orbit around the ghost; orbit pitch stays within Pitch min / max.
- With no place/relocate, two-finger rotate yaws the camera and `⌥`+two-finger orbits from the current look-at.

## Non-goals

- Perfect keyboard-vs-popup arbitration beyond existing input gates.
- Changing pan or zoom semantics based on selection.
- Free rotate of arbitrary click-selected city objects (out of scope unless a later design reopens it).
- Orbit-around-click-selection (same — out of scope; use current look-at).

## Production selection (best-effort)

In-game detection lives in `CitiesSelectionContext` behind `ISelectionContext`, ordered by `SelectionGesturePriority`:

1. **Relocate:** `BuildingTool.m_relocate != 0` → object yaw and orbit pivot use the ghost (`m_angle` / cursor preview). Do **not** mutate the buffer building at the old cell.
2. **Placement ghost:** new place with prefab → object yaw / orbit pivot on tool ghost (even if `m_selectedInstance` is still set).
3. **Selected instance / none:** no object yaw and **no** orbit Target re-home → Maps+ camera yaw / orbit from current look-at.

Hover is not used. Unit tests cover priority / `AllowsObjectYaw` / `AllowsOrbitPivot` without Unity.
