# Architecture / Host audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/mod/Host/**`, `Policy/InputGates.cs`, `Policy/VanillaCameraSuppress.cs`  
**Contracts:** features _Under the hood_, ADR 0005 / 0006, system architecture, lessons L6–L9 / L13  
**Verdict:** Conditional

## Strengths

- No AppKit P/Invoke in Host; default source is library `AppleGestureSource` with policy callbacks only (`GesturePipeline.cs`).
- Lifecycle matches the tick contract: enable → settings/runtime/Harmony; city load arms capture; simulation tick drives pipeline; disable unpatches and flushes (`Mod.cs`, `LoadingExtension.cs`, `GestureThreading.cs`, `ModRuntime.cs`).
- Tick walks Capture → Policy → Apply: gates, dequeue frame, `GestureSession`, `FeelMath.Apply` (`GesturePipeline.cs`).
- Exactly two Harmony patch attribute sites (scroll suppress + orbit flush) — closer to L8 than shipping’s wider set (`Patcher.cs`).
- Inject seam is library-backed (`InjectGestureSource` + `E2eInjectFileProtocol`).
- Contacts / IPC / DragLowPass capture modules are absent from the rewrite Host path.

## Weaknesses

- Host/gate files remain near–shipping clones (high line identity on `Mod.cs`, `Patcher.cs`, `GesturePipeline.cs`, `InputGates.cs`) — L13 / ADR 0005 defect, not a strength.
- Every tick runs `EnsureInjectSourceIfArmed` and `EnsureCaptureSource` (L6 hot-swap ceremony on the production path).
- Dual capture construction (Mod enable + pipeline factory/EnsureCaptureSource).
- Alias hops: `Mod.Pipeline`, `Mod.InjectSource`, `ModRuntime.Inject`.
- Dead mouse-rotate suppress API (`ShouldSuppressVanillaMouseRotate` always false) with leftover reflection in `HandleMouseEventsPatch`.
- Options/Debug title/version chrome lives in `Host/Mod.cs` instead of UI hosts.
- `PatchAll(assembly)` is broader than the documented two patches (fragile).

## Critical improvements

### P0

1. Redesign Host/gate control flow from contracts (not shipping diffs). **Closed (feedback cycle 2026-09-03):** tick ensure removed; aliases removed; mouse-rotate suppress deleted.

### P1

1. Arm inject once at enable; remove per-tick inject/capture ensure. **Done.**
2. One default-source factory; drop aliases. **Done.**
3. Move title helpers into OptionsHost / DebugHost. (Titles still on Mod; low priority.)

### P2

1. Explicit Harmony registration for the two patch types only. (PatchAll retained; only two patch classes remain.)
