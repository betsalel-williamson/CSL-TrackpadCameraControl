# Settings and hot configuration

## Intent

No feel or binding parameter is hardcoded in camera or gesture logic. Defaults exist only in the settings schema. Players and developers experiment mid-session from the **in-game Assist / tuning panel** or **Options → Trackpad Camera Control**. Both surfaces edit the same live ModSettings. Values persist across quit.

## Surfaces

| Surface                         | Tunables                                                                 | Assist chrome (pads / buttons) |
| ------------------------------- | ------------------------------------------------------------------------ | ------------------------------ |
| In-game Assist / tuning panel   | Preset, Reset, enables, inverts, drag scales, button steps, low-pass     | Yes                            |
| Options → Trackpad Camera Control | Same tunables (number fields, not sliders)                             | No (needs a live camera)       |

One binding layer parses, clamps, and writes fields. A change on either surface updates live settings immediately and schedules a durable write.

## Presets

| Preset | Role                                                      |
| ------ | --------------------------------------------------------- |
| Maps+  | Default seed — map-app-aligned; modifier+two-finger orbit |
| CAD    | Seed — three-finger orbit                                 |
| Custom | Any manual override after editing a seeded field          |

Applying Maps+ or CAD seeds the orbit trigger and does **not** wipe custom scales. **Reset to factory** restores schema defaults and writes them to disk. Named **Save as… / Load** user presets are reserved on the persist envelope for a later slice.

## Tunables (all hot)

- Gesture preset (Maps+ / CAD) with short seed descriptions
- [Assist UI](./assist-ui-camera-chrome.md) enabled (in-game panel master switch)
- Per-op enable: pan, zoom, yaw (rotate), orbit
- Orbit trigger: modifier+two-finger / three-finger / both / off
- [Orbit latch](../glossary/orbit-latch.md) behavior (always on when orbit engages — not a separate toggle)
- Per-op **drag scale** and **button step** (separate; buttons are not multiplied by drag scale)
- Per-op invert / reverse
- Deadzones and thresholds (motion, pinch, rotate, finger-count hysteresis)
- Per-op low-pass enable + alpha (drag only; buttons skip)
- Require game focus; ignore when cursor over UI
- Capture backend: **AppleGestures** (default) or **Contacts** (legacy); `TRACKPAD_CAPTURE_BACKEND` env overrides when set

## Durable persist

- Load once at mod enable (before the gesture pipeline starts).
- Write after successful field applies (debounced) and flush on mod disable, city unload, and panel close.
- Envelope: `schemaVersion`, `current` (full settings), reserved empty `userPresets[]`.
- Missing or corrupt file → factory defaults, then persist the recovered blob.

## Orbit latch (contract)

Once orbit engages from the configured trigger, the session stays in orbit until touch-up (Ended / Cancelled / zero fingers), even if the modifier key is released. While latched: orbit and yaw rotate may apply; pan and zoom do not — regardless of Concurrent resolve mode.

## Hot-apply contract

- Settings updates from either UI or `ApplyPreset` take effect immediately.
- Binding resolver, gesture session, and camera applicator read live settings each frame.
- No mod disable/enable cycle required for tuning.
- Prefer raw primitive streaming from the backend so feel changes never need a native restart.

## Acceptance

- Change a drag scale in the in-game panel and see the same number in Options (and the reverse) without restart.
- Quit and relaunch; tunables are restored from disk.
- Reset to factory clears values on both surfaces and persists.
- Switch Maps+ → CAD and have three-finger orbit work on the next gesture without wiping custom scales.
- Grep of camera/gesture logic finds no numeric feel literals outside the settings defaults factory.
