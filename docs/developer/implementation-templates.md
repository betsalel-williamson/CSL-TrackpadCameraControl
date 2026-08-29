# Implementation templates

Templates under `templates/` remain the copy-paste source of truth for new files — keep contracts in `docs/features/`. Capture sources live under `src/TrackpadCapture/` and compile into the mod DLL; see [local MVP install](./local-mvp-install.md).

## Template index

| Template                                           | Purpose                                                          |
| -------------------------------------------------- | ---------------------------------------------------------------- |
| `templates/mod/IUserMod.cs.template`               | CitiesHarmony-safe mod entry (no HarmonyLib in IUserMod)         |
| `templates/mod/ModSettings.cs.template`            | Settings object matching [settings schema](./settings-schema.md) |
| `templates/mod/GestureBindingResolver.cs.template` | Map IPC primitives → camera ops from live settings               |
| `templates/mod/CameraApplicator.cs.template`       | Apply deltas to CameraController targets                         |
| `templates/native/stub/README.md`                  | Win/Linux unsupported backend notes                              |
| `templates/shared/gesture-frame.md`                | Human-readable wire field list aligned with IPC shard            |

## Quick-start order (MVP+)

1. `./scripts/install-mod-local.sh` — build + copy DLL to Addons/Mods (see [local MVP install](./local-mvp-install.md)).
2. Restart the game; enable the mod in Content Manager; pinch zooms via in-process capture.
3. Inspect `${TMPDIR}/trackpad-camera-control.log` while gesturing.
4. Later: Options UI, more gestures, other OS backends.

## Do not

- Put final implementation into `templates/` — templates stay minimal.
- Hardcode sensitivities in CameraApplicator — read ModSettings.
- Resolve pan/orbit inside the capture / bridge host — emit primitives only.
