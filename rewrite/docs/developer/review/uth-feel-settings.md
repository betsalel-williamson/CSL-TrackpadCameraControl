# Feel / Settings audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/mod/Feel/**`  
**Contracts:** _Under the hood_, _Feel catalog_, ADR 0005  
**Verdict:** Conditional

## Strengths

- One `FeelCatalog` matches section order and ship field ids/labels.
- One `FeelEditor` owns Load / Save as / Delete / Reset / dirty→New Preset / ApplyGain.
- Schema v1 `SettingsStore` with injectable path; Feel layer has no Unity/Cities/AppKit usings.
- No QA dump types under Feel.

## Weaknesses

- Dirty model can double-write: `EnsureDirtyNewPreset` → `SaveUserPreset`/`SaveEnvelope` immediately, then `ApplyGain` also marks store dirty (breaks one dirty → one coalesced flush).
- Live blob still carries Assist steps, Enable-per-op, invert, and chrome fields the catalog hides.
- Catalog Sensitivity is always Slider; Numeric unused for Debug.
- `showDebugPanel` not wired through editor to chrome fields (`AssistUiEnabled` naming remains).
- Dead `ModSettingsStore` alias on `SettingsStore`.

## Critical improvements

### P0

None for catalog/editor _shape_ — blockers are hosts ([UI hosts audit](./uth-ui-hosts.md)).

### P1

1. Single coalesced flush path for New Preset dirty + sensitivity edits. **Closed (feedback cycle 2026-09-03):** `UpsertUserPresetInMemory` + one `MarkDirtyAndMaybeFlush`.
2. Wire catalog chrome actions (including show debug) through FeelEditor only. **Done** (`SetShowDebugPanel`).

### P2

1. Trim or module-gate Assist button-step fields when Assist is off.
2. Delete `ModSettingsStore` alias; encode Options vs Debug control kinds in catalog or host policy. **Done** (alias deleted; FeelHostMapping).
