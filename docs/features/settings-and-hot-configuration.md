# Settings and hot configuration

## Intent

No feel or binding parameter is hardcoded in camera or gesture logic. Defaults exist only in the settings schema. Players experiment mid-session from the **in-game Assist / tuning panel** or **Options → Trackpad Camera Control**. Both surfaces edit the same live ModSettings. Values persist across quit.

## Surfaces

| Surface                           | Product-surface tunables                                      |
| --------------------------------- | ------------------------------------------------------------- |
| In-game Assist / tuning panel     | Feel presets, Reset, enables, reverse, Sensitivity, pitch limits |
| Options → Trackpad Camera Control | Same tunables (number fields, multi-column by op)             |

One binding layer parses, rounds to two decimals, validates **> 0** for Sensitivity, and writes fields. A change on either surface updates live settings immediately and schedules a durable write.

Assist chrome pads/buttons and button-step fields appear only when `EnableAssistChrome` is on. Capture-backend picker and [low-pass](../glossary/low-pass.md) appear only when `EnableContactsCapture` is on. CAD gesture-style switcher appears only when `EnableCadGestureStyle` is on. See [feature flags](./adr/0003-feel-profiles-and-product-flags.md).

## Feel presets

A [feel preset](../glossary/feel-preset.md) stores sensitivities, reverse flags, enables, and orbit pitch min/max — **not** [gesture style](../glossary/gesture-style.md).

| Profile | Role |
| ------- | ---- |
| Default | Factory / Reset — playtest Maps+ feel ([Sensitivity](../glossary/sensitivity.md) factory defaults) |
| Slow    | Default sensitivities × 0.75 (two decimals); reverse and pitch limits unchanged |
| Fast    | Default sensitivities × 1.25 (two decimals); reverse and pitch limits unchanged |
| Named   | Save as… / Load on the persist `userPresets` envelope |

## Tunables (product surface, all hot)

Under each op heading (**Pan**, **Zoom**, **Rotate**, **Orbit**): short meaning + activation, then:

- Per-op enable
- Per-op reverse / invert (axes as applicable)
- Per-op [Sensitivity](../glossary/sensitivity.md) (any **> 0**, two decimals)
- Orbit: Pitch min / max (two decimals; applied as clamp)

Also: [feel preset](../glossary/feel-preset.md) row (Slow / Default / Fast, Save as… / Load, Reset to factory), Assist UI enabled (panel master switch).

## Tunables (flagged off)

- Gesture-style switcher (CAD) — `EnableCadGestureStyle`
- Capture backend picker and [low-pass](../glossary/low-pass.md) — `EnableContactsCapture`
- [Button step](../glossary/button-step.md) / Assist chrome — `EnableAssistChrome`
- Deadzones and resolve-mode advanced fields (schema retains; not required on the slim surface)

## Apply math (contract)

Let `raw` be the resolved gesture delta for that axis. Optional [low-pass](../glossary/low-pass.md) may replace `raw` when Contacts capture is enabled.

**Invert:** if the matching invert flag is on, multiply the signed delta by `-1` after scaling.

### Continuous path (trackpad; chrome pads when flagged on)

| Op    | After [Sensitivity](../glossary/sensitivity.md) | Camera write |
| ----- | ----------------------------------------------- | ------------ |
| Pan   | `mx = dx * PanSensitivityX`, `my = dy * PanSensitivityY`, then `mx,my *= Size` | Camera-relative XZ |
| Zoom  | `delta = pinch * ZoomSensitivity` | `Size' = Size * (1 - delta)` (clamped) |
| Yaw   | `delta = rotate * YawRotateSensitivity` | `AngleX' = AngleX + delta` |
| Orbit | `dyaw = dx * OrbitYawSensitivity`, `dpitch = dy * OrbitPitchSensitivity` | `AngleX' += dyaw`, `AngleY' += dpitch` then clamp pitch to Pitch min / max |

### Button path (only when `EnableAssistChrome`)

One-shot delta from [button step](../glossary/button-step.md), then invert and the same camera write. **Do not** multiply by Sensitivity. Skip low-pass.

## Durable persist

- Load once at mod enable (before the gesture pipeline starts).
- Write after successful field applies (debounced) and flush on mod disable, city unload, and panel close.
- Envelope: `schemaVersion`, `current` (full settings), `userPresets[]` for named feel profiles.
- Missing or corrupt file → factory defaults, then persist the recovered blob.

## Orbit latch (contract)

Once orbit engages from the configured trigger, the session stays in orbit until touch-up (Ended / Cancelled / zero fingers), even if the modifier key is released. While latched: orbit and yaw rotate may apply; pan and zoom do not — regardless of Concurrent resolve mode.

## Input gates (contract)

- **Menu / Options open:** no mod camera ops; UI owns two-finger scroll.
- **Pointer over any active popup / HUD panel:** no mod camera ops from two-finger; UI scroll; keyboard passthrough unless a text field is focused.
- **Require game focus:** no camera ops when the game is unfocused.
- Device scroll split: see [vanilla camera suppress](./vanilla-camera-suppress.md).

## Hot-apply contract

- Settings updates from either UI take effect immediately.
- Binding resolver, gesture session, and camera applicator read live settings each frame.
- No mod disable/enable cycle required for tuning.

## Acceptance

- Change a Sensitivity in the in-game panel and see the same number in Options (and the reverse) without restart; values show two decimals.
- Quit and relaunch; tunables and named feel presets are restored from disk.
- Reset to factory and Slow / Default / Fast match the feel-profile contract.
- Save as… / Load round-trips a named profile.
- While flags are off: no CAD switcher, no backend picker, no Btn fields, no low-pass.
- Options layout groups ops in multiple columns like the Assist panel.
- Grep of camera/gesture logic finds no numeric feel literals outside the settings defaults factory.
