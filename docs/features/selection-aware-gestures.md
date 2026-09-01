# Selection-aware gestures

## Intent

While **placing a new** building/prop or **relocating** one, Maps+ two-finger rotate turns the ghost. Option-orbit always turns around the **current camera look-at** (including after panning away during place/relocate — no snap back to ghost, Relocate-click, or old cell). Clicking an already-placed object must not steal two-finger rotate.

## Gesture contract

| Condition                         | Gesture                      | Result                                                                                                     |
| --------------------------------- | ---------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Relocate or new placement ghost   | Two-finger rotate            | Rotate the **ghost** (`m_angle`) — not the old-cell buffer during relocate                                 |
| Click-selected placed object only | Two-finger rotate            | Camera [yaw](../glossary/yaw.md) (no object spin)                                                          |
| Relocate or new placement ghost   | Option (`⌥`)+two-finger drag | Camera [orbit](../glossary/orbit.md) from **current** look-at (do not re-home Target; pitch still clamped) |
| No place/relocate ghost           | Option (`⌥`)+two-finger drag | Camera [orbit](../glossary/orbit.md) from **current** look-at (do not re-home Target)                      |
| No selection / no placement       | Two-finger rotate            | Camera [yaw](../glossary/yaw.md)                                                                           |

Base Maps+ pan / pinch zoom and [orbit latch](../glossary/orbit-latch.md) remain as in [trackpad camera](./trackpad-camera.md). Selection does not change pinch zoom or two-finger pan.

## Acceptance criteria

- During new place or relocate, two-finger rotate turns the ghost; the camera does not yaw from that gesture.
- During relocate, Escape cancel must not leave the original building spun (ghost-only yaw).
- Clicking a placed object does **not** enable object rotate; two-finger rotate yaws the camera.
- After panning away (including during place/relocate), Option-orbit must not snap the look-at back to ghost, Relocate-click, old cell, or any prior pivot.
- During place/relocate, two-finger rotate turns the ghost; `⌥`+two-finger orbits from the current look-at; orbit pitch stays within vanilla **0–90°**.
- With no place/relocate ghost, two-finger rotate yaws the camera and `⌥`+two-finger orbits from the current look-at.

## Non-goals

- Perfect keyboard-vs-popup arbitration beyond existing input gates.
- Changing pan or zoom semantics based on selection.
- Free rotate of arbitrary click-selected city objects (out of scope unless a later design reopens it).
- Orbit-around-click-selection (same — out of scope; use current look-at).

## Production selection (best-effort)

In-game detection lives in `CitiesSelectionContext` behind `ISelectionContext`, ordered by `SelectionGesturePriority`:

1. **Relocate:** `BuildingTool.m_relocate != 0` → object yaw updates `m_angle` and render angles `m_mouseAngle` / `m_cachedAngle` (relocate ghost does not follow `m_angle` alone). Do **not** mutate the buffer building at the old cell. Option-orbit does **not** re-home Target.
2. **Placement ghost:** new place with prefab → object yaw on tool ghost (even if `m_selectedInstance` is still set). Option-orbit does **not** re-home Target.
3. **Selected instance / none:** no object yaw and **no** orbit Target re-home → Maps+ camera yaw / orbit from current look-at.

Hover is not used. Unit tests cover priority / `AllowsObjectYaw` / `AllowsOrbitPivot` without Unity.
