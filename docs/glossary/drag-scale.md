# Drag scale

Multiplier on continuous camera deltas from the trackpad or Assist chrome drag pads. Player-facing Options and panel labels say **drag scale**. The settings fields are named `*Sensitivity*` (for example `PanSensitivityX`, `ZoomSensitivity`) — same value; see [sensitivity](./sensitivity.md).

Default is `1.0` (identity). Larger values move the camera more per unit of gesture delta; `0` freezes that axis for drag input.

**Drag math (after optional low-pass):** `scaled = raw * dragScale`, then apply invert if set. Pan also multiplies by camera `Size` before the camera-relative XZ write. Zoom: `Size' = Size * (1 - scaledPinch)`. Yaw / orbit: add scaled angle deltas to `AngleX` / `AngleY`.

**Not** the same as [button step](./button-step.md): buttons use a separate one-shot size and are **not** multiplied by drag scale.
