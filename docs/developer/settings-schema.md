# Settings schema

Logical schema for ModSettings. Field names in source may differ; this shard is the contract. Defaults belong only in the settings defaults factory — not in camera update logic.

Canonical UI term: **[Sensitivity](../glossary/sensitivity.md)**. Synonyms in older docs/code: drag scale, speed, scale. Product floats round to **two decimal places**; Sensitivity values must be **> 0**.

Product-surface gates: [feature flags](./feature-flags.md). Planning: [AppleKit Maps+ feel surface design](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md).

## Identity / gesture style

| Field         | Type                        | Default  | Hot |
| ------------- | --------------------------- | -------- | --- |
| GesturePreset | enum: MapsPlus, CAD, Custom | MapsPlus | yes |

Schema-retained. With `EnableCadGestureStyle` off, product UI does not expose a Maps+/CAD switcher; shipped style is Maps+ (`⌥`+two-finger orbit). Gesture style is **not** a [feel preset](../glossary/feel-preset.md).

## Enables

| Field           | Type | Default                          | Hot |
| --------------- | ---- | -------------------------------- | --- |
| AssistUiEnabled | bool | true (development); false (ship) | yes |
| PanEnabled      | bool | true                             | yes |
| ZoomEnabled     | bool | true                             | yes |
| YawEnabled      | bool | true                             | yes |
| OrbitEnabled    | bool | true                             | yes |

Schema field `AssistUiEnabled` shows or hides the in-game **Debug** panel (feel presets + tunables). Product UI labels it Debug; the schema name stays `AssistUiEnabled`. Development defaults keep it on for easier camera-path validation; shipping defaults turn it off. Assist **chrome** (pads / nudge buttons) is separate and gated by `EnableAssistChrome`.

## Gesture resolve mode

| Field              | Type                                       | Default    | Hot |
| ------------------ | ------------------------------------------ | ---------- | --- |
| GestureResolveMode | enum: Concurrent, SessionLock, PrimaryOnly | Concurrent | yes |

See [gesture resolve mode](../glossary/gesture-resolve-mode.md). PrimaryOnly priority when multiple candidates exist: Orbit > Zoom > Yaw > Pan. Schema-retained; not required on the slim product surface.

## Orbit trigger

| Field        | Type                                                | Default               | Hot |
| ------------ | --------------------------------------------------- | --------------------- | --- |
| OrbitTrigger | enum: ModifierPlusTwoFinger, ThreeFinger, Both, Off | ModifierPlusTwoFinger | yes |

Maps+ uses ModifierPlusTwoFinger (Option on macOS). CAD would use ThreeFinger when `EnableCadGestureStyle` is on. [Orbit latch](../glossary/orbit-latch.md) always applies when orbit engages.

## Sensitivity (factory Default feel)

Used by trackpad gestures (and Assist chrome pads when `EnableAssistChrome` is on). Field names use `*Sensitivity*`.

| Field                 | Type  | Factory Default | Hot |
| --------------------- | ----- | --------------- | --- |
| PanSensitivityX       | float | 0.50            | yes |
| PanSensitivityY       | float | 0.50            | yes |
| ZoomSensitivity       | float | 1.00            | yes |
| YawRotateSensitivity  | float | 2.00            | yes |
| OrbitYawSensitivity   | float | 10.00           | yes |
| OrbitPitchSensitivity | float | 10.00           | yes |

**Numeric policy:** each Sensitivity must be **> 0**; parse/apply and display round to two decimals.

**Product Sensitivity sliders:** for each axis, UI range is **0.1×–2×** that axis’s factory Default value, with step ≈ **10%** of the factory default (still rounded to two decimals; values must stay **> 0**).

## Orbit pitch limits

| Field         | Type  | Factory Default | Hot |
| ------------- | ----- | --------------- | --- |
| OrbitPitchMin | float | 7.00            | yes |
| OrbitPitchMax | float | 90.00           | yes |

Applied as a clamp after orbit pitch writes. Both limits and live pitch must stay **> 0** (no non-positive pitch). Display/store at two decimals. No yaw angle clamp in this contract. **UI:** Pitch min / max appear on the **Debug panel** only (develop); Options does not expose angle limit fields.

## Button steps

Used by Assist chrome nudge buttons only — product UI when `EnableAssistChrome` is on. Schema-retained while the flag is off. UI label: **button step**. Field names use `*ButtonScale*`. **Not** multiplied by Sensitivity.

| Field                 | Type  | Default seed | Hot |
| --------------------- | ----- | ------------ | --- |
| PanButtonScaleX       | float | 0.05         | yes |
| PanButtonScaleY       | float | 0.05         | yes |
| OrbitYawButtonScale   | float | 2.00         | yes |
| OrbitPitchButtonScale | float | 2.00         | yes |
| ZoomButtonScale       | float | 0.05         | yes |
| YawRotateButtonScale  | float | 2.00         | yes |

Exact button-step seeds may be tuned in the defaults factory; document new seeds here when they change. Product floats still round to two decimals.

## Apply math (contract)

Let `raw` be the resolved gesture delta for that axis (centroid, pinch, or rotate). Optional [low-pass](../glossary/low-pass.md) may replace `raw` with an EMA-smoothed value on the continuous path only — and only when Contacts capture is enabled (`EnableContactsCapture`).

**Invert:** if the matching invert flag is on, multiply the signed delta by `-1` after scaling.

### Continuous path (trackpad; chrome pads when flagged on)

| Op    | After Sensitivity                                                                                    | Camera write                                                              |
| ----- | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Pan   | `mx = dx * PanSensitivityX`, `my = dy * PanSensitivityY`, then `mx,my *= Size`                       | Camera-relative XZ: `target += right*mx + forward*my`                     |
| Zoom  | `delta = pinch * ZoomSensitivity`                                                                    | `Size' = Size * (1 - delta)` (clamped)                                    |
| Yaw   | `delta = rotate * YawRotateSensitivity`                                                              | `AngleX' = AngleX + delta`                                                |
| Orbit | `dyaw = dx * OrbitYawSensitivity`, `dpitch = dy * OrbitPitchSensitivity`                             | `AngleX' += dyaw`, `AngleY' += dpitch`, then clamp pitch to min / max     |

### Button path (chrome nudges only; `EnableAssistChrome`)

Build a one-shot delta from the button step and a sign (`±1`), then apply invert and the same camera write as above. **Do not** multiply by Sensitivity. Skip low-pass.

| Op    | One-shot input before invert                                      |
| ----- | ----------------------------------------------------------------- |
| Pan   | `dx = signX * PanButtonScaleX`, `dy = signY * PanButtonScaleY`    |
| Zoom  | `pinch = sign * ZoomButtonScale`                                  |
| Yaw   | `rotate = sign * YawRotateButtonScale`                            |
| Orbit | `dx = signYaw * OrbitYawButtonScale`, `dy = signPitch * OrbitPitchButtonScale` |

### Low-pass (continuous only; Contacts)

When enabled for an op under Contacts capture: first sample seeds state; later `smoothed += alpha * (raw - smoothed)`. Reset on touch-up. Buttons skip this stage.

## Inverts

| Field            | Type | Factory Default | Hot |
| ---------------- | ---- | --------------- | --- |
| InvertPanX       | bool | true            | yes |
| InvertPanY       | bool | false           | yes |
| InvertOrbitYaw   | bool | false           | yes |
| InvertOrbitPitch | bool | false           | yes |
| InvertZoom       | bool | false           | yes |
| InvertYawRotate  | bool | false           | yes |

Factory Default feel: Pan Reverse X on, Y off (playtest Maps+).

## Thresholds

| Field                 | Type  | Default seed   | Hot |
| --------------------- | ----- | -------------- | --- |
| MotionDeadzone        | float | small positive | yes |
| PinchEpsilon          | float | small positive | yes |
| RotateEpsilon         | float | small positive | yes |
| FingerCountHysteresis | float | small positive | yes |

Schema-retained; not required on the slim product surface.

## Per-op low-pass (Contacts only)

EMA on continuous deltas after resolve, before apply — see glossary **low-pass**. Product UI and processing when `EnableContactsCapture` is on. Buttons skip low-pass. The former single `Smoothing` field is retired.

| Field               | Type      | Default | Hot |
| ------------------- | --------- | ------- | --- |
| PanLowPassEnabled   | bool      | false   | yes |
| PanLowPassAlpha     | float 0–1 | 0.30    | yes |
| ZoomLowPassEnabled  | bool      | false   | yes |
| ZoomLowPassAlpha    | float 0–1 | 0.30    | yes |
| YawLowPassEnabled   | bool      | false   | yes |
| YawLowPassAlpha     | float 0–1 | 0.30    | yes |
| OrbitLowPassEnabled | bool      | false   | yes |
| OrbitLowPassAlpha   | float 0–1 | 0.30    | yes |

## Gates and capture

| Field            | Type                          | Default       | Hot |
| ---------------- | ----------------------------- | ------------- | --- |
| RequireGameFocus | bool                          | true          | yes |
| IgnoreOverUi     | bool                          | true          | yes |
| BridgeEnabled    | bool                          | false         | yes |
| CaptureBackend   | enum: Contacts, AppleGestures | AppleGestures | yes |
| DebugOverlay     | bool                          | false         | yes |

`CaptureBackend` selects the in-process interpreter: **AppleGestures** (default, shipped) is AppKit scroll/magnify/rotate (no Accessibility). **Contacts** is the legacy MultitouchSupport path — product UI when `EnableContactsCapture` is on. Launch override: `TRACKPAD_CAPTURE_BACKEND=apple` or `contacts` (env wins when set).

**IgnoreOverUi** (default on): when the pointer is over any active popup / HUD panel, skip mod camera ops from two-finger; leave scroll to UI. **Menu / Options open** is a separate, stronger gate (no mod camera; UI owns scroll). Precise trackpad vs mouse-wheel scroll split lives with [vanilla camera suppress](../glossary/vanilla-camera-suppress.md).

## Feel presets and persist envelope

Primary player model: **[feel presets](../glossary/feel-preset.md)** (sensitivities, reverse, enables, pitch limits) — not gesture-style seeds.

| Profile | Contract |
| ------- | -------- |
| Default / Reset to factory | Factory Default table above (InvertPanX true; Sensitivity seeds; OrbitPitchMin/Max 7–90) |
| Slow | Default Sensitivity fields × **0.75**; reverse and pitch limits unchanged; round to two decimals |
| Fast | Default Sensitivity fields × **1.25**; reverse and pitch limits unchanged; round to two decimals |
| **New Preset** | Scratch identity when the player dirties an active built-in or named preset; autosave writes here; built-ins Slow / Default / Fast are never overwritten |
| Named Save as… / Load | Full feel set in `userPresets[]`; after Save as…, the named preset is selected; further edits dirty back to **New Preset** |

| Field | Type | Role | Hot |
| ----- | ---- | ---- | --- |
| ActiveFeelPresetName | string | Active feel identity in the preset dropdown (built-in name, named user preset, or **New Preset**) | yes |

Live settings load and save through a versioned XML file under the Cities user-data tree (`…/TrackpadCameraControl/settings.xml`):

| Element        | Role                                                        |
| -------------- | ----------------------------------------------------------- |
| schemaVersion  | Envelope version                                            |
| current        | Full ModSettings blob (includes active feel preset name)    |
| userPresets[]  | Named feel profiles for Save as… / Load                     |

Missing or corrupt file → factory defaults (no crash), then persist the recovered blob. **Reset to factory** restores schema defaults into `current` and writes the file.

GesturePreset / CAD, CaptureBackend / Contacts, button steps, and low-pass remain in the schema for flagged surfaces; they are not the primary preset model.

Options and the in-game Debug panel both bind the same fields through one apply layer; every change autosaves. Number fields edit Sensitivity (sliders), pitch limits, and (when flagged) button steps and low-pass params.

## Validation rule

Camera and gesture modules must read these fields at use-time. A contributor checklist: no magic numbers for feel outside the defaults factory.
