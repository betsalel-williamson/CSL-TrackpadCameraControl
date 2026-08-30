# Platform backends

## Intent

Keep product language and Options **platform-neutral**. Isolate OS capture behind a shared gesture-primitive contract and `IGestureSource`. Capture runs **in-process** in the mod DLL (C#).

## Policy

| Layer                                      | Stance                                                                  |
| ------------------------------------------ | ----------------------------------------------------------------------- |
| Features, client Outcomes, settings schema | No required OS brand in the capability story                            |
| First shipping backend                     | **macOS** AppleKit (AppleGestures). Contacts behind `EnableContactsCapture` |
| Windows / Linux                            | Stubs with the same interface; contributor implementations welcome      |

## Backend contract

A backend must:

- Emit [gesture primitives](./ipc-gesture-primitives.md) while the game is focused (when configured).
- Avoid deciding pan vs orbit vs zoom (C# bindings own that).
- Fail soft when unsupported or disconnected (do not crash the game). Vanilla scroll-zoom and mouse-rotate stay gated by [vanilla camera suppress](./vanilla-camera-suppress.md) while the mod is on.

## macOS (v1)

- **AppleGestures / AppleKit (shipped):** in-process AppKit local monitor (scroll / magnify / rotate → same primitives). No Accessibility. Precise scroll deltas drive pan; non-precise (mouse wheel) are not mapped to pan.
- **Contacts:** in-process MultitouchSupport contacts → primitives. Product UI and default path only when `EnableContactsCapture` is on (or `TRACKPAD_CAPTURE_BACKEND=contacts` for maintainers).
- Interpreters may log frames to a capture log file (path overridable with `TRACKPAD_CAPTURE_LOG`).
- Maps+ orbit modifier defaults to Option (`⌥`).
- Client notes for Mission Control live under the client guide’s platform conflicts shard.

## Windows / Linux (stubs)

- Compile-time or runtime “unsupported” path.
- Future: Precision Touchpad or equivalent contact streaming mapped to the same primitives.

## Acceptance

- High-level feature shards describe trackpads and presets, not “Mac-only product.”
- README and client install state which backends ship today without rewriting the capability contract.
- Durable docs do not treat a C helper binary as the capture path.
