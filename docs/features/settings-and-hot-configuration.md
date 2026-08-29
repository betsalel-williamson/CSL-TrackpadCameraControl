# Settings and hot configuration

## Intent

No feel or binding parameter is hardcoded in camera or gesture logic. Defaults exist only in the settings schema. Players and developers experiment mid-session; Options UI will expose the same fields later.

## Presets

| Preset | Role                                                      |
| ------ | --------------------------------------------------------- |
| Maps+  | Default seed — map-app-aligned; modifier+two-finger orbit |
| CAD    | Seed — three-finger orbit                                 |
| Custom | Any manual override after editing a seeded field          |

Applying a preset copies that preset’s defaults into editable fields (including orbit trigger). Reset restores the selected preset or factory defaults. Maps+ and CAD are exclusive seeds; the schema still allows Custom and `OrbitTrigger.Both` for later Options UI.

## Tunables (all hot)

- Gesture preset
- [Gesture resolve mode](../glossary/gesture-resolve-mode.md): Concurrent (default) / SessionLock / PrimaryOnly
- [Assist UI](./assist-ui-camera-chrome.md) enabled (chrome master switch; style follows Gesture preset)
- Per-op enable: pan, zoom, yaw, orbit
- Orbit trigger: modifier+two-finger / three-finger / both / off
- [Orbit latch](../glossary/orbit-latch.md) behavior (always on when orbit engages — not a separate toggle)
- Per-op sensitivity and invert
- Deadzones and thresholds (motion, pinch, rotate, finger-count hysteresis)
- Smoothing factor or off
- Require game focus; ignore when cursor over UI
- Backend enable / reconnect / debug overlay
- Capture backend: Contacts (Multitouch / bridge) or AppleGestures (in-process AppKit); `TRACKPAD_CAPTURE_BACKEND` env overrides when set

## Orbit latch (contract)

Once orbit engages from the configured trigger, the session stays in orbit until touch-up (Ended / Cancelled / zero fingers), even if the modifier key is released. While latched: orbit and yaw rotate may apply; pan and zoom do not — regardless of Concurrent resolve mode.

## Hot-apply contract

- Settings updates (Options later, or in-memory / `ApplyPreset` now) take effect immediately.
- Binding resolver, gesture session, and camera applicator read live settings each frame or via change callbacks.
- No mod disable/enable cycle required for tuning.
- Prefer raw primitive streaming from the backend so feel changes never need a native restart.

## Acceptance

- Change pan sensitivity mid-drag (or between drags) and observe the new scale without restart.
- Switch Maps+ → CAD via `ApplyPreset` (or Options when shipped) and have three-finger orbit work on the next gesture.
- Grep of camera/gesture logic finds no numeric feel literals outside the settings defaults factory.
