# Architecture audit (rewrite mod v1)

**Audience:** Contributors and agents executing the v1 prototype cleanup on `rewrite/mod` — reconciling the built three-plane layout with greenfield redesign lessons L6–L9 and the features-guide system architecture shard.

**Scope:** Host lifecycle (`Mod`, `ModRuntime`, `GesturePipeline`), Capture plane wiring, Harmony scope, inject/E2E seams, and compile-gated prototype modules. Settings schema, UI chrome, logging, and release display are out of scope here — see sibling audit shards.

**As-built snapshot:** Default ship build (`EnableCadGestureStyle`, `EnableContactsCapture`, `EnableAssistChrome` all false) runs AppKit capture → Policy session + style table → Apply, with narrow Harmony patches. Prototype carryover still leaves dead factories, on-disk Contacts/IPC types, and alias hops that violate L6/L9.

---

## Strengths — what the three-plane architecture does well

The rewrite already implements the target separation described in the features-guide system architecture shard and L8.

| Plane         | What works                                                                                                                            | Evidence                                                                                    |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| **Capture**   | Single primitive contract; AppKit backend fills frames without camera decisions; honest two-finger defaults for scroll/magnify/rotate | `Capture/GestureFrame.cs`, `Capture/AppleGestureMapper.cs`, `Capture/AppleGestureSource.cs` |
| **Policy**    | Per-tick gate sync, orbit latch / rotate ownership, style binding table resolve — no cached focus or camera pose                      | `Policy/InputGates.cs`, `Policy/GestureSession.cs`, `Policy/StyleBindingResolver.cs`        |
| **Apply**     | Feel math isolated from capture; pitch clamp treated as apply constant                                                                | `Apply/CameraApplicator.cs`, `Apply/CameraControllerZoom.cs`                                |
| **Host tick** | One simulation entry point walks Capture → Policy → Apply each frame                                                                  | `Host/GestureThreading.cs` → `Host/GesturePipeline.cs`                                      |
| **Harmony**   | Narrow scope: precise trackpad scroll suppress prefix, deferred orbit velocity flush postfix — not used as a policy input cache       | `Host/Patcher.cs` (`HandleScrollWheelEventPatch`, `HandleMouseEventsPatch`)                 |
| **Lifecycle** | Enable/disable owns runtime bag, patch apply/remove, capture arm on city load                                                         | `Host/Mod.cs`, `Host/ModRuntime.cs`, `Host/LoadingExtension.cs`                             |

Additional wins aligned with L3 and L10:

- **`IGestureSource`** gives Policy a backend-agnostic dequeue loop — the right abstraction for tier A inject tests and optional in-game harness.
- **`GameModifierKeys.Enrich`** merges Unity keyboard state into frame modifiers on the in-process AppKit path (Option-orbit parity when NSEvent modifier flags lag game focus).
- **`ModRuntime`** keeps settings, pipeline, and active flag in one lifecycle bag instead of scattering statics across planes.

---

## Weaknesses — prototype carryover and dead paths

### L6 violations — useless redirection

| Issue                                    | Location                                                                                                     | Why it violates L6                                                                                                                      |
| ---------------------------------------- | ------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------- |
| **Dual capture factories**               | `Host/Mod.CreateCaptureSource`, `Host/GesturePipeline.EnsureCaptureSource`, `Capture/CaptureBackendFlags.cs` | Ship path always resolves to `AppleGestureSource`; backend selection and hot-swap are dead weight when Contacts is off.                 |
| **Maintainer env override for Contacts** | `CaptureBackendFlags.Resolve` — `TRACKPAD_CAPTURE_BACKEND=contacts`                                          | Env can force a backend that is not compiled into the ship DLL — redirection without a supported seam.                                  |
| **Mod alias shims**                      | `Host/Mod.cs` — `Pipeline`, `InjectSource` static accessors                                                  | Alias hops for legacy call sites; `ModRuntime` already holds the pipeline.                                                              |
| **`ModRuntime.Inject` property**         | `Host/ModRuntime.cs`                                                                                         | Exists only for E2E; duplicates `_source` already reachable via `Pipeline.Source`.                                                      |
| **Tick-path inject hot-swap**            | `GesturePipeline.EnsureInjectSourceIfArmed`                                                                  | Scans env/flag files every tick to swap capture source — test concern on the production tick path.                                      |
| **`FeatureFlags` const mirrors**         | `Settings/FeatureFlags.cs`                                                                                   | Documented as compile-only, but the type itself is a runtime-visible mirror of `#if` — no second implementation earns its keep on ship. |
| **Empty `DragLowPass` shell**            | `Capture/DragLowPass.cs` (compiled on ship, body `#if`-gated)                                                | Type ships in the DLL with no-op methods when Contacts is off — exactly the “empty object on tick path” L9 rejects.                     |
| **Settings cache fallback**              | `Mod.EnsureSettingsInternal` + `_settingsCache` when `Runtime` is null                                       | Secondary settings path beside `ModRuntime.Settings`.                                                                                   |

### L9 violations — compile-gated stubs instead of omission

Greenfield intent (L9): unfinished modules are **absent from the v1 tree**, not hidden behind `#if` with files still on disk.

| Module                | Still on disk today                                                                                                                                                                                                   | Ship DLL behavior                             | Problem                                                                        |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------- | ------------------------------------------------------------------------------ |
| **Contacts capture**  | `Capture/InProcessGestureSource.cs`, `Capture/DragLowPass.cs`, `Capture/CaptureBackendFlags.cs`, `Settings/ModSettings.CaptureBackend`, UI `#if ENABLE_CONTACTS_CAPTURE` blocks, csproj link to `src/TrackpadCapture` | Types compile-removed or no-op; AppKit forced | Prototype surface preserved for a backend v1 will never ship.                  |
| **IPC bridge**        | `Capture/IpcGestureSource.cs`                                                                                                                                                                                         | `Compile Remove` when Contacts off            | Obsolete inter-process path; file and docs references remain (F1).             |
| **CAD gesture style** | `Policy/CadSeed.cs`, `GesturePreset.CAD`, `#if ENABLE_CAD_GESTURE_STYLE` in `ModSettings.ApplyGesturePreset`, UI preset buttons                                                                                       | Omitted from ship DLL                         | v1 should be Maps+ only; CAD belongs in v2 docs/seeds, not compile stubs (F4). |

### IPC vs inject — do not conflate

| Path                         | Mechanism                                                                | Verdict                                                                                                                          |
| ---------------------------- | ------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------- |
| **IPC pipeline (remove)**    | Unix socket (`IpcGestureSource`) to external TrackpadBridge process      | Obsolete; violates L6. Delete entirely in R2.1.                                                                                  |
| **In-process inject (keep)** | `InjectGestureSource` queue + optional `E2eInjectFileProtocol` file poll | Maintainer-only test seam; env/flag gated; no external process. See [Critical improvements](#critical-improvements-prioritized). |

### Harmony scope creep (minor)

`Host/Patcher.cs` also patches Options keymapping panels and `SavedInputKey` for vanilla camera label watch — related to UI parity, not camera suppress/orbit flush. Acceptable for v1 parity (L11) but worth noting: Harmony surface is slightly wider than the two patches named in the features-guide system architecture shard.

---

## Critical improvements (prioritized)

### P0 — Remove Contacts and IPC capture (R2.1)

**Delete these files** (not `#if` off — remove from tree):

| File                                            | Reason                                       |
| ----------------------------------------------- | -------------------------------------------- |
| `rewrite/mod/Capture/IpcGestureSource.cs`       | Obsolete IPC bridge (F1).                    |
| `rewrite/mod/Capture/InProcessGestureSource.cs` | Contacts MultitouchSupport backend (F3).     |
| `rewrite/mod/Capture/DragLowPass.cs`            | Contacts-only low-pass; empty shell on ship. |
| `rewrite/mod/Capture/CaptureBackendFlags.cs`    | Dual-backend selection dead on v1.           |

**Trim these files:**

| File                                               | Change                                                                                                                                                                                                                                         |
| -------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `rewrite/mod/Host/Mod.cs`                          | Remove `CreateCaptureSource`; always construct `AppleGestureSource` (or inject when E2E armed at enable). Remove `CaptureBackendFlags.Resolve` logging. Remove `Mod.Pipeline` / `Mod.InjectSource` shims — callers use `Mod.Runtime.Pipeline`. |
| `rewrite/mod/Host/GesturePipeline.cs`              | Remove `EnsureCaptureSource`, `#if ENABLE_CONTACTS_CAPTURE` low-pass blocks, Contacts reconnect branches. Single production source: `AppleGestureSource`.                                                                                      |
| `rewrite/mod/Settings/ModSettings.cs`              | Remove `CaptureBackend` property and filter fields tied to Contacts.                                                                                                                                                                           |
| `rewrite/mod/Settings/ModOptions.cs`               | Remove `CaptureBackendLabels`, index helpers.                                                                                                                                                                                                  |
| `rewrite/mod/Settings/LegacyModSettings.cs`        | Remove `CaptureBackend` (file deleted entirely in R2.3).                                                                                                                                                                                       |
| `rewrite/mod/Ui/OptionsSettingsUi.cs`              | Remove `#if ENABLE_CONTACTS_CAPTURE` backend dropdown.                                                                                                                                                                                         |
| `rewrite/mod/Ui/TuningPanelHost.cs`                | Remove `#if ENABLE_CONTACTS_CAPTURE` backend buttons and filter UI.                                                                                                                                                                            |
| `rewrite/mod/TrackpadCameraControl.Rewrite.csproj` | Remove `EnableContactsCapture` property group, `ENABLE_CONTACTS_CAPTURE` define, Contacts `Compile Include` / `Compile Remove` blocks.                                                                                                         |
| `rewrite/mod/Settings/FeatureFlags.cs`             | Remove `EnableContactsCapture` const block.                                                                                                                                                                                                    |
| `rewrite/mod/Capture/GameModifierKeys.cs`          | Update comment — no longer “out-of-process / Contacts”; keep enrich for AppKit + Unity keyboard parity.                                                                                                                                        |

**Keep these Capture files:**

| File                             | Reason                                                            |
| -------------------------------- | ----------------------------------------------------------------- |
| `Capture/IGestureSource.cs`      | Core abstraction for backend and inject.                          |
| `Capture/GestureFrame.cs`        | Shared primitive contract (L3).                                   |
| `Capture/AppleGestureSource.cs`  | v1 ship backend.                                                  |
| `Capture/AppleGestureMapper.cs`  | AppKit → frame mapping.                                           |
| `Capture/InjectGestureSource.cs` | Tier A inject + E2E harness queue.                                |
| `Capture/GestureCaptureLog.cs`   | Dev capture trace (R3.1 may replace — not architecture-blocking). |
| `Capture/GameModifierKeys.cs`    | Modifier merge for Option-orbit on AppKit path.                   |

### P0 — Remove CAD compile module (R2.2)

**Delete:**

| File                            | Reason                                      |
| ------------------------------- | ------------------------------------------- |
| `rewrite/mod/Policy/CadSeed.cs` | CAD is v2-only; no v1 style table consumer. |

**Trim:**

| File                                                         | Change                                                                               |
| ------------------------------------------------------------ | ------------------------------------------------------------------------------------ |
| `rewrite/mod/Settings/ModSettings.cs`                        | Remove `GesturePreset.CAD` usage in `ApplyGesturePreset`; Maps+ / Custom only on v1. |
| `rewrite/mod/Policy/TrackpadGesture.cs`                      | Remove `#if ENABLE_CAD_GESTURE_STYLE` catalog branches.                              |
| `rewrite/mod/Ui/OptionsSettingsUi.cs`, `TuningPanelHost*.cs` | Remove CAD preset `#if` UI.                                                          |
| `rewrite/mod/TrackpadCameraControl.Rewrite.csproj`           | Remove `EnableCadGestureStyle` / `ENABLE_CAD_GESTURE_STYLE`.                         |
| `rewrite/mod/Settings/FeatureFlags.cs`                       | Remove `EnableCadGestureStyle` block.                                                |

Document CAD as a future gesture preset in feature shards only — no seed type in v1 code.

### P1 — Keep E2E inject; simplify host wiring

**Recommendation: KEEP** `InjectGestureSource`, `E2eInjectFileProtocol`, and env/flag gating in `Mod.IsE2eInjectEnabled`.

| Rationale                  | Detail                                                                                                                                            |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| Legitimate test seam (L10) | Tier A tests inject frames directly (`MapsPlusGoldenFixturesTests`, `InputGatesTests`). In-game smoke uses file protocol without IPC or Contacts. |
| Not player surface         | Active only when `TRACKPAD_E2E_INJECT=1` or `e2e-inject.flag` exists — never in normal play.                                                      |
| Distinct from F1/F3        | In-process queue, not Unix socket bridge; does not justify keeping Contacts or `IpcGestureSource`.                                                |

**Simplify (same commit group or follow-up):**

| File                      | Change                                                                                             |
| ------------------------- | -------------------------------------------------------------------------------------------------- |
| `Host/ModRuntime.cs`      | Drop redundant `Inject` property; tests use `Pipeline.Source as InjectGestureSource`.              |
| `Host/GesturePipeline.cs` | Move inject arming to `Mod.OnEnabled` only; remove `EnsureInjectSourceIfArmed` from tick hot path. |
| `Host/Mod.cs`             | Collapse inject detection to enable-time source selection; remove `InjectSource` shim.             |

Do **not** delete `E2eInjectFileProtocol` unless tier C in-game smoke is retired — it is the only file-based harness seam left after IPC removal.

### P2 — Collapse alias and settings paths (R2.3 / R4.1)

| File                       | Change                                                                                                                                    |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `Host/Mod.cs`              | Single settings access via `Mod.Runtime.Settings`; remove `_settingsCache` fallback where possible.                                       |
| `Settings/FeatureFlags.cs` | Delete file or reduce to MSBuild doc comment in csproj — call sites must use `#if` only (L9).                                             |
| `Policy/` + settings       | Consolidate `OrbitTrigger` enum vs style table orbit rows (F5) — tracked in settings/policy audit; architecture expects one resolve path. |

### P3 — Logging (R3.1, cross-cutting)

`GestureCaptureLog` is a bespoke file logger (F2). Not a plane-boundary violation, but Host and Capture call it from enable and frame paths. Replace with agreed standard logging in R3.1; architecture unchanged.

---

## File disposition summary

```text
DELETE (v1 cleanup)
  Capture/IpcGestureSource.cs
  Capture/InProcessGestureSource.cs
  Capture/DragLowPass.cs
  Capture/CaptureBackendFlags.cs
  Policy/CadSeed.cs
  Settings/LegacyModSettings.cs          (R2.3)
  Settings/FeatureFlags.cs               (optional — or strip to csproj comments)

KEEP (ship + test seams)
  Capture/IGestureSource.cs
  Capture/GestureFrame.cs
  Capture/AppleGestureSource.cs
  Capture/AppleGestureMapper.cs
  Capture/InjectGestureSource.cs
  Capture/GameModifierKeys.cs
  Capture/GestureCaptureLog.cs           (until R3.1)
  Host/E2eInjectFileProtocol.cs
  Host/GesturePipeline.cs                (trimmed)
  Host/ModRuntime.cs                     (trimmed)
  Host/Mod.cs                            (trimmed)
  Host/GestureThreading.cs
  Host/Patcher.cs
  Host/LoadingExtension.cs
  Policy/* (except CadSeed.cs)
  Apply/*
```

---

## Recommended atomic commit group mapping (R2.x)

Maps to [v1 audit and cleanup plan](./v1-audit-plan.md) phase R2. Architecture-owned files per group:

| Group    | Concern                                 | Architecture files (primary)                                                                                                                                                                                                              | Commit subject                                                   |
| -------- | --------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| **R2.1** | Remove IPC + Contacts capture           | Delete: `IpcGestureSource`, `InProcessGestureSource`, `DragLowPass`, `CaptureBackendFlags`. Trim: `Mod.cs`, `GesturePipeline.cs`, `ModSettings.CaptureBackend`, UI backend blocks, csproj Contacts gates. **Keep** inject + E2E protocol. | `refactor(rewrite): remove IPC and Contacts capture from v1`     |
| **R2.2** | Remove CAD compile module               | Delete: `CadSeed.cs`. Trim: `ModSettings.ApplyGesturePreset`, `TrackpadGesture.cs`, UI `#if ENABLE_CAD_GESTURE_STYLE`, csproj CAD define, `FeatureFlags` CAD block.                                                                       | `refactor(rewrite): drop CAD gesture module from v1 DLL`         |
| **R2.3** | v1 settings schema (architecture touch) | Remove settings alias hops that fed dead capture/CAD paths; delete `LegacyModSettings.cs`. Collapse `Mod.EnsureSettingsInternal` duplication where R2.1/R2.2 expose it.                                                                   | `refactor(rewrite): v1 settings schema without legacy migration` |

Follow-on groups (not R2, but architecture-adjacent):

| Group    | Concern                        | Architecture note                                                                                                                  |
| -------- | ------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------- |
| **R3.1** | Logging standardization        | Replace `GestureCaptureLog` call sites in `Mod`, `AppleGestureSource`, `GesturePipeline`.                                          |
| **R4.1** | Modifier / orbit consolidation | Single modifier resolve; style table subsumes `OrbitTrigger` where parity allows.                                                  |
| **R6.2** | SA + doc sync                  | Update Semgrep paths after deletions; align platform-backends and [feature flags](../feature-flags.md) shards with AppKit-only v1. |

**Gate after R2:** `dotnet test TrackpadCameraControl.sln`, `npm run sa:rewrite`, `npm run docs:rewrite`.

---

## Acceptance (architecture slice)

When R2 architecture work is complete, the default rewrite DLL build has:

- One capture backend on disk: AppKit (`AppleGestureSource` + mapper).
- No IPC, Contacts, `CaptureBackend*`, or `DragLowPass` types in `rewrite/mod`.
- No CAD seed or `ENABLE_CAD_GESTURE_STYLE` code paths.
- Inject + `E2eInjectFileProtocol` retained as env-gated maintainer seams only.
- `GesturePipeline.Tick` without backend hot-swap or Contacts `#if` branches.
- Harmony limited to scroll suppress, orbit flush, and parity key-label hooks.

---

## Related

- [Organized product feedback](./v1-product-feedback.md) — F1, F3, F4, inject keep evaluation
- [v1 audit and cleanup plan](./v1-audit-plan.md) — phased R0–R7
- Features guide: system architecture shard — target planes and tick contract
- Features guide: greenfield redesign lessons — L6, L8, L9
- [Harnesses and testing](../harnesses-and-testing.md) — tier A inject vs tier B capture
