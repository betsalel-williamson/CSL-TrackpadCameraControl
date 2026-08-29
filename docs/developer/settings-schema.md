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

`AssistUiEnabled` shows or hides the in-game Assist / tuning panel (chrome + tunables). Development defaults keep it on for easier camera-path validation; shipping defaults turn it off.

## Gesture resolve mode

| Field              | Type                                       | Default    | Hot |
| ------------------ | ------------------------------------------ | ---------- | --- |
| GestureResolveMode | enum: Concurrent, SessionLock, PrimaryOnly | Concurrent | yes |

See [gesture resolve mode](../glossary/gesture-resolve-mode.md). PrimaryOnly priority when multiple candidates exist: Orbit > Zoom > Yaw > Pan.

## Orbit trigger

| Field        | Type                                                | Default               | Hot |
| ------------ | --------------------------------------------------- | --------------------- | --- |
| OrbitTrigger | enum: ModifierPlusTwoFinger, ThreeFinger, Both, Off | ModifierPlusTwoFinger | yes |

Maps+ seeds ModifierPlusTwoFinger (Option on macOS). CAD seeds ThreeFinger. [Orbit latch](../glossary/orbit-latch.md) always applies when orbit engages.

## Drag scales

Used by trackpad gestures and Assist chrome drag pads. UI label: **drag scale**.

| Field                 | Type  | Default seed | Hot |
| --------------------- | ----- | ------------ | --- |
| PanSensitivityX       | float | 1.0          | yes |
| PanSensitivityY       | float | 1.0          | yes |
| OrbitYawSensitivity   | float | 1.0          | yes |
| OrbitPitchSensitivity | float | 1.0          | yes |
| ZoomSensitivity       | float | 1.0          | yes |
| YawRotateSensitivity  | float | 1.0          | yes |

## Button steps

Used by Assist chrome nudge buttons only. Not multiplied by drag scale. UI label: **button step**.

| Field                | Type  | Default seed | Hot |
| -------------------- | ----- | ------------ | --- |
| PanButtonScaleX      | float | 0.05         | yes |
| PanButtonScaleY      | float | 0.05         | yes |
| OrbitYawButtonScale  | float | 2.0          | yes |
| OrbitPitchButtonScale| float | 2.0          | yes |
| ZoomButtonScale      | float | 0.05         | yes |
| YawRotateButtonScale | float | 2.0          | yes |

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

## Thresholds

| Field                 | Type  | Default seed   | Hot |
| --------------------- | ----- | -------------- | --- |
| MotionDeadzone        | float | small positive | yes |
| PinchEpsilon          | float | small positive | yes |
| RotateEpsilon         | float | small positive | yes |
| FingerCountHysteresis | float | small positive | yes |

## Per-op low-pass (drag only)

EMA on drag deltas after resolve, before apply. Buttons skip low-pass. The former single `Smoothing` field is retired.

| Field               | Type      | Default | Hot |
| ------------------- | --------- | ------- | --- |
| PanLowPassEnabled   | bool      | false   | yes |
| PanLowPassAlpha     | float 0–1 | 0.3     | yes |
| ZoomLowPassEnabled  | bool      | false   | yes |
| ZoomLowPassAlpha    | float 0–1 | 0.3     | yes |
| YawLowPassEnabled   | bool      | false   | yes |
| YawLowPassAlpha     | float 0–1 | 0.3     | yes |
| OrbitLowPassEnabled | bool      | false   | yes |
| OrbitLowPassAlpha   | float 0–1 | 0.3     | yes |

## Gates and capture

| Field            | Type                          | Default       | Hot |
| ---------------- | ----------------------------- | ------------- | --- |
| RequireGameFocus | bool                          | true          | yes |
| IgnoreOverUi     | bool                          | true          | yes |
| BridgeEnabled    | bool                          | false         | yes |
| CaptureBackend   | enum: Contacts, AppleGestures | AppleGestures | yes |
| DebugOverlay     | bool                          | false         | yes |

`CaptureBackend` selects the in-process interpreter: **AppleGestures** (default, **current**) is AppKit scroll/magnify/rotate (no Accessibility). **Contacts** is the legacy MultitouchSupport path. Launch override: `TRACKPAD_CAPTURE_BACKEND=apple` or `contacts` (env wins when set).

## Persist envelope

Live settings load and save through a versioned XML file under the Cities user-data tree (`…/TrackpadCameraControl/settings.xml`). Document shape:

| Element        | Role                                                         |
| -------------- | ------------------------------------------------------------ |
| schemaVersion  | Envelope version                                             |
| current        | Full ModSettings blob (what the mod reads and writes today)  |
| userPresets[]  | Reserved empty; Save as… / Load named presets later          |

Missing or corrupt file → factory defaults (no crash), then persist the recovered blob. **Reset to factory** restores schema defaults into `current` and writes the file. Built-in Maps+ / CAD stay in-code seeds (`ApplyPreset`), not rows in `userPresets`.

Options and the in-game Assist / tuning panel both bind the same fields through one apply layer; number fields (not sliders) edit drag scales, button steps, and low-pass params.

## Validation rule

Camera and gesture modules must read these fields at use-time. A contributor checklist: no magic numbers for feel outside the defaults factory.
