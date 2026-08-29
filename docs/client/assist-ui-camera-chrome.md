# Assist UI camera chrome

Optional on-screen [Assist UI](../glossary/assist-ui.md) nudges the camera with the same controls as trackpad gestures — useful when you want map- or CAD-style buttons, or when you need to confirm pan / zoom / yaw / orbit work without a working trackpad backend.

## Turn it on or off

1. Open Options → Trackpad Camera Control.
2. Toggle **Assist UI**.
3. Return to the city view — no restart required.

Development builds may default Assist UI **on**. Shipping defaults turn it **off** so gesture-only players keep a clean viewport unless they opt in.

## How to use it

1. Move the cursor toward the assist chrome corner — the compact controls appear.
2. Use **zoom + / −** and the **yaw / compass** nudge.
3. Expand for the **pan** pad and **orbit** control.
4. Move away and idle — chrome auto-hides again.

Chrome style follows your [Gesture preset](./gesture-presets.md):

- **Maps+** — map-app style (zoom stack, compass).
- **CAD** — viewport / viewcube-style orbit.
- **Custom** — keeps the last preset’s chrome until you re-apply Maps+ or CAD.

If you disable an axis in Options (for example orbit), that chrome control is unavailable.

## Validate camera controls

With Assist UI on:

1. Expand the chrome (or use compact zoom / yaw).
2. Nudge each enabled axis and confirm the camera moves.
3. If chrome moves the camera but trackpad gestures do not, the apply path is fine — check the backend / bridge and [OS gesture conflicts](./os-gesture-conflicts.md).

Assist UI does not replace installing or connecting a trackpad backend for gesture play.
