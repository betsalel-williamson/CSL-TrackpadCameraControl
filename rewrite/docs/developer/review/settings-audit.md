# Settings / schema audit (rewrite v1)

Specialist pass over `rewrite/mod/Settings/` and the [settings schema](../settings-schema.md) contract. Scope: greenfield v1 cleanup — drop prototype migration paths, align persisted fields with tick consumers, and map work to commits **R2.3** and **R4.1** in the [v1 audit plan](./v1-audit-plan.md).

**As-built sources reviewed:** `ModSettings.cs`, `ModSettingsStore.cs`, `LegacyModSettings.cs`, `ModOptions.cs`, `FeatureFlags.cs`, `FeelProfiles.cs`, `settings-schema.md`, `settings_field_graph.py`.

## Strengths

- **Feel vs style separation is clear in code and docs.** `FeelProfiles` copies enables, gains, deadbands, and sign-invert only; gesture style (`GesturePreset`, style table, per-op bindings) stays out of feel presets (L2). `ActiveFeelPresetName` and the `userPresets[]` envelope match the schema shard.
- **Control-systems naming is consistent.** Persist uses gain / step / deadband / sign invert; Options UI maps to product “Sensitivity” via `ModOptions.GainToSensitivityUi` / `SensitivityUiToGain` with three-decimal rounding policy.
- **Versioned XML envelope is well structured.** `schemaVersion` + `current` + `userPresets[]`, dirty-bit autosave, corrupt/missing → factory + rewrite, and injectable path for tests.
- **Style binding table is the real tick consumer for resolve.** `StyleBindingResolver` reads `StyleTable` only (L1, ADR 0004); Maps+ seed rows match documented chords (two-finger pan/pinch/rotate, Option+two-finger orbit).
- **Static analysis anticipates cleanup.** `settings_field_graph.py` classifies chrome, seed identity, XML aliases, and schema non-fields (`OrbitTrigger`, `BridgeEnabled`); Semgrep flags dead three-finger paths outside `CadSeed`.
- **Module gates are documented.** [Feature flags](../feature-flags.md) and [settings schema](../settings-schema.md) agree that Contacts filters, CAD preset, and Assist button steps are compile-gated — not ship ceremony.

## Weaknesses

- **Nine prototype schema versions with full migration ladder.** `CurrentSchemaVersion = 9` plus `LegacyModSettings`, `TryLoadLegacy`, schema 1 scroll-unit fold (`V1ScrollUnit`), and eight XML alias property pairs on `ModSettings` exist only for early rewrite iterations. v1 greenfield intent (F6) is a single write path with no alias hops.
- **Dual gesture representation.** Tick resolve consumes `[XmlIgnore] StyleTable`, but eight persisted per-op fields (`ZoomGesture` / `*GestureModifier` × 4) duplicate seed data. They are written on preset apply and shown in Debug labels via `TrackpadGestureCatalog.GetBinding`, yet `StyleBindingResolver` never reads them — drift risk if table and fields disagree.
- **OrbitTrigger enum is dead ceremony.** Defined on `ModSettings.cs`, persisted only on `LegacyModSettings`, never mapped in `ToModSettings()`. Policy uses `StyleBindingResolver.IsOrbitTriggerActive` (style rows), not the enum. Schema shard and SA already mark `OrbitTrigger` as a non-field (L6).
- **CaptureBackend and Contacts filter fields on the live blob.** `CaptureBackend`, `PanFilterEnabled` / `*FilterAlpha` (×4 ops) persist and surface in Options/Debug when `ENABLE_CONTACTS_CAPTURE` compiles, but ship path is AppKit-only (F3, L9). Low-pass runs only through `DragLowPass` on the Contacts path.
- **CAD gesture preset still in the DLL surface.** `GesturePreset` (MapsPlus / CAD / Custom), `ApplyGesturePreset` CAD branch, `CadSeed`, and Options/Debug preset switchers remain behind `#if ENABLE_CAD_GESTURE_STYLE`. User intent: CAD is a **v2 docs-only preset**, not v1 code (F4).
- **Button-step fields ship without Assist module.** `PanStep*` / `ZoomStep` / `RotateStep` / `Orbit*Step` persist and appear in Options/Debug UI, but [settings schema](../settings-schema.md) ties steps to `EnableAssistChrome` only. `CameraApplicator` reads steps on the chrome nudge path — without the module compiled, persisted steps are unused ceremony (L6).
- **FeatureFlags const mirrors vs doc rule.** [Feature flags](../feature-flags.md) forbids a runtime facade; `FeatureFlags.cs` exists as `#if` const mirrors for tests/docs. Acceptable if call sites stay `#if`-only — but the type still invites runtime misuse.
- **EnsureMapsPlusRotateBinding workaround.** Store load re-seeds rotate bindings when `RotateGesture == None`, patching Mono `XmlSerializer` alias self-assignment bugs. Removing aliases (R2.3) should allow deleting this guard.
- **Doc/code version skew.** Code comments reference “schema ≥3”, “schema 7+”, “schema 8”, “schema 9”; the schema shard describes the **target** live surface, not the nine-step history. No single “v1 ship” version number in code today.

## Critical improvements

### Proposed v1 schema version: **1**

Reset the persist envelope to **`schemaVersion = 1`** for the greenfield ship surface. Treat prior versions 1–9 as **prototype history only** — not migrated forward.

| v1 envelope rule                    | Detail                                                                                     |
| ----------------------------------- | ------------------------------------------------------------------------------------------ |
| Missing / corrupt / unknown version | Factory defaults → save as v1                                                              |
| No legacy loader                    | Delete `LegacyModSettings` and pre-v3 branches                                             |
| No XML aliases                      | Live element names only; no deserialize-only shadow properties                             |
| Module-off fields omitted           | No `CaptureBackend`, filter rows, CAD preset identity, or Assist steps in default DLL XML  |
| Style on ship                       | Maps+ seed via in-memory `StyleTable` at load; no free-form gesture remap persistence (L1) |

After R2.3 lands, bumping to v1 on first save rewrites existing dev `settings.xml` files — acceptable for rewrite greenfield; document in PR #46.

### R2.3 — Remove legacy settings types

**Commit subject:** `refactor(rewrite): v1 settings schema without legacy migration`

| Action                    | Targets                                                                                                                                         |
| ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| Delete file               | `LegacyModSettings.cs` (`LegacyModSettings`, `LegacySettingsEnvelope`, `LegacyNamedPreset`)                                                     |
| Collapse store            | Remove `TryLoadLegacy`, `EngineeringNamesSchemaVersion`, `MigrateScrollUnitIntoGain`, `V1ScrollUnit`, schema-version migration comments for 2–9 |
| Set version               | `CurrentSchemaVersion = 1`; single `TryLoad` / `Envelope` path                                                                                  |
| Remove XML aliases        | All `[XmlElement(...)]` alias pairs on `ModSettings` (Yaw*, PinchEpsilon, RotateEpsilon, YawDeadband, YawFilter*)                               |
| Remove alias workaround   | `EnsureMapsPlusRotateBinding` (verify with tier A load/save tests)                                                                              |
| Drop enum definition      | `OrbitTrigger` enum (legacy-only; style table owns orbit)                                                                                       |
| Trim `CopyFrom` / factory | Stop copying deleted fields                                                                                                                     |

**Coordination:** R2.1 removes `CaptureBackend` from settings; R2.2 removes CAD preset branches — R2.3 should land after or in the same phase so the v1 blob shape is stable.

### R4.1 — Modifier / orbit trigger consolidation

**Commit subject:** `refactor(rewrite): consolidate modifier and orbit resolve for v1`

| Action                                                                        | Rationale                                                                                                                                                                                                               |
| ----------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Single modifier source on tick path                                           | `StyleBindingRow.RequiredModifiers` / `ForbiddenModifiers` + capture `frame.modifiers` (enriched by `GameModifierKeys` in-process) — not parallel per-op `GestureModifierKey` fields unless they **drive** table reseed |
| Remove persisted per-op gesture + modifier fields **or** stop persisting them | Prefer `[XmlIgnore]` runtime mirror synced from `MapsPlusSeed` on load; Debug labels read table rows, not duplicate properties (L1, L5)                                                                                 |
| Rename confusing API                                                          | Consider renaming `IsOrbitTriggerActive` → orbit-match helper without “Trigger” enum connotation                                                                                                                        |
| Audit `GestureModifierKey` vs `GestureModifiers`                              | Catalog/display enum vs capture flags — keep both only if boundaries are explicit (catalog seeds table; capture supplies runtime flags)                                                                                 |
| Gate button-step fields                                                       | Move `*Step` fields behind `ENABLE_ASSIST_CHROME` or drop from v1 persist (schema module row)                                                                                                                           |
| Align `GestureResolveMode`                                                    | Has tick consumer (`GestureSession`); confirm v1 ships **Concurrent** only or document if PrimaryOnly/SessionLock remain experimental                                                                                   |

**Tests:** Maps+ golden fixtures unchanged; settings field graph passes without `--allow-schema-non-field`; no reads of deleted fields.

## Field deletion list

Fields and types to **remove from the v1 live blob, Options, and Debug** (default ship build). Grouped by cleanup commit.

### R2.3 — Legacy migration and aliases

| Item                                                                                                                                                                                                                                          | Notes                             |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------- |
| Entire `LegacyModSettings.cs`                                                                                                                                                                                                                 | Schema 1–2 loader                 |
| `MigrateScrollUnitIntoGain` / `V1ScrollUnit`                                                                                                                                                                                                  | Schema 1 → 2 fold                 |
| `TryLoadLegacy` / legacy envelope types                                                                                                                                                                                                       | Single load path                  |
| `OrbitTrigger` enum                                                                                                                                                                                                                           | Non-field; style table owns orbit |
| XML aliases: `YawGestureXml`, `YawGestureModifierXml`, `PinchEpsilonXml`, `RotateEpsilonXml`, `YawDeadbandXml`, `YawEnabledXml`, `YawRotateGainXml`, `YawRotateStepXml`, `SignInvertYawRotateXml`, `YawFilterEnabledXml`, `YawFilterAlphaXml` | Deserialize-only hops             |
| `EnsureMapsPlusRotateBinding`                                                                                                                                                                                                                 | Alias bug workaround              |

### R2.1 coordination — Contacts / capture (settings surface)

| Field                                                                                                           | Notes                                                                   |
| --------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------- |
| `CaptureBackend`                                                                                                | AppKit-only ship; env override goes with `CaptureBackendFlags` deletion |
| `PanFilterEnabled`, `PanFilterAlpha`                                                                            | Contacts low-pass                                                       |
| `ZoomFilterEnabled`, `ZoomFilterAlpha`                                                                          | Contacts low-pass                                                       |
| `RotateFilterEnabled`, `RotateFilterAlpha`                                                                      | Contacts low-pass                                                       |
| `OrbitFilterEnabled`, `OrbitFilterAlpha`                                                                        | Contacts low-pass                                                       |
| `ModOptions.CaptureBackendLabels`, `CaptureBackendToIndex`, `IndexToCaptureBackend`, `ApplyCaptureBackendIndex` | Options/Debug chrome                                                    |

### R2.2 coordination — CAD preset (docs-only v2)

| Field / type                                                           | Notes                                          |
| ---------------------------------------------------------------------- | ---------------------------------------------- |
| `GesturePreset.CAD`, `GesturePreset.Custom`                            | Maps+ only in v1 DLL                           |
| `ApplyGesturePreset` CAD branch                                        | Document CAD seeds in features/docs for v2     |
| `ModOptions.GesturePresetLabels`, `CadDescription`, preset switcher UI | Remove from ship UI                            |
| `Policy/CadSeed.cs`                                                    | Delete from v1 tree; retain seed table in docs |

### R4.1 — Modifier / style duplication

| Field                                                                              | Notes                                                                           |
| ---------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- |
| `ZoomGesture`, `ZoomGestureModifier`                                               | Prefer table-only or runtime-only sync from seed                                |
| `PanGesture`, `PanGestureModifier`                                                 | Same                                                                            |
| `RotateGesture`, `RotateGestureModifier`                                           | Same                                                                            |
| `OrbitGesture`, `OrbitGestureModifier`                                             | Same                                                                            |
| `PanStepX`, `PanStepY`, `ZoomStep`, `RotateStep`, `OrbitYawStep`, `OrbitPitchStep` | Module-gated Assist chrome; omit from v1 persist unless `EnableAssistChrome` on |

### Already non-fields (ensure absent after cleanup)

| Name                                             | Notes                                  |
| ------------------------------------------------ | -------------------------------------- |
| `OrbitPitchMin`, `OrbitPitchMax`                 | Apply constants only (legacy had them) |
| `FingerCountHysteresis`                          | Legacy only                            |
| `BridgeEnabled`                                  | Legacy only; out-of-process bridge     |
| Runtime `FeatureFlags` facade usage on tick path | Compile `#if` only                     |

## Keep for v1 (tick + chrome + envelope)

Aligns with [settings schema](../settings-schema.md):

| Kind                        | Fields                                                                                                                                                                       |
| --------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Feel (tick)**             | `PanEnabled` … `OrbitEnabled`; `PanGainX/Y`, `ZoomGain`, `RotateGain`, `OrbitYawGain`, `OrbitPitchGain`; sign invert ×6; `MotionDeadband`, `PinchDeadband`, `RotateDeadband` |
| **Gates (tick)**            | `RequireGameFocus`, `IgnoreOverUi`                                                                                                                                           |
| **Style (tick, in-memory)** | `[XmlIgnore] StyleTable` seeded from Maps+ at load                                                                                                                           |
| **Resolve (tick)**          | `GestureResolveMode` (confirm ship value)                                                                                                                                    |
| **Chrome**                  | `AssistUiEnabled`, `ActiveFeelPresetName`, `IncludeSystemInfoInCopy`, `DebugPanelDismissed`, `DebugPanelPosX/Y`, `DebugOverlay`                                              |
| **Envelope**                | `schemaVersion` (=1), `current`, `userPresets[]` (feel snapshots only)                                                                                                       |

## Commit mapping summary

| Commit   | Scope                                     | Settings outcome                                                                           |
| -------- | ----------------------------------------- | ------------------------------------------------------------------------------------------ |
| **R2.3** | Delete legacy types; collapse store to v1 | `schemaVersion = 1`, no aliases, no `LegacyModSettings`, no migration ladder               |
| **R4.1** | Modifier / orbit consolidation            | One style-table modifier path; drop duplicate gesture fields and orphan steps; tests green |

**Related commits (same phase, settings touch):**

| Commit | Settings touch                                                                            |
| ------ | ----------------------------------------------------------------------------------------- |
| R2.1   | Remove `CaptureBackend` + filter fields and UI bindings                                   |
| R2.2   | Remove CAD preset enum values, UI, and `CadSeed` from DLL                                 |
| R6.2   | Refresh [settings-schema.md](../settings-schema.md) and SA exclusions to match v1 reality |

## Validation gates

After R2.3 + R4.1:

```bash
dotnet test TrackpadCameraControl.sln
npm run sa:rewrite          # settings_field_graph + Semgrep
npm run docs:rewrite
```

Tier A: settings load/save round-trip, feel preset CRUD, Maps+ golden fixtures unchanged.
