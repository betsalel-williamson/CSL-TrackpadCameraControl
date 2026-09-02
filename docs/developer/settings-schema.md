# Settings schema

Logical schema for ModSettings. Field names in source may differ; this shard is the contract. Defaults belong only in the settings defaults factory — not in camera update logic.

Canonical UI term: **[Sensitivity](../glossary/sensitivity.md)**. Persist / code names use control-systems language (**gain**). Synonyms in older docs: drag scale, speed, scale. Sensitivity gains round to **three decimal places** (`RoundGain`); button-step fields round to **two**; gain values must be **> 0**.

Product-surface gates: [feature flags](./feature-flags.md). Planning: [AppleKit Maps+ feel surface design](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md).

## Identity / gesture style

| Field         | Type                        | Default  | Hot |
| ------------- | --------------------------- | -------- | --- |
| GesturePreset | enum: MapsPlus, CAD, Custom | MapsPlus | yes |

Schema-retained. With `EnableCadGestureStyle` off, product UI does not expose a Maps+/CAD switcher; shipped style is Maps+ (`⌥`+two-finger orbit). Gesture style is **not** a [feel preset](../glossary/feel-preset.md).

## Enables

| Field           | Type | Default | Hot |
| --------------- | ---- | ------- | --- |
| AssistUiEnabled | bool | false   | yes |
| PanEnabled      | bool | true    | yes |
| ZoomEnabled     | bool | true    | yes |
| YawEnabled      | bool | true    | yes |
| OrbitEnabled    | bool | true    | yes |

Schema field `AssistUiEnabled` shows or hides the in-game **Debug** panel (feel presets + tunables). Product UI labels it Debug; the schema name stays `AssistUiEnabled`. Factory/ship default is **off** so gesture-only players keep a clean viewport; enable it from Options when you want the floating panel. Existing settings.xml that already saved `true` keeps Debug on. Legacy schema 1–2 loads without the element still default **on** via `LegacyModSettings` for migration. Assist **chrome** (pads / nudge buttons) is separate and gated by `EnableAssistChrome`.

## Gesture resolve mode

| Field              | Type                                       | Default    | Hot |
| ------------------ | ------------------------------------------ | ---------- | --- |
| GestureResolveMode | enum: Concurrent, SessionLock, PrimaryOnly | Concurrent | yes |

See [gesture resolve mode](../glossary/gesture-resolve-mode.md). PrimaryOnly priority when multiple candidates exist: Orbit > Zoom > Rotate > Pan. Schema-retained; not required on the slim product surface.

## Orbit trigger

| Field        | Type                                                | Default               | Hot |
| ------------ | --------------------------------------------------- | --------------------- | --- |
| OrbitTrigger | enum: ModifierPlusTwoFinger, ThreeFinger, Both, Off | ModifierPlusTwoFinger | yes |

Maps+ uses ModifierPlusTwoFinger (Option on macOS). CAD would use ThreeFinger when `EnableCadGestureStyle` is on. [Orbit latch](../glossary/orbit-latch.md) always applies when orbit engages.

## Sensitivity / gain (factory Default feel)

Used by trackpad gestures (and Assist chrome pads when `EnableAssistChrome` is on). Options labels say **Sensitivity**; schema/XML fields use `*Gain*`.

| Field          | Type  | Factory Default | Hot |
| -------------- | ----- | --------------- | --- |
| PanGainX       | float | 0.005           | yes |
| PanGainY       | float | 0.005           | yes |
| ZoomGain       | float | 1.00            | yes |
| YawRotateGain  | float | 2.00            | yes |
| OrbitYawGain   | float | 1.00            | yes |
| OrbitPitchGain | float | 1.00            | yes |

**Numeric policy:** each gain must be **> 0**; parse/apply round to **three** decimals (pan/orbit after folding the former 0.01 AppKit scroll unit into defaults).

**Product Sensitivity sliders:** for each axis, UI range is **0.1×–2×** that axis’s factory Default value, with step ≈ **10%** of the factory default (rounded to three decimals; values must stay **> 0**).

**Schema 2:** AppKit scroll deltas are raw; schema 1 files migrate by ×0.01 on pan/orbit gain and ÷0.01 on motion deadband (legacy element `MotionDeadzone`).

**Schema 6:** `PinchDeadband` / `YawDeadband` replace misnamed `PinchEpsilon` / `RotateEpsilon` (activation deadbands, not filter epsilon). Schema 3–5 files still load those legacy elements; save rewrites schema 6 names.

**Schema 7–8:** Per-op trackpad gesture bindings (`ZoomGesture` / `ZoomGestureModifier`, and the same for Pan / Rotate / Orbit). Schema 8 renames Rotate bindings from `YawGesture*` → `RotateGesture*` (yaw/pitch remain Orbit axes). Owned by **gesture style** (`GesturePreset` / `ApplyGesturePreset`); orthogonal to feel presets (Slow/Default/Fast). No remap UI yet. Missing elements load Maps+ factory defaults.

**Schema 3:** XML element names move to engineering language (`*Gain*`, `*Step*`, `MotionDeadband`, `*Filter*`, `SignInvert*`). Schema 1–2 files deserialize via the legacy shape and rewrite as schema 3.

**Schema 4:** Debug QoL prefs persist in `current`:

| Field                   | Type | Default | Hot |
| ----------------------- | ---- | ------- | --- |
| IncludeSystemInfoInCopy | bool | true    | no  |
| DebugPanelDismissed     | bool | false   | no  |

**Schema 5:** Debug panel position persists in `current`:

| Field          | Type  | Default | Hot |
| -------------- | ----- | ------- | --- |
| DebugPanelPosX | float | 40      | no  |
| DebugPanelPosY | float | 60      | no  |

Reset to factory restores feel fields only — panel position is preserved.

Missing elements on load get factory defaults; schema bump rewrites the envelope.

## Orbit pitch limits

| Field         | Type  | Factory Default | Hot |
| ------------- | ----- | --------------- | --- |
| OrbitPitchMin | float | 0.00            | yes |
| OrbitPitchMax | float | 90.00           | yes |

Schema-retained for presets / older XML. **Live orbit clamp matches vanilla** `CameraController` normal play: **0°–90°**. Drag uses `AddAngleVelocity` (vanilla integrates and clamps); the mod only floors further downward pitch at **0°** so free-camera **−90°** cannot be reached via our path. Button / absolute `AngleY` writes clamp to **0…90**. Fields are not exposed in Options or the Debug panel.

## Button steps

Used by Assist chrome nudge buttons only — product UI when `EnableAssistChrome` is on. Schema-retained while the flag is off. UI label: **button step**. Field names use `*Step*` (schema ≤2: `*ButtonScale*`). **Not** multiplied by gain / Sensitivity.

| Field          | Type  | Default seed | Hot |
| -------------- | ----- | ------------ | --- |
| PanStepX       | float | 0.05         | yes |
| PanStepY       | float | 0.05         | yes |
| OrbitYawStep   | float | 2.00         | yes |
| OrbitPitchStep | float | 2.00         | yes |
| ZoomStep       | float | 0.05         | yes |
| YawRotateStep  | float | 2.00         | yes |

Exact button-step seeds may be tuned in the defaults factory; document new seeds here when they change. Button-step fields round to two decimals; Sensitivity gains use three (`RoundGain`).

## Apply math (contract)

Let `raw` be the resolved gesture delta for that axis (centroid, pinch, or rotate). Optional [low-pass](../glossary/low-pass.md) may replace `raw` with an EMA-smoothed value on the continuous path only — and only when Contacts capture is enabled (`EnableContactsCapture`).

**Sign invert:** if the matching `SignInvert*` flag is on, multiply the signed delta by `-1` after scaling. Options may still label these **Invert** / Reverse.

### Continuous path (trackpad; chrome pads when flagged on)

| Op    | After gain                                                       | Camera write                                                          |
| ----- | ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| Pan   | `mx = dx * PanGainX`, `my = dy * PanGainY`, then `mx,my *= Size` | Camera-relative XZ: `target += right*mx + forward*my`                 |
| Zoom  | `delta = pinch * ZoomGain`                                       | `Size' = Size * (1 - delta)` (clamped)                                |
| Yaw   | `delta = rotate * YawRotateGain`                                 | `AngleX' = AngleX + delta`                                            |
| Orbit | `dyaw = dx * OrbitYawGain`, `dpitch = dy * OrbitPitchGain`       | `AngleX' += dyaw`, `AngleY' += dpitch`, then clamp pitch to min / max |

### Button path (chrome nudges only; `EnableAssistChrome`)

Build a one-shot delta from the button step and a sign (`±1`), then apply sign invert and the same camera write as above. **Do not** multiply by gain. Skip filter (low-pass).

| Op    | One-shot input before sign invert                                |
| ----- | ---------------------------------------------------------------- |
| Pan   | `dx = signX * PanStepX`, `dy = signY * PanStepY`                 |
| Zoom  | `pinch = sign * ZoomStep`                                        |
| Yaw   | `rotate = sign * YawRotateStep`                                  |
| Orbit | `dx = signYaw * OrbitYawStep`, `dy = signPitch * OrbitPitchStep` |

### Filter / low-pass (continuous only; Contacts)

When enabled for an op under Contacts capture: first sample seeds state; later `smoothed += alpha * (raw - smoothed)`. Reset on touch-up. Buttons skip this stage.

## Sign invert (polarity)

| Field                | Type | Factory Default | Hot |
| -------------------- | ---- | --------------- | --- |
| SignInvertPanX       | bool | true            | yes |
| SignInvertPanY       | bool | false           | yes |
| SignInvertOrbitYaw   | bool | false           | yes |
| SignInvertOrbitPitch | bool | false           | yes |
| SignInvertZoom       | bool | false           | yes |
| SignInvertYawRotate  | bool | false           | yes |

Factory Default feel: Pan Reverse X on, Y off (playtest Maps+).

## Gesture bindings (schema 7–8)

Composable **gesture + optional modifier** per camera op. Source of truth for **Gesture(s):** op-heading lines. Defaults come from Maps+/CAD **gesture style** tables via `ApplyGesturePreset` (not from feel Slow/Default/Fast). No edit UI yet.

**Rotate** is the product op name (Cities camera rotate). Schema fields use `RotateGesture*`. Yaw/pitch axes belong to **Orbit** (`OrbitYawGain` / `OrbitPitchGain`), not this op. Schema 8 renames former `YawGesture*` elements; load still accepts schema 7 XML.

| Field                 | Type                    | Maps+ default   | Hot |
| --------------------- | ----------------------- | --------------- | --- |
| ZoomGesture           | enum TrackpadGesture    | Pinch           | yes |
| ZoomGestureModifier   | enum GestureModifierKey | None            | yes |
| PanGesture            | enum TrackpadGesture    | TwoFingerDrag   | yes |
| PanGestureModifier    | enum GestureModifierKey | None            | yes |
| RotateGesture         | enum TrackpadGesture    | TwoFingerRotate | yes |
| RotateGestureModifier | enum GestureModifierKey | None            | yes |
| OrbitGesture          | enum TrackpadGesture    | TwoFingerDrag   | yes |
| OrbitGestureModifier  | enum GestureModifierKey | Option          | yes |

CAD `ApplyGesturePreset` keeps Zoom/Pan/Rotate the same and sets Orbit to ThreeFingerDrag + None (and syncs `OrbitTrigger`). Drag = continuous deltas; Swipe/Tap enum values are catalog stubs for future ports.

Feel presets (Sensitivity / deadbands) apply on top of whichever gesture style is active and must not rewrite these fields.

## Thresholds

| Field                 | Type  | Default seed   | Hot |
| --------------------- | ----- | -------------- | --- |
| MotionDeadband        | float | small positive | yes |
| PinchDeadband         | float | small positive | yes |
| YawDeadband           | float | small positive | yes |
| FingerCountHysteresis | float | small positive | yes |

Schema-retained; **Debug panel** exposes MotionDeadband, PinchDeadband, and YawDeadband per op section for QA tuning. Options product surface does not show these fields.

## Per-op filter / low-pass (Contacts only)

EMA on continuous deltas after resolve, before apply — see glossary **low-pass**. Product UI and processing when `EnableContactsCapture` is on. Buttons skip filter. Schema fields use `*Filter*`; Options may still say **Low-pass**. The former single `Smoothing` field is retired.

| Field              | Type      | Default | Hot |
| ------------------ | --------- | ------- | --- |
| PanFilterEnabled   | bool      | false   | yes |
| PanFilterAlpha     | float 0–1 | 0.30    | yes |
| ZoomFilterEnabled  | bool      | false   | yes |
| ZoomFilterAlpha    | float 0–1 | 0.30    | yes |
| YawFilterEnabled   | bool      | false   | yes |
| YawFilterAlpha     | float 0–1 | 0.30    | yes |
| OrbitFilterEnabled | bool      | false   | yes |
| OrbitFilterAlpha   | float 0–1 | 0.30    | yes |

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

| Profile                    | Contract                                                                                                                                                 |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Default / Reset to factory | Factory Default table above (SignInvertPanX true; gain seeds; OrbitPitchMin/Max 0–90 schema seeds)                                                       |
| Slow                       | Default gain fields × **0.75**; reverse and pitch limits unchanged; round to three decimals (`RoundGain`)                                                |
| Fast                       | Default gain fields × **1.25**; reverse and pitch limits unchanged; round to three decimals (`RoundGain`)                                                |
| **New Preset**             | Scratch identity when the player dirties an active built-in or named preset; autosave writes here; built-ins Slow / Default / Fast are never overwritten |
| Named Save as… / Load      | Full feel set in `userPresets[]`; after Save as…, the named preset is selected; further edits dirty back to **New Preset**                               |

| Field                | Type   | Role                                                                                              | Hot |
| -------------------- | ------ | ------------------------------------------------------------------------------------------------- | --- |
| ActiveFeelPresetName | string | Active feel identity in the preset dropdown (built-in name, named user preset, or **New Preset**) | yes |

Live settings load and save through a versioned XML file under the Cities user-data tree (`…/TrackpadCameraControl/settings.xml`):

| Element       | Role                                                     |
| ------------- | -------------------------------------------------------- |
| schemaVersion | Envelope version                                         |
| current       | Full ModSettings blob (includes active feel preset name) |
| userPresets[] | Named feel profiles for Save as… / Load                  |

Missing or corrupt file → factory defaults (no crash), then persist the recovered blob. **Reset to factory** restores schema defaults into `current` and writes the file.

GesturePreset / CAD, CaptureBackend / Contacts, button steps, and low-pass remain in the schema for flagged surfaces; they are not the primary preset model.

Options and the in-game Debug panel both bind the same fields through one apply layer; every change autosaves. Number fields edit Sensitivity (sliders) and (when flagged) button steps and low-pass params. Orbit pitch min/max remain schema-only — not exposed on either product surface.

## Validation rule

Camera and gesture modules must read these fields at use-time. A contributor checklist: no magic numbers for feel outside the defaults factory.
