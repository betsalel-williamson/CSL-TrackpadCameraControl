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

In-game detection lives in `CitiesSelectionContext` behind `ISelectionContext`:

- **Placement tools:** `BuildingTool` / `PropTool` with a prefab → two-finger rotate adjusts tool `m_angle`; Option-orbit pivots on reflected `ToolBase.m_mousePosition` when present.
- **Placed objects:** reflects `m_selectedInstance` or `m_hoverInstance` (`InstanceID`) from tool/manager types (owner varies by game build), then reads/writes `Building` / `PropInstance` buffers.
- **Fail soft:** missing fields, null singletons, or exceptions → treat as no selection (Maps+ camera yaw / orbit). Unit tests use a fake; they do not need Unity.
