# Install and first run

How players get the mod running. Who this is for: [Personas](./personas.md). Where maintainers announce it: `docs/developer/community-and-marketing.md`.

## Distribution paths

| Path | Who | How |
| --- | --- | --- |
| **Beta (current)** | Early adopters / testers | GitHub Release source archive → build/install per `docs/developer/local-mvp-install.md` |
| **Steam Workshop** | Most players (when published) | Subscribe in Workshop; enable in Content Manager with Cities Harmony |
| **Local dev** | Contributors | Same install script as beta; see developer guide |

Until Workshop publishes, treat GitHub Release + local install as the supported player/tester path. Do not imply a Workshop item exists before it does.

## Requirements

- Cities: Skylines I
- [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) subscribed and enabled — **required** for [vanilla camera suppress](../glossary/vanilla-camera-suppress.md) (without it, trackpad pan may still fight vanilla scroll-zoom)
- Trackpad Camera Control installed via a path in the table above
- A **supported trackpad backend** for your OS (v1 ships macOS AppleKit; other platforms show unsupported until a backend exists)

## Vanilla camera while the mod is on

While Trackpad Camera Control is **enabled**:

- **Precise trackpad** two-finger scroll → mod [pan](../glossary/pan.md); vanilla scroll-zoom suppressed for that path
- **Mouse wheel** (not precise) → vanilla zoom (mod does not map wheel to pan)
- **Off:** mouse-drag camera rotate when the rotate-camera binding is held
- **Still on:** edge pan, keyboard camera keys, gamepad / analog, free-cam / follow, one-finger tools and UI

Disable the mod in Content Manager to restore full vanilla camera input. There is no Options checkbox for this.

## First run (current)

Shipped capture is **AppleKit** with [Maps+](../glossary/maps-plus-preset.md) gesture style. Tune [Sensitivity](../glossary/sensitivity.md) and [feel presets](./feel-presets.md) from Options or the optional [Debug panel](./debug-ui.md); CAD, Contacts, and pad/button chrome stay off the product surface unless their flags are on.

1. Subscribe and enable **Cities Harmony**.
2. Enable **Trackpad Camera Control** in Content Manager.
3. Load a city or start a new game.
4. With the game focused, try two-finger drag (pan), pinch (zoom), two-finger rotate (yaw / rotate selection), and Option (`⌥`)+two-finger drag (orbit). Trackpad pan should not also vanilla-zoom; a real mouse wheel should still zoom.
5. Open Options → Trackpad Camera Control to adjust Sensitivity sliders or Slow / Default / Fast from the feel dropdown. Orbit pitch matches the game **0–90°**.
6. Confirm edge pan (cursor at screen edge) and keyboard camera keys still move the camera.

If gestures do nothing, check that the game is focused and the OS is not consuming the gesture (see [OS gesture conflicts](./os-gesture-conflicts.md)). Contributors can inspect the capture log under the process temp directory.
