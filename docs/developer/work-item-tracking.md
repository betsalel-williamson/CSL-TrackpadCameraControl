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

**Vanilla camera suppress** — while the mod is enabled, Harmony skips vanilla scroll-zoom and mouse-drag rotate; edge pan, keyboard, and gamepad stay. Cities Harmony is required. Next: Options UI, then Assist UI wiring; in-process deploy capture remains later. An Options checkbox to leave vanilla camera on is deferred.
