# Install and first run

## Requirements

- Cities: Skylines I
- [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) subscribed and enabled
- Trackpad Camera Control installed (local Mods folder for development; Steam Workshop later)
- A **supported trackpad backend** for your OS (v1 ships macOS; other platforms show unsupported until a backend exists)

## First run (MVP)

The current proof slice is **pinch → zoom** only. Other gestures and Options arrive later.

1. Start the macOS TrackpadBridge (dev path) so the mod can connect.
2. Enable the mod in Content Manager.
3. Load a city or start a new game.
4. With the game focused, pinch on the trackpad — the camera should zoom.

If pinch does nothing, check that the game is focused, the bridge is running, and the OS is not consuming the gesture (see [OS gesture conflicts](./os-gesture-conflicts.md)).

## First run (v1)

When full presets ship:

1. Enable the mod in Content Manager.
2. Load a city or start a new game.
3. Open Options → Trackpad Camera Control.
4. Confirm the gesture preset is **Maps+** (default).
5. Try two-finger drag (pan), pinch (zoom), two-finger rotate (yaw), and modifier+two-finger drag (orbit — Option on macOS).

## Companion mods

[ACME](https://steamcommunity.com/sharedfiles/filedetails/?id=2778750497) is recommended for camera suite features (zoom limits, saved positions). This mod only adds trackpad gestures.
