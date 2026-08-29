# Implementation templates

Phase 1 is docs-only. When coding starts, copy scaffolds from `templates/` at the repo root. Templates are **starting points**, not durable product docs — keep contracts in `docs/features/`.

## Template index

| Template | Purpose |
| --- | --- |
| `templates/mod/IUserMod.cs.template` | CitiesHarmony-safe mod entry (no HarmonyLib in IUserMod) |
| `templates/mod/ModSettings.cs.template` | Settings object matching [settings schema](./settings-schema.md) |
| `templates/mod/GestureBindingResolver.cs.template` | Map IPC primitives → camera ops from live settings |
| `templates/mod/CameraApplicator.cs.template` | Apply deltas to CameraController targets |
| `templates/native/mac/TrackpadBridge.main.c.template` | First backend (macOS) — native multitouch + socket loop skeleton |
| `templates/native/stub/README.md` | Win/Linux unsupported backend notes |
| `templates/shared/gesture-frame.md` | Human-readable wire field list aligned with IPC shard |

## Quick-start order (later phases)

1. Copy mod templates into `mod/`; wire CitiesHarmony.API.
2. Implement ModSettings + Options UI bound to every schema field.
3. Spike TrackpadBridge logging primitives to stdout.
4. Connect IPC; show debug overlay.
5. Fill CameraApplicator; tune default seeds; update settings-schema defaults.

## Do not

- Put final implementation into `templates/` — templates stay minimal.
- Hardcode sensitivities in CameraApplicator — read ModSettings.
- Resolve pan/orbit inside the native bridge — emit primitives only.
