# Rewrite mod assembly

`TrackpadCameraControl.Rewrite` — CSL surface over the gesture library (`rewrite/src`).

## Layout

| Folder    | Role                                                      |
| --------- | --------------------------------------------------------- |
| `Feel/`   | Catalog, editor, settings store, feel profiles            |
| `Ui/`     | OptionsHost, DebugHost                                    |
| `Policy/` | Style table, Maps+ seed, resolve, session, gates          |
| `Apply/`  | Pure FeelMath + thin Cities adapters                      |
| `Host/`   | IUserMod, pipeline, Patcher (two Harmony patches), ModLog |

No AppKit P/Invoke here — capture lives in `TrackpadCameraControl.Gestures`.
