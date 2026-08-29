# Button step

Size of one Assist chrome nudge-button click for a camera op (pan, zoom, yaw, orbit). Player-facing label: **button step**. Settings fields are named `*ButtonScale*` (for example `PanButtonScaleX`, `ZoomButtonScale`) — same value, synonym.

**Button math:** `delta = sign * buttonStep` (per axis), then apply invert if set, then the same camera write as drag (pan × `Size`, zoom size factor, yaw/orbit angle add). **Does not** multiply by [drag scale](./drag-scale.md) / [sensitivity](./sensitivity.md). Skips [low-pass](./low-pass.md).
