# Install and first run

How players get the mod running. Who this is for: [Personas](./personas.md). Where maintainers announce it: `docs/developer/community-and-marketing.md`.

## Distribution paths

| Path               | Who                           | How                                                                                     |
| ------------------ | ----------------------------- | --------------------------------------------------------------------------------------- |
| **Beta (current)** | Early adopters / testers      | GitHub Release source archive → build/install per `docs/developer/local-mvp-install.md` |
| **Steam Workshop** | Most players (when published) | Subscribe in Workshop; enable in Content Manager with Cities Harmony                    |
| **Local dev**      | Contributors                  | Same install script as beta; see developer guide                                        |

Until Workshop publishes, treat GitHub Release + local install as the supported player/tester path. Do not imply a Workshop item exists before it does. When published, the Workshop and Content Manager title is **Trackpad Camera Control (macOS)** — paste-ready storefront copy lives in `docs/developer/workshop-storefront.md`.

## Requirements

- Cities: Skylines I
- [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) subscribed and enabled — **required** for [vanilla camera suppress](../glossary/vanilla-camera-suppress.md) (without it, trackpad pan may still fight vanilla scroll-zoom)
- Trackpad Camera Control installed via a path in the table above
- A **supported trackpad backend** for your OS (v1 ships macOS AppleKit; other platforms show unsupported until a backend exists)
- **Tested macOS versions** are listed in the developer QA checklist (`docs/developer/qa-checklist.md`, known-good table). If your Mac is not listed, try it and share your setup — Workshop comment or a GitHub issue — so we can expand the list.

## Vanilla camera while the mod is on

While Trackpad Camera Control is **enabled**:

- **Precise trackpad** two-finger scroll → mod [pan](../glossary/pan.md); vanilla scroll-zoom suppressed for that path
- **Mouse wheel** (not precise) → vanilla zoom (mod does not map wheel to pan)
- **Still on:** middle-mouse drag orbit when the rotate-camera binding is held (same as vanilla)
- **Still on:** edge pan, keyboard camera keys, gamepad / analog, free-cam / follow, one-finger tools and UI

Disable the mod in Content Manager to restore full vanilla camera input. There is no Options checkbox for this.

## First run (current)

Shipped capture is **AppleKit** with [Maps+](../glossary/maps-plus-preset.md) gesture style. Tune [Sensitivity](../glossary/sensitivity.md) and [feel presets](./feel-presets.md) from Options or the optional [Debug panel](./debug-ui.md). Contacts and pad/button chrome stay off the product surface unless their maintainer flags are on. CAD three-finger orbit is a future style, not a v1 player choice.

1. Subscribe and enable **Cities Harmony**.
2. Enable **Trackpad Camera Control** in Content Manager.
3. Load a city or start a new game. If the **macOS arrow** and the in-game cursor fight (or Shift-Tab to Steam overlay swaps which cursor you get), that is a known Steam/Unity Mac issue — not something this mod fixes in v1. Workaround: Shift-Tab out of the overlay back to the game, or Cmd-Tab once. Details: [`docs/developer/qa-mac-boot-cursor.md`](../developer/qa-mac-boot-cursor.md).
4. Within a few seconds of the city appearing, with the game focused, try two-finger drag (pan), pinch (zoom), two-finger rotate (yaw / rotate selection), and Option (`⌥`)+two-finger drag (orbit). Trackpad pan should not also vanilla-zoom; a real mouse wheel should still zoom. If you use a mouse, middle-click drag should still orbit the camera while trackpad gestures are active.
5. You do not need to open the Debug panel or Options first — gestures work out of the box; the Debug panel is optional for live tuning (factory default off).
6. Open Options → Trackpad Camera Control to adjust Sensitivity sliders or Slow / Default / Fast from the feel dropdown. Orbit pitch matches the game **0–90°**.
7. Confirm edge pan (cursor at screen edge) and keyboard camera keys still move the camera.

If gestures do nothing after the city has loaded and the game is focused, wait a few seconds and retry — do not open the Debug panel as a workaround. Then check OS gesture conflicts ([OS gesture conflicts](./os-gesture-conflicts.md)) and that Cities Harmony is enabled. Contributors can inspect the capture log under the process temp directory.
