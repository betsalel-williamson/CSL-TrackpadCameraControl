# Gesture style

How trackpad gestures map to camera ops — especially how [orbit](./orbit.md) is triggered. The shipped style is [Maps+](./maps-plus-preset.md) (AppleKit): two-finger pan, pinch zoom, two-finger yaw, Option (`⌥`)+two-finger orbit.

[CAD](./cad-preset.md) (three-finger orbit) remains in code behind `EnableCadGestureStyle` and is not on the product surface while that flag is off.

Gesture style is **not** a [feel preset](./feel-preset.md) (Sensitivity / Slow–Fast / Save as…). Feel presets apply the same way on top of Maps+, CAD, or a future OS layout — they never rewrite per-op gesture bindings.
