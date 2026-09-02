# Sensitivity

Multiplier on continuous camera deltas from the trackpad (and from debug chrome pads when that flagged surface is enabled). Player-facing Options and panel labels say **Sensitivity**, matching Cities: Skylines I camera Options wording.

In code and persisted XML the same multiplier is **gain** (`PanGainX`, `ZoomGain`, …). Default factory values live only in the settings schema. Larger values move the camera more per unit of gesture delta. Product-surface **Sensitivity** sliders run from about **0×** to **2×** the factory default for that field (three-decimal apply after the AppKit scroll unit was folded into defaults).

Option-orbit drag feeds the same **angle velocity** path as middle mouse button drag (vanilla inertia + lerp); Sensitivity / gain is the single multiplier — there is no separate scroll “scale” constant.

**Math (after optional filter / low-pass when that path is active):** `scaled = raw * gain`, then apply sign invert if set. Pan also multiplies by camera `Size` before the camera-relative XZ write. Zoom: `Size' = Size * (1 - scaledPinch)`. Rotate: add scaled delta to `AngleX`. Orbit drag: `AddAngleVelocity` (middle mouse button path) — yaw + pitch around the pivot. Orbit pitch uses vanilla 0–90°; drag floors at 0; button steps clamp to that range.

**Synonyms** (same multiplier): UI **Sensitivity**; code/XML **gain**; older docs [drag scale](./drag-scale.md), speed, scale.

**Not** the same as [button step](./button-step.md): buttons use a separate one-shot size and are not multiplied by gain. Button-step UI is gated off the product surface.
