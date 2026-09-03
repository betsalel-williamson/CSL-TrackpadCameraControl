# Style binding table

Single source of truth for Policy **resolve**: rows map Capture primitives (finger count, modifiers, pinch, rotate) plus session state into a camera / selection op set.

On the ship surface the table is **seeded** so Maps+ chords match shipping (two-finger pan, pinch zoom, two-finger rotate, Option (`⌥`)+two-finger orbit, with latch and rotate-owned-contact rules as session policy). Resolve must not keep a parallel hardcoded Maps+ path beside the table.

A [feel preset](./feel-preset.md) never rewrites table rows. Player remapping of style rows is not on the ship UI unless a compiled gesture-style switcher exists.
