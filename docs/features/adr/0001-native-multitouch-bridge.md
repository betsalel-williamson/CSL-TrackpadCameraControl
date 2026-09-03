# ADR 0001 — OS trackpad bridge

## Status

Accepted (amended: in-process capture in the mod DLL)

## Context

Cities: Skylines I does not expose multi-finger trackpad gestures to mods. Synthetic middle-mouse injection loses pinch and rotate fidelity and fights UI. Each OS needs its own capture path, but the mod should share one primitive stream and binding layer.

CS1 Workshop norms ship a managed C# DLL. True multitouch on macOS needs private MultitouchSupport-style APIs that can break across OS updates. An out-of-process TrackpadBridge host was useful while capture was untrusted, but playtesting already requires rebuilding the mod and restarting the game. A companion process plus IPC is extra latency and an extra moving part without a shorter iteration loop.

A separate C helper duplicated Multitouch and wire logic beside the mod. Capture lives in managed C# compiled into the mod DLL.

## Decision

Capture OS contacts and Apple-classified gestures **in-process** in the mod DLL behind a shared **gesture primitive** contract and a single `IGestureSource` seam. Two macOS interpreters (flag-selected):

| Interpreter       | Role                                                                                      |
| ----------------- | ----------------------------------------------------------------------------------------- |
| **Contacts**      | MultitouchSupport contacts → primitives (portable template for other OSes).               |
| **AppleGestures** | AppKit local monitor (scroll / magnify / rotate) → the same primitives. No Accessibility. |

Inspect capture via a **session capture log** (`TRACKPAD_CAPTURE_LOG`). The C# TrackpadBridge console host remains in the repo as an optional dev socket experiment; it is not the playtest path. The prior **C** helper is **retired**. Binding and camera writes always stay in C#. Ship the **macOS** path first; Windows and Linux remain stubs behind the same interface.

Language surface for the mod DLL: **net35** with **C# 9**, using Mono-safe BCL APIs only.

## Consequences

- True pinch / multi-finger gestures become possible where a backend exists.
- Playtest loop is rebuild mod → restart game. Capture logs append to a session file for inspection.
- Private or unstable OS APIs can break — isolate per backend and fail soft.
- Product docs stay platform-neutral; backend specifics stay in [platform backends](../platform-backends.md).
- Contributors validate capture and bindings with xUnit and harnesses documented under the developer guide.
