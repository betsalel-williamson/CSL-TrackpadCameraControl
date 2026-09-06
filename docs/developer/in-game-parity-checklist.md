# In-game parity checklist (tier C)

Run this against a **primary** local install built from the UX contract. Optional A/B against `bootstrap/` is historical reference only — last install wins on the shared `TrackpadCameraControl` Mods folder.

Pass means the **player-visible Maps+ surface** matches the v1 contract (Options/Debug look and interaction, Maps+ chords). It does **not** mean C# matches the historical prototype. Confirm which tree is loaded from Debug Copy when A/B testing.

```bash
./scripts/install-mod-local.sh              # primary → Mods/TrackpadCameraControl
./scripts/install-mod-local.sh --bootstrap  # optional historical A/B → same folder
# Requires CitiesManaged / ICities.dll on this machine.
```

## UI parity (look and interaction)

| Check                                                                | Pass? |
| -------------------------------------------------------------------- | ----- |
| Options section order: General → Zoom → Pan → Rotate → Orbit         |       |
| Feel presets: Slow / Default / Fast / New Preset / Save as… / Delete |       |
| Sensitivity sliders hot-apply without restart                        |       |
| Debug panel mirrors Options feel controls (same labels)              |       |
| No CAD / Contacts / Assist chrome on ship flags                      |       |

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

Tier A/B (`dotnet test` + SA gates) do **not** replace this checklist. Capture honesty and Harmony orbit flush order need a real Cities session. Source similarity to `bootstrap/mod/` is not a pass.
