# ADR 0001 — OS trackpad bridge

## Status

Accepted (amended for C# capture dual-path)

## Context

Cities: Skylines I does not expose multi-finger trackpad gestures to mods. Synthetic middle-mouse injection loses pinch and rotate fidelity and fights UI. Each OS needs its own capture path, but the mod should share one primitive stream and binding layer.

CS1 Workshop norms ship a managed C# DLL. True multitouch on macOS needs private MultitouchSupport-style APIs that can crash or break across OS updates. During development, isolating that capture outside the game process is valuable; for shipping, the preferred shape is in-process so packaging matches other mods.

A separate C helper duplicated Multitouch and wire logic beside the mod. Capture now lives in managed C# so one library serves both the isolation host and the eventual in-process path.

## Decision

Capture OS contacts in a shared **C#** TrackpadCapture library behind a shared **gesture primitive** contract and a single `IGestureSource` seam. Two backends:

| Path                    | Role                                                                                                                                                                             |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Dev / isolation**     | Thin TrackpadBridge **console host** (references TrackpadCapture) streams primitives over local IPC. Prefer this while validating capture, for crash isolation and easy restart. |
| **Deploy / in-process** | Same TrackpadCapture logic loaded in-process into the mod DLL. Target Workshop-shaped packaging once capture is trusted.                                                         |

The prior **C** TrackpadBridge helper is **retired**. Binding and camera writes always stay in C#. Ship the **macOS** Multitouch path first; Windows and Linux remain stubs behind the same interface.

Language surface for mod-loaded assemblies: **netstandard2.0** with **C# 9**, using Mono-safe BCL APIs only.

## Consequences

- True pinch / multi-finger gestures become possible where a backend exists.
- Dev builds may run a companion C# bridge host beside the mod; deploy builds aim for in-process only.
- Private or unstable OS APIs can break — isolate per backend and fail soft.
- Product docs stay platform-neutral; backend specifics stay in [platform backends](../platform-backends.md).
- Contributors validate capture and bindings with xUnit and harnesses documented under the developer guide.
