# TrackpadBridge (macOS)

Dev IPC host: MultitouchSupport → [GestureFrame](../../shared/protocol/gesture-frame.md) over a Unix socket.

The prior C helper (`TrackpadBridge.c` + `make`) is **retired**. Use the C# host:

```bash
dotnet run --project src/TrackpadBridge
```

Default socket: `$TMPDIR/trackpad-camera-control.sock` (override with `TRACKPAD_BRIDGE_SOCKET`).

Optional: `TRACKPAD_BRIDGE_DEBUG=1` logs contact counts.

Pinch with two fingers while a client is connected to emit frames. Ctrl-C stops the bridge.

Capture logic lives in `src/TrackpadCapture/` (shared with the eventual in-process mod path).
