# Settings schema

Logical schema for ModSettings. Field names in source may differ; this shard is the contract. Defaults belong only in the settings defaults factory — not in camera update logic.

## Identity

| Field         | Type                        | Default  | Hot |
| ------------- | --------------------------- | -------- | --- |
| GesturePreset | enum: MapsPlus, CAD, Custom | MapsPlus | yes |

## Enables

| Field           | Type | Default                          | Hot |
| --------------- | ---- | -------------------------------- | --- |
| AssistUiEnabled | bool | true (development); false (ship) | yes |
| PanEnabled      | bool | true                             | yes |
| ZoomEnabled     | bool | true                             | yes |
| YawEnabled      | bool | true                             | yes |
| OrbitEnabled    | bool | true                             | yes |

`AssistUiEnabled` shows or hides the optional [Assist UI](../glossary/assist-ui.md) chrome. Chrome style follows `GesturePreset` (no separate style field). Development defaults keep Assist UI on for easier camera-path validation; shipping defaults turn it off.

## Orbit trigger

| Field        | Type                                                | Default               | Hot |
| ------------ | --------------------------------------------------- | --------------------- | --- |
| OrbitTrigger | enum: ModifierPlusTwoFinger, ThreeFinger, Both, Off | ModifierPlusTwoFinger | yes |

Maps+ seeds ModifierPlusTwoFinger (Option on macOS). CAD seeds ThreeFinger.

## Sensitivities

| Field                 | Type  | Default seed | Hot |
| --------------------- | ----- | ------------ | --- |
| PanSensitivityX       | float | 1.0          | yes |
| PanSensitivityY       | float | 1.0          | yes |
| OrbitYawSensitivity   | float | 1.0          | yes |
| OrbitPitchSensitivity | float | 1.0          | yes |
| ZoomSensitivity       | float | 1.0          | yes |
| YawRotateSensitivity  | float | 1.0          | yes |

Exact default numbers are tuned during implementation; change them only in the defaults factory and document the new seeds here.

## Inverts

| Field            | Type | Default | Hot |
| ---------------- | ---- | ------- | --- |
| InvertPanX       | bool | false   | yes |
| InvertPanY       | bool | false   | yes |
| InvertOrbitYaw   | bool | false   | yes |
| InvertOrbitPitch | bool | false   | yes |
| InvertZoom       | bool | false   | yes |
| InvertYawRotate  | bool | false   | yes |

## Thresholds and smoothing

| Field                 | Type             | Default seed   | Hot |
| --------------------- | ---------------- | -------------- | --- |
| MotionDeadzone        | float            | small positive | yes |
| PinchEpsilon          | float            | small positive | yes |
| RotateEpsilon         | float            | small positive | yes |
| FingerCountHysteresis | float            | small positive | yes |
| Smoothing             | float 0–1 or off | off / 0        | yes |

## Gates and bridge

| Field            | Type | Default | Hot |
| ---------------- | ---- | ------- | --- |
| RequireGameFocus | bool | true    | yes |
| IgnoreOverUi     | bool | true    | yes |
| BridgeEnabled    | bool | true    | yes |
| DebugOverlay     | bool | false   | yes |

## Validation rule

Camera and gesture modules must read these fields at use-time. A contributor checklist: no magic numbers for feel outside the defaults factory.
