# Options polish, New Preset, selection rotate — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development. One subagent per atomic group. Docs-first (MDCP), then TDD/code.

**Goal:** Clamp pan to city bounds; sync + autosave Options ↔ debug popup; full title-bar drag; redesign Options (TM:PE section rhythm, Sensitivity sliders, preset dropdown with **New Preset** dirty autosave); rename Assist → Debug; remove Enable/Reverse from UI; pitch 7–90; selection-aware two-finger rotate (object) and ⌥-orbit around selection.

**Architecture:** Shared `ModSettings` + store remain the single source of truth; UI surfaces notify each other on change. Feel identity tracks active preset name (`New Preset` scratch). Gesture pipeline branches yaw vs object-rotate and orbit pivot when a selection exists. Pan apply clamps target to city bounds.

**Tech Stack:** C# CS1 mod, ColossalUI, xUnit, Harmony as needed for selection/tool APIs.

**Worktree:** `/Users/saul/Repos/personal/city-skylines-mod-trackpad/.worktrees/feat-in-game-tuning-panel`  
**Branch:** `feat/in-game-tuning-panel`  
**LOOKUP:** `docs/developer/work-item-tracking.md`  
**Design notes:** Approved playtest pass (2026-08-30) + selection rotate; update durable shards then code.

## Global Constraints

- Canonical UI: **Sensitivity** sliders (0.1×–2× factory default, step ≈ 10% of default).
- Pitch min **7**, pitch max **90**, pitch always > 0; **no yaw angle clamp**.
- Built-ins Slow/Default/Fast never overwritten; dirty edits → **New Preset** autosave.
- Product UI: no Enable-per-op, no Reverse; Debug (not Assist) for floating panel.
- Options title: mod name + version.
- Section order: General → Zoom → Pan → Rotate → Orbit; HR then section title then rows (indented).
- Compile-time `ENABLE_*` flags unchanged.
- One concern per commit; `dotnet test` / `npm run docs:check` as applicable; no push; leave `package-lock.json` alone.
- No implementation samples in durable MDCP shards.

---

## Atomic commit groups

### D1 — Client + glossary (docs)

**Subject:** `docs: debug panel, New Preset, and selection rotate for clients`

**Files:** `docs/client/options-and-hot-tuning.md`, `docs/client/feel-presets.md`, `docs/client/assist-ui-camera-chrome.md` (rename/retarget to debug), `docs/client/index.md`, `docs/glossary/assist-ui.md` → debug-ui or rewrite, `docs/glossary/feel-preset.md`, `docs/glossary/index.md`, pan/orbit/yaw activation notes as needed.

### D2 — Feature contracts (docs)

**Subject:** `docs: settings sync, pan bounds, and selection-aware gestures`

**Files:** `docs/features/settings-and-hot-configuration.md`, `docs/features/trackpad-camera.md`, `docs/features/assist-ui-camera-chrome.md`, `docs/features/index.md`, new feature shard for selection rotate if mitosis warrants.

### D3 — Developer schema + work-item (docs)

**Subject:** `docs: schema New Preset pitch defaults and work-item phase`

**Files:** `docs/developer/settings-schema.md`, `docs/developer/work-item-tracking.md`

### D4 — Docs validate

**Subject:** `docs: check Options polish shards` (only if fixes needed)

**Files:** as required by `npm run docs:check`

### C1 — Pan bounds + pitch factory

**Subject:** `fix: clamp pan to city bounds and set pitch 7-90`

**Files:** `mod/CameraApplicator.cs`, `mod/ModSettings.cs`, `mod/ICameraController.cs` / camera zoom helper as needed, tests.

### C2 — New Preset dirty autosave + UI sync notify

**Subject:** `feat: New Preset dirty autosave and Options-debug settings sync`

**Files:** `mod/FeelProfiles.cs`, `mod/ModSettingsStore.cs`, `mod/ModOptions.cs`, `mod/ModSettings.cs` (active preset name), tests.

### C3 — Options UI redesign

**Subject:** `feat: Options layout sliders preset dropdown and Debug rename`

**Files:** `mod/OptionsSettingsUi.cs`, `mod/ModOptions.cs`, `mod/Mod.cs` (title/version if needed).

### C4 — Debug popup title-bar drag + slim mirror

**Subject:** `feat: debug panel full title-bar drag and shared slim controls`

**Files:** `mod/TuningPanelHost.cs` (rename conceptually to Debug), visibility setting rename if feasible without huge churn (`AssistUiEnabled` → keep field name in schema with Debug UI label OK).

### C5 — Selection-aware rotate / ⌥-orbit

**Subject:** `feat: rotate selected object and Option-orbit around selection`

**Files:** gesture pipeline / applicator / new selection helper, Harmony if required, tests with fakes.

### C6 — Changeset

**Subject:** `chore: changeset for Options polish and selection rotate`

**Files:** `.changeset/*.md`

---

## Execution order

1. D1 ∥ D2 ∥ D3 (parallel subagents) → D4  
2. C1 → C2 → C3 ∥ C4 (C4 after C2 for sync API) → C5 → C6  

Mark groups Done in this file’s status table when committing (optional; keep git log authoritative).
