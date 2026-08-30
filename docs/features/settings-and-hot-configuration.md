# Settings and hot configuration

## Intent

No feel or binding parameter is hardcoded in camera or gesture logic. Defaults exist only in the settings schema. Players experiment mid-session from the **in-game Debug panel** or **Options → Trackpad Camera Control**. Both surfaces edit the same live ModSettings; every change applies immediately and autosaves. Values persist across quit.

## Surfaces

| Surface                           | Product-surface tunables                                                                 |
| --------------------------------- | ---------------------------------------------------------------------------------------- |
| In-game Debug panel               | Feel presets (incl. **New Preset** dirty), Reset, Sensitivity, Debug UI enable |
| Options → Trackpad Camera Control | Feel presets + Sensitivity sliders; window title is mod name + version; **no** pitch angle fields |

One binding layer parses, rounds gains to three decimals (button steps to two), validates Options Sensitivity against the slider contract on player drag, and writes fields. A change on either surface updates live settings immediately and schedules a durable write (autosave).

Debug chrome pads/buttons and button-step fields appear only when `EnableAssistChrome` is on. Capture-backend picker and [low-pass](../glossary/low-pass.md) appear only when `EnableContactsCapture` is on. CAD gesture-style switcher appears only when `EnableCadGestureStyle` is on. See [feature flags](./adr/0003-feel-profiles-and-product-flags.md).

## Layout (contract)

Section order on both product surfaces: **General → Zoom → Pan → Rotate → Orbit**.

Section rhythm (not CSS): prior content → horizontal rule → section title (indented) → control rows (further indented). Sensitivity: label + slider on one row.

Product UI does **not** expose Enable-per-op or Reverse. Reverse and per-op enables remain in schema / factory feel data for apply math when set; the mod master on/off is the player enable switch.

## Feel presets

A [feel preset](../glossary/feel-preset.md) stores sensitivities, reverse flags, enables, and orbit pitch min/max — **not** [gesture style](../glossary/gesture-style.md).

| Profile | Role |
| ------- | ---- |
| Default | Factory / Reset — playtest Maps+ feel ([Sensitivity](../glossary/sensitivity.md) factory defaults) |
| Slow    | Default sensitivities × 0.75 (three decimals); reverse and pitch limits unchanged |
| Fast    | Default sensitivities × 1.25 (three decimals); reverse and pitch limits unchanged |
| New Preset | Scratch identity when the player dirties a built-in or named preset; autosave writes here |
| Named   | Save as… / Load on the persist `userPresets` envelope |

### Dirty model and built-ins

- Built-in **Slow / Default / Fast** are immutable: never overwritten by edits or autosave.
- Editing feel while a built-in or named preset is active switches the active identity to **New Preset**; autosave persists into that scratch profile.
- Preset dropdown: selecting an entry **loads** it; **Save as…** is the last entry.
- After **Save as…**, the named preset is selected; further edits dirty to **New Preset** again.

## Tunables (product surface, all hot)

Under each op heading (**Zoom**, **Pan**, **Rotate**, **Orbit**) after **General**: short meaning + activation, then:

- Per-op [Sensitivity](../glossary/sensitivity.md) **slider** (Options only): min **0.1×** that field’s factory default, max **2×**, step ≈ **10%** of factory default; display/apply **three** decimals for Sensitivity gains (button steps **two** decimals)
- Orbit: schema seeds OrbitPitchMin/Max **0** / **90** (vanilla). Live clamp is hardcoded to that range — not Options/Debug-tunable. Drag floors at **0°**; button writes clamp **0…90**. No yaw angle clamp.

Also: [feel preset](../glossary/feel-preset.md) row (Slow / Default / Fast / New Preset when dirty, Save as… / Load, Reset to factory), Debug UI enabled (panel master switch).

## Tunables (flagged off)

- Gesture-style switcher (CAD) — `EnableCadGestureStyle`
- Capture backend picker and [low-pass](../glossary/low-pass.md) — `EnableContactsCapture`
- [Button step](../glossary/button-step.md) / Debug chrome — `EnableAssistChrome`
- Deadzones and resolve-mode advanced fields (schema retains; not required on the slim surface)
- Enable-per-op and Reverse controls (schema/factory retain; not on product UI)

## Apply math (contract)

Let `raw` be the resolved gesture delta for that axis. Optional [low-pass](../glossary/low-pass.md) may replace `raw` when Contacts capture is enabled.

**Invert:** if the matching invert flag is on in settings, multiply the signed delta by `-1` after scaling (schema-backed; not a product UI control).

### Continuous path (trackpad; chrome pads when flagged on)

| Op    | After [Sensitivity](../glossary/sensitivity.md) | Camera write |
| ----- | ----------------------------------------------- | ------------ |
| Pan   | `mx = dx * PanSensitivityX`, `my = dy * PanSensitivityY`, then `mx,my *= Size` | Camera-relative XZ, then **clamp target via `ICameraController.ClampPanTarget`** (unlocked game area / `ClampPoint`) |
| Zoom  | `delta = pinch * ZoomSensitivity` | `Size' = Size * (1 - delta)` (clamped) |
| Yaw   | `delta = rotate * YawRotateSensitivity` | `AngleX' = AngleX + delta` (no yaw angle clamp). When a selection is active, two-finger rotate targets the object instead — see [selection-aware gestures](./selection-aware-gestures.md). |
| Orbit | Drag: `AddAngleVelocity(dx * OrbitYawSensitivity, dy * OrbitPitchSensitivity)` (middle mouse button-style; vanilla LateUpdate damps/lerps). Button: absolute angle step + pitch clamp. Place/relocate ghost may re-home Target; otherwise orbit from current look-at. |

### Button path (only when `EnableAssistChrome`)

One-shot delta from [button step](../glossary/button-step.md), then invert and the same camera write. **Do not** multiply by Sensitivity. Skip low-pass.

## Durable persist

- Load once at mod enable (before the gesture pipeline starts).
- Write after every successful field apply (debounced autosave) and flush on mod disable, city unload, and panel close.
- Envelope: `schemaVersion`, `current` (full settings), `userPresets[]` for named feel profiles (and **New Preset** scratch as applicable).
- Missing or corrupt file → factory defaults, then persist the recovered blob.
- Built-in Slow / Default / Fast blobs are never mutated on disk by player edits.

## Orbit latch (contract)

Once orbit engages from the configured trigger, the session stays in orbit until touch-up (Ended / Cancelled / zero fingers), even if the modifier key is released. While latched: **orbit only** (yaw rotate is suppressed so twist noise cannot double-write angles); pan and zoom do not — regardless of Concurrent resolve mode.

## Input gates (contract)

- **Menu / Options open:** no mod camera ops; UI owns two-finger scroll.
- **Pointer over any active popup / HUD panel:** no mod camera ops from two-finger; UI scroll; keyboard passthrough unless a text field is focused.
- **Require game focus:** no camera ops when the game is unfocused.
- Device scroll split: see [vanilla camera suppress](./vanilla-camera-suppress.md).

## Hot-apply contract

- Live **ModSettings** update immediately from either surface; binding resolver, gesture session, and camera applicator read live settings each frame.
- **Debug panel** UI rebuilds on `SettingsChanged` so edits from Options appear in the floating panel without restart.
- **Options controls** bind at page build (ColossalUI / UIHelperBase cannot rebuild sliders in place). Leave and re-enter Options (or reopen the page) to see Debug edits reflected in slider positions.
- Debug may hold Sensitivity **outside** the Options slider **0.1×–2×** range; Options sliders clamp only when the player moves them.
- No mod disable/enable cycle required for tuning.

## Acceptance

- Change a Sensitivity in Debug: gestures respond immediately; Debug UI shows the new value; Options sliders match after reopening the page (and the reverse for Options → Debug rebuild). Every edit autosaves; Sensitivity gains display **three** decimals; button steps **two**.
- Quit and relaunch; tunables and named feel presets are restored from disk.
- Editing a built-in switches active identity to **New Preset**; Slow / Default / Fast remain unchanged after autosave.
- Save as… selects the named preset; further edits dirty to **New Preset** again.
- Reset to factory and Slow / Default / Fast match the feel-profile contract.
- Product UI shows no Enable-per-op or Reverse; section order is General → Zoom → Pan → Rotate → Orbit.
- Pitch clamps to vanilla **0°–90°** (hardcoded in apply; not Options/Debug-tunable); pan apply clamps the camera target to the unlocked game area; yaw has no angle clamp.
- While flags are off: no CAD switcher, no backend picker, no Btn fields, no low-pass.
- Grep of camera/gesture logic finds no numeric feel literals outside the settings defaults factory.
