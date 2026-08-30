# In-game Assist + tuning panel — Design

**Date:** 2026-08-29  
**Status:** Approved  
**Scope:** Floating in-game Assist / tuning panel mirrored with Options tunables; drag vs button scales; per-op low-pass; durable XML settings

## Goal

Players tune feel with precise number fields from the city view or Options, feel drag vs button scales via Assist chrome, and keep settings across quit. Named user presets can land later on the same file envelope.

## Locked decisions

| Concern | Choice |
| --- | --- |
| Surfaces | In-game panel (chrome + tunables) and Options (tunables only); one ModSettings |
| Controls | Number fields, not sliders |
| Drag vs button | Separate scales; buttons skip low-pass and are not multiplied by drag scale |
| Low-pass | Per-op enable + alpha EMA on drag only |
| Persist | Versioned XML envelope (`schemaVersion`, `current`, reserved `userPresets`) |
| Reset | Factory defaults this slice; Save as… / Load later |
| Built-in presets | Maps+ / CAD seed orbit trigger only; do not wipe custom scales |

## Non-goals

- Named Save as… / Load UI
- Assist chrome inside Options
- Corner auto-hide chrome refinement
- 1€ filter extras beyond alpha
