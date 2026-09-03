# csl-trackpad-camera-control

## 1.0.0

### Major Changes

- 95e6029: First public macOS v1: Maps+ trackpad camera (pan, pinch zoom, Option-orbit, rotate), Options/Debug feel, and Harmony vanilla-camera suppress. Windows and Linux remain unsupported.

### Minor Changes

- 7c9cc4c: Maps+/AppleKit feel surface: Sensitivity factory defaults, Slow/Default/Fast feel presets, precise-scroll pan vs mouse-wheel zoom, menu/popup input gates, and a slim Options/Assist UI (Contacts, CAD style, and Assist chrome gated off).
- dd32f97: Native Debug chrome (close, Options gear, translucent idle, field alignment); Options controls nested in AddGroup with short section titles; Debug reopen chip hidden when Options Show debug panel is off; orbit yaw/pitch factory Sensitivity 1.00; Mod.OptionsTitle / assembly Version synced from package.json via MSBuild.
- 954852b: Full gesture camera ops: pan, zoom, yaw, and orbit with Concurrent/SessionLock/PrimaryOnly resolve modes and orbit latch (in-memory settings / ApplyPreset; Options UI later).
- 7c9cc4c: In-game Assist / tuning panel with chrome and number-field tunables, mirrored in Options (no sliders); drag vs button scales, per-op low-pass, and durable settings across quit.
- 619c698: In-process trackpad capture in the mod DLL (AppKit default, Contacts still available). No companion TrackpadBridge process for playtest; frames log to a temp file.
- 7c9cc4c: Options polish and selection-aware rotate: pan clamped to city bounds; Options and Debug share live settings with autosave and New Preset dirty workflow; Sensitivity sliders, Debug rename, pitch vanilla 0–90°, and full title-bar drag; place/relocate ghosts take two-finger rotate and Option-orbit pivot — click-selection does not.
- 6885542: In-game Options for AppKit vs Contacts (legacy) capture and per-op sensitivities; changes apply without restart.
- 619c698: While the mod is enabled, vanilla scroll-zoom and mouse-drag camera rotate are suppressed so two-finger pan does not fight Unity scroll. Edge pan, keyboard, and gamepad camera controls stay. Cities Harmony is required; disable the mod to restore full vanilla camera input.

### Patch Changes

- 55f9053: Fix boot focus/cursor activation on city load so the game window receives OS focus without alt-tab.
- cb7dd3a: Clarify docs and comments: in-process AppKit capture is the default playtest path; TrackpadBridge socket remains an optional dev experiment.
- 357527b: Add a labelled Copy button beside the Debug panel build footer so devs can paste Built (UTC), asm identity, and optional QA context (OS, Mac model, input device models, Unity/Harmony/game assembly versions) into issues or chat.
- 520ddaa: Expose per-op activation deadbands (MotionDeadband, PinchDeadband, RotateDeadband) in the Debug panel; settings XML schema 6 renames former PinchEpsilon / RotateEpsilon.
- b0ab4dc: Debug footer: Include system info + Copy, then Built (local). Title shows assembly identity. Copy paste leads with TrackpadCameraControl asm, then Built (UTC); no product Mod line. Panel clamped on-screen.
- 4ee40f3: Prefer in-place Debug panel field and label refresh on Reset instead of full window redraw.
- b065ed6: Debug title-bar gear opens Options focused on Trackpad Camera Control (`SelectMod`), not the last-used Options category.
- 55d4218: Fix Debug panel title-bar drag on HiDPI by using Colossal UIDragHandle (ray/plane) instead of Input.mousePosition.
- 4977ebd: Persist Debug panel screen position across mod reload (schema 5).
- 7e12981: Debug panel body softens when the pointer leaves it so the city view shows through while tuning.
- ae37774: Persist Debug panel Copy checkbox and dismiss state across sessions (settings schema 4).
- 31fd679: Fix Debug panel Reset: restore Default feel preset only (not full factory settings) and defer UI rebuild to the next tick.
- c03de0b: Defer Mac Steam-overlay cursor ownership to v2; revert Round 3 cursor hide experiments.
- 5792f00: Follow Paradox Automate for local mod deploy and auto-reload (post-build Mods copy + AssemblyVersion Major.Minor.*), keep product semver on Options/Content Manager title, show Built (UTC) + asm in the Debug panel footer, and recreate the Debug panel on OnEnabled after auto-reload Destroy.
- ec2954f: Feel Save as… is a button (enabled when dirty) that opens a name dialog on Debug and Options; dialog suggests New Preset N. Named user presets can be deleted (applies Default). Disabled Save as… / Delete labels use grey text.
- 40ecb8f: Arm gesture capture on city load so pan, zoom, and orbit work within seconds without opening the Debug panel.
- 6c4519c: Block mod and vanilla camera input when the game window loses focus until refocus.
- 86b5af2: v1 player docs describe Maps+ only; CAD three-finger orbit is framed as a future gesture style.
- 582f849: Restore vanilla middle-mouse drag orbit while the mod is enabled; trackpad gestures and mouse orbit coexist.
- 13e1b16: Internal refactor: consolidate mod lifecycle into ModRuntime; remove duplicate camera seam. No player-visible behavior change expected.
- 9727682: Options Sensitivity sliders use 0.1×–2× factory (UI mid = Default; piecewise).
  Debug Tab order: Zoom→Pan→Rotate→Orbit fields, then Include system info (Feel name excluded).
- 4ee40f3: Rename Rotate feel fields (RotateGain/Step/Deadband) so yaw stays Orbit-only.
- 4ee40f3: Rename product Rotate op to CameraOp.Rotate / RotateGesture* (schema 8); yaw/pitch stay Orbit axes.

## 0.2.0

### Minor Changes

- 0e2e5da: C# TrackpadCapture + TrackpadBridge host (retire C), LangVersion 9 pin, xUnit/headless e2e, and in-game inject smoke harness.
- f23d9ad: Pinch → zoom MVP: macOS TrackpadBridge IPC, shared GestureFrame protocol, and C# camera pipeline with IGestureSource seam for a future in-process deploy path.
- f5b06ab: Add commitlint, husky, and changesets; publish the package as public npm (`publishConfig.access`).

### Patch Changes

- 9f8b233: Document optional Assist UI camera chrome (design, feature, and client guides).
- f8f834d: Upgrade @changesets/cli to v3 and align release workflow inputs with changesets/action v2.
- 93a7f2a: Pin GitHub Actions by commit SHA (checkout v7, setup-node v7, setup-dotnet v6, changesets/action v2) instead of floating version tags.
- f0244a9: Add macOS-first `scripts/bootstrap-dev.sh` (and npm `bootstrap*` scripts) so contributors can scriptably install and verify host + project tooling.
- 1d31a27: Add husky pre-commit (lint-staged) and pre-push (format:check + docs) hooks with shared npm scripts for consistent local gates.
- 23a5535: Require Node 22.12+, stop overwriting husky hooksPath, and simplify pre-push/CI scoping (no eval, no paths-filter fan-out).
