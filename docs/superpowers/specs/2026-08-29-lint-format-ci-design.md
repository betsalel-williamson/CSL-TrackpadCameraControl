# Lint / Format / CI — Design

**Date:** 2026-08-29  
**Status:** Approved  
**Scope:** Phase 1 contributor tooling (no Cities: Skylines game assembly build yet)

## Goal

Contributors get one consistent style path for **C# mod code**, **native C**, and **docs**, enforced in CI — without needing Cities: Skylines assemblies to build.

## Decisions

| Concern | Choice | Rationale |
| --- | --- | --- |
| Scope | Config + CI + minimal `mod/` / `native/mac/` scaffolds | Real paths for format checks; no game DLL compile |
| C# style | CSharpier + UDK-style `.editorconfig` (LF, not CRLF) | Contributor consistency above typical CS1 repos |
| Native C | clang-format (LLVM base, indent 4) | Matches EditorConfig indent |
| Docs | Prettier + markdownlint via MDCP presets; `docs:check --require-lint` | Aligns with existing MDCP tooling |
| CI | GitHub Actions: docs job + code-format job | mdcp-inspired verify; no assembly download |

## Local surface

- `.editorconfig` — shared baseline
- CSharpier (local .NET tool) — `mod/**/*.cs`
- clang-format — `native/**/*.c`
- `npm run format` / `format:check` — orchestrate docs + csharp + native
- `npm run docs` — compile + check with lint required

## Scaffolds

- `mod/` — SDK-style library (`netstandard2.0`), no game refs; seeded from `templates/mod/`
- `native/mac/` — `TrackpadBridge.c` seeded from template; win/linux stay stub READMEs

## Out of scope

- Vale / prose lint
- Pre-commit hooks
- CSM-style game-assembly download / `dotnet build` against Cities DLLs
- Real CitiesHarmony package references

## References

- UrbanDevKit `.editorconfig` / code-style wiki (baseline)
- [mdcp CI](https://github.com/betsalel-williamson/mdcp/blob/main/.github/workflows/ci.yml) (docs verify pattern)
- Joystick Camera Control (mod project shape only — not CI)
