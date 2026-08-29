# ADR 0001 — Native OS trackpad bridge

## Status

Accepted (amended for dual-path capture)

## Context

Cities: Skylines I does not expose multi-finger trackpad gestures to mods. Synthetic middle-mouse injection loses pinch and rotate fidelity and fights UI. Each OS needs its own capture path, but the mod should share one primitive stream and binding layer.

CS1 Workshop norms ship a managed C# DLL. True multitouch on macOS needs private MultitouchSupport-style APIs that can crash or break across OS updates. During development, isolating that capture outside the game process is valuable; for shipping, the preferred shape is in-process so packaging matches other mods.

## Decision

Capture OS contacts behind a shared **gesture primitive** contract and a single `IGestureSource` seam in C#. Two backends:

| Path                    | Role                                                                                                                                                                     |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Dev / isolation**     | Small native helper (TrackpadBridge) streams primitives over local IPC. Prefer this while validating capture, for crash isolation, restart, and hot-swap of native code. |
| **Deploy / in-process** | Same Multitouch capture loaded in-process (bundled native library or P/Invoke) into the mod DLL. Target Workshop-shaped packaging once capture is trusted.               |

Ship the **macOS** Multitouch path first; Windows and Linux remain stubs behind the same interface. Binding and camera writes always stay in C#.

## Consequences

- True pinch / multi-finger gestures become possible where a backend exists.
- Dev builds may run a companion helper beside the mod; deploy builds aim for in-process only.
- Private or unstable OS APIs can break — isolate per backend and fail soft.
- Product docs stay platform-neutral; backend specifics stay in [platform backends](../platform-backends.md).
