# Button step

Size of one debug chrome nudge-button click for a camera op. Player-facing label when chrome is enabled: **button step**. Settings fields are named `*ButtonScale*`.

**Button math:** `delta = sign * buttonStep` (per axis), then apply invert if set, then the same camera write as continuous input. **Does not** multiply by [Sensitivity](./sensitivity.md). Skips [low-pass](./low-pass.md).

Button-step UI and chrome stay off the product surface (flag-gated). Mouse and keyboard camera stay with the game.
