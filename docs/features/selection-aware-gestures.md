# Selection-aware gestures

## Intent

When a city object is selected, Maps+ two-finger rotate and Option-orbit target the selection instead of free camera yaw / world orbit. With no selection, prior Maps+ camera behavior applies.

## Gesture contract

| Condition | Gesture | Result |
| --------- | ------- | ------ |
| Object selected | Two-finger rotate | Rotate the **selected object** (not camera yaw) |
| Object selected | Option (`⌥`)+two-finger drag | Camera **orbit around** the selected object (pitch still clamped) |
| No selection | Two-finger rotate | Camera [yaw](../glossary/yaw.md) |
| No selection | Option (`⌥`)+two-finger drag | Camera [orbit](../glossary/orbit.md) (yaw + pitch) |

Base Maps+ pan / pinch zoom and [orbit latch](../glossary/orbit-latch.md) remain as in [trackpad camera](./trackpad-camera.md). Selection does not change pinch zoom or two-finger pan.

## Acceptance criteria

- With a selection, two-finger rotate turns the selected object; the camera does not yaw from that gesture.
- With a selection, `⌥`+two-finger orbits the camera around the selection; orbit pitch stays within Pitch min / max.
- With no selection, two-finger rotate yaws the camera and `⌥`+two-finger orbits as Maps+ / AppleKit.
- Clearing the selection restores camera yaw / orbit for those gestures without restart.

## Non-goals

- Perfect keyboard-vs-popup arbitration beyond existing input gates.
- Changing pan or zoom semantics based on selection.

## Production selection (best-effort)

In-game detection lives in `CitiesSelectionContext` behind `ISelectionContext`, ordered by `SelectionGesturePriority`:

1. **Relocate:** `BuildingTool.m_relocate != 0` → two-finger rotate adjusts tool `m_angle` only (ghost preview). Do **not** mutate the buffer building at the old cell, so Escape cancel restores cleanly. Option-orbit pivots on cursor preview when present.
2. **Selected instance:** `InstanceManager.m_selectedInstance` (validated when `IsValid` exists) → rotate / pivot on live `Building` / `PropInstance` buffer position (follows relocate commit).
3. **Placement ghost:** `BuildingTool` / `PropTool` with prefab and no relocate → rotate tool `m_angle`; orbit pivots on `ToolBase.m_mousePosition`.
4. **Fail soft / none:** missing fields or exceptions → Maps+ camera yaw / orbit.

Hover is not used (it flickers as the camera moves). Unit tests cover priority without Unity.
