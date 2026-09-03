# Under-the-hood system review (2026-09-03)

**Audience:** Contributors judging whether the as-built `rewrite/` tree matches the under-the-hood redesign contracts.

**Procedure:** Project skill `system-architecture-review` (`.agents/skills/system-architecture-review/`). Specialist lanes ran in parallel; findings live in sibling shards.

**Overall verdict: Conditional** (feedback cycle in progress — P0/P1 closed below)

The stack split is real (gesture library owns AppKit; mod consumes frames). Style-table resolve, FeelMath purity, one catalog / two hosts structure, fake-per-layer tests, and layer-import lint are strengths.

## Keep

- ADR 0006 boundary: AppKit only in `rewrite/src`; no AppKit in `rewrite/mod`
- Style binding table as resolve SOT; Maps+ seed; FeelMath without Unity usings
- FeelCatalog + FeelEditor as shared inventory/write API (hosts must not own field lists)
- Tier A goldens with camera-port fake only; `FakeOsGestureSource` as OS-only stand-in
- `layer_import_lint` inside `sa:rewrite`

## Feedback cycle (2026-09-03)

Closed from this Conditional review:

| Item                    | Change                                                                                                                                                                                                                                                                      |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| P0 Host — tick hot-swap | Removed `EnsureInjectSourceIfArmed` / `EnsureCaptureSource` from `GesturePipeline.Tick`; inject still chosen once in `Mod.OnEnabled` when E2E armed; Tick still polls inject file protocol when source is `InjectGestureSource`.                                            |
| P0 Host — aliases       | Removed `Mod.Pipeline` / `Mod.InjectSource` / `ModRuntime.Inject`; callers use `Mod.Runtime.Pipeline` / `Pipeline.Source`. Shared `FeelEditor` on `ModRuntime` / `Mod.Editor`.                                                                                              |
| P0 Host — Harmony mouse | `ShouldRunVanillaMouseEvents()` with no rotate-binding parameter; deleted `ShouldSuppressVanillaMouseRotate` and `IsCameraMouseRotateHeld` reflection. Prefix blocks when unfocused; Postfix still flushes orbit. PatchAll kept (only two patch classes).                   |
| P0 UI hosts             | `OptionsHost.Build` maps catalog kinds through `FeelEditor` (dropdown / buttons / checkbox / sliders). `DebugHost` accepts `FeelEditor`, wires `SettingsChanged`, `ApplyVisibility` from `AssistUiEnabled && !DebugPanelDismissed`. `FeelHostMapping.MapKind` + host tests. |
| P1 Feel dirty flush     | `EnsureDirtyNewPreset` only sets New Preset + `UpsertUserPresetInMemory` (no `SaveEnvelope`); `ApplyGain` owns one `MarkDirtyAndMaybeFlush`. Regression tests added.                                                                                                        |
| P1 Unity adapters       | Moved `GameUiContext` / `GameModifierKeys` to `Apply/`; `IGameUiContext` stays pure in `Policy/`. Dropped Policy lint carve-outs.                                                                                                                                           |
| P2 aliases / queue      | Deleted `CameraApplicator`, `CameraControllerZoom`, `ModSettingsStore`. Inject queue capped at 64 like Apple.                                                                                                                                                               |

### Remaining Conditional items

- Debug floating Colossal panel chrome (visibility + descriptors wired; full floating UI still thin).
- SA doc/root alignment (scripts README layer-import list; Semgrep roots mod vs mod+src) — partial / optional follow-up.
- Selection-port goldens; trim Assist button-step fields when Assist is off.
- Specialist shards below describe the pre-cycle audit; prefer this Feedback cycle table for as-built status.

## Specialist shards

- [Architecture / Host](./uth-architecture-host.md) — superseded in part by Feedback cycle
- [Gesture library / Capture](./uth-gesture-library.md)
- [Policy / Apply](./uth-policy-apply.md) — adapter move closed in Feedback cycle
- [Feel / Settings](./uth-feel-settings.md) — dirty flush closed in Feedback cycle
- [UI hosts](./uth-ui-hosts.md) — Options/Debug mapping closed in Feedback cycle
- [Tests / SA](./uth-tests-sa.md) — host-mapping + coalesced-flush tests added

## Related

- Features _Under the hood_, ADR 0005 / ADR 0006
- Closed clone experiment: [about this review guide](./about-this-guide.md)
