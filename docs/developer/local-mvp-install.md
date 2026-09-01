# Local MVP install (macOS)

Prove gestures with the in-process capture path (mod DLL only) and a local Mods-folder install. This is the **beta / contributor deploy** path until Steam Workshop packaging ships.

Player-facing expectations after install: `docs/client/install-and-first-run.md`. When to point people at Release vs Workshop vs soft Discord: [Community and marketing](./community-and-marketing.md).

## Deploy roles

| Path                          | Role today                                                         |
| ----------------------------- | ------------------------------------------------------------------ |
| This local install            | Beta testers and contributors prove the mod on a real game install |
| GitHub Release source archive | Versioned input to this install (no prebuilt Workshop item yet)    |
| Steam Workshop                | Future community subscribe path — not this script                  |

## Beta from a GitHub Release

1. Open the latest [GitHub Release](https://github.com/betsalel-williamson/CSL-TrackpadCameraControl/releases) and download the **Source code** zip/tarball (or `git clone` and `git checkout` the release tag).
2. Follow **Build and install the mod** below.
3. You need your own Cities: Skylines install (Managed assemblies).

## Build and install the mod

```bash
chmod +x scripts/install-mod-local.sh
./scripts/install-mod-local.sh
```

Requires Cities: Skylines Managed assemblies (default Steam macOS path). Override with `CitiesManaged=…` or `CITIES_MODS=…`.

Restart the game after first install, or keep the game running and rebuild — post-build deploy + `AssemblyVersion` wildcards follow [Paradox Automate](https://skylines.paradoxwikis.com/Advanced_Mod_Setup#Automate) (see [mod reload during development](./mod-reload-during-development.md)). Capture uses **in-process AppKit** inside the mod DLL — there is no companion process to start. Switch to Contacts (legacy) from Options, or `TRACKPAD_CAPTURE_BACKEND=contacts`.

Optional symlink instead of post-build copy:

```bash
./scripts/install-mod-local.sh --symlink
```

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

Cities: Skylines may show **“This mod was not made with the current game version…”**. That check only compares the mod’s **ICities** assembly reference to the game’s current `ICities.dll` — it does **not** mean the mod is broken. Rebuild/install with `./scripts/install-mod-local.sh` after game patches (it references your Steam Managed folder). Then [reload the mod](./mod-reload-during-development.md) or restart the game.

For automated inject smoke (no real pinch), see [harnesses and testing](./harnesses-and-testing.md).
