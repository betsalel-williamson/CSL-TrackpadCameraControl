# macOS capture

Playtest uses **in-process AppKit** in the mod DLL (scroll / magnify / rotate → gesture primitives). Optional **TrackpadBridge** socket host below is for dev experiments only.

```bash
./scripts/install-mod-local.sh
```

Restart Cities. Inspect:

```bash
tail -f "${TMPDIR:-/tmp}/trackpad-camera-control.log"
```

Default interpreter is **AppKit** (shipped). A MultitouchSupport **Contacts** interpreter may still compile behind `EnableContactsCapture`, but it is **unfinished / not QA’d** — not a playtest recipe. See [platform backends](../../docs/features/platform-backends.md).

See [local MVP install](../../docs/developer/local-mvp-install.md).

## Optional: TrackpadBridge IPC host

Out-of-process MultitouchSupport over a Unix socket. Not the playtest path (`BridgeEnabled` stays off). The prior C helper (`TrackpadBridge.c` + `make`) is retired.

```bash
dotnet run --project src/TrackpadBridge
```

Default socket: `$TMPDIR/trackpad-camera-control.sock` (override with `TRACKPAD_BRIDGE_SOCKET`). Optional: `TRACKPAD_BRIDGE_DEBUG=1`.

## AppKit probe (headless, not the game)

Logs scroll / magnify / rotate / swipe to stderr. Not a camera backend.

```bash
./scripts/apple-gesture-probe.sh
```

Click the probe window and gesture on it. No Accessibility permission.
