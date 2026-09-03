# In-game parity checklist (tier C)

Run this **after** a rewrite DLL exists that was built from the UX contract, not as a sign-off of the quarantined clone. Enable **only one** Content Manager entry per session.

Compare shipping vs `Trackpad Camera Control (Rewrite)` on the same city save. Pass means the **player cannot tell the surfaces apart**. It does **not** mean rewrite C# matches shipping C#.

```bash
./scripts/install-mod-local.sh              # shipping → Mods/TrackpadCameraControl
./scripts/install-mod-local.sh --rewrite    # rewrite → Mods/TrackpadCameraControl.Rewrite
# Requires CitiesManaged / ICities.dll on this machine.
```

## UI parity (look and interaction)

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

Tier A/B (`dotnet test` rewrite fixtures + SA gates) do **not** replace this checklist. Capture honesty and Harmony orbit flush order need a real Cities session. Source similarity to `mod/` is not a pass.
