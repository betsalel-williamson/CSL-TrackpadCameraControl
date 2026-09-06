# Native (retired C helper)

The macOS **C** TrackpadBridge helper is **retired**. There are no `.c` / `.h` sources in this tree.

| Need                        | Where                                                             |
| --------------------------- | ----------------------------------------------------------------- |
| Ship capture (AppKit)       | `src/TrackpadCameraControl.Gestures`                              |
| Historical Multitouch / IPC | `bootstrap/src/TrackpadCapture`, `bootstrap/src/TrackpadBridge`   |
| Historical AppKit probe     | `bootstrap/src/AppleGestureProbe`                                 |
| Platform policy             | [Platform backends](../docs/features/platform-backends.md)        |
| ADR                         | [ADR 0001](../docs/features/adr/0001-native-multitouch-bridge.md) |

Windows / Linux backends remain **unsupported** in v1 (noop gesture source on non-macOS). Do not reintroduce a C make target for playtest.
