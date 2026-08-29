# Work item tracking

## Where scope lives

| Kind | Location |
| --- | --- |
| Durable contracts | `docs/features/`, `docs/client/`, `docs/developer/` |
| Architecture decisions | `docs/features/adr/` |
| Temporary tasks / bugs | GitHub Issues on this repository |
| Session plan | Cursor plan / issue body — not durable shards |

## Delivery conventions

- One focused work item per branch.
- Docs-first for capability changes: update shards, `npm run docs:check`, then code.
- Atomic commits: one concern per commit (docs bootstrap, feature shard, template, etc.).
- Do not commit secrets, Steam API keys, or local game paths with usernames if avoidable.

## Current phase

**Phase 1 — Docs and high-level design** (this tree). Implementation templates are placeholders for phase 2+.
