# ADR 0006 — Gesture library vs CSL mod surface

## Status

Accepted (rewrite target)

## Context

Trackpad capture is useful beyond Cities: Skylines I. Shipping and the first rewrite clone mixed OS sampling, Unity concerns, and CSL-specific policy/UI in one assembly. That blocked reuse and produced overlapping imports (AppKit beside `ICities`, feel UI beside P/Invoke).

Players still need a CSL-specific feel surface and Maps+ camera policy. The under-the-hood redesign must separate **what any Unity title could reuse** from **what only this mod owns**.

## Decision

1. **Three stack layers:** native OS → [gesture library](../../glossary/gesture-library.md) → [CSL mod surface](../../glossary/mod-surface.md). See [under the hood](../under-the-hood.md).

2. **Gesture library** (`rewrite/src`) owns the shared primitive frame, backend interface, OS→frame mappers, and inject test seam. It may include optional **game-agnostic** UnityEngine helpers. It must not reference `ICities`, ColossalUI, Cities Harmony, Maps+ seeds, or feel catalog/editor types.

3. **CSL mod surface** (`rewrite/mod`) owns style table + Maps+ seed, feel catalog/editor/store, Options/Debug hosts, Policy resolve, pure FeelMath, thin Cities adapters, and Harmony suppress/orbit flush. It references the library and consumes frames. It must not own AppKit / Multitouch P/Invoke.

4. **Non-overlapping imports** are a required consequence. Each project and preferably each file imports only its layer’s dependencies. Automated layer-import lint fails the gate on violations. Tests use fakes that stand in for **one** subsystem (OS, Unity engine, or game libraries).

## Consequences

- A second Unity game can consume the library without bringing CSL Options or Harmony.
- CSL can swap OS backends without rewriting feel UI or resolve.
- Tick planes remain Capture → Policy → Apply inside the mod’s simulation tick; Capture _implementation_ lives in the library.
- Root shipping `src/TrackpadCapture` Contacts/IPC experiments are not revived; the library is greenfield from the frame contract ([platform backends](../platform-backends.md)).
