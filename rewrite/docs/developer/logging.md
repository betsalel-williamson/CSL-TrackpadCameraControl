# Logging (rewrite v1)

**Audience:** Contributors debugging the rewrite mod in Cities or headless tests.

## Default path

The rewrite mod logs through **`ModLog`** (`rewrite/mod/Host/ModLog.cs`):

| Build                  | Behavior                                                                                |
| ---------------------- | --------------------------------------------------------------------------------------- |
| In-game (`HAS_CITIES`) | `UnityEngine.Debug.Log` with prefix `[TrackpadCameraControl]` — visible in `Player.log` |
| Headless tests         | No-op unless tests set `ModLog.TestSink`                                                |

Lifecycle messages (mod enable, gestures armed, AppKit monitor start/fail, focus activation, Options navigation errors) always use `ModLog.Info`.

## Maintainer capture trace

Per-frame capture dumps are **off by default**. Enable only when debugging capture fill:

```bash
export TRACKPAD_CAPTURE_TRACE=1
```

Then restart Cities (or re-arm capture). Traces go through the same `ModLog` path as other messages.

## What we removed

v1 does **not** use the shipping mod's `GestureCaptureLog` file logger (hidden temp files, per-line flush). Player-visible parity does not include log file format.

## Related

- [Static analysis and quality](./static-analysis-and-quality.md)
- [Harnesses and testing](./harnesses-and-testing.md) — inject `ModLog.TestSink` in Tier A/B tests when asserting log output
