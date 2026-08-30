# Sensitivity

Multiplier on continuous camera deltas from the trackpad (and from Assist chrome pads when that surface is enabled). Player-facing Options and panel labels say **Sensitivity**, matching Cities: Skylines I camera Options wording.

Default factory values live only in the settings schema. Larger values move the camera more per unit of gesture delta. Product-surface fields accept any value **greater than zero** and round to **two decimal places** (`0.xx`).

**Math (after optional low-pass when that path is active):** `scaled = raw * Sensitivity`, then apply invert if set. Pan also multiplies by camera `Size` before the camera-relative XZ write. Zoom: `Size' = Size * (1 - scaledPinch)`. Yaw / orbit: add scaled angle deltas to `AngleX` / `AngleY`. Orbit pitch is then clamped to Pitch min / max.

**Synonyms** (same multiplier; prefer **Sensitivity** in UI and new docs): [drag scale](./drag-scale.md), speed, scale. Settings field names remain `*Sensitivity*` (for example `PanSensitivityX`).

**Not** the same as [button step](./button-step.md): buttons use a separate one-shot size and are not multiplied by Sensitivity. Button-step UI is gated by `EnableAssistChrome`.
