# Install and first run

## Requirements

- Cities: Skylines I
- [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) subscribed and enabled — **required** for [vanilla camera suppress](../glossary/vanilla-camera-suppress.md) (without it, two-finger pan may still fight vanilla scroll-zoom)
- Trackpad Camera Control installed (local Mods folder for development; Steam Workshop later)
- A **supported trackpad backend** for your OS (v1 ships macOS; other platforms show unsupported until a backend exists)

## Vanilla camera while the mod is on

While Trackpad Camera Control is **enabled**:

- **Off:** vanilla scroll-zoom, and mouse-drag camera rotate when the rotate-camera binding is held
- **Still on:** edge pan, keyboard camera keys, gamepad / analog, free-cam / follow, one-finger tools and UI

Disable the mod in Content Manager to restore full vanilla camera input. There is no Options checkbox for this.

## First run (current)

Capture uses in-process **AppKit** by default (no Accessibility, no companion bridge). Contacts (legacy Multitouch) stays available from Options.

1. Subscribe and enable **Cities Harmony**.
2. Enable **Trackpad Camera Control** in Content Manager.
3. Load a city or start a new game.
4. With the game focused, try two-finger drag (pan), pinch (zoom), two-finger rotate (yaw), and modifier+two-finger drag (orbit — Option on macOS). Pan should not also vanilla-scroll-zoom.
5. Open Options → Trackpad Camera Control to change sensitivities or switch to Contacts (legacy).
6. Confirm edge pan (cursor at screen edge) and keyboard camera keys still move the camera.

If gestures do nothing, check that the game is focused and the OS is not consuming the gesture (see [OS gesture conflicts](./os-gesture-conflicts.md)). Contributors can inspect the capture log under the process temp directory.

## Companion mods

[ACME](https://steamcommunity.com/sharedfiles/filedetails/?id=2778750497) is recommended for camera suite features (zoom limits, saved positions). This mod only adds trackpad gestures plus the vanilla camera gate above.
