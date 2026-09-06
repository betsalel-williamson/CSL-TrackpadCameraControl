# Rewrite tree (clean-architecture parity)

Parallel product tree for a greenfield architecture rewrite that keeps **UI 1:1** and **Maps+ end-user behavioral parity** with the shipping mod under repo-root `mod/`.

| Path     | Status                                                                            |
| -------- | --------------------------------------------------------------------------------- |
| `docs/`  | MDCP target contracts                                                             |
| `mod/`   | `TrackpadCameraControl.Rewrite` — Capture / Policy / Apply planes                 |
| `src/`   | Deferred extras (Contacts sources link from root `src/` when enabled)             |
| `tests/` | Deferred rewrite fixtures                                                         |
| Install  | `./scripts/install-mod-local.sh --rewrite` → `Mods/TrackpadCameraControl.Rewrite` |

Shipping as-built docs remain under repo-root `docs/` until cutover.
