# Under-the-hood system review (2026-09-03)

**Audience:** Contributors judging whether the as-built `rewrite/` tree matches the under-the-hood redesign contracts.

**Procedure:** Project skill `system-architecture-review` (`.agents/skills/system-architecture-review/`). Specialist lanes ran in parallel; findings live in sibling shards.

**Overall verdict: Pass** (architecture P0/P1 closed; tier C playtest remains human)

The stack split is real (gesture library owns AppKit; mod consumes frames). Style-table resolve, FeelMath purity, one catalog / two hosts structure, fake-per-layer tests, and layer-import lint are strengths.

## Keep

- ADR 0006 boundary: AppKit only in `rewrite/src`; no AppKit in `rewrite/mod`
- Style binding table as resolve SOT; Maps+ seed; FeelMath without Unity usings
- FeelCatalog + FeelEditor as shared inventory/write API (hosts must not own field lists)
- Tier A goldens with camera-port fake only; `FakeOsGestureSource` as OS-only stand-in
- `layer_import_lint` inside `sa:rewrite`

## Feedback cycle (2026-09-03)

Closed from this Conditional review:

| Item                    | Change                                                                                                                                                                                                                                                                                                                         |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| P0 Host — tick hot-swap | Removed `EnsureInjectSourceIfArmed` / `EnsureCaptureSource` from `GesturePipeline.Tick`; inject still chosen once in `Mod.OnEnabled` when E2E armed; Tick still polls inject file protocol when source is `InjectGestureSource`.                                                                                               |
| P0 Host — aliases       | Removed `Mod.Pipeline` / `Mod.InjectSource` / `ModRuntime.Inject`; callers use `Mod.Runtime.Pipeline` / `Pipeline.Source`. Shared `FeelEditor` on `ModRuntime` / `Mod.Editor`.                                                                                                                                                 |
| P0 Host — Harmony mouse | `ShouldRunVanillaMouseEvents()` with no rotate-binding parameter; deleted `ShouldSuppressVanillaMouseRotate` and `IsCameraMouseRotateHeld` reflection. Prefix blocks when unfocused; Postfix still flushes orbit. PatchAll kept (only two patch classes).                                                                      |
| P0 UI hosts             | `OptionsHost.Build` maps catalog kinds through `FeelEditor` (dropdown / buttons / checkbox / sliders). `DebugHost` is a FeelCatalog skin via `FeelHostBinder` + `BuildPanelModel`; HAS_CITIES floating panel (title drag, close → `DismissDebugPanel`, reopen chip, position persist). `FeelHostMapping.MapKind` + host tests. |
| P1 Feel dirty flush     | `EnsureDirtyNewPreset` only sets New Preset + `UpsertUserPresetInMemory` (no `SaveEnvelope`); `ApplyGain` owns one `MarkDirtyAndMaybeFlush`. Regression tests added.                                                                                                                                                           |
| P1 Unity adapters       | Moved `GameUiContext` / `GameModifierKeys` to `Apply/`; `IGameUiContext` stays pure in `Policy/`. Dropped Policy lint carve-outs.                                                                                                                                                                                              |
| P1 Selection port       | `FakeSelectionContext` (selection-only) + golden tests for Maps+ rotate → object yaw vs camera yaw.                                                                                                                                                                                                                            |
| P2 aliases / queue      | Deleted `CameraApplicator`, `CameraControllerZoom`, `ModSettingsStore`. Inject queue capped at 64 like Apple.                                                                                                                                                                                                                  |
| P2 Assist button-step   | `*Step*` fields remain on `ModSettings` for future Assist module; FeelCatalog / hosts omit them; regression test locks catalog ids. FeelMath button path retained for tests.                                                                                                                                                   |

### Optional follow-up (non-blocking)

- SA doc/root alignment (scripts README layer-import list; Semgrep roots mod vs mod+src).
- Tier C in-game playtest for floating Debug chrome pixel parity.

## Specialist shards

- [Architecture / Host](./uth-architecture-host.md) — superseded in part by Feedback cycle
- [Gesture library / Capture](./uth-gesture-library.md)
- [Policy / Apply](./uth-policy-apply.md) — adapter move + selection golden closed
- [Feel / Settings](./uth-feel-settings.md) — dirty flush + Assist step hygiene closed
- [UI hosts](./uth-ui-hosts.md) — Options/Debug mapping closed
- [Tests / SA](./uth-tests-sa.md) — host-mapping, coalesced-flush, selection goldens added

## Related

- Features _Under the hood_, ADR 0005 / ADR 0006
- Closed clone experiment: [about this review guide](./about-this-guide.md)
