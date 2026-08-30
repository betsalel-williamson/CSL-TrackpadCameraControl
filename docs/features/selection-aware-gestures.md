# Selection-aware gestures

## Intent

While **placing a new** building/prop or **relocating** one, Maps+ two-finger rotate turns the ghost. Option-orbit can pivot on the ghost / selection. Clicking an already-placed object must **not** steal two-finger rotate (that stays camera yaw). With no placement/relocate, prior Maps+ camera behavior applies.

## Gesture contract

| Condition | Gesture | Result |
| --------- | ------- | ------ |
| Relocate or new placement ghost | Two-finger rotate | Rotate the **ghost** (`m_angle`) — not the old-cell buffer during relocate |
| Click-selected placed object only | Two-finger rotate | Camera [yaw](../glossary/yaw.md) (no object spin) |
| Relocate / placement / selected instance | Option (`⌥`)+two-finger drag | Camera **orbit around** that pivot (pitch still clamped) |
| No selection / no placement | Two-finger rotate | Camera [yaw](../glossary/yaw.md) |
| No selection / no placement | Option (`⌥`)+two-finger drag | Camera [orbit](../glossary/orbit.md) (yaw + pitch) |

Base Maps+ pan / pinch zoom and [orbit latch](../glossary/orbit-latch.md) remain as in [trackpad camera](./trackpad-camera.md). Selection does not change pinch zoom or two-finger pan.

## Acceptance criteria

- During new place or relocate, two-finger rotate turns the ghost; the camera does not yaw from that gesture.
- During relocate, Escape cancel must not leave the original building spun (ghost-only yaw).
- Clicking a placed object does **not** enable object rotate; two-finger rotate yaws the camera.
- With a selection or ghost, `⌥`+two-finger can orbit around that pivot; orbit pitch stays within Pitch min / max.
- With no selection / placement, two-finger rotate yaws the camera and `⌥`+two-finger orbits as Maps+ / AppleKit.

## Non-goals

- Perfect keyboard-vs-popup arbitration beyond existing input gates.
- Changing pan or zoom semantics based on selection.
- Free rotate of arbitrary click-selected city objects (out of scope unless a later design reopens it).

## Production selection (best-effort)

In-game detection lives in `CitiesSelectionContext` behind `ISelectionContext`, ordered by `SelectionGesturePriority`:

1. **Relocate:** `BuildingTool.m_relocate != 0` → two-finger rotate adjusts tool `m_angle` only (ghost). Do **not** mutate the buffer building at the old cell. Option-orbit pivots on cursor preview when present.
2. **Placement ghost:** new place with prefab (even if `m_selectedInstance` is still set) → rotate tool `m_angle`; orbit pivots on `ToolBase.m_mousePosition`.
3. **Selected instance:** used for **orbit pivot** only when no place/relocate tool is armed. Does **not** enable object yaw.
4. **Fail soft / none:** missing fields or exceptions → Maps+ camera yaw / orbit.

Hover is not used (it flickers as the camera moves). Unit tests cover priority / `AllowsObjectYaw` without Unity. This behavior is from this mod, not a third-party conflict.
