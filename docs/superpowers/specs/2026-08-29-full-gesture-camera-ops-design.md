# Full gesture camera ops — Design

**Date:** 2026-08-29  
**Status:** Approved  
**Scope:** Pan, zoom, yaw, and orbit end-to-end with concurrent resolve modes and orbit latch; no Options UI

## Goal

Trackpad players get full Maps+/CAD camera fluency (pan, pinch zoom, rotate yaw, latched orbit) without a mouse. Settings stay in-memory / `ApplyPreset` until Options UI lands.

## Locked decisions

| Concern       | Choice                                                           |
| ------------- | ---------------------------------------------------------------- |
| Scope         | Gesture completeness only                                        |
| Preset schema | Keep Custom / OrbitTrigger.Both; defaults exclusive Maps+ vs CAD |
| Resolve modes | Concurrent (default), SessionLock, PrimaryOnly                   |
| Orbit latch   | Hold until touch-up even if modifier released                    |
| While latched | Orbit only; no yaw rotate, pan, or zoom                          |
| Architecture  | Op-set resolver + GestureSession + ICameraController             |

## Architecture

```text
GestureFrame → GestureSession → BindingResolver (op set)
            → CameraApplicator → ICameraController → CS1 CameraController
```

## Deferred (later phases)

1. **Options UI** — Maps+/CAD dropdown, resolve mode, all tunables.
2. **Assist UI chrome** — existing assist-ui design/shards; not wired in this slice.
3. **Harmony Options checkbox** — deferred. Vanilla scroll-zoom and mouse-drag rotate are suppressed whenever the mod is on; see `2026-08-29-vanilla-camera-suppress-design.md`.

## Acceptance

- Maps+ defaults: pan, zoom, yaw, modifier+two-finger orbit in-game.
- `ApplyPreset(CAD)` → three-finger orbit on next gesture.
- Orbit latch and Concurrent / SessionLock / PrimaryOnly behave per feature/settings contracts.
- One-finger tools remain usable; missing bridge fails soft.
