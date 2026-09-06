# Organized product feedback (rewrite v1)

Feedback from the original code review and greenfield redesign, applied to the **built** `rewrite/mod` tree. v1 treats internal prototypes as complete; the rewrite must not ship prototype seams.

## Theme: prototype carryover (violates L6, L9)

The rewrite copied shipping structure including compile-gated stubs. Greenfield intent: **omit** unfinished modules from the DLL entirely — not `#if` blocks, env overrides, or schema rows for dead backends.

| ID  | Concern                       | User intent                                                                           | Evidence in rewrite today                                                                                                                                                                   | Greenfield lesson                              |
| --- | ----------------------------- | ------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| F1  | **IPC pipeline**              | Remove — inter-process capture bridge is obsolete                                     | `Capture/IpcGestureSource.cs` (Compile Remove when Contacts off, but file remains); docs still mention bridge paths                                                                         | L6 — delete useless redirection                |
| F2  | **Logging**                   | Adopt default / best-practice logging, not a bespoke capture file logger              | `Capture/GestureCaptureLog.cs` used from Mod, Pipeline, AppleGestureSource, Options                                                                                                         | L6 — every type earns a seam or goes           |
| F3  | **Contacts subsystem**        | Remove entirely from rewrite — not used in v1                                         | `InProcessGestureSource`, `CaptureBackendFlags`, `DragLowPass`, `CaptureBackend` on settings, UI `#if ENABLE_CONTACTS_CAPTURE`, csproj links `src/TrackpadCapture`                          | L9 — compile-time modules omitted, not stubbed |
| F4  | **CAD gesture style**         | Future gesture **preset** (v2, user feedback); not enabled in v1                      | `Policy/CadSeed.cs`, `GesturePreset.CAD`, `ApplyGesturePreset` CAD branch, `#if ENABLE_CAD_GESTURE_STYLE` UI                                                                                | L9 — seeds in docs only until module ships     |
| F5  | **Modifier keys**             | Review for duplication / false knobs vs Maps+ parity                                  | `GameModifierKeys`, per-op `GestureModifierKey`, style table modifiers, legacy orbit trigger enum                                                                                           | L1, L5 — tick consumers only                   |
| F6  | **Legacy settings schemas**   | v1 greenfield — drop early prototype migration paths                                  | `LegacyModSettings.cs`, schema 1–8 migration in `ModSettingsStore`, XML alias properties on `ModSettings`, `OrbitTrigger` enum                                                              | L6, L12 — one write path, doc/code alignment   |
| F7  | **Version / release process** | Dev builds expose build identity; **releases** expose semver only (major.minor.patch) | `BuildInfo`, `GetAssemblyIdentityDisplay`, Debug footer shows UTC timestamp; process partially documented in root `docs/developer/commits-and-releases.md` but rewrite needs explicit shard | L12 — contract clarity                         |

## Theme: keep (parity constraints)

These must **not** regress while stripping prototypes:

| Keep                                                | Why                                      |
| --------------------------------------------------- | ---------------------------------------- |
| Three-plane architecture (Capture → Policy → Apply) | L8 — greenfield core                     |
| Style binding table + Maps+ seed resolve            | L1, L11 — parity vs hardcoded heuristics |
| Feel profiles / hot tuning UI 1:1 with shipping     | L2, L11                                  |
| Harmony scope (scroll suppress, orbit flush)        | L8                                       |
| Tier A/B tests + `sa:rewrite` gates                 | L10                                      |
| E2E inject for automated harness (maintainer-only)  | Test seam — evaluate vs F1/F3 scope      |

## Theme: process expectations

| Expectation               | Implementation                                                                                     |
| ------------------------- | -------------------------------------------------------------------------------------------------- |
| Specialist review by area | Parallel sub-agents → MDCP shards under this guide                                                 |
| Atomic commit groups      | One concern per commit; numbered groups in [v1 audit plan](./v1-audit-plan.md)                     |
| Hooks enabled             | `npm run hooks:pre-commit` (lint-staged), `hooks:pre-push` (docs gates) on every push              |
| Incremental cleanup       | Strip → fix → test → docs per phase; update PR after each phase                                    |
| Tier C in-game A/B        | Separate gate after automated cleanup ([in-game parity checklist](../in-game-parity-checklist.md)) |

## Acceptance (v1 ship surface)

When cleanup is complete, the default rewrite DLL build (`Enable*` all false) contains:

- AppKit in-process capture only
- Maps+ style table resolve only (no CAD code paths)
- No Contacts / IPC / legacy migration types on disk in `rewrite/mod`
- Logging via agreed standard (not orphan file logger unless explicitly dev-gated)
- Settings schema v1 with no XML alias hops for retired prototype fields
- Version display: product semver in Options title; build stamp only on Debug / dev builds per release shard
