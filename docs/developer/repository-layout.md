# Repository layout

Primary ship tree after the greenfield cutover. Stack layers: glossary _gesture library_ vs _mod surface_ (features ADR 0006).

## Tree

| Path         | Role                                                                     |
| ------------ | ------------------------------------------------------------------------ |
| `docs/`      | MDCP contracts (this docs tree)                                          |
| `src/`       | Gesture library — frame, AppKit backend, inject seam (no Cities types)   |
| `mod/`       | CSL mod surface — Feel, Ui, Policy, Apply, Host (no AppKit P/Invoke)     |
| `tests/`     | Behavior fixtures, capture-session coverage, static-analysis gates       |
| `scripts/`   | Install, format, SA gates, CI helpers                                    |
| `bootstrap/` | Historical pre-cutover prototype (reference / optional A/B install only) |

There is no separate `rewrite/` tree — that work was promoted to the paths above.

## Deploy identity

| Surface                       | Name                                                                                                     |
| ----------------------------- | -------------------------------------------------------------------------------------------------------- |
| Content Manager / Mods folder | `TrackpadCameraControl`                                                                                  |
| Content Manager title         | **Trackpad Camera Control (macOS)**                                                                      |
| Assembly / project            | `TrackpadCameraControl.Rewrite` (mod) + `TrackpadCameraControl.Gestures` (library); ship DLL names match |
| Local install                 | `./scripts/install-mod-local.sh` (see [Local MVP install](./local-mvp-install.md))                       |

Primary and bootstrap share one playtest folder. Last install wins. Tell them apart from Debug / Copy assembly identity, not from a second Content Manager row.

## Historical prototype

| Path               | Role                                                             |
| ------------------ | ---------------------------------------------------------------- |
| `bootstrap/mod/`   | Pre-cutover monolith mod                                         |
| `bootstrap/src/`   | Pre-cutover TrackpadCapture / TrackpadBridge / AppleGestureProbe |
| `bootstrap/tests/` | Pre-cutover test project                                         |

Do not revive Contacts/IPC from `bootstrap/` into the gesture library. Optional install: `./scripts/install-mod-local.sh --bootstrap`.

## Native C tree

The retired C TrackpadBridge helper is gone. `native/README.md` points at the gesture library and historical bootstrap capture tools. Ship capture is managed C# in the gesture library.

## Planning (not durable MDCP)

Session design specs, plans, and SDD scratch live **outside** the MDCP guides — typically under local `.superpowers/` (gitignored) or the issue tracker. Do not commit planning backlogs under `docs/`. Git history keeps prior session text.

## Naming

| Surface         | Name                            |
| --------------- | ------------------------------- |
| Product display | Trackpad Camera Control (macOS) |
| Mods folder     | TrackpadCameraControl           |
| GitHub repo     | CSL-TrackpadCameraControl       |

Folder and assembly names stay PascalCase `TrackpadCameraControl*` forever. North-star lessons: greenfield redesign lessons; stack story: features _Under the hood_.
