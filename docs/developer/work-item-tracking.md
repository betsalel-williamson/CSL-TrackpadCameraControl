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

**Options polish + New Preset + selection rotate** — Debug panel naming, Options layout and Sensitivity sliders, **New Preset** dirty autosave, pitch 0–90°, pan city-bounds clamp, and selection-aware rotate / ⌥-orbit.

Design: [Options polish, New Preset, selection rotate](../superpowers/specs/2026-08-30-options-polish-selection-rotate-design.md).  
Plan: [Options polish, New Preset, selection rotate](../superpowers/plans/2026-08-30-options-polish-selection-rotate-plan.md).

Prior foundation (shipped on this branch): [AppleKit Maps+ feel surface](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md).
