# Options and hot tuning

You can tune Trackpad Camera Control from **two places**. They share the same settings: change a value in one, and the other shows it when you open it. Changes apply **without restarting** the game or the mod, and they **survive quit**.

## Where to edit

1. **In-game Assist / tuning panel** — floating window while a city is loaded (when Assist UI is on). Same feel and per-op tunables as Options.
2. **Options → Trackpad Camera Control** — same tunables as number fields and checkboxes, laid out in **multi-column groups by op** (same grouping as the panel).

Assist chrome (drag pads and nudge buttons) and Btn / button-step fields appear only when `EnableAssistChrome` is on. Capture-backend picker and low-pass appear only when `EnableContactsCapture` is on. A CAD gesture-style switcher appears only when `EnableCadGestureStyle` is on. With those flags off (shipped surface), you see Maps+/AppleKit feel controls only — no Btn, LP, CAD, or backend picker.

## Feel presets

The presets row offers **Slow | Default | Fast**, **Save as… / Load**, and **Reset to factory**. These are [feel presets](../glossary/feel-preset.md) (sensitivities and related feel), not gesture-style seeds. Details and Default numbers: [feel presets](./feel-presets.md).

## Per-op sections

For **Pan**, **Zoom**, **Rotate** (yaw), and **Orbit**, each heading briefly states **what it means** and **what activates it** on the shipped Maps+ style:

| Op | Meaning | Activation (Maps+) |
| --- | --- | --- |
| Pan | Slide the camera laterally | Two-finger drag |
| Zoom | Change camera distance / size | Pinch (mouse wheel uses vanilla zoom) |
| Rotate | Yaw around the vertical axis | Two-finger rotate |
| Orbit | Pitch + yaw around the pivot | Option (`⌥`)+two-finger drag |

Controls on the product surface (flags off): **Enable**, **Reverse** (where applicable), **Sensitivity** fields, and for Orbit **Pitch min / max**. Labels say **Sensitivity** (not drag scale). Values must be **greater than zero** and round to **two decimal places**. Pitch min / max also use two decimals; orbit pitch stays within those limits.

## Menus, Options, and popups

When Options or another game menu is open: the mod does **not** apply camera ops — two-finger scroll belongs to the UI.

When the cursor is over an active popup or HUD panel (Assist panel or another mod’s popup): two-finger input scrolls the UI, not the city camera. Keyboard camera may still move unless a text field is focused.

## How to experiment

1. Enable the mod and load a city.
2. Open the floating panel (or Options → Trackpad Camera Control).
3. Try Slow / Default / Fast, or edit a Sensitivity / reverse / Pitch min / max.
4. Perform the gesture — new values apply immediately.
5. Optionally Save as… a named feel profile; quit and relaunch to confirm values came back.

Factory defaults live only in the settings schema. See [install and first run](./install-and-first-run.md) for Maps+ gesture basics.
