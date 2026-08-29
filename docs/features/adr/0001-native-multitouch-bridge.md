# ADR 0001 — Native OS trackpad bridge

## Status

Accepted

## Context

Cities: Skylines I does not expose multi-finger trackpad gestures to mods. Synthetic middle-mouse injection loses pinch and rotate fidelity and fights UI. Each OS needs its own capture path, but the mod should share one primitive stream and binding layer.

## Decision

Use a small **native OS helper** (TrackpadBridge) per platform that streams raw gesture primitives over local IPC to the C# mod, which drives CameraController. Ship the **macOS** backend first; Windows and Linux remain stubs behind the same interface.

## Consequences

- True pinch / multi-finger gestures become possible where a backend exists.
- Packaging may include a native binary beside the mod on supported platforms.
- Private or unstable OS APIs can break — isolate per backend and fail soft.
- Product docs stay platform-neutral; backend specifics stay in [platform backends](../platform-backends.md).
