# Options and hot tuning

Open **Options → Trackpad Camera Control** while a city is loaded. Changes apply **without restarting** the game or the mod.

## What you can change (this slice)

- **Interpreter:** AppKit (current) or Contacts (legacy Multitouch). AppKit is the default. Contacts is the older contact-stream path, kept for comparison while tuning.
- **Sensitivity:** pan X / pan Y, orbit yaw / pitch, zoom, and two-finger yaw rotate.

If `TRACKPAD_CAPTURE_BACKEND` is set in the environment that launched the game, that value wins over the interpreter dropdown until you unset it.

## How to experiment

1. Enable the mod and load a city.
2. Open Options → Trackpad Camera Control.
3. Change a sensitivity slider, or switch interpreter.
4. Perform the gesture again — new values apply immediately.

Factory defaults live only in the settings schema. Nothing about feel is baked into the camera code as a fixed constant.

Preset, invert, deadzone, smoothing, and Assist UI controls are not in Options yet; they still use in-memory defaults.
