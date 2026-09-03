# Rewrite tree (clean-architecture parity)

Parallel product tree for a greenfield architecture rewrite that keeps **UI 1:1** and **Maps+ end-user behavioral parity** with the shipping mod under repo-root `mod/`.

| Path                     | Status                                                               |
| ------------------------ | -------------------------------------------------------------------- |
| `docs/`                  | MDCP target contracts (source of truth for this tree)                |
| `mod/`, `src/`, `tests/` | Deferred until docs gates pass                                       |
| Install                  | `./scripts/install-mod-local.sh --rewrite` (stub until `mod` builds) |

Shipping as-built docs remain under repo-root `docs/` until cutover. See `docs/features/greenfield-redesign-lessons.md` in this tree once authored.
