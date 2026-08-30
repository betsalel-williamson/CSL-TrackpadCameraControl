# Feel presets

A [feel preset](../glossary/feel-preset.md) stores how the camera *feels*: per-op enables, reverse flags, [Sensitivity](../glossary/sensitivity.md) values, and orbit Pitch min / max. It does **not** change [gesture style](../glossary/gesture-style.md) — shipped play stays [Maps+](../glossary/maps-plus-preset.md) (AppleKit), including Option (`⌥`)+two-finger for [orbit](../glossary/orbit.md).

Use the feel-preset row in Options → Trackpad Camera Control or the in-game Assist / tuning panel.

## Built-in profiles

| Profile | What it does |
| --- | --- |
| **Default** | Factory / playtest feel (same values **Reset to factory** restores) |
| **Slow** | Default’s Sensitivity fields × **0.75**, rounded to two decimals |
| **Fast** | Default’s Sensitivity fields × **1.25**, rounded to two decimals |

Slow and Fast keep the same reverse flags and Pitch min / max as Default. They only scale Sensitivity.

### Default (factory) numbers

| Setting | Value |
| --- | --- |
| Pan Reverse X / Y | on / off |
| Pan Sensitivity X / Y | 0.50 |
| Zoom Sensitivity | 1.00 |
| Yaw (rotate) Sensitivity | 2.00 |
| Orbit yaw / pitch Sensitivity | 10.00 / 10.00 |
| Gesture style | Maps+ (`⌥`+two-finger orbit) |
| Capture | AppleKit |
| Orbit Pitch min / max | Starter pair (for example −80 / 80); editable in the UI |

Sensitivity fields accept any value **greater than zero** and display at **two decimal places**.

## Save as… / Load

**Save as…** stores a named profile of the full feel set (enables, reverse, sensitivities, pitch limits, and other product-surface feel fields). **Load** restores a named profile. Named profiles persist with your other mod settings across quit.

## Reset to factory

Restores the Default profile above and saves it. Does not switch gesture style away from Maps+.

## Not a feel preset

Maps+ vs CAD orbit triggers are [gesture style](../glossary/gesture-style.md), not feel presets. With CAD off the product surface, there is no Maps+/CAD preset switcher — see [gesture presets](./gesture-presets.md).
