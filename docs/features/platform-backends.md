# Platform backends

## Intent

Keep product language and Options **platform-neutral**. Isolate OS capture behind a shared gesture-primitive contract and `IGestureSource`. Capture runs **in-process** in the mod DLL (C#).

## Policy

| Layer                                      | Stance                                                                             |
| ------------------------------------------ | ---------------------------------------------------------------------------------- |
| Features, client Outcomes, settings schema | No required OS brand in the capability story                                       |
| First shipping backend                     | **macOS AppKit** (AppleGestures) — only validated path                             |
| Windows / Linux                            | Stubs; not supported in v1                                                         |
| Contacts (MultitouchSupport)               | **Future / unfinished** — code may remain behind `EnableContactsCapture`; not QA’d |
| TrackpadBridge socket host                 | Dev experiment only (`BridgeEnabled` off); not playtest                            |

## Backend contract

A backend must:

- Emit [gesture primitives](./ipc-gesture-primitives.md) while the game is focused (when configured).
- Avoid deciding pan vs orbit vs zoom (C# bindings own that).
- Fail soft when unsupported or disconnected (do not crash the game). Precise trackpad scroll suppress applies while the mod is on; mouse wheel and middle-mouse orbit remain vanilla — see [vanilla camera suppress](./vanilla-camera-suppress.md).

## macOS (v1)

- **AppleGestures / AppKit (shipped):** in-process AppKit local monitor (scroll / magnify / rotate → same primitives). No Accessibility. Precise scroll deltas drive pan; non-precise (mouse wheel) are not mapped to pan. This is the **only** path playtested for v1.
- **Contacts (future):** MultitouchSupport contact streaming was an early alternate interpreter. It is **not** productized: compile flag `EnableContactsCapture` may still exist for experiments, but Contacts was **not** troubleshot for ship and must not be offered to players or treated as a maintainer QA path. Revisit only with a dedicated validation pass.
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
