# Repository layout

Target layout for the clean-architecture rewrite under `rewrite/`. Code and tests stay deferred until docs gates pass; this shard names the folders and deploy identity so later phases land in the right place.

## Tree

| Path               | Role                                                                    |
| ------------------ | ----------------------------------------------------------------------- |
| `rewrite/docs/`    | MDCP target contracts (this docs tree)                                  |
| `rewrite/mod/`     | Cities: Skylines I mod assembly (`TrackpadCameraControl.Rewrite`)       |
| `rewrite/src/`     | Capture and shared libraries consumed by the rewrite mod                |
| `rewrite/tests/`   | Behavior fixtures, capture-session coverage, static-analysis gates      |
| `rewrite/scripts/` | Rewrite-only helpers (optional; root scripts may also target this tree) |

Empty or stub folders are intentional during docs-first phases. The rewrite tree README lives at repo path `rewrite/README.md` (outside this MDCP root — plain pointer only).

## Deploy identity

| Surface                       | Name                                                                                                     |
| ----------------------------- | -------------------------------------------------------------------------------------------------------- |
| Content Manager / Mods folder | `TrackpadCameraControl.Rewrite`                                                                          |
| Assembly / project            | `TrackpadCameraControl.Rewrite`                                                                          |
| Local install                 | Root script `./scripts/install-mod-local.sh --rewrite` (see [Local MVP install](./local-mvp-install.md)) |

Shipping deploy remains `TrackpadCameraControl` under repo-root `mod/`. The rewrite DLL must never overwrite that folder.

## Relation to repository root

| Root                                                  | Rewrite                                   |
| ----------------------------------------------------- | ----------------------------------------- |
| `docs/`                                               | As-built shipping contracts until cutover |
| `rewrite/docs/`                                       | Target contracts (this guide)             |
| `mod/` → Mods/`TrackpadCameraControl`                 | Shipping playtest / Share path            |
| `rewrite/mod/` → Mods/`TrackpadCameraControl.Rewrite` | Parallel A/B playtest path                |
| Root `src/`, `tests/`, `scripts/`                     | Shipping tree tooling                     |
| `rewrite/src/`, `rewrite/tests/`                      | Rewrite implementations and gates         |

Root `docs/` and `rewrite/docs/` stay separate MDCP roots (`npm run docs` vs `npm run docs:rewrite`). Do not mix shard links across those roots.

## Naming

| Surface              | Name                          |
| -------------------- | ----------------------------- |
| Product display      | Trackpad Camera Control       |
| Rewrite Mods folder  | TrackpadCameraControl.Rewrite |
| Shipping Mods folder | TrackpadCameraControl         |
| GitHub repo          | CSL-TrackpadCameraControl     |

Folder and assembly names stay PascalCase `TrackpadCameraControl*` forever. North-star lessons for every shard in this tree: greenfield redesign lessons.
