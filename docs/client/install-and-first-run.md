# Install and first run

How players get Trackpad Camera Control running. Who this is for: [Personas](./personas.md).

## Distribution paths

| Path               | Who                      | How                                                                           |
| ------------------ | ------------------------ | ----------------------------------------------------------------------------- |
| **Beta (current)** | Early adopters / testers | GitHub Release source archive → build/install per developer local MVP install |
| **Steam Workshop** | Most players (when live) | Subscribe; enable in Content Manager with Cities Harmony                      |
| **Local rewrite**  | Contributors / testers   | `rewrite/docs/developer/local-mvp-install.md` with the rewrite assembly       |

Until Workshop publishes, treat GitHub Release + local install as the supported player/tester path. When published, the Workshop and Content Manager title is **Trackpad Camera Control (macOS)**.

## Getting started (macOS)

v1 is **macOS only**. Windows and Linux may show the mod in Content Manager; trackpad gestures will not work.

1. Subscribe to Cities Harmony and **enable** it.
2. Install this mod and **enable** it in Content Manager.
3. Load a city (not menus-only). Click the game window so it is focused.
4. Two-finger drag **pans**, pinch **zooms**, two-finger twist **rotates** heading, Option (`⌥`)+two-finger drag **orbits**.
5. Open **Options → Trackpad Camera Control** for Sensitivity and Slow / Default / Fast.

Cities Harmony must be enabled or two-finger pan may still fight vanilla scroll-zoom. Skyve is optional (load order helper) and **not** required.

## Requirements

- Cities: Skylines I
- Cities Harmony subscribed and enabled (needed for precise-trackpad scroll suppress and Option-orbit velocity flush)
- Trackpad Camera Control installed via a path above
- A **supported trackpad backend** (ship: **macOS AppKit** only)

## Vanilla camera while the mod is on

While Trackpad Camera Control is **enabled**:

- **Precise trackpad** two-finger scroll → mod [pan](../glossary/pan.md); vanilla scroll-zoom suppressed for that path
- **Mouse wheel** (not precise) → vanilla [zoom](../glossary/zoom.md)
- **Still on:** middle-mouse drag [orbit](../glossary/orbit.md) when the rotate-camera binding is held
- **Still on:** edge pan, keyboard camera keys, gamepad / analog, free-cam / follow, one-finger tools and UI

Disable the mod in Content Manager to restore full vanilla camera input. There is no Options checkbox for this.

## First run (ship Maps+)

Shipped Capture is **AppKit** with Maps+ [gesture style](../glossary/gesture-style.md). Tune Sensitivity and [feel presets](./feel-presets.md) from Options (or the optional Debug panel). There is **no** player capture-backend switcher, CAD style switcher, Contacts low-pass, or Assist chrome on the ship DLL.

1. Enable **Cities Harmony** and **Trackpad Camera Control**.
2. Load a city. With the game focused, try two-finger drag (pan), pinch (zoom), two-finger rotate (yaw / rotate selection), and Option (`⌥`)+two-finger drag (orbit).
3. Trackpad pan should not also vanilla-zoom; a real mouse wheel should still zoom; middle-click drag should still orbit.
4. You do not need Options or Debug first — gestures work out of the box.
5. Open Options → Trackpad Camera Control to adjust Sensitivity or Slow / Default / Fast. Orbit pitch matches the game **0–90°** (not an Options field).
6. Confirm edge pan and keyboard camera keys still move the camera.

If gestures do nothing after the city has loaded and the game is focused, wait a few seconds and retry. Then confirm Cities Harmony is enabled and the game window has focus.
