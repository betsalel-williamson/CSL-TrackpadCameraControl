# Local MVP install (macOS)

Prove pinch → zoom with the in-process capture path (mod DLL only) and a local install.

## Beta from a GitHub Release

1. Open the latest [GitHub Release](https://github.com/betsalel-williamson/CSL-TrackpadCameraControl/releases) and download the **Source code** zip/tarball (or `git clone` and `git checkout` the release tag).
2. Follow **Build and install the mod** below.
3. You need your own Cities: Skylines install (Managed assemblies). Steam Workshop packaging comes later.

## Build and install the mod

```bash
chmod +x scripts/install-mod-local.sh
./scripts/install-mod-local.sh
```

Requires Cities: Skylines Managed assemblies (default Steam macOS path). Override with `CitiesManaged=…` or `CITIES_MODS=…`.

Restart the game after install. Capture runs inside the mod DLL — there is no companion process to start.

## Capture log

Frames and start/fail lines append to a log file so you can inspect without IPC:

```bash
tail -f "${TMPDIR:-/tmp}/trackpad-camera-control.log"
```

Override the path with `TRACKPAD_CAPTURE_LOG` when launching the game.

To use MultitouchSupport contacts instead of Apple AppKit events, launch the **game** with:

```bash
TRACKPAD_CAPTURE_BACKEND=contacts
```

Default is `apple` (in-process AppKit). Env wins when set. You can also set `ModSettings.CaptureBackend` in memory.

The prior native C `make` target under `native/mac/` is retired. The C# `src/TrackpadBridge` host is an optional experiment (`BridgeEnabled`), not the playtest path.

## In game

1. Enable **Cities Harmony** (**required** for [vanilla camera suppress](../glossary/vanilla-camera-suppress.md) — without it, two-finger pan may still fight vanilla scroll-zoom).
2. Enable **Trackpad Camera Control** in Content Manager.
3. Load a city; keep the game focused; pinch and two-finger-drag on the trackpad.

If capture fails to start, the mod stays enabled and gestures do nothing (fail soft). Check the capture log. Vanilla scroll-zoom and mouse-drag rotate stay suppressed until you disable the mod.

### Content Manager version warning

Cities: Skylines may show **“This mod was not made with the current game version…”**. That check only compares the mod’s **ICities** assembly reference to the game’s current `ICities.dll` — it does **not** mean the mod is broken. Rebuild/install with `./scripts/install-mod-local.sh` after game patches (it references your Steam Managed folder). Then disable and re-enable the mod (or restart the game).

For automated inject smoke (no real pinch), see [harnesses and testing](./harnesses-and-testing.md).
