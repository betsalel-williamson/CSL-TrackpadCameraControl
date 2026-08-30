# Work item tracking

## Where scope lives

| Kind                   | Location                                            |
| ---------------------- | --------------------------------------------------- |
| Durable contracts      | `docs/features/`, `docs/client/`, `docs/developer/` |
| Architecture decisions | `docs/features/adr/`                                |
| Temporary tasks / bugs | GitHub Issues on this repository                    |
| Session plan           | Cursor plan / issue body — not durable shards       |

## Delivery conventions

- One focused work item per branch.
- Docs-first for capability changes: update shards, `npm run docs:check`, then code.
- Atomic commits: one concern per commit (docs bootstrap, feature shard, template, etc.).
- Conventional commit subjects (`feat:`, `fix:`, `docs:`, …) — see [commits and releases](./commits-and-releases.md).
- Add a changeset for releasable changes (`npm run changeset`).
- Do not commit secrets, Steam API keys, or local game paths with usernames if avoidable.

## Current phase

**AppleKit Maps+ feel surface** — slim product surface (Maps+/AppleKit), feel presets, scroll/UI gating, Sensitivity naming, orbit pitch limits, and feature flags.

Design: [AppleKit Maps+ feel surface](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md).

**Docs (MDCP):** G1–G2 done (glossary + feature contracts). G3–G5 docs in flight (client workflows, developer schema/flags, ADR). G6 validates after those land.

**Then product code:** feature flags, input gates (menu/popup + precise vs wheel), feel presets (Slow/Default/Fast + Save as…/Load), and UI against those shards.
