# Feel presets

A [feel preset](../glossary/feel-preset.md) stores how the camera _feels_: [Sensitivity](../glossary/sensitivity.md) values. It does **not** change [gesture style](../glossary/gesture-style.md) — shipped play stays [Maps+](../glossary/maps-plus-preset.md) (AppleKit), including Option (`⌥`)+two-finger for [orbit](../glossary/orbit.md).

Use the feel-preset **dropdown** in Options → Trackpad Camera Control or the in-game [Debug panel](./debug-ui.md).

## Built-in profiles

| Profile     | What it does                                                        |
| ----------- | ------------------------------------------------------------------- |
| **Default** | Factory / playtest feel (same values **Reset to factory** restores) |
| **Slow**    | Default’s Sensitivity values × **0.75**, rounded to three decimals  |
| **Fast**    | Default’s Sensitivity values × **1.25**, rounded to three decimals  |

Built-ins (**Slow**, **Default**, **Fast**) are **immutable** — the mod never overwrites them. Slow and Fast only scale Sensitivity.

### Default (factory) numbers

| Setting                       | Value                        |
| ----------------------------- | ---------------------------- |
| Pan Sensitivity X / Y         | 0.005 / 0.005                |
| Zoom Sensitivity              | 1.00                         |
| Yaw (rotate) Sensitivity      | 2.00                         |
| Orbit yaw / pitch Sensitivity | 1.00 / 1.00                  |
| Gesture style                 | Maps+ (`⌥`+two-finger orbit) |
| Capture                       | AppleKit                     |
| Orbit pitch                   | vanilla **0** / **90**       |

**Sensitivity** sliders use a **[0, 1]** UI track mapped piecewise to about **0.1×–2×** factory (mid = Default).

## Dropdown, New Preset, and Save as…

Selecting an entry in the dropdown **loads** that profile immediately.

If you edit while a built-in (or any named preset) is active, the active identity becomes **New Preset** and the change **autosaves** there (Options and Debug dropdowns update live). **Save as…** (next to Reset on Debug; in Options General) becomes **enabled** when you are on New Preset. Click it to open a name dialog prefilled with the next free **New Preset 1**, **New Preset 2**, … (or the current named preset if you dirty a named profile). Type or edit the name, then Cancel / OK. You cannot save as Slow / Default / Fast; saving over another named preset replaces it without a confirm. Cancel leaves New Preset unsaved as a named profile. Named profiles persist with your other mod settings across quit.

## Reset to factory

Restores the Default profile above and saves it. Does not switch gesture style away from Maps+.

## Not a feel preset

Which fingers trigger orbit is [gesture style](../glossary/gesture-style.md), not a feel preset. v1 ships Maps+ only — see [gesture presets](./gesture-presets.md).
