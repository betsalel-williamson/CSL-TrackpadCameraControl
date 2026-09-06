# ADR 0002 — Gesture presets as seeds

## Status

**Historical.** Superseded for product language by [ADR 0003 — Feel profiles and product flags](./0003-feel-profiles-and-product-flags.md). Superseded for resolve ownership by [ADR 0004 — Style table-driven resolve](./0004-style-table-driven-resolve.md).

Kept only so agents reading older notes understand why “Maps+/CAD presets” appeared in early design.

## Context (historical)

Map-app-aligned gestures often avoid OS three-finger conflicts; CAD users expect three-finger orbit. Hardcoding either profile locks out the other audience. Experimentation requires mid-session changes. Preset names must not imply a single OS.

## Decision (historical)

Expose **Maps+** and **CAD** as Options **presets that seed** a fully editable binding and feel table. Every parameter lives in ModSettings and applies hot. Defaults exist only in the settings schema. OS-specific modifier key labels belong in client platform notes, not in the preset product name.

## Why superseded

| Concern                        | Successor                                                                                         |
| ------------------------------ | ------------------------------------------------------------------------------------------------- |
| Player-facing “preset” meaning | ADR 0003 — feel profile (Slow / Default / Fast), not Maps+/CAD                                    |
| Maps+/CAD as Options presets   | ADR 0003 — gesture style; CAD behind compile `EnableCadGestureStyle`                              |
| Who owns chord → op mapping    | ADR 0004 — style binding table is resolve SOT; Maps+ is **seed data** for parity, not a UI preset |

Gesture style as a distinct concern from feel remains valid; the rewrite encodes that split in ADR 0003 and table-driven resolve in ADR 0004.

## Consequences (historical → rewrite)

- Do not reintroduce Maps+/CAD as player “presets that seed bindings” on the Options surface.
- Do not document a live binding table unless Policy resolve consumes it (ADR 0004).
- Camera/gesture code must not embed feel literals; style chords live in the seeded table.
