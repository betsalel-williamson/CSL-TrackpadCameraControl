# Platform backends

## Intent

Keep product language and Options **platform-neutral**. Isolate OS capture behind one shared primitive/frame contract in the [gesture library](../glossary/gesture-library.md). The [mod surface](../glossary/mod-surface.md) only selects and connects an `IGestureSource` — it does not own AppKit P/Invoke ([ADR 0006](./adr/0006-gesture-library-vs-mod-surface.md)).

## Policy

| Layer                                      | Stance                                         |
| ------------------------------------------ | ---------------------------------------------- |
| Features, client Outcomes, settings schema | No required OS brand in the capability story   |
| v1 shipping backend                        | **macOS AppKit** — only validated Capture path |
| Windows / Linux                            | Stubs; not supported in v1                     |
| Contacts / IPC / socket bridge             | **Removed** from rewrite v1 — not in the tree  |

## Backend contract

A backend in the gesture library must:

- Emit the shared Capture primitive contract while the game is focused (when configured).
- Emit an **honest finger count** for the active contact set (lesson L4).
- Avoid deciding pan vs orbit vs zoom (Policy style resolve owns that).
- Fail soft when unsupported or disconnected (do not crash the game). Precise trackpad scroll suppress applies while the mod is on; mouse wheel and middle-mouse orbit remain vanilla — see [vanilla camera suppress](./vanilla-camera-suppress.md).

## macOS (v1)

- **AppKit (ship):** in-process AppKit local monitor (scroll / magnify / rotate) → the same primitives, implemented under `rewrite/src`. No Accessibility. Precise scroll deltas drive pan; non-precise (mouse wheel) are not mapped to pan. This is the **only** path playtested for v1.
- **Finger count:** AppKit reports two-finger contact for scroll/magnify/rotate events on the ship path. Maps+ seed chords use two-finger rows only.
- Maps+ orbit modifier defaults to Option (`⌥`).
- Maintainer E2E inject (`InjectGestureSource`) is a test seam only — not a player backend.

## Windows / Linux (stubs)

- Compile-time or runtime “unsupported” path.
- Future: Precision Touchpad or equivalent contact streaming mapped to the same primitives.

## Acceptance

- High-level feature shards describe trackpads and feel presets, not “Mac-only product.”
- README and client install state which backends ship today without rewriting the capability contract.
- Durable docs do not treat a C helper binary or socket host as the Capture path.
- Rewrite v1: AppKit (+ inject harness) live in the gesture library; the mod wires the source only.
