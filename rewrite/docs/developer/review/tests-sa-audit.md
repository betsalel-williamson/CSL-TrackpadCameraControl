# Tests / static-analysis audit (rewrite v1)

Specialist pass for `rewrite/tests` coverage, `sa:rewrite` scripts, and breakage when IPC / Contacts / CAD modules are removed. Scope: `rewrite/tests/`, `rewrite/scripts/sa-rewrite.sh`, `rewrite/scripts/semgrep/`, `settings_field_graph.py`, `native_leak_pairing.py`.

**Related:** [harnesses and testing](../harnesses-and-testing.md), [static analysis and quality](../static-analysis-and-quality.md), [organized product feedback](./v1-product-feedback.md) (F1, F3, F4), [v1 audit plan](./v1-audit-plan.md).

## Strengths

- **Tier A golden Maps+ fixtures exist and pass.** `MapsPlusGoldenFixturesTests` (9 facts) exercises pan, pinch, rotate, Option+orbit, latch/handoff, and style-table resolve through `GestureSession` + `FakeCameraController` — core parity contract (L10 tier A).
- **Light tier B AppKit mapper coverage.** `CaptureSessionLightTests` (7 facts) proves two-finger defaults, Option modifier propagation, honest finger override, and orbit resolve from mapped frames — without hardware.
- **Input gates covered.** `InputGatesTests` (7 facts) for menu/over-UI and game-focus policy.
- **`sa:rewrite` orchestrator is complete.** `sa-rewrite.sh` runs Semgrep ERROR rules, settings field graph, and native leak pairing; exposed as `npm run sa:rewrite` with granular subcommands. **Current status: PASS** (46 settings fields OK, leak pairing OK on `rewrite/mod` + legacy capture roots).
- **Semgrep encodes greenfield lessons.** Empty-catch ban, dead-alias ban, dead-three-finger-on-appkit-path (with `CadSeed.cs` excluded) — aligned with L4/L6.
- **Settings graph documents consumers.** Filter fields correctly attributed to `Capture/DragLowPass.cs`; `CaptureBackend` to `CaptureBackendFlags.cs`; chrome fields skipped — gives a deletion checklist for R2.

## Weaknesses

- **No `ModBuildInfoTests` in rewrite.** Version/display regression unguarded (see [release audit](./release-audit.md)).
- **No UI or Harmony tests.** Entire ColossalUI surface and postfix order are tier C only — expected per [harnesses and testing](../harnesses-and-testing.md) but leaves R2 UI deletes unproven in automation.
- **No Contacts / IPC / CAD compile-on test matrix.** Tests always run against default ship constants (`Enable*` false). Removing files will not be caught by tests that only ever built AppKit+Maps+ — only compile + SA gates.
- **Tier B does not cover `GesturePipeline` or capture source swap.** `GesturePipeline` still references `InProcessGestureSource` and `DragLowPass` behind `ENABLE_CONTACTS_CAPTURE`; no test asserts ship path never constructs them.
- **Semgrep / SA still assume removable modules exist.** `rewrite.dead-three-finger-on-appkit-path` excludes `CadSeed.cs`; `native_leak_pairing.py` scans `src/TrackpadCapture` even when rewrite csproj does not link it on ship — noise after F3 deletion unless scan roots shrink.
- **`FeatureFlags.cs` runtime const mirrors** contradict [feature flags](../feature-flags.md) (“do not introduce runtime facade”) — tests/docs may read consts; ship path should not depend on them.
- **lint-staged does not include `rewrite/mod/**/*.cs` yet** (plan R6.1) — format drift on rewrite C# vs shipping.

## What breaks when IPC / Contacts / CAD are removed

### IPC + Contacts (R2.1 — F1, F3)

| Area                                             | Break / required follow-up                                                                                                                                                        |
| ------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Compile**                                      | Delete or stop `Compile Remove` for `IpcGestureSource.cs`, `InProcessGestureSource.cs`, `DragLowPass.cs`; remove csproj link to `src/TrackpadCapture/*.cs`.                       |
| **`Mod.CreateCaptureSource`**                    | `#else` branch already returns `AppleGestureSource`; delete `#if ENABLE_CONTACTS_CAPTURE` fork and `InProcessGestureSource` reference.                                            |
| **`GesturePipeline`**                            | Remove `DragLowPass` field, low-pass tick branches, and `SwapCaptureSource` / `InProcessGestureSource` paths.                                                                     |
| **`CaptureBackendFlags`, `CaptureBackend` enum** | Delete or collapse to AppKit-only no-op; settings graph currently expects `CaptureBackend` reader — field must leave `ModSettings` or graph fails.                                |
| **Settings fields**                              | `Pan/Zoom/Rotate/OrbitFilter*` only read in `DragLowPass.cs` today — **settings graph will FAIL** until fields are removed from `ModSettings` or readers deleted with the module. |
| **`ModOptions` labels**                          | `CaptureBackendLabels`, `ApplyCaptureBackendIndex`, low-pass apply helpers — delete with UI.                                                                                      |
| **UI `#if ENABLE_CONTACTS_CAPTURE`**             | Safe to delete blocks; no test references.                                                                                                                                        |
| **`sa:rewrite`**                                 | Re-run; update `native_leak_pairing.py` roots if `src/TrackpadCapture` is no longer a rewrite dependency.                                                                         |
| **Tests**                                        | Existing 23 tests should still pass (AppKit-only). Add negative test optional: ship build never references `InProcessGestureSource`.                                              |

### CAD gesture style (R2.2 — F4)

| Area                                 | Break / required follow-up                                                                                                                                                |
| ------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`Policy/CadSeed.cs`**              | Delete file; remove `ENABLE_CAD_GESTURE_STYLE` from csproj.                                                                                                               |
| **`ModSettings.ApplyGesturePreset`** | Remove `GesturePreset.CAD` branch and `CadSeed.CreateTable()` call; keep Maps+ only or map CAD to docs-only v2 preset.                                                    |
| **`GesturePreset` enum / XML**       | Decide: remove `CAD` value or keep deserialize-only stub for old saves (R2.3 legacy settings scope).                                                                      |
| **Semgrep**                          | Remove `CadSeed.cs` path exclude from `rewrite.dead-three-finger-on-appkit-path` once file is gone — rule still valuable for accidental three-finger rows on AppKit path. |
| **`TrackpadGesture.cs` comments**    | CAD catalog comments remain; ensure no compiled three-finger bindings outside deleted seed.                                                                               |
| **Tests**                            | No CAD-specific tests today — **gap**: tier A should still pass Maps+ only; add regression that default `StyleTable` never contains finger-count-3 orbit rows.            |

### Assist chrome (implicit in R2 / flags doc)

| Area                              | Break / required follow-up                                                                                                                                                                                                                         |
| --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **UI `#if ENABLE_ASSIST_CHROME`** | Button-step fields in Options/Debug — delete blocks; `PanStep*` / `ZoomStep*` / `Orbit*Step` / `RotateStep` still have tick readers in `CameraApplicator` — **settings graph keeps them OK** unless applicator stops reading unused steps on ship. |
| **Product decision**              | Shipping omits button-step UI on ship DLL but fields persist in schema; v1 may keep fields without UI or strip if no tick consumer on ship (verify applicator).                                                                                    |

## Critical improvements

1. **After R2.1:** remove filter and `CaptureBackend` fields from `ModSettings`; update `settings_field_graph.py` exclusions if any chrome-only remnants remain; trim leak-pairing scan roots.
2. **Add `ModBuildInfoTests`** (R5.1) and optional **Maps+ style-table regression** asserting no three-finger rows in factory `StyleTable`.
3. **Extend tier B** with one `GesturePipeline` smoke test using `InjectGestureSource` / E2E inject flag path — proves enable → frame → session without Contacts.
4. **Update Semgrep** post-deletion: drop `CadSeed.cs` exclude; add rule or graph fail for `CaptureBackend` / `*FilterEnabled` if they reappear without consumers (plan R6.2).
5. **Include rewrite C# in lint-staged** (R6.1) so SA + format run before push alongside `sa:rewrite` on R2 commits.

## Commit mapping

| Plan commit | Concern                               | SA / test impact                                                                                     |
| ----------- | ------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| **R2.1**    | Remove IPC + Contacts                 | Fix settings graph consumers; shrink leak-pairing roots; re-run `dotnet test` + `npm run sa:rewrite` |
| **R2.2**    | Remove CAD module                     | Update Semgrep CadSeed exclude; add style-table regression test                                      |
| **R2.3**    | Legacy settings collapse              | May add migration tests or delete `LegacyModSettings` fixtures                                       |
| **R5.1**    | `ModBuildInfoTests` for rewrite       | +6 tests approx.                                                                                     |
| **R6.1**    | lint-staged for `rewrite/mod/**/*.cs` | Pre-commit format on rewrite edits                                                                   |
| **R6.2**    | SA rules + shard sync                 | `rewrite/scripts/semgrep/rewrite.yml`, `scripts/README.md`, feature-flags / settings-schema shards   |
| **R1.1**    | This shard                            | `rewrite/docs/developer/review/tests-sa-audit.md`                                                    |

## Current baseline (audit date)

```text
dotnet test rewrite/tests/...  → 23 passed, 0 failed
npm run sa:rewrite             → PASS (semgrep ERROR, settings graph, leak pairing)
```
