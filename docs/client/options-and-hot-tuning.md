# Options and hot tuning

You can tune Trackpad Camera Control from **two places**. They share the same live **ModSettings**: camera feel updates immediately from either surface, and every change **autosaves** — no restart, and values **survive quit**.

**Debug panel** controls refresh when settings change. **Options** sliders bind when the page is built — after editing in Debug, leave and re-enter Options to see updated slider positions. Debug may show Sensitivity outside the Options **0.1×–2×** range until you move an Options slider (sliders clamp on drag only).

## Where to edit

1. **In-game Debug panel** — floating window while a city is loaded (when Debug is on). Same feel and per-op tunables as Options. See [Debug panel](./debug-ui.md).
2. **Options → Trackpad Camera Control** — same tunables; the Options window title is the **mod name + version**.

On the shipped surface you see Maps+/AppleKit feel controls only. Pad/button chrome, capture-backend / low-pass, and CAD gesture-style switchers stay behind their own flags.

## Feel presets

A **dropdown** lists built-in and named [feel presets](../glossary/feel-preset.md), loads the profile **on select**, and ends with **Save as…**. Dirty edits while a built-in is active move you to **New Preset** and autosave there. Details: [feel presets](./feel-presets.md).

## Layout and per-op sections

Sections appear in order **General → Zoom → Pan → Rotate → Orbit**. Rhythm uses native Colossal **AddGroup**: a short group title with the native glow underline, and controls nested in that group’s Content.

For each camera op, the **title** is on its own line; the next lines state **Maps+ activation** and **Keymapping(s):** with the player’s current Cities binding labels (from Options keymappings, not hardcoded prose). Those binding lines **refresh when keymappings change** via game keymapping events (Debug panel rebuilds; Options descriptions update while the mod tab is open). If nothing is bound, the line still reads **Keymapping(s): none**.

| Op     | Meaning                                              | Activation (Maps+)           | Keymapping(s) (still on)                     |
| ------ | ---------------------------------------------------- | ---------------------------- | -------------------------------------------- |
| Zoom   | Change camera distance / size                        | Pinch                        | Mouse-wheel option label + zoom key bindings |
| Pan    | Slide the camera laterally                           | Two-finger drag              | Edge scrolling option + move key bindings    |
| Rotate | Yaw the camera, or rotate a place/relocate ghost     | Two-finger rotate            | Rotate-left / rotate-right bindings          |
| Orbit  | Pitch + yaw around the pivot (or around a selection) | Option (`⌥`)+two-finger drag | Rotate-camera mouse binding                  |

Product-surface Options controls: **Sensitivity** sliders only (about **0.1×–2×** factory default). Orbit pitch follows vanilla **0–90°** (no separate Pitch min/max controls). There is no Enable-per-op or Reverse UI. Labels say **Sensitivity** (not drag scale).

With a **new place or relocate** ghost: two-finger rotate turns the **ghost**; ⌥+two-finger may orbit around that ghost. Otherwise rotate is camera yaw and ⌥+two-finger orbits from the **current** look-at (no snap to a prior pivot).

## Menus, Options, and popups

When Options or another game menu is open: the mod does **not** apply camera ops — two-finger scroll belongs to the UI.

When the cursor is over an active popup or HUD panel (Debug panel or another mod’s popup): two-finger input scrolls the UI, not the city camera. Keyboard camera may still move unless a text field is focused.

## How to experiment

1. Enable the mod and load a city.
2. Open the Debug panel (or Options → Trackpad Camera Control).
3. Try Slow / Default / Fast from the dropdown, or drag a Sensitivity slider.
4. Perform the gesture — new values apply immediately and autosave.
5. Optionally **Save as…** a named feel profile; quit and relaunch to confirm values came back.

Factory defaults live only in the settings schema. See [install and first run](./install-and-first-run.md) for Maps+ gesture basics.
