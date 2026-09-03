# UI hosts audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/mod/Ui/**`  
**Contracts:** _Under the hood_, _Feel catalog_, ADR 0005 / UI parity  
**Verdict:** Fail (for player-visible parity) / Pass (for “not a second product”)

## Strengths

- Only `OptionsHost` and `DebugHost` — clone-era TuningPanelHost / OptionsSettingsUi / QA dumps are gone.
- Both hosts share `OptionsHost.BuildDescriptors()` from FeelCatalog; tests lock shared inventory.
- Hosts do not own a parallel field product definition.

## Weaknesses

- Options `Build` maps every control to `AddCheckbox` and only notifies change — no dropdown/slider/button binds through FeelEditor (not UX parity).
- Debug host is scaffolding (`IsCreated` / empty visibility) — no floating panel chrome or numeric Sensitivity.
- Hosts take `ModSettings` but do not apply through a shared `FeelEditor` instance.
- Catalog `Kind` is ignored at build time.
- `FeelControlDescriptor` mirrors catalog fields without a toolkit-port seam.

## Critical improvements

### P0

1. Implement Options skin from catalog kinds + FeelEditor (UI parity contract).
2. Implement Debug as a skin over the same catalog + editor (floating chrome + numeric Sensitivity).

### P1

1. Pass one FeelEditor into both hosts; all writes through editor dirty/autosave.

### P2

1. Collapse descriptor DTO or add an explicit UI toolkit port for mapping tests.
