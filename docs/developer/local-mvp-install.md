# Local MVP install (macOS)

Prove pinch → zoom with the TrackpadBridge (dev path) and a local mod DLL.

## Build and install the mod

```bash
chmod +x scripts/install-mod-local.sh
./scripts/install-mod-local.sh
```

Requires Cities: Skylines Managed assemblies (default Steam macOS path). Override with `CitiesManaged=…` or `CITIES_MODS=…`.

## Run the bridge

```bash
cd native/mac && make && ./TrackpadBridge
```

Optional: `TRACKPAD_BRIDGE_DEBUG=1` logs contact counts; `TRACKPAD_BRIDGE_SOCKET=/path/to.sock` overrides the socket.

## In game

1. Enable **Cities Harmony** (optional for this MVP — not required for pinch zoom).
2. Enable **Trackpad Camera Control** in Content Manager.
3. Load a city; keep the game focused; pinch on the trackpad.

If the bridge is not running, the mod stays enabled and does nothing (fail soft).
