# v1 audit and cleanup plan (rewrite mod)

**Status:** Closed. R0–R6 ran against a source clone. That experiment failed the greenfield intent (features ADR 0005). **Do not run R7 as the success path** for the cloned DLL.

Further implementation follows the recovery design _Rewrite from UX contract, not source clone_ (repo-root session spec) after maintainer approval: quarantine clone sources, lock the UX inventory, oracle tests, then replace the assembly.

The phases below are a record of what was attempted.

Phased review and implementation after [organized product feedback](./v1-product-feedback.md). Each phase ended with commit(s) on `cursor/rewrite-parity-cc6b`.

## Phase R0 — Plan and feedback shards (docs)

**Goal:** MDCP shards for feedback, plan, and review guide index.

| Commit | Concern                        | Files                                                                | Subject                                                   |
| ------ | ------------------------------ | -------------------------------------------------------------------- | --------------------------------------------------------- |
| R0.1   | Review guide + feedback + plan | `rewrite/docs/developer/review/*`, `rewrite/docs/developer/index.md` | `docs(rewrite): add v1 audit plan and organized feedback` |

**Gate:** `npm run docs:rewrite`

## Phase R1 — Specialist area audits (docs only)

**Goal:** Parallel specialist passes produce one shard per area: strengths, weaknesses, critical improvements, proposed file deletions.

| Sub-agent            | Shard output                                     | Scope                                                                |
| -------------------- | ------------------------------------------------ | -------------------------------------------------------------------- |
| Architecture         | [architecture-audit.md](./architecture-audit.md) | Three planes, Harmony, dead factories, inject/E2E                    |
| Capture              | [capture-audit.md](./capture-audit.md)           | IPC, Contacts, AppKit, logging, modifier keys                        |
| Settings / schema    | [settings-audit.md](./settings-audit.md)         | Legacy migration, aliases, CaptureBackend, OrbitTrigger, style table |
| UI / product surface | [ui-audit.md](./ui-audit.md)                     | Options/Debug 1:1, gated chrome residue, version display             |
| Release / versioning | [release-audit.md](./release-audit.md)           | Dev vs release semver, BuildInfo, changesets alignment               |
| Tests / SA           | [tests-sa-audit.md](./tests-sa-audit.md)         | Tier A/B gaps after deletions, Semgrep rule updates                  |

| Commit | Concern                 | Files                                      | Subject                                     |
| ------ | ----------------------- | ------------------------------------------ | ------------------------------------------- |
| R1.1   | Specialist audit shards | `rewrite/docs/developer/review/*-audit.md` | `docs(rewrite): specialist v1 audit shards` |

**Gate:** `npm run docs:rewrite`

## Phase R2 — Strip prototype surface (code)

**Goal:** Delete F1, F3, F4 code from `rewrite/mod`; remove compile-gated stubs that should not exist in v1 tree.

| Commit | Concern                       | Files (indicative)                                                                                                                     | Subject                                                          |
| ------ | ----------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| R2.1   | Remove IPC + Contacts capture | Delete `IpcGestureSource`, `InProcessGestureSource`, `DragLowPass`, `CaptureBackend*`, csproj links; trim `GesturePipeline`, `Mod`, UI | `refactor(rewrite): remove IPC and Contacts capture from v1`     |
| R2.2   | Remove CAD compile module     | Delete `CadSeed.cs`, CAD branches in `ModSettings`, `TrackpadGesture`, UI `#if` blocks; doc CAD as v2 preset only                      | `refactor(rewrite): drop CAD gesture module from v1 DLL`         |
| R2.3   | Remove legacy settings types  | Delete `LegacyModSettings.cs`; collapse `ModSettingsStore` to v1 schema; remove XML alias properties                                   | `refactor(rewrite): v1 settings schema without legacy migration` |

**Gates:** `dotnet test TrackpadCameraControl.sln`, `npm run sa:rewrite`

## Phase R3 — Logging standardization (code + docs)

**Goal:** Replace bespoke `GestureCaptureLog` with agreed practice (likely `UnityEngine.Debug.Log` when `HAS_CITIES`, no-op or test sink otherwise); optional env-gated capture trace as thin wrapper if still needed for tier B.

| Commit | Concern                               | Files                                                 | Subject                                            |
| ------ | ------------------------------------- | ----------------------------------------------------- | -------------------------------------------------- |
| R3.1   | Logging adapter + call-site migration | New thin logger, delete or shrink `GestureCaptureLog` | `refactor(rewrite): adopt standard logging for v1` |
| R3.2   | Developer shard for logging contract  | `rewrite/docs/developer/logging.md`                   | `docs(rewrite): document v1 logging contract`      |

**Gates:** tests + `sa:rewrite`

## Phase R4 — Modifier keys and policy cleanup (code)

**Goal:** Consolidate modifier resolution (F5); remove dead `OrbitTrigger` if style table subsumes orbit chords; ensure L1 tick consumers for all persisted fields.

| Commit | Concern                                | Files                     | Subject                                                            |
| ------ | -------------------------------------- | ------------------------- | ------------------------------------------------------------------ |
| R4.1   | Modifier / orbit trigger consolidation | Policy + settings + tests | `refactor(rewrite): consolidate modifier and orbit resolve for v1` |

**Gates:** tests + golden fixtures unchanged behavior

## Phase R5 — Version and release alignment (code + docs)

**Goal:** F7 — Options title = semver only on release builds; Debug panel retains assembly identity + UTC for dev; document rewrite-specific release checklist.

| Commit | Concern                | Files                                       | Subject                                                                 |
| ------ | ---------------------- | ------------------------------------------- | ----------------------------------------------------------------------- |
| R5.1   | Version display policy | `Mod.cs`, UI footer, tests                  | `fix(rewrite): separate dev build identity from release semver display` |
| R5.2   | Release process shard  | `rewrite/docs/developer/release-process.md` | `docs(rewrite): dev vs release version display`                         |

**Gates:** `ModBuildInfoTests` equivalent for rewrite

## Phase R6 — Hooks, SA, docs sync

**Goal:** Extend lint-staged to `rewrite/mod/**/*.cs`; update Semgrep dead-code rules; refresh feature-flags / settings-schema / platform-backends shards to match v1 reality.

| Commit | Concern                                  | Files                                                                      | Subject                                                   |
| ------ | ---------------------------------------- | -------------------------------------------------------------------------- | --------------------------------------------------------- |
| R6.1   | lint-staged + format scripts for rewrite | `package.json`, `scripts/*`                                                | `chore: include rewrite C# in pre-commit formatting`      |
| R6.2   | SA rules + doc alignment                 | `rewrite/scripts/*`, `rewrite/docs/features/*`, `rewrite/docs/developer/*` | `docs(rewrite): align shards and SA with v1 ship surface` |

**Gates:** `npm run hooks:pre-commit`, `npm run hooks:pre-push`, full test suite

## Phase R7 — In-game tier C (human) — not the clone success path

**Do not** treat in-game A/B of the **cloned** rewrite DLL as proof the architecture succeeded. Tier C remains valid later for a rebuild that matches [UI parity](../../glossary/ui-parity.md) without source identity.

Checklist (visual/interaction only): [in-game parity checklist](../in-game-parity-checklist.md).

## Hook discipline (every commit)

```bash
# staged format (automatic via husky)
git commit -m "type(scope): subject"

# before push
npm run hooks:pre-push   # docs + (on main) format:check
```

Emergency skip only when fixing hook infrastructure: `HUSKY=0` — prefer fixing the failure.

## PR tracking

Draft PR #46 captured the clone experiment. Recovery work is a separate branch/spec; do not mark the clone PR ready as a greenfield rewrite.
