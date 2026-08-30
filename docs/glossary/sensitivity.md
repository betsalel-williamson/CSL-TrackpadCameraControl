# Sensitivity

Multiplier on continuous camera deltas from the trackpad (and from debug chrome pads when that flagged surface is enabled). Player-facing Options and panel labels say **Sensitivity**, matching Cities: Skylines I camera Options wording.

Default factory values live only in the settings schema. Larger values move the camera more per unit of gesture delta. Product-surface **Sensitivity** sliders run from about **0.1×** to **2×** the factory default for that field.

**Math (after optional low-pass when that path is active):** `scaled = raw * Sensitivity`, then apply invert if set. Pan also multiplies by camera `Size` before the camera-relative XZ write. Zoom: `Size' = Size * (1 - scaledPinch)`. Yaw / orbit: add scaled angle deltas to `AngleX` / `AngleY`. Orbit pitch is then clamped to Pitch min / max (7–90°).

**Synonyms** (same multiplier; prefer **Sensitivity** in UI and new docs): [drag scale](./drag-scale.md), speed, scale. Settings field names remain `*Sensitivity*` (for example `PanSensitivityX`).

**Not** the same as [button step](./button-step.md): buttons use a separate one-shot size and are not multiplied by Sensitivity. Button-step UI is gated off the product surface.
