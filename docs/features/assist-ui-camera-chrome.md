# Assist UI camera chrome

## Intent

Give players an on-screen assist surface for [pan](../glossary/pan.md), [zoom](../glossary/zoom.md), [yaw](../glossary/yaw.md), and [orbit](../glossary/orbit.md) that matches map-app or CAD viewport conventions — and that proves the same camera pipeline gestures use, without requiring Multitouch capture. Chrome lives in the **in-game Assist / tuning panel** for this phase (corner auto-hide is deferred).

## End-user outcomes

- Show or hide the in-game Assist / tuning panel (hot).
- Drive pan, zoom, yaw, and orbit from drag pads and nudge buttons in that panel.
- Tune the same feel parameters beside the chrome (mirrored in Options without chrome).
- Confirm camera ops work in-game even when the trackpad backend is missing or flaky.

## Form

One floating panel while a city is loaded:

1. Preset picker (Maps+ / CAD), description, Reset to factory.
2. Per-op sections with chrome (pad + buttons) and tunables (enable, reverse, drag scale, button step, low-pass).
3. Closable; a small control reopens it. Development defaults keep the panel on.

Corner auto-hide chrome from the earlier hybrid design remains a later refinement.

## Control contract

- Assist UI emits the same **camera ops** as trackpad gestures.
- **Drag** pads use **drag scale** and optional per-op low-pass.
- **Buttons** use **button step**, skip low-pass, and are not multiplied by drag scale.
- Ops flow through the shared apply path (inverts, per-op enables).
- Assist UI does **not** write the camera through a second path and does **not** synthesize OS Multitouch frames.
- Disabled ops hide or grey out matching chrome controls and must not fire.

## Style contract

| Gesture preset | Chrome feel                                       |
| -------------- | ------------------------------------------------- |
| Maps+          | Map-app chrome (zoom stack, compass-style yaw)    |
| CAD            | Viewport-style orbit affordance                   |
| Custom         | Last applied seed’s chrome until a preset re-seed |

There is no separate Assist UI style enum — Gesture preset owns style.

## Options

| Setting           | Default (development) | Default (ship) | Hot |
| ----------------- | --------------------- | -------------- | --- |
| Assist UI enabled | On                    | Off            | yes |

Related live settings: Gesture preset, per-op enables, drag scales, button steps, inverts, low-pass. See [settings and hot configuration](./settings-and-hot-configuration.md). Cities Options mirrors the tunables but does not host chrome.

## Validation boundary

Chrome motion proves applicator + settings. It does **not** prove platform capture or the bridge — those remain separate checks.

## Acceptance criteria

- With Assist UI on, the floating panel shows chrome and tunables while a city is loaded.
- Zoom / yaw / pan / orbit from pads and buttons move the camera through the shared apply path.
- Turning Assist UI off removes the panel immediately (no restart).
- Switching Maps+ ↔ CAD updates orbit seed on the next gesture without wiping custom scales.
- Disabled camera ops do not fire from chrome.
- Gestures and Assist UI share one apply path in the same session.
- One-finger building tools remain usable outside the panel.

## Non-goals

- Replacing trackpad gestures as the primary input.
- ACME camera-suite features (saved positions, zoom limits, free-cam).
- Injecting fake Multitouch frames from the assist UI.
- Assist chrome inside the Cities Options page.
