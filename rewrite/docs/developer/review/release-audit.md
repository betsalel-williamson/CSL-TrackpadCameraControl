# Release / versioning audit (rewrite v1)

Specialist pass for dev vs release version policy, MSBuild `BuildInfo`, and alignment with root Changesets workflow. Scope: `rewrite/mod/Host/Mod.cs`, `TrackpadCameraControl.Rewrite.csproj`, root `docs/developer/commits-and-releases.md`, shipping `ModBuildInfoTests` parity.

**Related:** [organized product feedback](./v1-product-feedback.md) (F7), [v1 audit plan](./v1-audit-plan.md) phase R5.

## Strengths

- **Single product semver source.** `TrackpadCameraControl.Rewrite.csproj` reads `package.json` `"version"` into `Version`, `FileVersion`, `InformationalVersion`, and generated `BuildInfo.ProductVersion` — same model as shipping and root [commits and releases](../../../../docs/developer/commits-and-releases.md).
- **Assembly identity separated from product version.** `AssemblyVersion` is `Major.Minor.*` with `Deterministic=false`, so each compile gets a new build/revision for Cities auto-reload; `GetAssemblyIdentityDisplay()` reads `typeof(Mod).Assembly.GetName().Version` for the Debug title bar.
- **Display APIs are split by surface.** `OptionsTitle` / `IUserMod.Name` → product semver; `DebugPanelTitle` → assembly identity; footer → UTC timestamp in clipboard, local time in panel — matches root [mod reload during development](../../../../docs/developer/mod-reload-during-development.md) table.
- **Semgrep bans the old alias.** `rewrite.legacy-alias-method` flags `GetAssemblyVersionDisplay()` forwarding to product version; rewrite `Mod.cs` already omits that hop (shipping mod still has it).
- **Build stamp is MSBuild-generated.** `GenerateBuildInfo` target writes `BuildInfo.g.cs` with `ProductVersion` and `BuildTimestampUtc` before compile — no hand-edited version constants in source.

## Weaknesses

- **No rewrite `ModBuildInfoTests`.** Shipping has six tests in `tests/TrackpadCameraControl.Tests/ModBuildInfoTests.cs`; rewrite test project has **zero** coverage of `GetProductVersionDisplay`, assembly identity, or footer formats — regression risk on R5 edits.
- **F7 policy not fully implemented.** Product feedback asks: dev builds expose build identity; **releases** expose semver only. Today both dev and release builds show assembly identity on the Debug title and UTC/local build stamps in the footer; there is no `Release` configuration or MSBuild property gating “dev-only” chrome.
- **Options title always appends full package semver.** Correct for storefront copy, but dev workflows that bump only assembly build/revision still show stable `1.0.0` on Options — contributors must know to look at Debug title, not Options, for “did my build load?”
- **Rewrite-specific release shard missing.** Root `commits-and-releases.md` documents the monorepo workflow; there is no `rewrite/docs/developer/release-process.md` for rewrite folder name, deploy path (`TrackpadCameraControl.Rewrite`), or beta checklist — called out in plan R5.2.
- **Duplicate XML summary on `GetAssemblyBuildTimestampUtcDisplay`.** Stale “Legacy alias” comment from removed `GetAssemblyVersionDisplay` — undermines doc trust.
- **`InternalsVisibleTo` exposes build helpers to tests but tests do not use them yet.** Test seam exists; coverage does not.

## Critical improvements

1. **Port `ModBuildInfoTests` to `rewrite/tests`** (namespace `TrackpadCameraControl.Rewrite`, title prefix `Trackpad Camera Control Rewrite (macOS)`). Gate R5.1 on green tests.
2. **Define release build contract explicitly** in a new `rewrite/docs/developer/release-process.md` shard:
   - Options / Content Manager: `package.json` semver only (no assembly wildcard on InformationalVersion).
   - Debug panel: assembly identity + build timestamp for **dev** builds; document whether release builds hide assembly identity or retain footer for support paste.
3. **Optional MSBuild switch** (e.g. `-p:DevBuildIdentity=true` default locally, false in release CI) if product requires semver-only chrome on release DLLs — implement only after F7 wording is confirmed with tier C.
4. **Fix `Mod.cs` XML docs** — remove orphan summary; document `GetProductVersionDisplay` vs `GetAssemblyIdentityDisplay` in one place.
5. **Wire rewrite into Changesets narrative** — confirm rewrite DLL version bumps ride the same `npm run version-packages` flow as shipping (no separate `rewrite/package.json`).

## Commit mapping

| Plan commit | Concern                                                  | Primary files                                                                            |
| ----------- | -------------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| **R5.1**    | Version display policy + `ModBuildInfoTests` for rewrite | `rewrite/mod/Host/Mod.cs`, `rewrite/tests/.../ModBuildInfoTests.cs`                      |
| **R5.2**    | Rewrite release process shard                            | `rewrite/docs/developer/release-process.md`, link from `rewrite/docs/developer/index.md` |
| **R1.1**    | This shard                                               | `rewrite/docs/developer/review/release-audit.md`                                         |
| **R6.2**    | Doc alignment with root release docs                     | Cross-link from `release-process.md` to `docs/developer/commits-and-releases.md`         |
