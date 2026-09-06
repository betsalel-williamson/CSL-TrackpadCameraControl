# Feel presets

A [feel preset](../glossary/feel-preset.md) stores how the camera _feels_: Sensitivity values. It does **not** change [gesture style](../glossary/gesture-style.md) — ship play stays Maps+ (AppKit), including Option (`⌥`)+two-finger for [orbit](../glossary/orbit.md).

Use the feel-preset **dropdown** in Options → Trackpad Camera Control or the in-game Debug panel.

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
| Capture                       | AppKit                       |
| Orbit pitch                   | vanilla **0** / **90**       |

**Sensitivity** sliders use a **[0, 1]** UI track mapped piecewise to about **0.1×–2×** factory (mid = Default).

## Dropdown, New Preset, Save as…, and Delete

Selecting an entry in the dropdown **loads** that profile immediately.

If you edit while a built-in (or any named preset) is active, the active identity becomes **New Preset** and the change **autosaves** there (Options and Debug dropdowns update live). **Save as…** becomes **enabled** when you are on New Preset. Click it to open a name dialog prefilled with the next free **New Preset 1**, **New Preset 2**, … (or the current named preset if you dirty a named profile). You cannot save as Slow / Default / Fast; saving over another named preset replaces it without a confirm. Cancel leaves New Preset unsaved as a named profile. Named profiles persist with your other mod settings across quit.

**Delete** is **enabled** only when the active feel is a **named user preset** you saved — not Slow / Default / Fast, and not New Preset. Click it to remove that named profile immediately (no confirm). The active feel switches to **Default**. Disabled Save as… / Delete labels show **grey** text.

## Reset to factory

Restores the Default profile above and saves it. Does not switch gesture style away from Maps+.

## Not a feel preset

Which fingers trigger orbit is [gesture style](../glossary/gesture-style.md), not a feel preset. Ship DLL: Maps+ only.
