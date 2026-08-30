# AppleKit Maps+ feel surface — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the approved AppleKit Maps+ feel surface (flags, factory feel, Sensitivity numerics, pitch limits, feel presets, scroll/UI gates, slim Options/Assist UI) against durable MDCP shards.

**Architecture:** `FeatureFlags` gates product UI and Contacts/LP/chrome. Factory Default feel lives in `ModSettings`. Feel profiles (Slow/Default/Fast + named Save/Load) mutate the same settings blob. AppKit precise-scroll vs wheel splits pan vs vanilla zoom. `InputGates` skip mod camera when menus are open or the pointer is over popups. Orbit pitch clamps to settings min/max.

**Tech Stack:** C# / Cities: Skylines I mod, xUnit tests, Harmony vanilla suppress, ColossalUI Options + Assist panel.

**Spec:** `docs/superpowers/specs/2026-08-29-applekit-feel-surface-design.md`  
**Worktree:** `/Users/saul/Repos/personal/city-skylines-mod-trackpad/.worktrees/feat-in-game-tuning-panel`  
**Branch:** `feat/in-game-tuning-panel`

## Global Constraints

- Canonical UI term: **Sensitivity** (not Drag / drag scale).
- Sensitivity values **> 0**; all product floats round to **two decimal places**.
- Feature flags default **off**: `EnableCadGestureStyle`, `EnableContactsCapture`, `EnableAssistChrome`.
- Factory Default: InvertPanX on; Pan 0.50/0.50; Zoom 1.00; Yaw 2.00; Orbit 10.00/10.00; Pitch min/max −80/80.
- Slow = Default sensitivities × 0.75; Fast × 1.25; reverse + pitch limits unchanged.
- TDD: failing tests first where the repo already tests that area; `dotnet test` green before commit.
- One concern per commit; conventional subjects; no push; no amend of others’ commits.
- Do not edit durable MDCP shards unless a contract bug blocks code (prefer code matches docs).
- Leave `package-lock.json` unstaged unless required.

---

## File map

| Unit | Files |
| --- | --- |
| FeatureFlags | `mod/FeatureFlags.cs` (new) |
| Settings / numeric | `mod/ModSettings.cs`, `mod/ModOptions.cs`, tests |
| Pitch clamp | `mod/CameraApplicator.cs`, tests |
| Feel profiles | `mod/FeelProfiles.cs` (new), `mod/ModSettingsStore.cs`, `mod/ModOptions.cs`, tests |
| Scroll split | `mod/AppleGestureMapper.cs`, `mod/AppleGestureSource.cs`, `mod/VanillaCameraSuppress.cs`, `mod/Patcher.cs` as needed, tests |
| Input gates | `mod/InputGates.cs` (new), `mod/GesturePipeline.cs`, tests |
| LP gate | `mod/DragLowPass.cs`, `mod/Mod.cs` / capture source selection |
| UI | `mod/OptionsSettingsUi.cs`, `mod/TuningPanelHost.cs`, `mod/ModOptions.cs` strings |

---

## Task C1: FeatureFlags + factory Default + numeric policy + pitch fields + applicator clamp

**Commit:** `feat: flags, factory feel defaults, pitch clamp, and Sensitivity rounding`

- [ ] Add `mod/FeatureFlags.cs` with three `public const bool` (or static readonly) defaults **false**, names exact: `EnableCadGestureStyle`, `EnableContactsCapture`, `EnableAssistChrome`.
- [ ] Update `ModSettings` property defaults / `CreateFactoryDefaults` / `CopyFrom` for playtest Default + `OrbitPitchMin`/`OrbitPitchMax` (−80/80) + `InvertPanX = true`.
- [ ] Update `ModOptions`: round to 2 decimals; Sensitivity apply rejects/ignores ≤ 0; remove or raise old ScaleMax ceiling for product Sensitivity (allow any > 0).
- [ ] `CameraApplicator` orbit: after pitch write, clamp `AngleY` to `[OrbitPitchMin, OrbitPitchMax]` (swap if min>max defensively).
- [ ] Tests: factory defaults; Round2; Sensitivity ≤0 rejected; pitch clamp; flags off.
- [ ] `dotnet test` → commit.

## Task C2: Feel profiles (Slow / Default / Fast + Save as… / Load)

**Commit:** `feat: Slow Default Fast and named feel profile save load`

- [ ] Add `FeelProfiles` (or methods on ModOptions): ApplyDefault / ApplySlow / ApplyFast (multiply Default sensitivity fields by 0.75/1.25 from **factory Default table**, not from current dirty values — or: Slow/Fast relative to factory Default constants; Reset = factory Default). Spec: Slow/Fast multiply Default’s sensitivity fields; use factory Default as base when applying built-ins.
- [ ] Named Save as… / Load via `ModSettingsStore` `userPresets` envelope (name + full feel snapshot).
- [ ] Wire `ModOptions` apply helpers used by UI later.
- [ ] Tests for Slow/Fast rounding, Save/Load round-trip, Reset.
- [ ] `dotnet test` → commit.

## Task C3: Scroll device split + input gates + suppress policy

**Commit:** `feat: precise trackpad pan vs mouse wheel and UI input gates`

- [ ] Thread `hasPreciseScrollingDeltas` from AppKit event through mapper; non-precise scroll → do not emit pan frame (or mark non-precise and drop in pipeline).
- [ ] `VanillaCameraSuppress.ShouldSkipScrollWheel`: skip only when precise trackpad world pan path is active; allow wheel; allow scroll when menu open or over UI.
- [ ] `InputGates`: menu/Options open → no mod camera; pointer over UI/popup → no mod camera (`IgnoreOverUi`); require game focus as today.
- [ ] `GesturePipeline.Tick` consults gates before apply; reset low-pass on skip if needed.
- [ ] Tests for suppress policy + gate helpers (fakeable); mapper precise vs not.
- [ ] `dotnet test` → commit.

## Task C4: Product-surface UI (Options + Assist panel)

**Commit:** `feat: slim Sensitivity UI with feel presets and pitch limits`

- [ ] Gate CAD switcher, capture backend, Btn fields, LP on `FeatureFlags`.
- [ ] Labels: **Sensitivity**; per-op short meaning + activation (`⌥` for orbit).
- [ ] Feel preset row: Slow | Default | Fast | Save as… | Load | Reset.
- [ ] Pitch min/max fields; multi-column Options like panel where feasible.
- [ ] Descriptions without “modifier+”; use Option/`⌥`.
- [ ] Smoke: project builds; tests still green (UI may be lightly tested).
- [ ] Commit.

## Task C5: Contacts/LP gating + capture backend force + changeset

**Commit:** `feat: gate low-pass and Contacts behind EnableContactsCapture`

- [ ] `DragLowPass.Filter` no-ops unless `FeatureFlags.EnableContactsCapture` (and settings enables).
- [ ] When flag off, force AppleGestures capture path (ignore UI backend / treat Contacts as unavailable on product surface; env override may remain for maintainers — prefer: flag off ⇒ AppleKit source).
- [ ] Changeset under `.changeset/` describing player-facing feel surface.
- [ ] Full `dotnet test` + note local install script for human.
- [ ] Commit.

---

## Execution notes for subagents

- Cwd: worktree path above.
- Read design + `docs/developer/settings-schema.md` + `docs/developer/feature-flags.md` before coding.
- Do not start the next task’s files until this task is committed (avoid ModSettings merge conflicts across parallel agents).
- After each task: self-review against Global Constraints; return commit hash + test result.
