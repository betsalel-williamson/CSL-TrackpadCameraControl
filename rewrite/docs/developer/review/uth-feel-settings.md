# Feel / Settings audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/mod/Feel/**`  
**Contracts:** _Under the hood_, _Feel catalog_, ADR 0005  
**Verdict:** Pass

## Strengths

- One `FeelCatalog` matches section order and ship field ids/labels.
- One `FeelEditor` owns Load / Save as / Delete / Reset / dirty→New Preset / ApplyGain / debug chrome (`SetShowDebugPanel`, `DismissDebugPanel`, `SaveDebugPanelPosition`).
- Schema v1 `SettingsStore` with injectable path; Feel layer has no Unity/Cities/AppKit usings.
- No QA dump types under Feel.
- Assist button-step fields stay on `ModSettings` for future module; catalog and hosts never expose `*Step*` ids (regression test).

## Weaknesses

- Live blob still carries Enable-per-op, invert, and chrome fields the catalog hides (by design — tick consumers, not product UI).
- Catalog Sensitivity is always Slider; Numeric unused for Debug (acceptable — slider maps to gain).

## Critical improvements

### P0

None for catalog/editor _shape_ — blockers were hosts ([UI hosts audit](./uth-ui-hosts.md)). **Closed.**

### P1

1. Single coalesced flush path for New Preset dirty + sensitivity edits. **Closed (feedback cycle 2026-09-03):** `UpsertUserPresetInMemory` + one `MarkDirtyAndMaybeFlush`.
2. Wire catalog chrome actions (including show debug) through FeelEditor only. **Done** (`SetShowDebugPanel`, `DismissDebugPanel`).

### P2

1. Trim or module-gate Assist button-step fields when Assist is off. **Closed:** fields kept for future Assist; catalog omits; regression test; FeelMath path retained for tests.
2. Delete `ModSettingsStore` alias; encode Options vs Debug control kinds in catalog or host policy. **Done** (alias deleted; FeelHostMapping).
