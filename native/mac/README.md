# TrackpadBridge (macOS)

Build and run the multitouch helper that streams [GestureFrame](../../shared/protocol/gesture-frame.md) primitives over a Unix socket.

```bash
cd native/mac
make
./TrackpadBridge
```

Default socket: `$TMPDIR/trackpad-camera-control.sock` (override with `TRACKPAD_BRIDGE_SOCKET`).

Pinch with two fingers while a client is connected to emit frames. Ctrl-C stops the bridge.
