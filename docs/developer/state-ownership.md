# State ownership

Where Trackpad Camera Control keeps data, and what must stay derived from the game each frame.

## Sources of truth

| Layer                     | Owns                                  | Examples                                                                         |
| ------------------------- | ------------------------------------- | -------------------------------------------------------------------------------- |
| **Disk (XML)**            | Player preferences and feel presets   | Gains, `RequireGameFocus`, Assist/Debug toggles                                  |
| **RAM cache**             | Working copy of settings + dirty flag | `ModSettingsStore` loads once, `MarkDirty` / `FlushIfNeeded` writes when changed |
| **Game (each frame)**     | Live simulation and UI                | Focus, menus, pointer-over-UI, selection, `CameraController` pose                |
| **Mod runtime instance**  | Async bridges and temporal filters    | Gesture queues, session latch, low-pass, pending orbit queue                     |
| **Harmony frame buffers** | Cross-patch snapshots                 | `VanillaCameraSuppress.PreciseTrackpadScroll`, `MenuOrOverUi`                    |

## Do not statically cache

These must be **re-queried** from Colossal UI / Unity each tick (via [`InputGates`](../mod/InputGates.cs) or [`CitiesSelectionContext`](../mod/CitiesSelectionContext.cs)):

- Game window focus (`Application.isFocused`)
- Menu / Options open
- Pointer over UI / popups
- Selection, relocate, placement tool state
- Camera position, angles, zoom (read/write through `ICameraController`, do not mirror in mod statics)

Adding mod statics for these creates drift and bugs (e.g. unfocused input, stale menu gates).

## ModRuntime lifecycle

While the mod is enabled in Content Manager, [`ModRuntime`](../mod/ModRuntime.cs) holds:

- `GesturePipeline` (capture source, session, low-pass, camera seam)
- Reference to `ModSettings` from the store (same object Options edits)

Created in `Mod.OnEnabled`, destroyed in `Mod.OnDisabled`. Harmony patches and `GestureThreading` read `Mod.Runtime` instead of scattered statics.

## VanillaCameraSuppress (buffers only)

Not preferences. Two flags synced for Harmony timing:

| Flag                    | Written by                                       | Purpose                                               |
| ----------------------- | ------------------------------------------------ | ----------------------------------------------------- |
| `PreciseTrackpadScroll` | AppKit scroll callback                           | Last scroll event was precise trackpad vs mouse wheel |
| `MenuOrOverUi`          | `InputGates.SyncFrameState()` each pipeline tick | Menu/popup open snapshot for scroll suppress policy   |

Policy decisions live in [`InputGates`](../mod/InputGates.cs), not in this type.

## Persisted vs session

User-facing Debug QoL lives in **`ModSettings`** schema ≥4 and `settings.xml`:

| Setting                     | Schema field              | Default |
| --------------------------- | ------------------------- | ------- |
| Include system info in Copy | `IncludeSystemInfoInCopy` | `true`  |
| Debug panel dismissed (X)   | `DebugPanelDismissed`     | `false` |
| Debug panel position X/Y    | `DebugPanelPosX/Y`        | 40 / 60 |

Focus, menu, pointer-over-UI, selection, and camera pose remain derived each frame — not static UI fields.

**Boot focus / capture arm:** on city load, `LoadingExtension` requests AppKit key-window focus and arms gesture capture connect. Neither is stored — focus and gates are re-queried each tick via `InputGates`.

## Related

- [Settings schema](./settings-schema.md)
- [Harnesses and testing](./harnesses-and-testing.md)
- [Vanilla camera suppress](../glossary/vanilla-camera-suppress.md)
