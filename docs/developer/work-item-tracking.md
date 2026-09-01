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

**v1.0.0 launch QA** — maintainer in-game checklist, Workshop title/description, known-good platform row, Workshop Share checklist (preview image, Harmony required item), and a major changeset so the Changesets version PR becomes **1.0.0**.

In-game pass/fail lists and the recorded pre-release suite: [QA checklist](./qa-checklist.md). Paste-ready Workshop copy: [Workshop storefront](./workshop-storefront.md). Release and Share: [Release process](./release-process.md). Version PR: [commits and releases](./commits-and-releases.md).

Design: [Options polish, New Preset, selection rotate](../superpowers/specs/2026-08-30-options-polish-selection-rotate-design.md).  
Session plans live under `docs/superpowers/plans/` locally (gitignored) — not durable shards.

Prior foundation (shipped on this branch): [AppleKit Maps+ feel surface](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md).
