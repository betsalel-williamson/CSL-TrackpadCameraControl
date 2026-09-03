# Selection-aware gestures

## Intent

While **placing a new** building/prop or **relocating** one, Maps+ two-finger rotate turns the ghost. Option-orbit always turns around the **current camera look-at** (including after panning away during place/relocate — no snap back to ghost, Relocate-click, or old cell). Clicking an already-placed object must not steal two-finger rotate.

Policy re-queries selection each tick; Capture does not decide object vs camera yaw. Apply writes either camera heading or ghost angles per the table below.

## Gesture contract

| Condition                         | Gesture                      | Result                                                                                   |
| --------------------------------- | ---------------------------- | ---------------------------------------------------------------------------------------- |
| Relocate or new placement ghost   | Two-finger rotate            | Rotate the **ghost** — not the old-cell buffer during relocate                           |
| Click-selected placed object only | Two-finger rotate            | Camera yaw (no object spin)                                                              |
| Relocate or new placement ghost   | Option (`⌥`)+two-finger drag | Camera orbit from **current** look-at (do not re-home Target; pitch still apply-clamped) |
| No place/relocate ghost           | Option (`⌥`)+two-finger drag | Camera orbit from **current** look-at (do not re-home Target)                            |
| No selection / no placement       | Two-finger rotate            | Camera yaw                                                                               |

Base Maps+ pan / pinch zoom and orbit latch remain as in [trackpad camera](./trackpad-camera.md). Selection does not change pinch zoom or two-finger pan. Style resolve still comes from the Maps+ seed table ([ADR 0004](./adr/0004-style-table-driven-resolve.md)).

## Acceptance criteria

- During new place or relocate, two-finger rotate turns the ghost; the camera does not yaw from that gesture.
- During relocate, Escape cancel must not leave the original building spun (ghost-only yaw).
- Clicking a placed object does **not** enable object rotate; two-finger rotate yaws the camera.
- After panning away (including during place/relocate), Option-orbit must not snap the look-at back to ghost, Relocate-click, old cell, or any prior pivot.
- During place/relocate, two-finger rotate turns the ghost; `⌥`+two-finger orbits from the current look-at; orbit pitch stays within vanilla **0–90°** (apply constant).
- With no place/relocate ghost, two-finger rotate yaws the camera and `⌥`+two-finger orbits from the current look-at.

## Non-goals

- Perfect keyboard-vs-popup arbitration beyond existing input gates.
- Changing pan or zoom semantics based on selection.
- Free rotate of arbitrary click-selected city objects (out of scope unless a later design reopens it).
- Orbit-around-click-selection (same — out of scope; use current look-at).

## Selection priority (contract)

Detection is best-effort against the live game tools, ordered:

1. **Relocate** — object yaw updates the relocate ghost angles only. Do **not** mutate the buffer building at the old cell. Option-orbit does **not** re-home Target.
2. **Placement ghost** — new place with prefab → object yaw on tool ghost (even if a selected instance is still set). Option-orbit does **not** re-home Target.
3. **Selected instance / none** — no object yaw and **no** orbit Target re-home → Maps+ camera yaw / orbit from current look-at.

Hover is not used. Behavior tests cover priority and allow-object-yaw / allow-orbit-pivot without requiring a full Unity play session.
