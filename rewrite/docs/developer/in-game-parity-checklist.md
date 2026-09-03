# In-game parity checklist (tier C)

Run this **after** installing both mods. Enable **only one** Content Manager entry per session.

```bash
./scripts/install-mod-local.sh              # shipping → Mods/TrackpadCameraControl
./scripts/install-mod-local.sh --rewrite    # rewrite → Mods/TrackpadCameraControl.Rewrite
# Requires CitiesManaged / ICities.dll on this machine.
```

Compare shipping vs `Trackpad Camera Control (Rewrite)` on the same city save.

## UI 1:1

| Check                                                                | Pass? |
| -------------------------------------------------------------------- | ----- |
| Options section order: General → Zoom → Pan → Rotate → Orbit         |       |
| Feel presets: Slow / Default / Fast / New Preset / Save as… / Delete |       |
| Sensitivity sliders hot-apply without restart                        |       |
| Debug panel mirrors Options feel controls (same labels)              |       |
| No CAD / Contacts / Assist chrome on ship flags (both builds)        |       |

## Gestures / dynamics (Maps+)

| Check                                                             | Pass? |
| ----------------------------------------------------------------- | ----- |
| Two-finger drag pans                                              |       |
| Pinch zooms                                                       |       |
| Two-finger rotate yaws (or selection-aware when placing)          |       |
| Option + two-finger drag orbits; latch until fingers up           |       |
| Orbit pitch stays in vanilla 0–90 feel                            |       |
| Unfocused / over UI / menu: mod camera idle; UI scroll OK         |       |
| Precise trackpad scroll does not fight vanilla zoom when mod pans |       |
| Mouse wheel zoom / MMB orbit still vanilla                        |       |

## Notes

Tier A/B (`dotnet test` rewrite fixtures + SA gates) do **not** replace this checklist. Capture honesty and Harmony orbit flush order need a real Cities session.
