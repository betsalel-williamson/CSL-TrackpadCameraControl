# Prior art and scope

## Survey verdict

No Cities: Skylines I Workshop or GitHub mod implements true multitouch / map-app-style multi-gesture camera control. Trackpad players rely on vanilla Rotate Camera Modifier remaps or OS middle-click utilities.

## Closest relatives

| Work | Overlap | Our stance |
| --- | --- | --- |
| ACME | Camera suite, mouse-drag pan, zoom-to-cursor | Coexist; do not reimplement |
| Joystick Camera Control | Continuous non-mouse axes → camera | Primary prior art for camera math and options patterns |
| Mouse Drag Camera family | RMB pan | Superseded by ACME; mouse-only |
| Zoom It! / Zoom To Cursor | Zoom behavior | Leave to ACME |
| Vanilla + OS remappers (e.g. BetterTouchTool, Karabiner) | Synthetic middle mouse | Document as fallback, not our primary path |

## Scope discipline

This mod owns **gesture input + hot binding profiles**. It does not own saved camera positions, zoom-limit overhaul, free-cam, or FPS modes.

## Release gate

Re-check Steam Workshop for trackpad / touchpad / multitouch camera mods before the first public release and update this shard if anything new appears.
