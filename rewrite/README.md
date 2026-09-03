# Rewrite tree (clean-architecture parity)

Parallel product tree for a greenfield architecture rewrite that keeps **[UI parity](docs/glossary/ui-parity.md)** (player-facing look and interactions) and **Maps+ dynamics** with the shipping mod under repo-root `mod/`. Internals use three stack layers: native OS → [gesture library](docs/glossary/gesture-library.md) → [CSL mod surface](docs/glossary/mod-surface.md) ([ADR 0006](docs/features/adr/0006-gesture-library-vs-mod-surface.md)).

| Path     | Status                                                                            |
| -------- | --------------------------------------------------------------------------------- |
| `docs/`  | MDCP target contracts (L1–L13; start at _Under the hood_)                         |
| `src/`   | Gesture library — frame, AppKit backend, inject seam                              |
| `mod/`   | CSL mod surface — Feel, Ui, Policy, Apply, Host                                   |
| `tests/` | Behavior oracles (tier A/B); fakes stand in for one layer each                    |
| Install  | `./scripts/install-mod-local.sh --rewrite` → `Mods/TrackpadCameraControl.Rewrite` |

Shipping as-built docs remain under repo-root `docs/` until cutover.
