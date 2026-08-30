# Options and hot tuning

You can tune Trackpad Camera Control from **two places**. They share the same live settings: change a value in one, and the other shows it right away. Every change **autosaves** — no restart, and values **survive quit**.

## Where to edit

1. **In-game Debug panel** — floating window while a city is loaded (when Debug is on). Same feel and per-op tunables as Options. See [Debug panel](./debug-ui.md).
2. **Options → Trackpad Camera Control** — same tunables; the Options window title is the **mod name + version**.

On the shipped surface you see Maps+/AppleKit feel controls only. Pad/button chrome, capture-backend / low-pass, and CAD gesture-style switchers stay behind their own flags.

## Feel presets

A **dropdown** lists built-in and named [feel presets](../glossary/feel-preset.md), loads the profile **on select**, and ends with **Save as…**. Dirty edits while a built-in is active move you to **New Preset** and autosave there. Details: [feel presets](./feel-presets.md).

## Layout and per-op sections

Sections appear in order **General → Zoom → Pan → Rotate → Orbit**. Rhythm: prior content → horizontal rule → section title → rows.

For each camera op, a short heading states **what it means** and **what activates it** on the shipped Maps+ style:

| Op | Meaning | Activation (Maps+) |
| --- | --- | --- |
| Zoom | Change camera distance / size | Pinch (mouse wheel uses vanilla zoom) |
| Pan | Slide the camera laterally | Two-finger drag (stays within city bounds) |
| Rotate | Yaw the camera, or rotate a selected object | Two-finger rotate |
| Orbit | Pitch + yaw around the pivot (or around a selection) | Option (`⌥`)+two-finger drag |

Product-surface controls: **Sensitivity** sliders (about **0.1×–2×** factory default) and, for Orbit, **Pitch min / max** (**7–90°**, always above zero). There is no Enable-per-op or Reverse UI. Labels say **Sensitivity** (not drag scale).

With a **selection**: two-finger rotate turns the **selected object**; ⌥+two-finger **orbits the camera around** that object. With **no selection**, rotate is camera yaw and ⌥+two-finger is normal orbit.

## Menus, Options, and popups

When Options or another game menu is open: the mod does **not** apply camera ops — two-finger scroll belongs to the UI.

When the cursor is over an active popup or HUD panel (Debug panel or another mod’s popup): two-finger input scrolls the UI, not the city camera. Keyboard camera may still move unless a text field is focused.

## How to experiment

1. Enable the mod and load a city.
2. Open the Debug panel (or Options → Trackpad Camera Control).
3. Try Slow / Default / Fast from the dropdown, or drag a Sensitivity / Pitch min / max control.
4. Perform the gesture — new values apply immediately and autosave.
5. Optionally **Save as…** a named feel profile; quit and relaunch to confirm values came back.

Factory defaults live only in the settings schema. See [install and first run](./install-and-first-run.md) for Maps+ gesture basics.
