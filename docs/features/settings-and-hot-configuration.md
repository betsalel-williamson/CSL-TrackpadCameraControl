# Settings and hot configuration

## Intent

No feel or binding parameter is hardcoded in camera or gesture logic. Defaults exist only in the settings schema. Players and developers experiment mid-session from the Options panel.

## Presets

| Preset | Role                                                      |
| ------ | --------------------------------------------------------- |
| Maps+  | Default seed — map-app-aligned; modifier+two-finger orbit |
| CAD    | Seed — three-finger orbit                                 |
| Custom | Any manual override after editing a seeded field          |

Applying a preset copies that preset’s defaults into editable fields. Reset restores the selected preset or factory defaults.

## Tunables (all hot)

- Gesture preset
- [Assist UI](./assist-ui-camera-chrome.md) enabled (chrome master switch; style follows Gesture preset)
- Per-op enable: pan, zoom, yaw, orbit
- Orbit trigger: modifier+two-finger / three-finger / both / off
- Per-op sensitivity and invert
- Deadzones and thresholds (motion, pinch, rotate, finger-count hysteresis)
- Smoothing factor or off
- Require game focus; ignore when cursor over UI
- Backend enable / reconnect / debug overlay

## Hot-apply contract

- Options writes update ModSettings immediately (or on slider release).
- Binding resolver and camera applicator read live settings each frame or via change callbacks.
- No mod disable/enable cycle required for tuning.
- Prefer raw primitive streaming from the backend so feel changes never need a native restart.

## Acceptance

- Change pan sensitivity mid-drag (or between drags) and observe the new scale without restart.
- Switch Maps+ → CAD and have three-finger orbit work on the next gesture.
- Grep of camera/gesture logic finds no numeric feel literals outside the settings defaults factory.
