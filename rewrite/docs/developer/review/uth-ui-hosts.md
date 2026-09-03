# UI hosts audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/mod/Ui/**`  
**Contracts:** _Under the hood_, _Feel catalog_, ADR 0005 / UI parity  
**Verdict:** Pass (architecture) / Conditional (tier C pixel parity — human playtest)

## Strengths

- Only `OptionsHost` and `DebugHost` — clone-era TuningPanelHost / OptionsSettingsUi / QA dumps are gone.
- Both hosts share `FeelHostBinder` over `FeelCatalog` descriptors; tests lock shared inventory via `BuildPanelModel`.
- Hosts do not own a parallel field product definition.
- `FeelHostMapping` maps catalog kinds to toolkit widgets; no checkbox-for-all regression.

## Weaknesses

- Tier C floating Debug chrome (drag feel, two-column density) not automated — architecture is covered by unit-testable panel model + HAS_CITIES binder.

## Critical improvements

### P0

1. Implement Options skin from catalog kinds + FeelEditor (UI parity contract). **Closed (feedback cycle 2026-09-03):** dropdown / buttons / checkbox / sensitivity sliders via FeelEditor + FeelHostMapping.
2. Implement Debug as a skin over the same catalog + editor (floating chrome + numeric Sensitivity). **Closed:** `FeelHostBinder.BindCatalog`, `DebugHost.BuildPanelModel`, HAS_CITIES floating panel (title, drag, close → dismiss, reopen chip, position).

### P1

1. Pass one FeelEditor into both hosts; all writes through editor dirty/autosave. **Done.**

### P2

1. Collapse descriptor DTO or add an explicit UI toolkit port for mapping tests. **Done via FeelHostMapping + FeelPanelEntry.**
