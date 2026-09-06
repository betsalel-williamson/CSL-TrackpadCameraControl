# Capture layer audit (rewrite v1)

Specialist review of `rewrite/mod/Capture/` and Host wiring that selects, polls, and logs gesture sources. Scope aligns with [organized product feedback](./v1-product-feedback.md) items F1 (IPC), F2 (logging), F3 (Contacts), and F5 (modifier keys).

**Audience:** Contributors executing [v1 audit plan](./v1-audit-plan.md) phase R2–R4.

**As-built date:** 2026-09-03 (`rewrite/mod` on `cursor/rewrite-parity-cc6b`).

---

## Scope

| In scope                                                   | Out of scope (other shards)                       |
| ---------------------------------------------------------- | ------------------------------------------------- |
| `IGestureSource` contract and v1 implementations           | Policy style table / `GestureSession` chord logic |
| `AppleGestureSource`, `AppleGestureMapper`, `GestureFrame` | CAD gesture preset module                         |
| Host wiring: `GesturePipeline`, `Mod.CreateCaptureSource`  | Legacy settings migration (`LegacyModSettings`)   |
| Prototype removals: IPC, Contacts, `CaptureBackend*`       | Release/version display                           |
| `GestureCaptureLog` vs standard logging                    | Assist chrome UI                                  |

**Keep for v1:** `AppleGestureSource` (AppKit in-process), `InjectGestureSource` (maintainer E2E harness), `IGestureSource` abstraction, 64-frame bounded queues, fail-soft connect/reconnect in `GesturePipeline`.

---

## Strengths

### Clean three-plane seam

Capture exposes a narrow `IGestureSource` contract (`Connect`, `Disconnect`, `TryDequeue`, `IsConnected`) with no camera or policy knowledge. `GesturePipeline` owns polling, reconnect cooldown (~1 s), and hands normalized `GestureFrame` values to Policy. This matches L8 (Capture → Policy → Apply).

### AppKit path is production-ready

`AppleGestureSource` uses Cities' existing `NSApplication` via a local event monitor — no Accessibility, no extra window, no out-of-process bridge. It:

- Gates capture through `InputGates.ShouldCaptureGestures()` before enqueueing.
- Distinguishes precise trackpad scroll from mouse wheel via `hasPreciseScrollingDeltas` (feeds `VanillaCameraSuppress.PreciseTrackpadScroll`).
- Bounds the queue at 64 frames to avoid unbounded growth under load.
- Fails soft when AppKit is missing or monitor registration fails.

`AppleGestureMapper` maps NSEvent types/phases/modifiers honestly (including `EndGesture` finger-lift frames for session reset) and rejects non-precise scroll wheel events.

### Rewrite already dropped IPC from the play path

Unlike shipping `mod/Mod.cs`, rewrite `CreateCaptureSource` never constructs `IpcGestureSource`. Ship builds (`EnableContactsCapture=false`) compile only `AppleGestureSource` on the tick path. `EnsureCaptureSource` in `GesturePipeline` hot-swaps to AppleKit without backend branching when Contacts is off.

### Inject source is a valid test seam

`InjectGestureSource` plus `E2eInjectFileProtocol` gives Tier B harnesses deterministic frames without touching OS capture. It is env/file-gated (`Mod.IsE2eInjectEnabled`) and does not belong on the player surface.

### `GameModifierKeys` matches shipping parity

The implementation is identical to shipping `mod/GameModifierKeys.cs`: OR-merge Unity `Input.GetKey` into `frame.modifiers` under `HAS_CITIES`, fail-soft. Style-table orbit chords (Option+two-finger) depend on accurate modifier bits at resolve time.

---

## Weaknesses

### IPC code remains on disk (F1)

`rewrite/mod/Capture/IpcGestureSource.cs` (~245 lines, Unix socket + libc P/Invoke) is excluded from the ship DLL via `<Compile Remove>` but still lives in the tree. Docs and `GestureFrame`'s header comment reference a shared wire protocol (`shared/protocol/gesture_frame.h`). Shipping mod still selects IPC when `BridgeEnabled` is set; rewrite does not — the file is dead weight and confuses readers about the v1 architecture.

### Contacts subsystem is compile-gated, not deleted (F3)

When `EnableContactsCapture=false` (default ship), these types are **removed from compilation** but not from the repository:

| Artifact                                         | Role                                                                                |
| ------------------------------------------------ | ----------------------------------------------------------------------------------- |
| `InProcessGestureSource.cs`                      | MultitouchSupport → `GestureFrame`                                                  |
| `DragLowPass.cs`                                 | Contacts-only EMA filters on apply path                                             |
| `CaptureBackendFlags.cs` / `CaptureBackend` enum | Backend selection + env override                                                    |
| `ModSettings.CaptureBackend`                     | Persisted field still copied in feel presets                                        |
| `ModOptions.CaptureBackendLabels`, index helpers | Options/Debug backend picker data                                                   |
| `#if ENABLE_CONTACTS_CAPTURE` blocks             | `GesturePipeline`, `OptionsSettingsUi`, `TuningPanelHost`, `Mod.cs`, `FeatureFlags` |
| csproj links                                     | `src/TrackpadCapture/*.cs` when flag on                                             |

This violates L9 (omit unfinished modules from the v1 tree, not stub them behind `#if`). Maintainers can still set `TRACKPAD_CAPTURE_BACKEND=contacts` while the Contacts types are absent — `CaptureBackendFlags.Resolve` returns Contacts from env, but `CreateCaptureSource` and `EnsureCaptureSource` ignore it when the flag is off, producing **misleading log lines** (`mod enabled backend=Contacts`) while running AppKit.

### Bespoke file logger spans Capture and Host (F2)

`GestureCaptureLog` is a near-copy of shipping `mod/GestureCaptureLog.cs` (rewrite only changes `DefaultFileName`). Call sites:

| File                                | Usage                                             |
| ----------------------------------- | ------------------------------------------------- |
| `Capture/GestureCaptureLog.cs`      | Implementation                                    |
| `Capture/AppleGestureSource.cs`     | Lifecycle + **per-frame** `Frame()` on hot path   |
| `Capture/InProcessGestureSource.cs` | Contacts delegate + errors (delete with Contacts) |
| `Host/Mod.cs`                       | Enable line, `Close`/`PathResolver` on disable    |
| `Host/GesturePipeline.cs`           | `ArmCapture`                                      |
| `Policy/GameFocusActivation.cs`     | Focus activation                                  |
| `Ui/OptionsPanelNavigation.cs`      | Options navigation failure                        |

Problems:

- Silent until first write: no log file unless `TRACKPAD_CAPTURE_LOG` or default temp path is used — players never see output.
- Per-frame `GestureCaptureLog.Frame` in `AppleGestureSource.OnBlock` adds lock + flush overhead when logging is active.
- Cross-cutting diagnostics (Options nav, focus policy) share a capture-specific type — wrong seam.
- Tests depend on `PathResolver` injection (`GestureCaptureLogScope` in test project).

### `GameModifierKeys` doc and placement are stale (F5)

The class doc still cites "out-of-process capture" and "Contacts still benefit when HID reads miss a key." v1 is in-process AppKit only; `AppleGestureMapper.MapModifiers` already reads NSEvent `modifierFlags`. `GameModifierKeys` ORs Unity keyboard state as a belt-and-suspenders merge — reasonable for Maps+ parity with shipping, but:

- Lives under `Capture/` though it reads Unity `Input` (Apply/Host concern).
- Overlap with persisted per-op `GestureModifierKey` settings is easy to misread as duplication; they operate at different layers (frame enrichment vs binding table).
- No unit test in rewrite proves AppKit-only still needs the Unity merge.

### `GestureFrame` carries IPC protocol baggage

The struct comment references `shared/protocol/gesture_frame.h` and fixed 48-byte layout for wire compatibility. v1 in-process-only capture does not need a socket-oriented contract on the primary type (though keeping a stable blittable layout is harmless for inject/harness).

### Host wiring still branches for deleted backends

`GesturePipeline.EnsureCaptureSource`, `Mod.CreateCaptureSource`, and `Mod.OnEnabled` logging all reference `CaptureBackendFlags` / `CaptureBackend` even when ship builds cannot use Contacts. `FeatureFlags.EnableContactsCapture` mirrors a compile symbol that should not exist in v1.

---

## Critical improvements

Ordered by dependency. Each row maps to a commit group in [Recommended commits](#recommended-commits).

### 1. Delete IPC entirely (F1)

| Action                                                     | Path                                                                            |
| ---------------------------------------------------------- | ------------------------------------------------------------------------------- |
| Delete                                                     | `rewrite/mod/Capture/IpcGestureSource.cs`                                       |
| Remove csproj `<Compile Remove>` entry for IPC (file gone) | `rewrite/mod/TrackpadCameraControl.Rewrite.csproj`                              |
| Trim protocol comment on struct if desired                 | `rewrite/mod/Capture/GestureFrame.cs`                                           |
| Update SA / scripts that scan IPC or bridge paths          | `rewrite/scripts/README.md`, `native_leak_pairing.py` (if still listing bridge) |
| Refresh platform-backends shard                            | `rewrite/docs/features/platform-backends.md`                                    |

No rewrite call sites remain; deletion is zero behavioral risk on ship builds.

### 2. Remove Contacts subsystem from rewrite tree (F3)

**Delete capture files:**

- `rewrite/mod/Capture/InProcessGestureSource.cs`
- `rewrite/mod/Capture/DragLowPass.cs`
- `rewrite/mod/Capture/CaptureBackendFlags.cs` (enum + resolver)

**Simplify Host:**

- `rewrite/mod/Host/GesturePipeline.cs` — remove all `#if ENABLE_CONTACTS_CAPTURE`, `EnsureCaptureSource` becomes unconditional `AppleGestureSource` (or no-op if already Apple), remove `_lowPass` field and filter calls.
- `rewrite/mod/Host/Mod.cs` — `CreateCaptureSource` returns `new AppleGestureSource()` only; remove backend log suffix or log `"backend=AppleGestures"` literally.

**Remove settings / UI surface:**

- `rewrite/mod/Settings/ModSettings.cs` — drop `CaptureBackend` property and copy paths.
- `rewrite/mod/Settings/ModOptions.cs` — remove `CaptureBackendLabels`, index helpers, `ApplyCaptureBackendIndex`.
- `rewrite/mod/Settings/FeatureFlags.cs` — remove `EnableContactsCapture` and `ENABLE_CONTACTS_CAPTURE` symbol docs.
- `rewrite/mod/Ui/OptionsSettingsUi.cs` — remove `#if ENABLE_CONTACTS_CAPTURE` capture picker block.
- `rewrite/mod/Ui/TuningPanelHost.cs` — remove `AddCaptureBackendButtons` and related `#if` blocks.

**Csproj:**

- Remove `EnableContactsCapture` property, `ENABLE_CONTACTS_CAPTURE` define, TrackpadCapture link `<ItemGroup>`, and all `<Compile Remove>` for deleted files.
- Remove filter-related settings apply methods from Options if they only served Contacts low-pass (`ApplyPanFilterAlpha`, etc.) **only if** no ship path uses them — verify against settings shard before deleting feel fields.

**Tests (rewrite / shared test project):**

- Remove or rewrite `CaptureBackendTests`, `ModOptionsTests` capture-backend cases, `FeelProfilesTests` CaptureBackend assertions.
- Delete Contacts-specific golden fixtures if any.

**Docs:**

- `rewrite/docs/developer/feature-flags.md`, `settings-schema.md`, `platform-backends.md` — Contacts as omitted, not compile-gated.

**Do not delete:** `AppleGestureSource`, `AppleGestureMapper`, `InjectGestureSource`, `IGestureSource`, `GestureFrame`.

### 3. Replace `GestureCaptureLog` with standard logging (F2)

See [Logging recommendation](#logging-recommendation). Migrate all call sites listed in Weaknesses, then delete `rewrite/mod/Capture/GestureCaptureLog.cs` (or shrink to a thin deprecated forwarder for one release — prefer delete for v1 greenfield).

Update tests: replace `GestureCaptureLogScope` with a test double on the new logger interface, or assert via injectable sink.

### 4. Clarify `GameModifierKeys` for AppKit-only v1 (F5)

| Action                                                                                                                             | Path                                                         |
| ---------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| Rewrite class doc: in-process AppKit supplies modifiers via NSEvent; Unity merge covers edge cases where event flags lag held keys | `rewrite/mod/Capture/GameModifierKeys.cs`                    |
| **Option A (minimal):** keep call in `GesturePipeline.Tick` for shipping parity                                                    | `rewrite/mod/Host/GesturePipeline.cs`                        |
| **Option B (consolidate):** move type to `Policy/` or `Host/` since it uses Unity Input, not capture hardware                      | new path + pipeline import                                   |
| Add Tier A test: frame with zero modifiers + mocked `Input.GetKey(Option)` → enriched frame matches orbit chord                    | `tests/TrackpadCameraControl.Rewrite.Tests/` or shared tests |

Do **not** remove `GameModifierKeys` without a Maps+ in-game parity pass — shipping mod keeps it on the tick path.

### 5. Align Host logging and arm semantics

| Action                                                                                                             | Path                                                                                                      |
| ------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------- |
| Route lifecycle messages through new logger                                                                        | `Host/Mod.cs`, `Host/GesturePipeline.cs`, `Policy/GameFocusActivation.cs`, `Ui/OptionsPanelNavigation.cs` |
| Gate verbose per-frame capture trace behind explicit debug flag — never default-on in `AppleGestureSource.OnBlock` | `Capture/AppleGestureSource.cs`                                                                           |

---

## Logging recommendation

Context: rewrite targets **net35** / Unity Mono (CS1). `UnityEngine.Debug.Log` is available when `HAS_CITIES` is defined (game + deploy builds with `ICities.dll`). Headless Tier A tests typically compile with `EnableCitiesRefs=false` — no UnityEngine reference.

### Recommended approach: thin `ModLog` adapter

Introduce a single static helper (e.g. `Host/ModLog.cs` or `Infrastructure/ModLog.cs`):

| Build                  | Behavior                                                                                                               |
| ---------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `HAS_CITIES`           | `Debug.Log("[TrackpadCameraControl] " + message)` — visible in `Player.log` / Cities log tools players already use     |
| Tests / no Cities refs | `[Conditional("MOD_LOG")]` no-op, or write to an injectable `Action<string>` test sink                                 |
| Verbose capture frames | Separate `[Conditional("MOD_CAPTURE_TRACE")]` method, **off by default** — replaces hot-path `GestureCaptureLog.Frame` |

**Why `Debug.Log` over alternatives:**

| Alternative                           | Verdict                                                                                                                      |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| **Keep `GestureCaptureLog` file I/O** | Poor default for v1: hidden temp files, flush-per-line cost, non-standard for CS1 mod debugging                              |
| **`Console.WriteLine`**               | Not surfaced in Unity player; useless in-game                                                                                |
| **Colossal / Cities logging APIs**    | Heavier dependency; `Debug.Log` is the conventional CS1 mod pattern and matches what players grep in `Player.log`            |
| **Third-party loggers (NLog, etc.)**  | Extra dependency on net35; rejected for v1                                                                                   |
| **Env-gated file trace**              | Acceptable **only** as optional maintainer mode behind `MOD_CAPTURE_TRACE` or explicit env — not the default diagnostic path |

### Migration notes

- **Lifecycle / error lines** (AppKit missing, monitor failed, mod enabled, gestures armed, focus activated): always `ModLog.Info` → `Debug.Log`.
- **Per-frame capture dump**: remove from default builds; if retained for maintainer debugging, compile behind `MOD_CAPTURE_TRACE` or check env once at startup (avoid per-frame env reads).
- **Shipping comparison**: shipping mod still uses `GestureCaptureLog`; rewrite v1 intentionally diverges (F2 / greenfield L6). Parity target is player-visible behavior, not log file format.
- **Test impact**: replace file assertions in `GestureCaptureLogTests` with sink-based tests on `ModLog` test hook.

---

## Recommended commits

Atomic groups aligned with [v1 audit plan](./v1-audit-plan.md) phases R2–R4. Run `dotnet test` and `npm run sa:rewrite` after each code commit.

| ID     | Subject                                                             | Files (primary)                                                                                                                                                                                                                          | Phase   |
| ------ | ------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| **C1** | `refactor(rewrite): delete IpcGestureSource and bridge references`  | Delete `Capture/IpcGestureSource.cs`; csproj; docs/scripts mentioning bridge socket                                                                                                                                                      | R2      |
| **C2** | `refactor(rewrite): remove Contacts capture module from v1 tree`    | Delete `InProcessGestureSource`, `DragLowPass`, `CaptureBackendFlags`; strip `#if ENABLE_CONTACTS_CAPTURE` from Pipeline, Mod, UI, FeatureFlags, csproj TrackpadCapture links; drop `CaptureBackend` from settings/options; update tests | R2      |
| **C3** | `docs(rewrite): align capture shards with AppKit-only v1`           | `docs/features/platform-backends.md`, `docs/developer/feature-flags.md`, `docs/developer/settings-schema.md`                                                                                                                             | R2 / R6 |
| **C4** | `refactor(rewrite): add ModLog and replace GestureCaptureLog`       | New `ModLog.cs`; migrate call sites; delete `GestureCaptureLog.cs`; update test doubles                                                                                                                                                  | R3      |
| **C5** | `docs(rewrite): document v1 logging contract`                       | `docs/developer/logging.md` (new), link from developer index                                                                                                                                                                             | R3      |
| **C6** | `refactor(rewrite): clarify GameModifierKeys for in-process AppKit` | `GameModifierKeys.cs` doc; optional move to Policy/Host; add unit test                                                                                                                                                                   | R4      |

**Suggested order:** C1 → C2 → C3 (docs can land with C2) → C4 → C5 → C6.

**Post-conditions (ship DLL, all `Enable*` false):**

- Single capture implementation: `AppleGestureSource`.
- No IPC, Contacts, `CaptureBackend`, or `DragLowPass` sources in `rewrite/mod`.
- Diagnostics via `Debug.Log` (in-game) / test sink (Tier A).
- `InjectGestureSource` unchanged for E2E.
- `GameModifierKeys.Enrich` still called from `GesturePipeline.Tick` unless parity testing proves NSEvent alone is sufficient.

---

## Related

- [Organized product feedback](./v1-product-feedback.md) — F1, F2, F3, F5
- [v1 audit and cleanup plan](./v1-audit-plan.md) — R2.1, R3, R4.1
- Features guide _Platform backends_
- [Feature flags](../feature-flags.md)
