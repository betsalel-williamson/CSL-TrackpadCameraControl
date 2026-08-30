# AppleKit Maps+ feel surface — Design

**Date:** 2026-08-29  
**Status:** Approved for implementation planning  
**Scope:** Slim product surface (Maps+/AppleKit), feel presets, scroll/UI gating, Sensitivity naming, orbit pitch limits  
**Approach:** Product-surface slim + feature flags (Option A)

## Goal

Ship a focused Maps+ / AppleKit trackpad camera experience: playtested default sensitivities, Slow/Default/Fast plus Save as…/Load feel profiles, correct behavior over Options/menus and popups, mouse-wheel zoom vs trackpad pan, and a consistent **Sensitivity** vocabulary — without exposing CAD, Contacts, low-pass, or Assist chrome until those flags are turned on.

## Locked decisions

| Concern | Choice |
| --- | --- |
| Packaging | Feature flags hide unfinished surfaces; leftover code may remain unused |
| Gesture style (ship) | Maps+ / AppleKit only (`⌥`+two-finger orbit) |
| Feel presets | Built-in Slow / Default / Fast + named Save as… / Load + Reset to factory |
| Preset meaning | A **feel profile** (sensitivities, reverse, pitch limits, enables) — not a gesture-style seed |
| Canonical term | **Sensitivity** (UI, code docs, glossary primary); synonyms: speed, scale, drag scale |
| Numeric fields | Any value **> 0**; round/clamp display and apply to **two decimal places** (`0.xx`) |
| Scroll by device | Precise trackpad scroll → pan + suppress vanilla zoom; mouse wheel → vanilla zoom |
| Menus / Options open | No mod camera ops; two-finger scrolls UI |
| Pointer over popups | No mod camera ops from two-finger; revert to UI scroll (any active popup, not only Assist) |
| Keyboard over popups | Passthrough; may move camera unless a text field is focused |
| Pitch limits | Editable Pitch min / max; apply clamps from those settings; starter defaults for playtest |
| Low-pass | Only when Contacts capture is enabled (rides `EnableContactsCapture`) |
| Btn / Assist chrome | Off product surface (`EnableAssistChrome` off); mouse/keyboard stay vanilla |

## Feature flags

Positive `Enable*` names, default **off** for ship. Same identifier in code (`FeatureFlags.*`), developer docs, and schema notes.

| Flag | When on | When off (ship now) |
| --- | --- | --- |
| `EnableCadGestureStyle` | CAD / three-finger orbit as a player choice | Maps+-only; no CAD switcher |
| `EnableContactsCapture` | Contacts interpreter + backend picker; LP UI and filtering for that path | AppleKit only; no LP |
| `EnableAssistChrome` | Assist nudge buttons + Btn sensitivity fields | No chrome buttons; no Btn fields |

There is no separate LP flag: low-pass UI and processing appear only under `EnableContactsCapture`.

## Feel profiles (Section 1)

### Default (factory / Reset)

Playtest profile:

| Setting | Value |
| --- | --- |
| Pan Reverse X / Y | on / off |
| Pan Sensitivity X / Y | 0.50 |
| Zoom Sensitivity | 1.00 |
| Yaw (rotate) Sensitivity | 2.00 |
| Orbit yaw / pitch Sensitivity | 10.00 / 10.00 |
| Gesture style | Maps+ (`⌥`+two-finger orbit) |
| Capture | AppleKit |
| Orbit Pitch min / max | Starter pair (e.g. −80 / 80); tunable in UI |

### Slow / Fast

Multiply **Default’s sensitivity fields** by **0.75** (Slow) and **1.25** (Fast). Reverse flags and orbit pitch min/max stay the same as Default. Round results to two decimals.

### Save as… / Load

Named user profiles persist the full feel set: per-op enables, reverse flags, sensitivities, pitch min/max (and any other product-surface feel fields). Durable with existing settings store / `userPresets` envelope.

### Not a preset

Maps+ vs CAD gesture-style switching is **not** a feel preset. With `EnableCadGestureStyle` off, do not show a Maps+/CAD preset switcher.

## Input gating (Section 2)

### Device split

Use AppKit `hasPreciseScrollingDeltas` (already observed in the Apple gesture probe; wire into mod capture):

| Input | Mod | Vanilla `HandleScrollWheelEvent` |
| --- | --- | --- |
| Trackpad two-finger (precise) | Pan (when gates allow) | Suppress so pan does not also zoom |
| Mouse wheel (not precise) | Do not map to pan | Allow → game zoom |

Edge pan, keyboard camera, and gamepad remain passthrough.

### Menu / Options open

When Options or another game menu is open: **no mod camera ops**. Two-finger and clicks belong to the UI. Do not suppress vanilla scroll so menus can scroll.

### Pointer over active popups

When the cursor is within any active popup / HUD panel (Assist tuning panel **or other mods’ popups**):

- Two-finger → **UI scroll**, not camera pan/orbit/zoom/yaw from the mod.
- Do not suppress the scroll path the UI needs.
- Keyboard: unchanged passthrough; may still move the camera unless an input field is focused.

`IgnoreOverUi` (default on) covers the popup case; menu-open is a separate, stronger gate.

## Product surface UI (Section 3)

### Per-op sections (Assist panel + Options)

For **Pan**, **Zoom**, **Rotate**, and **Orbit**, each heading includes a short description of **what it means** and **what activates it** (e.g. Orbit: pitch + yaw around the pivot — activated by `⌥`+two-finger).

Controls while flags are off: Enable, Reverse (as today), Sensitivity fields, Orbit Pitch min/max. No Btn, no LP, no capture-backend picker, no CAD switcher.

### Presets row

**Slow | Default | Fast** + **Save as… / Load** + **Reset to factory**.

### Layout

Options uses the same multi-column-by-op grouping as the Assist popup.

### Numerics

On parse/apply and when formatting fields: reject or ignore non-positive sensitivities where the contract is **> 0**; always round to two decimal places. Pitch min/max also display/store at two decimals.

## Docs and glossary (Section 4)

- Primary glossary entry: **Sensitivity**. Keep drag-scale / speed / scale as synonym pointers.
- Client and feature shards describe Maps+/AppleKit as the shipped surface; CAD, Contacts, LP, and Assist chrome as flagged-off capabilities using the same `Enable*` names.
- Preset docs: feel profiles (Slow/Default/Fast + Save as…/Load), not gesture-style seeds.
- Vanilla suppress docs: precise trackpad scroll suppressed for pan; mouse wheel allowed; no mod camera when menus open or pointer over popups.

## Architecture (units)

```text
[AppKit events] → classify precise vs wheel → GestureFrame
                      ↓
              MenuOpen? / OverPopup? → skip apply; allow UI scroll
                      ↓
              GesturePipeline → (LP only if Contacts) → CameraApplicator
                      ↓                              ↘ pitch clamp from settings
              VanillaCameraSuppress ← skip scroll only for precise trackpad when applying world pan

[Feel UI] Slow/Default/Fast | Save/Load | Reset → ModSettings → store
[Flags] FeatureFlags gate Options/panel builders and backend selection
```

Independent units:

1. **FeatureFlags** — three booleans; product-surface builders consult them.
2. **FeelProfiles** — Default table, Slow/Fast multipliers, named preset serialize/load.
3. **InputGates** — menu-open, over-UI/popup, game focus; shared by pipeline and suppress.
4. **ScrollDeviceSplit** — precise vs wheel in mapper + suppress policy.
5. **PitchLimits** — settings fields + clamp in applicator.
6. **NumericPolicy** — `> 0` and two-decimal round for product floats.

## Acceptance

- Options/menus open → two-finger scrolls UI; city does not pan from trackpad.
- Cursor over Assist or other popup → two-finger scrolls/drags UI, not pan; keys may move camera unless a field is focused.
- In world: trackpad pans; mouse wheel zooms; default Reverse X on.
- Slow / Default / Fast and Save as… / Load / Reset match the feel-profile contract; sensitivities show two decimals.
- Orbit pitch stays within Pitch min/max; fields editable.
- No CAD picker, backend picker, Btn, or LP while the three flags are off.
- Options layout multi-column like the popup; labels say **Sensitivity**.
- Per-op headings describe meaning + activation (`⌥` for orbit).

## Non-goals

- Enabling `EnableCadGestureStyle`, `EnableContactsCapture`, or `EnableAssistChrome` for players in this pass.
- Perfect keyboard-vs-popup arbitration beyond not stealing focused text fields.
- Final Sensitivity slider min/max range (keep free **> 0** until UX testing finishes).
- Deleting flagged-off code paths.

## Relationship to prior specs

- Extends in-game tuning panel / settings work; corrects “Maps+/CAD preset” language to **gesture style** vs **feel profile**.
- Updates vanilla camera suppress policy: not “always suppress scroll-zoom,” but **precise trackpad only**, plus UI/menu gates.
- Assist chrome remains deferred behind `EnableAssistChrome` (see assist-ui-camera-chrome design).
