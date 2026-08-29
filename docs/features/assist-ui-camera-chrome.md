# Assist UI camera chrome

## Intent

Give players an optional on-screen assist surface for [pan](../glossary/pan.md), [zoom](../glossary/zoom.md), [yaw](../glossary/yaw.md), and [orbit](../glossary/orbit.md) that matches map-app or CAD viewport conventions — and that proves the same camera pipeline gestures use, without requiring Multitouch capture.

## End-user outcomes

- Enable or disable [Assist UI](../glossary/assist-ui.md) from Options (hot).
- Use minimal corner chrome for quick zoom and yaw; expand for pan and orbit.
- Maps+ or CAD chrome style follows the Gesture preset.
- Confirm camera ops work in-game even when the trackpad backend is missing or flaky.

## Form

Hybrid chrome:

1. **Minimal corner chrome** — zoom ±, yaw / compass nudge, expand.
2. **Expanded panel** — pan pad plus orbit control (Maps+: compass + tilt; CAD: viewcube / gizmo-style), with the same zoom and yaw controls.

When enabled, chrome **auto-hides** after idle and **reveals** when the cursor approaches that corner (or while interacting).

## Control contract

- Assist UI emits the same **camera ops** as trackpad gestures.
- Ops flow through the shared apply path (sensitivities, inverts, per-op enables).
- Assist UI does **not** write the camera through a second path and does **not** synthesize OS Multitouch frames.
- Disabled ops (pan / zoom / yaw / orbit) hide or grey out matching chrome controls and must not fire.

## Style contract

| Gesture preset | Chrome feel                                       |
| -------------- | ------------------------------------------------- |
| Maps+          | Map-app chrome (zoom stack, compass ring)         |
| CAD            | Viewport / viewcube-style orbit affordance        |
| Custom         | Last applied seed’s chrome until a preset re-seed |

There is no separate Assist UI style enum — Gesture preset owns style.

## Options

| Setting           | Default (development) | Default (ship) | Hot |
| ----------------- | --------------------- | -------------- | --- |
| Assist UI enabled | On                    | Off            | yes |

Related live settings: Gesture preset, per-op enables, sensitivities, inverts. See [settings and hot configuration](./settings-and-hot-configuration.md).

## Validation boundary

Chrome motion proves applicator + settings. It does **not** prove platform capture or the bridge — those remain separate checks.

## Acceptance criteria

- With Assist UI on, approaching the corner reveals chrome; idle auto-hides.
- Zoom and yaw from minimal chrome move the camera; expanded pan and orbit do too.
- Turning Assist UI off removes chrome immediately (no restart).
- Switching Maps+ ↔ CAD updates chrome feel on the next reveal (no restart).
- Disabled camera ops do not fire from chrome.
- Gestures and Assist UI share one apply path in the same session.
- One-finger building tools remain usable outside the chrome.

## Non-goals

- Replacing trackpad gestures as the primary input.
- ACME camera-suite features (saved positions, zoom limits, free-cam).
- Injecting fake Multitouch frames from the assist UI.
