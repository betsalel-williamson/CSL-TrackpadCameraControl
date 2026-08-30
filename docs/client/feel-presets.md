# Feel presets

A [feel preset](../glossary/feel-preset.md) stores how the camera *feels*: [Sensitivity](../glossary/sensitivity.md) values. It does **not** change [gesture style](../glossary/gesture-style.md) — shipped play stays [Maps+](../glossary/maps-plus-preset.md) (AppleKit), including Option (`⌥`)+two-finger for [orbit](../glossary/orbit.md).

Use the feel-preset **dropdown** in Options → Trackpad Camera Control or the in-game [Debug panel](./debug-ui.md).

## Built-in profiles

| Profile | What it does |
| --- | --- |
| **Default** | Factory / playtest feel (same values **Reset to factory** restores) |
| **Slow** | Default’s Sensitivity values × **0.75**, rounded to two decimals |
| **Fast** | Default’s Sensitivity values × **1.25**, rounded to two decimals |

Built-ins (**Slow**, **Default**, **Fast**) are **immutable** — the mod never overwrites them. Slow and Fast only scale Sensitivity.

### Default (factory) numbers

| Setting | Value |
| --- | --- |
| Pan Sensitivity X / Y | 0.50 |
| Zoom Sensitivity | 1.00 |
| Yaw (rotate) Sensitivity | 2.00 |
| Orbit yaw / pitch Sensitivity | 10.00 / 10.00 |
| Gesture style | Maps+ (`⌥`+two-finger orbit) |
| Capture | AppleKit |
| Orbit pitch | vanilla **0** / **90** |

**Sensitivity** sliders run from about **0.1×** to **2×** the factory default for that field.

## Dropdown, New Preset, and Save as…

Selecting an entry in the dropdown **loads** that profile immediately. **Save as…** is the last entry: it stores a named copy of the current feel set; after save, that named preset stays selected.

If you edit while a built-in (or any named preset) is active, the active identity becomes **New Preset** and the change **autosaves** there. Further edits after **Save as…** dirties to **New Preset** again the same way. Named profiles persist with your other mod settings across quit.

## Reset to factory

Restores the Default profile above and saves it. Does not switch gesture style away from Maps+.

## Not a feel preset

Maps+ vs CAD orbit triggers are [gesture style](../glossary/gesture-style.md), not feel presets. With CAD off the product surface, there is no Maps+/CAD preset switcher — see [gesture presets](./gesture-presets.md).
