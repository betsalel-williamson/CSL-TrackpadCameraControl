# Repository layout

Target layout for the clean-architecture rewrite under `rewrite/`. Stack layers: glossary _gesture library_ vs _mod surface_ (features ADR 0006).

## Tree

| Path               | Role                                                                    |
| ------------------ | ----------------------------------------------------------------------- |
| `rewrite/docs/`    | MDCP target contracts (this docs tree)                                  |
| `rewrite/src/`     | Gesture library — frame, backends, inject seam (no Cities types)        |
| `rewrite/mod/`     | CSL mod surface — Feel, Ui, Policy, Apply, Host (no AppKit P/Invoke)    |
| `rewrite/tests/`   | Behavior fixtures, capture-session coverage, static-analysis gates      |
| `rewrite/scripts/` | Rewrite-only helpers (optional; root scripts may also target this tree) |

The rewrite tree README lives at repo path `rewrite/README.md` (outside this MDCP root — plain pointer only).

## Deploy identity

| Surface                       | Name                                                                                                     |
| ----------------------------- | -------------------------------------------------------------------------------------------------------- |
| Content Manager / Mods folder | `TrackpadCameraControl` (same as shipping; last install wins)                                            |
| Content Manager title         | **Trackpad Camera Control (macOS)** — same as shipping                                                   |
| Assembly / project            | `TrackpadCameraControl.Rewrite` (mod) + gesture library; file on disk is `TrackpadCameraControl.dll`     |
| Local install                 | Root script `./scripts/install-mod-local.sh --rewrite` (see [Local MVP install](./local-mvp-install.md)) |

Rewrite and shipping share one playtest folder. `--rewrite` overwrites shipping; a shipping install overwrites rewrite. Tell them apart from Debug / Copy assembly identity, not from a second Content Manager row.

## Relation to repository root

| Root                                                  | Rewrite                                   |
| ----------------------------------------------------- | ----------------------------------------- |
| `docs/`                                               | As-built shipping contracts until cutover |
| `rewrite/docs/`                                       | Target contracts (this guide)             |
| `mod/` → Mods/`TrackpadCameraControl`                 | Shipping playtest / Share path            |
| `rewrite/mod/` → Mods/`TrackpadCameraControl`         | Same folder; last `--rewrite` or shipping install wins |
| Root `src/`, `tests/`, `scripts/`                     | Shipping tree tooling                     |
| `rewrite/src/`, `rewrite/tests/`                      | Gesture library and rewrite gates         |

Root `docs/` and `rewrite/docs/` stay separate MDCP roots (`npm run docs` vs `npm run docs:rewrite`). Do not mix shard links across those roots. Do not revive shipping Contacts/IPC under root `src/TrackpadCapture` into the rewrite library.

## Naming

| Surface              | Name                          |
| -------------------- | ----------------------------- |
| Product display      | Trackpad Camera Control (macOS) |
| Mods folder (both)   | TrackpadCameraControl           |
| GitHub repo          | CSL-TrackpadCameraControl     |

Folder and assembly names stay PascalCase `TrackpadCameraControl*` forever. North-star lessons for every shard in this tree: greenfield redesign lessons; stack story: features _Under the hood_.
