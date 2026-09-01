# Harnesses and testing

How contributors validate Trackpad Camera Control without (and with) Cities: Skylines.

## Tiers

| Tier                     | What it proves                                                                                  | Needs game? | Where it runs               |
| ------------------------ | ----------------------------------------------------------------------------------------------- | ----------- | --------------------------- |
| **Unit** (xUnit)         | Frame layout, binding resolver, camera apply with fakes; Mac-only QA probes assert under Darwin | No          | Local + CI (macOS Validate) |
| **Native leak static**   | Pair native acquires with releases (GCHandle, CFString, devices, monitors)                      | No          | Local + CI                  |
| **Headless e2e**         | Gesture source → resolve → apply pipeline end-to-end with fake camera                           | No          | Local + CI                  |
| **In-game inject smoke** | Synthetic frames into the loaded mod change camera zoom                                         | Yes         | Local only                  |

Real Multitouch / trackpad hardware is **not** required for CI. Hardware gestures remain a manual check on macOS with the in-process mod — follow the [QA checklist](./qa-checklist.md) after local install (see [local MVP install](./local-mvp-install.md)). During active development, prefer the [mod reload during development](./mod-reload-during-development.md) loop over a full restart when possible.

To inspect Apple-classified events (scroll, magnify, rotate, swipe) without the mod, run `./scripts/apple-gesture-probe.sh` (C# `src/AppleGestureProbe`) and gesture on the probe window — see `native/mac/README.md`. No Accessibility. That probe does not emit `GestureFrame` values.

## Coverage blind spot (learned 2026-08-29)

Unit and headless e2e tests **construct `GestureFrame` values in memory** (or inject them). They prove resolver + applicator behavior when centroid / rotate / modifiers are already correct.

They do **not** prove that `MacTrackpadCapture` / Multitouch sampling **fills** those fields. A pinch-only capture backend made pan / yaw / orbit look “implemented” in tests while production frames still had `centroidDelta* = 0`, `rotateDelta = 0`, and `modifiers = 0`.

| Layer under test                         | Would catch missing pan/yaw/orbit in capture? |
| ---------------------------------------- | --------------------------------------------- |
| Resolver / `GestureSession` / applicator | No — frames are hand-built                    |
| Headless inject e2e                      | No — inject bypasses Multitouch               |
| In-game inject smoke                     | No — request protocol is pinch-only today     |
| `MultitouchGestureSession` unit tests    | Yes — contact samples → full primitives       |
| Manual in-process mod + in-game trackpad | Yes — end-to-end hardware path                |

When adding camera ops, require a capture-session (or Multitouch→frame) test for every new primitive the mod consumes — not only pipeline tests with pre-filled frames.

## Coverage blind spot (orbit velocity, 2026-08-30)

Applicator tests must **not** treat `AddAngleVelocity` as an immediate `AngleX`/`AngleY` write. Production queues pending deltas and flushes them from a Harmony postfix on `CameraController.HandleMouseEvents` (after vanilla inertia damp, before integrate). A fake that does `AngleX +=` inside `AddAngleVelocity` will pass while Option-orbit is dead in-game.

| Layer under test                                           | Would catch dead Option-orbit velocity?    |
| ---------------------------------------------------------- | ------------------------------------------ |
| Fake that integrates onto angles in `AddAngleVelocity`     | No — encodes the bug as success            |
| Queue + `SimulateVanillaOrbitFrame` (damp→flush→integrate) | Yes — for the queue/flush contract         |
| Harmony postfix / LateUpdate order                         | No — needs [in-game QA](./qa-checklist.md) |

## Unit tests

From the repository root:

```bash
npm test
# or
dotnet test tests/TrackpadCameraControl.Tests
```

### Coverage (line / branch / method)

Use coverage to **see** what the suite already exercises — and whether the same surface is piled on by many tests — not to chase a percentage.

```bash
npm run test:coverage
```

Coverlet prints a **module summary table** during the test run. The script then writes a **class-level TextSummary** and HTML under `TestResults/coverage-report/` (gitignored):

```bash
# after npm run test:coverage
open TestResults/coverage-report/index.html   # macOS
cat TestResults/coverage-report/Summary.txt
```

| Signal                                           | How to read it                                                                    |
| ------------------------------------------------ | --------------------------------------------------------------------------------- |
| Low % on a product class you care about          | Possible blind spot — add a behavior test only if a real contract is untested     |
| Very high % on tiny helpers + many similar tests | Likely overlapping / over-specified unit tests — prefer fewer behavior cases      |
| Capture / Harmony / UI still low                 | Expected — those need session tests or in-game QA, not more fake-frame unit tests |

There is **no coverage fail gate** in CI. The csharp validate job runs CollectCoverage so PR logs include Coverlet’s module table (full HTML report is local via `npm run test:coverage`).

Include filter is the mod assembly (`TrackpadCameraControl`); the test assembly is excluded.

Expect tests to cover resolver rules, wire/`GestureFrame` layout assumptions, applicator behavior against a fake zoom seam, and **native-resource pairing** (unmanaged leaks) — no Cities assemblies.

## Native leak static analysis

In-process capture pins GCHandles, may create CoreFoundation objects, and registers AppKit monitors and Multitouch devices. Those are not garbage-collected. `dotnet test` includes a source scan of `mod/` and `src/` that fails when an acquire has no matching release in the same file:

| Acquire                                        | Must also appear                                                                |
| ---------------------------------------------- | ------------------------------------------------------------------------------- |
| `GCHandle.Alloc`                               | `.Free()` at least as often; types with `GCHandle` fields must be `IDisposable` |
| `CFStringCreateWithCString` / `CreateCfString` | `CFRelease`                                                                     |
| `.DeviceStart(`                                | `.DeviceStop(`                                                                  |
| `addLocalMonitorForEventsMatchingMask`         | `removeMonitor:`                                                                |

A line may include `native-leak-ok:` plus a reason to skip that acquire (process-lifetime cache or ownership transferred to a caller that releases). Add that marker only with a reason; do not use it to silence a real leak.

This is pairing analysis, not a runtime leak detector. It will not catch a missing `Free` on one early-return path if another path in the same file calls `Free`.

## Headless e2e

Same `dotnet test` invocation; headless cases live in the test project and exercise the pipeline with fake `IGestureSource` / zoom seams. CI should run `dotnet test` without downloading game DLLs.

## In-game inject smoke

Local-only. Assumes Cities: Skylines is running, the mod is installed and enabled, a city is loaded, and inject mode is on.

Enable inject with any of:

- `TRACKPAD_E2E_INJECT=1` in the environment that launched the game
- `$TMPDIR/e2e-inject.flag`
- `e2e-inject.flag` beside the mod DLL

Kickoff (runs headless e2e first, then waits for an in-game result file):

```bash
chmod +x scripts/e2e-ingame-smoke.sh
./scripts/e2e-ingame-smoke.sh
```

Protocol (mod directory under Addons/Mods/TrackpadCameraControl):

1. Script writes `e2e-inject-request` (text float = pinchScaleDelta).
2. Mod enqueues a pinch frame, applies zoom, writes `e2e-inject-result` (camera size).
3. Script passes when the result file appears within `E2E_INGAME_TIMEOUT` (default 90s).

This does not synthesize OS Multitouch events.

## Language and BCL pin

Mod-loaded DLL targets **net35** (Cities: Skylines Unity Mono / mscorlib). Shared capture library and bridge host use **netstandard2.0** / **net8** with **C# 9**. Prefer Mono-safe BCL surfaces in the mod — see [contributor setup](./contributor-setup.md) and [lint and format](./lint-and-format.md).

## Related

- [QA checklist (in-game)](./qa-checklist.md) — pass/fail lists after local install
- [Local MVP install](./local-mvp-install.md) — in-process capture + local mod DLL
- Design decisions: `docs/superpowers/specs/2026-08-29-csharp-capture-tests-design.md`
