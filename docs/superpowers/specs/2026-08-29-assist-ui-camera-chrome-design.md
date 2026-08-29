# Assist UI camera chrome — Design

**Date:** 2026-08-29  
**Status:** Approved (docs only; implementation deferred)  
**Scope:** Optional in-game assist UI for pan, zoom, yaw, and orbit that shares the mod camera-op path

## Goal

Players and contributors can drive pan, zoom, yaw, and orbit from on-screen chrome that feels like map-app or CAD viewport UI, enable or disable that chrome from Options, and use it to confirm the in-game camera pipeline works without relying on Multitouch capture.

## Locked decisions

| Concern           | Choice                                                                                           |
| ----------------- | ------------------------------------------------------------------------------------------------ |
| Form              | Hybrid — minimal corner chrome + expandable full-axes panel                                      |
| Visibility        | Auto-hide after idle; reveal when the cursor approaches the chrome corner (or while interacting) |
| Style             | Follows Gesture preset (Maps+ → map chrome; CAD → CAD chrome; Custom → last applied seed style)  |
| Control path      | Same camera ops as gestures (shared apply path); no second camera writer                         |
| Master switch     | Options: Assist UI enabled (hot)                                                                 |
| Default (dev)     | On                                                                                               |
| Default (ship)    | Off                                                                                              |
| Multitouch inject | Out of scope for assist UI                                                                       |

## Architecture

```text
[Trackpad gestures] ─┐
                     ├─→ Camera ops ─→ Applicator ─→ CS1 camera
[Assist UI chrome] ──┘
         ↑
   ModSettings (AssistUiEnabled, GesturePreset, per-op enables, feel)
```

Gestures resolve primitives into camera ops. Assist UI emits the same ops. Sensitivities, inverts, and per-op enables apply identically. Assist UI does not synthesize OS Multitouch frames.

## UI composition

### Minimal chrome

- Zoom + / −
- Yaw / compass nudge
- Expand control

### Expanded panel

- Pan pad
- Orbit control (Maps+: compass + tilt; CAD: viewcube / gizmo-style)
- Same zoom and yaw controls (not a second binding)

### Style

- **Maps+:** flat map-app chrome (zoom stack, compass ring)
- **CAD:** denser viewport / viewcube-style orbit affordance
- **Custom:** last applied seed’s chrome until a preset is re-applied

## Options contract

| Field                   | Behavior                                               |
| ----------------------- | ------------------------------------------------------ |
| Assist UI enabled       | Master show/hide; hot-applied                          |
| Gesture preset          | Owns chrome style (no separate Assist UI style enum)   |
| Per-op enables          | Disabled ops hide or grey out matching chrome controls |
| Sensitivities / inverts | UI nudges use live ModSettings values                  |

## Validation story

Using chrome and seeing the camera move proves the applicator + settings path. It does **not** prove the OS capture backend — that remains a separate hardware / bridge check.

## Acceptance (when implemented)

- With Assist UI on, approaching the corner reveals chrome; idle auto-hides.
- Zoom / yaw from chrome move the camera; expanded pan / orbit do too.
- Turning Assist UI off removes chrome immediately.
- Switching Maps+ ↔ CAD updates chrome feel on next reveal (no restart).
- Disabled camera ops do not fire from chrome.
- Gestures and chrome share one apply path in the same session.

## Non-goals

- Replacing trackpad gestures as the primary input
- ACME-style saved views / free-cam suite
- Injecting fake Multitouch frames from the assist UI
- Implementing UI in the docs pass that approved this design

## Related durable shards

- Feature: Assist UI camera chrome
- Client: Assist UI camera chrome
- Settings schema / Options client notes
- Glossary: Assist UI
