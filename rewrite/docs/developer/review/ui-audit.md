# UI / product surface audit (rewrite v1)

Specialist pass for Options and Debug UI parity vs shipping, prototype `#if` residue, and version chrome. Scope: `rewrite/mod/Ui/*`, `OptionsSettingsUi`, `TuningPanelHost*`, `Mod.cs` title APIs.

**Related:** features guide _Parity with shipping_, [organized product feedback](./v1-product-feedback.md) (F3, F4, F7), [v1 audit plan](./v1-audit-plan.md).

## Strengths

- **Structural parity with shipping.** `OptionsSettingsUi` and `TuningPanelHost` mirror the shipping mod layout: section order General → Zoom → Pan → Rotate → Orbit; feel preset dropdown with Save as… / Delete / Reset; sensitivity sliders on the 0.1×–2× contract; Debug numeric fields and op headings wired through `ModOptions` and `VanillaCameraKeyLabelsWatch`.
- **Compile-gated prototype chrome is off by default.** With all `Enable*` MSBuild properties false (ship DLL), CAD style switcher, Contacts capture picker, Assist button-step fields, and low-pass rows are not compiled into either Options or Debug — matching the features guide “ship surface omits” rules for parity with shipping.
- **Debug panel UX matches shipping patterns.** Title-bar drag, native close/Options sprites, reopen chip gated by `AssistUiEnabled` + `DebugPanelDismissed`, in-place refresh before full rebuild, tab order on product fields, build-info footer with Include system info + Copy.
- **Version split is documented in code.** `Mod.OptionsTitle` uses product semver (`BuildInfo.ProductVersion`); `Mod.DebugPanelTitle` uses `AssemblyVersion` identity for reload QA; clipboard report leads with assembly identity and UTC build stamp — aligned with root [mod reload during development](../../../../docs/developer/mod-reload-during-development.md).

## Weaknesses

- **Heavy `#if` residue from CAD / Contacts / Assist.** `TuningPanelHost.cs` and `OptionsSettingsUi.cs` each carry a dozen-plus `ENABLE_CAD_GESTURE_STYLE`, `ENABLE_CONTACTS_CAPTURE`, and `ENABLE_ASSIST_CHROME` blocks. Shipping carries the same pattern; v1 greenfield intent (F3, F4) is to **delete** gated code and files, not maintain parallel compile trees.
- **Call sites still thread prototype parameters when gates are off.** `OptionsSettingsUi.Build` always passes button-step and low-pass arguments into `BuildOpGroup*` even though `#if` hides the controls — dead API surface and copy-paste risk when stripping modules.
- **Schema naming vs product labels.** `AssistUiEnabled` persists under an Assist-era name while Options shows “Show debug panel”; `ShowPanel` forces `AssistUiEnabled = true` on reopen — confusing for contributors and docs (not player-visible).
- **Mod display name diverges from shipping.** Rewrite Options/Debug titles prefix “Rewrite” (`Trackpad Camera Control Rewrite (macOS)`). Intentional for side-by-side install but breaks literal “cannot tell apart” tier C wording unless called out in the checklist.
- **Broken XML doc on `Mod.GetAssemblyBuildTimestampUtcDisplay`.** Orphan “Legacy alias” summary left after `GetAssemblyVersionDisplay` removal — doc drift, not runtime bug.
- **No automated UI tier.** No ColossalUI harness; parity depends on tier C manual A/B. Slider thumb placement workarounds (`ForceSliderUi`, `PlaceThumb`) are fragile and unproven in CI.

## Critical improvements

1. **R2 — Delete gated UI, not `#if` it.** Remove CAD / Contacts / Assist blocks from `TuningPanelHost*`, `OptionsSettingsUi`, and `TuningPanelHost.Refresh` preset-desc refresh; drop `AddCaptureBackendButtons`, `AddCadStyleButtons`, button-step rows, and low-pass rows entirely for v1 (plan R2.1–R2.2).
2. **Simplify `BuildOpGroup*` signatures** after module removal so Options build methods only accept ship-surface parameters (sensitivity + deadband paths).
3. **R5 — Clarify release vs dev version policy** in UI: keep product semver on `OptionsTitle` always; confirm Debug title / footer behavior for release builds (F7) and add rewrite `ModBuildInfoTests` (see [release audit](./release-audit.md)).
4. **Rename or document `AssistUiEnabled`** as debug-panel visibility in [settings schema](../settings-schema.md) when legacy Assist chrome is gone.
5. **Tier C gate:** run [in-game parity checklist](../in-game-parity-checklist.md) for Options slider positions, op-description refresh on keymap change, and Debug reopen chip when “Show debug panel” is off.

## Commit mapping

| Plan commit | Concern                                                       | Primary files                                                                                                                      |
| ----------- | ------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| **R2.1**    | Remove Contacts capture UI + backend wiring                   | `TuningPanelHost.cs` (`ENABLE_CONTACTS_CAPTURE` blocks), `OptionsSettingsUi.cs`, `Mod.cs` `CreateCaptureSource`, `GesturePipeline` |
| **R2.2**    | Remove CAD gesture style UI + seed compile path               | `TuningPanelHost.cs`, `OptionsSettingsUi.cs`, `TuningPanelHost.Refresh.cs`, `Policy/CadSeed.cs`, `ModSettings.ApplyGesturePreset`  |
| **R2.3**    | Settings schema trim (no orphan filter/button fields on ship) | `ModSettings.cs`, `ModOptions.cs`, UI call sites                                                                                   |
| **R5.1**    | Version display policy + tests                                | `Mod.cs`, `QaClipboardReport.cs`, new `ModBuildInfoTests` under `rewrite/tests`                                                    |
| **R1.1**    | This shard                                                    | `rewrite/docs/developer/review/ui-audit.md`                                                                                        |
