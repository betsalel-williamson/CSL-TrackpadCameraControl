# Policy / Apply audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/mod/Policy/**`, `rewrite/mod/Apply/**`  
**Contracts:** _Under the hood_, ADR 0004 / 0006, selection-aware gestures, lessons L1 / L5 / L8  
**Verdict:** Pass

## Strengths

- Style table is resolve SOT (`StyleBindingResolver`, `MapsPlusSeed`, `GestureSession`) — no parallel hardcoded chord path.
- `FeelMath.cs` uses only `System` — pitch clamp is apply constant 0–90 (L5).
- Cities camera/selection adapters are separate files talking through interfaces.
- No AppKit in Policy/Apply; production tick calls `FeelMath.Apply` from Host.
- Selection-aware rotate shape: object yaw first, then camera; orbit does not re-home Target.
- `FakeSelectionContext` goldens prove selection port on Maps+ rotate path.

## Weaknesses

- Display-only `GestureModifierKey` / `TrackpadGestureCatalog` parallel surface beside style-table modifiers.
- `TryGetSelectedWorldPosition` always false / unused in rewrite adapter.

## Critical improvements

### P0

None for resolve SOT / FeelMath purity / AppKit absence.

### P1

1. Move Unity/Colossal adapters out of pure Policy (or Host/Apply adapters folder) and drop lint carve-outs. **Closed (feedback cycle 2026-09-03):** `GameUiContext` / `GameModifierKeys` under `Apply/`; `IGameUiContext` stays in Policy.
2. Add selection-port golden coverage with a selection-only fake. **Closed:** `SelectionPortGoldenTests`.

### P2

1. Delete `CameraApplicator` / `CameraControllerZoom` aliases. **Done.**
2. Remove unused selection world-position stub or implement with a real consumer.
3. Collapse display gesture catalog ceremony where style table already supplies labels.
