# Options and hot tuning

You can tune Trackpad Camera Control from **two places**. They share the same settings: change a number in one, and the other shows it when you open it. Changes apply **without restarting** the game or the mod, and they **survive quit**.

## Where to edit

1. **In-game Assist / tuning panel** — floating window while a city is loaded. Includes Assist chrome (drag pads and nudge buttons) plus all tunables.
2. **Options → Trackpad Camera Control** — same tunables as number fields and checkboxes (no sliders). No chrome here; use the in-game panel to feel drag vs buttons.

## What you can change

- **Gesture preset:** Maps+ or CAD, with a short description of each seed (orbit trigger differs; scales stay yours until Reset).
- **Reset to factory default** — restores schema defaults and saves them.
- Per-op **enable** and **reverse** for pan, zoom, rotate (yaw), and orbit.
- **Drag scale** — continuous motion from the trackpad or chrome pads.
- **Button step** — one-shot nudges from chrome buttons (not multiplied by drag scale).
- **Low-pass** — enable/disable plus alpha per op (smooths drag only; buttons skip it).
- **Interpreter:** AppKit (current) or Contacts (legacy Multitouch), when you need to compare capture paths.

If `TRACKPAD_CAPTURE_BACKEND` is set in the environment that launched the game, that value wins over the interpreter dropdown until you unset it.

**Drag scale** vs **button step:** drag scale multiplies continuous deltas (trackpad and pads). Button step is the size of one chrome click. Use both when comparing how the same axis feels as a drag versus a tap.

## How to experiment

1. Enable the mod and load a city.
2. Open the floating panel (or Options → Trackpad Camera Control).
3. Type a drag scale or toggle reverse / low-pass.
4. Use a pad or button (in-game) or perform the gesture — new values apply immediately.
5. Quit and relaunch to confirm the values came back.

Factory defaults live only in the settings schema. Named Save as… / Load presets come later; Reset to factory is available now on both surfaces.
