# Settings schema

Minimal **live** schema for the rewrite. Every row names a tick consumer, or is marked chrome, XML alias, module-gated, or **non-field**. Ceremony fields without a consumer are forbidden (greenfield redesign lessons L1, L5, L12).

Canonical UI term: **Sensitivity**. Persist / engineering names use **gain**. Product numeric feel values that apply must be **> 0** and round to **three** decimal places.

## Classification

| Kind          | Meaning                                                                   |
| ------------- | ------------------------------------------------------------------------- |
| **tick**      | Read on the capture → policy → apply path (resolve, gates, or apply math) |
| **chrome**    | Options / Debug editor or panel chrome only — not camera math             |
| **alias**     | Legacy XML element accepted on load; rewrite saves the live name          |
| **module**    | Present only when the matching `Enable*` compile module is on             |
| **non-field** | Must **not** appear in the live blob, Options, or Debug                   |

## Live feel (tick)

| Field                                                                                                           | Role                   | Consumer                       |
| --------------------------------------------------------------------------------------------------------------- | ---------------------- | ------------------------------ |
| PanEnabled / ZoomEnabled / RotateEnabled / OrbitEnabled                                                         | Per-op master switches | Policy / apply skip when false |
| PanGainX / PanGainY / ZoomGain / RotateGain / OrbitYawGain / OrbitPitchGain                                     | Sensitivity            | Apply continuous path          |
| SignInvertPanX / SignInvertPanY / SignInvertZoom / SignInvertRotate / SignInvertOrbitYaw / SignInvertOrbitPitch | Polarity after gain    | Apply                          |
| MotionDeadband / PinchDeadband / RotateDeadband                                                                 | Activation thresholds  | Resolve / apply                |

Feel profiles (Slow / Default / Fast, Save as… / Load / Delete, **New Preset**) mutate this set only. They must not rewrite gesture style seeds (L2).

## Live gates (tick)

| Field            | Default | Consumer                                                                            |
| ---------------- | ------- | ----------------------------------------------------------------------------------- |
| RequireGameFocus | true    | Gates — skip apply when unfocused                                                   |
| IgnoreOverUi     | true    | Gates — skip mod camera when pointer over popup; menus are a stronger separate gate |

## Style seeds (tick — not free-form remaps)

Maps+ ships as **seed data** in a style binding table that resolve reads as the single source of truth (L1). Seeded chords for ship parity:

| Op     | Seed gesture      | Seed modifier |
| ------ | ----------------- | ------------- |
| Pan    | Two-finger drag   | None          |
| Zoom   | Pinch             | None          |
| Rotate | Two-finger rotate | None          |
| Orbit  | Two-finger drag   | Option        |

There is no player remap UI on ship. Do not hardcode Maps+ heuristics beside the table. CAD / alternate seeds exist only when `EnableCadGestureStyle` compiles in (module rows).

## Chrome (not tick math)

| Field                           | Role                                                        |
| ------------------------------- | ----------------------------------------------------------- |
| AssistUiEnabled                 | Show or hide the in-game Debug panel (product label: Debug) |
| ActiveFeelPresetName            | Feel identity in the preset dropdown                        |
| IncludeSystemInfoInCopy         | Debug Copy includes OS / device / assembly lines            |
| DebugPanelDismissed             | Panel closed via title-bar X                                |
| DebugPanelPosX / DebugPanelPosY | Floating panel position                                     |

Options and Debug share one editor API over the same live blob (L7). Reset to factory restores feel fields; panel position stays.

## Persist envelope

| Element       | Role                                                        |
| ------------- | ----------------------------------------------------------- |
| schemaVersion | Envelope version                                            |
| current       | Full live blob (includes active feel name and chrome prefs) |
| userPresets[] | Named feel profiles only — not gesture style                |

Missing or corrupt file → factory defaults, then persist the recovered blob. One dirty bit; coalesced autosave.

## Module-gated fields (omit from ship DLL schema surface)

When the compile module is **off**, these must not appear as live schema ceremony, stub UI, or tick-path no-ops (L6, L9).

| Module                  | Fields allowed only when on                          | Consumer when on                               |
| ----------------------- | ---------------------------------------------------- | ---------------------------------------------- |
| `EnableCadGestureStyle` | Gesture style identity / CAD seed rows               | Resolve                                        |
| `EnableContactsCapture` | Capture backend choice; per-op filter enable / alpha | Capture select; optional low-pass before apply |
| `EnableAssistChrome`    | Pan/Zoom/Rotate/Orbit button steps                   | Assist chrome nudge path only (not × gain)     |

Ship builds: AppKit capture only; no backend picker; no low-pass; no Assist pads/buttons; no Maps+/CAD switcher.

## Explicit non-fields

Do **not** add these to the live blob, Options, or Debug:

| Name                                              | Why                                                                                                             |
| ------------------------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| OrbitPitchMin / OrbitPitchMax                     | Pitch clamp is an **apply constant** (vanilla normal play **0°–90°**). Persisting unused knobs is theater (L5). |
| FingerCountHysteresis                             | Unused; no tick consumer. Prefer delete over schema retention.                                                  |
| Runtime FeatureFlags facade                       | Compile `#if` only — see [Feature flags](./feature-flags.md).                                                   |
| Parallel OrbitTrigger enum beside the style table | Style seeds own orbit activation; a second enum is redirection (L1, L6).                                        |
| Bridge / socket enable on the ship path           | Out-of-process bridge is not playtest; omit from live ship schema.                                              |

XML **aliases** may accept obsolete element names on load (legacy shipping files) and rewrite to live names or drop non-fields on save. Aliases are not tick consumers.

## Feel profile contract

| Profile                        | Contract                                                   |
| ------------------------------ | ---------------------------------------------------------- |
| Default / Reset to factory     | Factory gains, reverse, enables, deadbands                 |
| Slow                           | Default gains × **0.75**; reverse unchanged                |
| Fast                           | Default gains × **1.25**; reverse unchanged                |
| New Preset                     | Scratch identity after dirtying a built-in or named preset |
| Named Save as… / Load / Delete | Full feel set in `userPresets[]`                           |

Player “preset” language means feel profile — not gesture style (L2).

## Validation rule

Every live field must keep a named consumer in this shard. Adding a schema row without a tick, chrome, alias, or module rationale fails L1 / L12. Camera and gesture modules read feel at use-time — no magic feel numbers outside the defaults factory.
