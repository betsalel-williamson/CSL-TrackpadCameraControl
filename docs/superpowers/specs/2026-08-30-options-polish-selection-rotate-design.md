# Options polish, New Preset, selection rotate — Design

**Date:** 2026-08-30  
**Status:** Approved — MDCP docs then code (see plan). **Superseded (pitch):** locked min **7°** below — live clamp is vanilla **0°–90°** hardcoded in `CameraApplicator` (not product-tunable).  
**Branch / worktree:** `feat/in-game-tuning-panel`  
**Plan:** local session plan under `docs/superpowers/plans/` (gitignored).

## Goal

Polish Options and the in-game debug panel for Maps+/AppleKit play: reliable sync and autosave, clearer layout, Sensitivity sliders, **New Preset** dirty workflow, pan clamped to city bounds, pitch vanilla **0°–90°** (hardcoded apply), and selection-aware rotate / ⌥-orbit.

## Locked decisions

| Concern                       | Choice                                                                                                                                                                         |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Pan vs city                   | Clamp so camera cannot fly too far outside city bounds                                                                                                                         |
| Pitch                         | ~~Min **7°**~~ **Superseded:** vanilla **0°–90°** hardcoded in `CameraApplicator`; drag floors at **0°**; button clamp **0…90**; not Options/Debug-tunable; no yaw angle clamp |
| Sync                          | Popup and Options share live ModSettings; every change autosaves                                                                                                               |
| Title drag                    | Entire title bar is the drag handle                                                                                                                                            |
| Assist naming                 | Product name **Debug** (debug menu / panel), not Assist                                                                                                                        |
| Enable / Reverse UI           | Removed from product UI (mod on/off is master; reverse stays in schema/factory)                                                                                                |
| Sensitivity UI                | Slider: min 0.1× factory default, max 2×, step ≈ 10% of default; label + control one row                                                                                       |
| Section order                 | General → Zoom → Pan → Rotate → Orbit                                                                                                                                          |
| Section rhythm                | Prior content → HR → section title (indented) → rows (further indented)                                                                                                        |
| Window title                  | Mod name + version                                                                                                                                                             |
| Built-in presets              | Slow / Default / Fast — never overwritten                                                                                                                                      |
| Dirty edits                   | Active preset becomes **New Preset**; autosave writes there                                                                                                                    |
| After Save as…                | Named preset selected; further edits dirties to **New Preset** again                                                                                                           |
| Preset dropdown               | Load on select; **Save as…** last entry                                                                                                                                        |
| Selection + two-finger rotate | Rotate ghost when **placing new** or **relocating**; click-selected placed objects keep camera yaw                                                                             |
| Selection + ⌥+two-finger      | Place/relocate: orbit around ghost. Otherwise orbit from **current** look-at (no snap to last pivot)                                                                           |
| No selection                  | Prior Maps+ behavior (rotate = camera yaw; ⌥ = orbit)                                                                                                                          |

## Non-goals

- Re-enabling Enable-per-op or Reverse in the shipping UI this pass.
- Turning on compile-time `ENABLE_*` flags.
- Perfect keyboard-vs-popup arbitration beyond existing gates.
