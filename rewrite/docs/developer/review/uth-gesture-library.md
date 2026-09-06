# Gesture library / Capture audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/src/TrackpadCameraControl.Gestures/**`  
**Contracts:** _Under the hood_, platform backends, ADR 0006, glossary _gesture library_  
**Verdict:** Pass

## Strengths

- Library is BCL-only; no ICities / Colossal / Harmony / Feel / Maps+; csproj lists no Cities DLLs; `layer_import_lint` PASS.
- `GestureFrame` + `IGestureSource` + `InjectGestureSource` + AppKit mapper/source — camera decisions stay out of Capture.
- All AppKit / objc `DllImport` live only in `AppleGestureSource.cs`; mod has none.
- Honest two-finger AppKit defaults; EndGesture → 0 fingers; optional OS finger override seam (`AppleGestureMapper`).
- Capture-session light tests and `FakeOsGestureSource` (OS stand-in only) match fake-per-layer rules.
- Prior `rewrite/mod/Capture/` Contacts/IPC tree is gone.

## Weaknesses

- `GestureFrame` comment still frames IPC-era wire protocol narrative though Contacts/IPC are removed from rewrite v1.
- Platform-backends text implies Windows/Linux stubs; library only has Apple + Inject.
- Live AppKit path always maps default finger count (−1 → 2); override unused on production path (OK for Maps+; CAD remains gated).
- Mapper tests live under Rewrite.Tests; one fact crosses into Policy resolve.
- Inject queue unbounded vs Apple’s 64-cap.

## Critical improvements

### P0

None for this lane — Capture belongs in the library and matches ADR 0006.

### P1

1. Align platform-backends wording with Apple+Inject as-built (or add an explicit unsupported source type).
2. Trim IPC-wire framing on `GestureFrame` comments.

### P2

1. Tick `FakeOsGestureSource` through `GesturePipeline` in a test.
2. Bound inject queue or document intentional unbounded harness.
3. Pure-library facts for `IsValid` / Disconnect / fail-soft connect.
