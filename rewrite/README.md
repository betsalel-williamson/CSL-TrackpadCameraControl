# Rewrite tree (clean-architecture parity)

Parallel product tree for a greenfield architecture rewrite that keeps **[UI parity](docs/glossary/ui-parity.md)** (player-facing look and interactions) and **Maps+ dynamics** with the shipping mod under repo-root `mod/`. Internals must not be a source clone ([ADR 0005](docs/features/adr/0005-ux-parity-not-source-parity.md)).

| Path     | Status                                                                                                            |
| -------- | ----------------------------------------------------------------------------------------------------------------- |
| `docs/`  | MDCP target contracts (L1–L13)                                                                                    |
| `mod/`   | **Quarantined clone experiment** — do not extend as the v1 path; rebuild from the UX contract after spec approval |
| `src/`   | Deferred extras (Contacts sources link from root `src/` when enabled)                                             |
| `tests/` | Behavior oracles (keep/rewrite); do not lock cloned types                                                         |
| Install  | `./scripts/install-mod-local.sh --rewrite` → `Mods/TrackpadCameraControl.Rewrite`                                 |

Shipping as-built docs remain under repo-root `docs/` until cutover. Recovery design: repo-root `docs/superpowers/specs/2026-09-03-rewrite-from-ux-contract-design.md`.
