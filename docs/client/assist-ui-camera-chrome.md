# Assist UI camera chrome

The in-game **Assist / tuning panel** nudges the camera with the same controls as trackpad gestures — useful when you want map- or CAD-style pads and buttons, or when you need to confirm pan / zoom / yaw / orbit work without a working trackpad backend. Tunables in that panel are mirrored in Options (without the chrome).

## Turn it on or off

1. Open Options → Trackpad Camera Control, or use the floating panel itself.
2. Toggle **Assist UI** (panel visibility).
3. Return to the city view — no restart required.

Development builds may default Assist UI **on**. Shipping defaults turn it **off** so gesture-only players keep a clean viewport unless they opt in.

## How to use it

1. Load a city — the floating panel appears when Assist UI is on.
2. Pick **Maps+** or **CAD** (see [gesture presets](./gesture-presets.md)); read the short description.
3. For each op (pan, zoom, rotate, orbit): use the **drag pad** for continuous motion, or the **buttons** for one-shot steps.
4. Type **drag scale** and **button step** numbers; toggle reverse and low-pass as needed.
5. Close the panel if you want a clean view; reopen from the remaining control.

If you disable an axis (for example orbit), that chrome control is unavailable.

## Validate camera controls

With Assist UI on:

1. Nudge each enabled axis from a pad and from a button.
2. Confirm the camera moves.
3. If chrome moves the camera but trackpad gestures do not, the apply path is fine — check the backend and [OS gesture conflicts](./os-gesture-conflicts.md).

Assist UI does not replace installing or connecting a trackpad backend for gesture play.
