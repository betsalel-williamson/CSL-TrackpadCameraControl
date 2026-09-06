# Settings and hot configuration

## Intent

No feel parameter is hardcoded in Apply beyond documented apply constants (orbit pitch clamp). Style chords live in a **binding table** that Policy resolve consumes — Maps+ ships as seed data for parity ([ADR 0004](./adr/0004-style-table-driven-resolve.md)). Players experiment mid-session from the **in-game Debug panel** or **Options → Trackpad Camera Control**. Both surfaces edit one live settings blob; every change applies immediately and autosaves. Values persist across quit.

## Surfaces

| Surface                           | Product-surface tunables                                                                          |
| --------------------------------- | ------------------------------------------------------------------------------------------------- |
| In-game Debug panel               | Feel presets (incl. **New Preset** dirty), Reset, Sensitivity, Show debug panel                   |
| Options → Trackpad Camera Control | Feel presets + Sensitivity sliders; window title is mod name + version; **no** pitch angle fields |

One editor API serves both surfaces. A change updates the live blob immediately and sets **one dirty bit**; autosave coalesces to **one flush** (no double-write amplification). See [greenfield redesign lessons](./greenfield-redesign-lessons.md) L7.

Debug chrome pads/buttons and button-step fields appear only if `EnableAssistChrome` is **compiled** on. Capture-backend picker and low-pass exist only if `EnableContactsCapture` is compiled on. A CAD / Maps+ gesture-style switcher exists only if `EnableCadGestureStyle` is compiled on. Off modules are omitted from the ship DLL — not present as empty UI or tick no-ops.

## Layout (UI 1:1 with shipping)

Section order on both product surfaces: **General → Zoom → Pan → Rotate → Orbit**.

Options section rhythm is native Colossal **AddGroup**: short group title + native glow underline, with controls nested in the group Content. Sensitivity: label + slider on one row.

Product UI does **not** expose Enable-per-op or Reverse. Reverse and per-op enables remain in schema / factory feel data for Apply when set; the mod master on/off is the player enable switch.

Refuse cleanup that changes Options/Debug order, labels, or feel math relative to shipping ([parity with shipping](./parity-with-shipping.md)).

## Configuration layers

| Layer        | What it holds                                                           | Tick consumer                                      |
| ------------ | ----------------------------------------------------------------------- | -------------------------------------------------- |
| Style table  | Gesture style rows (finger count, modifiers, → op). Maps+ seed on ship  | **Policy resolve** (single source of truth)        |
| Feel profile | Sensitivities, reverse, per-op enables, deadbands (schema)              | **Apply** (and gates that read enables if present) |
| Gates        | Menu / over-UI / focus policy; precise-vs-wheel scroll split            | **Policy** each tick; suppress buffers for Harmony |
| Chrome       | Show debug panel; Assist pads/steps only when Assist module compiled on | Debug host / chrome emitter (not Capture)          |

Feel ≠ gesture style ([ADR 0003](./adr/0003-feel-profiles-and-product-flags.md)). Player “preset” means a feel profile. A style switcher stays compile-gated.

## Field → tick consumer (schema contract)

Every schema field names its consumer or is marked chrome-only / XML alias / non-field. Ceremony without a consumer is forbidden (lesson L1).

| Field / group                           | Consumer / classification                                                                |
| --------------------------------------- | ---------------------------------------------------------------------------------------- |
| Style binding table rows                | Policy resolve                                                                           |
| Zoom / Pan / Rotate / Orbit Sensitivity | Apply continuous path                                                                    |
| Invert / Reverse (schema)               | Apply (not product UI)                                                                   |
| Enable-per-op (schema)                  | Apply / Policy skip when off (not product UI)                                            |
| Deadbands (schema)                      | Apply when present; not required on slim product surface                                 |
| Feel preset identity + userPresets      | Settings load / dirty model; Apply reads active feel                                     |
| Show debug panel                        | Chrome (panel host)                                                                      |
| Orbit pitch min/max                     | **Not a feel field** — apply constant **0°–90°** (omit from feel blob and Options/Debug) |
| Button step / chrome pads               | Chrome emitter only when `EnableAssistChrome` compiled on; else omitted                  |
| Capture backend / low-pass              | Capture / Apply only when `EnableContactsCapture` compiled on; else omitted              |
| Gesture-style switcher                  | Policy seed selection only when `EnableCadGestureStyle` compiled on; else omitted        |
| XML alias / migrate keys                | Load path only (non-field for tick)                                                      |

## Feel presets

A feel preset stores sensitivities, reverse flags, and enables — **not** gesture style, and **not** pitch min/max.

| Profile    | Role                                                                                      |
| ---------- | ----------------------------------------------------------------------------------------- |
| Default    | Factory / Reset — playtest Maps+ feel (Sensitivity factory defaults)                      |
| Slow       | Default sensitivities × 0.75 (three decimals); reverse unchanged                          |
| Fast       | Default sensitivities × 1.25 (three decimals); reverse unchanged                          |
| New Preset | Scratch identity when the player dirties a built-in or named preset; autosave writes here |
| Named      | Save as… / Load / Delete on the persist `userPresets` envelope                            |

### Dirty model and built-ins

- Built-in **Slow / Default / Fast** are immutable: never overwritten by edits or autosave.
- Editing feel while a built-in or named preset is active switches the active identity to **New Preset**; autosave persists into that scratch profile.
- Preset dropdown: selecting an entry **loads** it. **Save as…** is a separate button, enabled on **New Preset**.
- After **Save as…**, the named preset is selected; further edits dirty to **New Preset** again.
- **Delete** is enabled only for a named user preset (not Slow / Default / Fast, not New Preset). It removes that profile, applies Default, and persists — no confirm.
- **One dirty → one flush:** Options and Debug share one editor API and one coalesced autosave path.

## Tunables (product surface, all hot)

Under each op heading (**Zoom**, **Pan**, **Rotate**, **Orbit**) after **General**: short meaning + activation, then:

- Per-op Sensitivity **slider** (Options only): UI **[0, 1]** maps piecewise to **0.1× / 1× / 2×** factory (mid = Default); step ≈ **10%** of factory on the high side; display/apply **three** decimals
- Orbit: pitch clamp is vanilla **0° / 90°** as an **apply constant** — not Options/Debug-tunable, not in the feel blob. Drag floors at **0°**; button writes (when Assist on) clamp **0…90**. No yaw angle clamp.

Also: feel preset row (Slow / Default / Fast / New Preset when dirty, Save as…, Delete, Reset to factory), **Show debug panel** (panel master switch; off also hides the floating Debug reopen chip).

## Tunables (compile-gated — omitted from ship DLL when off)

- Gesture-style switcher (CAD) — `EnableCadGestureStyle`
- Capture backend picker and low-pass — `EnableContactsCapture`
- Button step / Debug chrome — `EnableAssistChrome`

## Apply math (contract)

Let `raw` be the resolved gesture delta for that axis. Optional low-pass only when Contacts module is compiled on; ship applies raw → Sensitivity → camera.

**Invert:** if the matching invert flag is on in settings, multiply the signed delta by `-1` after scaling (schema-backed; not a product UI control).

### Continuous path (trackpad; chrome pads when Assist compiled on)

| Op     | After Sensitivity                                                                                        | Camera write                                                                                                                          |
| ------ | -------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| Pan    | `mx = dx * PanSensitivityX`, `my = dy * PanSensitivityY`, then `mx,my *= Size`                           | Camera-relative XZ, then clamp target to unlocked game area                                                                           |
| Zoom   | `delta = pinch * ZoomSensitivity`                                                                        | `Size' = Size * (1 - delta)` (clamped)                                                                                                |
| Rotate | `delta = rotate * RotateGain`                                                                            | Heading or place/relocate ghost — see [selection-aware gestures](./selection-aware-gestures.md). No yaw angle clamp.                  |
| Orbit  | Drag: queue angle velocity (yaw/pitch × Orbit sensitivities). Button: absolute step + pitch apply clamp. | Orbit always from current look-at (no Target re-home). Pitch apply constant **0°–90°**. Flush via Harmony postfix after vanilla damp. |

### Button path (only when Assist compiled on)

One-shot delta from button step, then invert and the same camera write. **Do not** multiply by Sensitivity. Skip low-pass.

## Durable persist

- Load once at mod enable (before the gesture pipeline starts).
- Write after every successful field apply (debounced autosave — **one dirty bit, one flush**) and flush on mod disable, city unload, and panel close.
- Envelope: `schemaVersion`, `current` (full settings), `userPresets[]` for named feel profiles (and **New Preset** scratch as applicable). Style table seed is part of `current` (or equivalent) and is what resolve reads.
- Missing or corrupt file → factory defaults (including Maps+ style seed), then persist the recovered blob.
- Built-in Slow / Default / Fast blobs are never mutated on disk by player edits.

## Orbit latch (contract)

Once orbit engages from the style-resolved trigger, the session stays in orbit until touch-up (Ended / Cancelled / zero fingers), even if the modifier key is released. While latched: **orbit only** (yaw rotate is suppressed so twist noise cannot double-write angles); pan and zoom do not — regardless of Concurrent resolve mode.

## Input gates (contract)

- **Menu / Options open:** no mod camera ops; UI owns two-finger scroll.
- **Pointer over any active popup / HUD panel:** no mod camera ops from two-finger; UI scroll; keyboard passthrough unless a text field is focused.
- **Require game focus:** no camera ops when the game is unfocused.
- Device scroll split: see [vanilla camera suppress](./vanilla-camera-suppress.md).

## Hot-apply contract

- Live settings update immediately from either surface; Policy resolve, session, and Apply read live settings each frame.
- **Debug panel** UI prefers in-place field/label refresh on settings change; it only Destroy/recreates when heading structure cannot be updated in place.
- **Options controls** bind at page build. Leave and re-enter Options (or reopen the page) to see Debug edits reflected in slider positions.
- Debug may hold Sensitivity **outside** the Options slider **0.1×–2×** range; Options sliders clamp only when the player moves them.
- No mod disable/enable cycle required for tuning.

## Acceptance

- Change a Sensitivity in Debug: gestures respond immediately; Debug UI shows the new value; Options sliders match after reopening the page (and the reverse for Options → Debug rebuild). Every edit autosaves once (one dirty → one flush); product numerics display **three** decimals.
- Quit and relaunch; tunables and named feel presets are restored from disk; Maps+ chords still resolve from the style table seed.
- Editing a built-in switches active identity to **New Preset**; Slow / Default / Fast remain unchanged after autosave.
- Save as… selects the named preset; further edits dirty to **New Preset** again.
- Delete on a named user preset removes it, applies Default, and persists; built-ins and New Preset cannot be deleted.
- Reset to factory and Slow / Default / Fast match the feel-profile contract.
- Product UI shows no Enable-per-op or Reverse; section order is General → Zoom → Pan → Rotate → Orbit (1:1 with shipping).
- Pitch clamps to vanilla **0°–90°** (apply constant; not Options/Debug-tunable; absent from feel blob); pan apply clamps the camera target to the unlocked game area; yaw has no angle clamp.
- On the ship surface: no CAD / gesture-style switcher, no backend picker, no Btn fields, no low-pass (those modules omitted from the DLL).
- Every schema field documents a tick consumer or chrome-only / alias / non-field classification.
