# Sensitivity

Multiplier on continuous camera deltas from the trackpad (and from debug chrome pads when that flagged surface is enabled). Player-facing Options and panel labels say **Sensitivity**, matching Cities: Skylines I camera Options wording.

Default factory values live only in the settings schema. Larger values move the camera more per unit of gesture delta. Product-surface **Sensitivity** sliders run from about **0.1×** to **2×** the factory default for that field (four-decimal apply after the AppKit scroll unit was folded into defaults).

Option-orbit drag feeds the same **angle velocity** path as middle mouse button drag (vanilla inertia + lerp); Sensitivity is the single gain — there is no separate scroll “scale” constant.

**Math (after optional low-pass when that path is active):** `scaled = raw * Sensitivity`, then apply invert if set. Pan also multiplies by camera `Size` before the camera-relative XZ write. Zoom: `Size' = Size * (1 - scaledPinch)`. Yaw: add scaled delta to `AngleX`. Orbit drag: `AddAngleVelocity` (middle mouse button path). Orbit pitch limits still apply at the edges / on button steps.

**Synonyms** (same multiplier; prefer **Sensitivity** in UI and new docs): [drag scale](./drag-scale.md), speed, scale. Settings field names remain `*Sensitivity*` (for example `PanSensitivityX`).

**Not** the same as [button step](./button-step.md): buttons use a separate one-shot size and are not multiplied by Sensitivity. Button-step UI is gated off the product surface.
