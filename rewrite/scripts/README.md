# Rewrite static-analysis scripts (Phase 3)

Lint gates for `rewrite/mod` per [static-analysis-and-quality.md](../docs/developer/static-analysis-and-quality.md) and greenfield lessons **L1 / L6 / L10**. These are structural checks — not tier A–C behavior proof.

## Quick start

From the repository root:

```bash
npm run sa:rewrite
```

Or individually:

```bash
npm run sa:rewrite:semgrep
npm run sa:rewrite:settings-graph
npm run sa:rewrite:leak-pairing
npm run sa:rewrite:layer-import
```

Equivalent:

```bash
bash rewrite/scripts/sa-rewrite.sh
```

## Prerequisites

| Tool      | Used by                      | Install if missing                                                 |
| --------- | ---------------------------- | ------------------------------------------------------------------ |
| `python3` | settings graph, leak pairing | system Python 3.9+                                                 |
| `semgrep` | Semgrep rules                | `pip install semgrep` (or `python3 -m pip install --user semgrep`) |

This environment typically has Semgrep on `PATH` (`~/.local/bin/semgrep`). The orchestrator prints install hints if the CLI is absent.

## 1. Semgrep (`semgrep/rewrite.yml`)

```bash
semgrep scan --config rewrite/scripts/semgrep/rewrite.yml --error --severity ERROR rewrite/mod
```

| Rule ID                                    | Intent                                                                                                                                                            |
| ------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `rewrite.ban-empty-catch`                  | Ban **truly empty** `catch { }` / `catch (Ex) { }` (no statements). Comment-only `// fail soft` catches are **not** matched (intentional Cities soft-fail today). |
| `rewrite.legacy-alias-method`              | Ban known one-hop aliases (`ApplyPreset`, `GetAssemblyVersionDisplay`, `SensitivityMin`/`Max`) — L6.                                                              |
| `rewrite.ensure-style-one-hop-alias`       | **WARNING** heuristic for `Ensure*` methods that only `return` another call.                                                                                      |
| `rewrite.dead-three-finger-on-appkit-path` | Flag `ThreeFinger*` bindings and finger-count-3 style rows on the AppKit ship path (L4).                                                                          |

### fingerCount hardcode policy

- **Allowed:** `AppleGestureMapper.AppKitActiveFingerCount = 2` and Maps+ seed rows with min/max **2**.
- **Flagged:** Three-finger claims on the AppKit-only ship compile path (v1 is Maps+ two-finger only).

## 2. Settings field → tick consumer graph

```bash
python3 rewrite/scripts/settings_field_graph.py
```

Parses public auto-properties on `rewrite/mod/Settings/ModSettings.cs`. Fails when a field is never **read** (`.Name` not followed by `=`) outside the Settings / UI persist layer.

### Exclusions / allowlists

| Kind                 | Fields / rule                                                                                                                     | Effect                                                                                               |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| **chrome**           | `AssistUiEnabled`, `ActiveFeelPresetName`, `IncludeSystemInfoInCopy`, `DebugPanelDismissed`, `DebugPanelPosX`/`Y`, `DebugOverlay` | Skipped (Options/Debug chrome)                                                                       |
| **seed_identity**    | `GesturePreset`                                                                                                                   | Skipped — Maps+ only on v1; tick path reads `StyleTable`                                             |
| **schema_non_field** | `BridgeEnabled`                                                                                                                   | **Fail** if present without an outside reader. Escape hatch: `--allow-schema-non-field` (warn only). |

Persist/UI layers excluded from the consumer search: `rewrite/mod/Settings/**`, `rewrite/mod/Ui/**`.

## 3. Native leak pairing

```bash
python3 rewrite/scripts/native_leak_pairing.py
```

Same pairing model as `tests/TrackpadCameraControl.Tests/NativeResourceLeakAnalyzer.cs`:

| Acquire                                        | Release                                                 |
| ---------------------------------------------- | ------------------------------------------------------- |
| `GCHandle.Alloc`                               | `.Free()` (and `GCHandle` fields require `IDisposable`) |
| `CFStringCreateWithCString` / `CreateCfString` | `CFRelease`                                             |
| `.DeviceStart(`                                | `.DeviceStop(`                                          |
| `addLocalMonitorForEventsMatchingMask`         | `removeMonitor:`                                        |

**Scan roots:** `rewrite/mod`, `src/TrackpadCapture`, `src/TrackpadBridge`, `src/AppleGestureProbe`.

**Allowlist marker:** put `native-leak-ok:` plus a reason on the acquire line for process-lifetime or ownership-transfer cases.

## Exit codes

| Code | Meaning                                                 |
| ---- | ------------------------------------------------------- |
| 0    | All ERROR gates passed                                  |
| 1    | One or more gates failed                                |
| 2    | Tooling missing (e.g. Semgrep) or ModSettings not found |

`npm run sa:rewrite` fails the process if any ERROR gate fails. Semgrep WARNINGs are printed but do not fail the orchestrator.
