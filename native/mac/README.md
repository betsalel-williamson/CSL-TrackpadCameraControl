# TrackpadBridge (macOS)

Optional experiment: MultitouchSupport → [GestureFrame](../../shared/protocol/gesture-frame.md) over a Unix socket. Playtest uses **in-process capture in the mod DLL** (see [local MVP install](../../docs/developer/local-mvp-install.md)); this host is not required.

The prior C helper (`TrackpadBridge.c` + `make`) is **retired**. To run this host:

```bash
dotnet run --project src/TrackpadBridge
```

Default socket: `$TMPDIR/trackpad-camera-control.sock` (override with `TRACKPAD_BRIDGE_SOCKET`).

Optional: `TRACKPAD_BRIDGE_DEBUG=1` logs contact counts.

Capture logic lives in `src/TrackpadCapture/` (compiled into the mod DLL; this host still references the same sources).

## Apple gesture probe (spike)

C# net8 logger (`src/AppleGestureProbe`) for scroll / magnify / rotate / swipe payloads via AppKit. Not a camera backend. Spec: `docs/superpowers/specs/2026-08-29-apple-gesture-events-spike-design.md`.

```bash
./scripts/apple-gesture-probe.sh
```

Click the probe window and gesture on it. Stderr lines are `src=local`. No Accessibility permission.
