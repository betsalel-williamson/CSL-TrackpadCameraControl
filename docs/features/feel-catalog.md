# Feel catalog

## Intent

One ordered inventory of player-facing feel controls. Options and Debug are **two hosts** over this catalog and one feel editor — not two product definitions ([under the hood](./under-the-hood.md), [ADR 0005](./adr/0005-ux-parity-not-source-parity.md)).

Source this inventory from [settings and hot configuration](./settings-and-hot-configuration.md) and from **playing shipping**, not from shipping UI class names.

## Section order

**General → Zoom → Pan → Rotate → Orbit** on both product surfaces ([UI parity](../glossary/ui-parity.md)).

## Fields (ship surface)

| Section | Field id                | Player label      | Options            | Debug                  | Notes                                       |
| ------- | ----------------------- | ----------------- | ------------------ | ---------------------- | ------------------------------------------- |
| General | `showDebugPanel`        | Show debug panel  | Toggle (**first**) | Hidden                 | Chrome; hides reopen chip when off          |
| General | `feelPreset`            | Feel preset       | Dropdown           | Dropdown               | Slow / Default / Fast / New Preset / named  |
| General | `saveAs`                | Save as…          | Button             | Button                 | Enabled on New Preset                       |
| General | `deletePreset`          | Delete            | Button             | Button                 | Named user presets only                     |
| General | `reset`                 | Reset             | Hidden             | Button (label `Reset`) | Loads Default feel; Debug only              |
| Zoom    | `zoomSensitivity`       | Sensitivity       | Slider             | Numeric                | 0.1×–2× Options contract; three decimals    |
| Zoom    | `zoomDeadband`          | Deadband          | Hidden             | Numeric                | Debug only                                  |
| Pan     | `panSensitivityX`       | Sensitivity X     | Slider             | Numeric                | Separate X/Y; do not lock axes              |
| Pan     | `panSensitivityY`       | Sensitivity Y     | Slider             | Numeric                | Separate X/Y; do not lock axes              |
| Pan     | `panDeadband`           | Deadband          | Hidden             | Numeric                | Debug only                                  |
| Rotate  | `rotateSensitivity`     | Sensitivity       | Slider             | Numeric                | Same Sensitivity contract                   |
| Rotate  | `rotateDeadband`        | Deadband          | Hidden             | Numeric                | Debug only                                  |
| Orbit   | `orbitYawSensitivity`   | Sensitivity yaw   | Slider             | Numeric                | Separate yaw/pitch                          |
| Orbit   | `orbitPitchSensitivity` | Sensitivity pitch | Slider             | Numeric                | Pitch clamp is apply constant — not a field |
| Orbit   | `orbitDeadband`         | Deadband          | Hidden             | Numeric                | Debug only                                  |

**Not on the ship surface:** master General Sensitivity, Options Reset, Enable-per-op, Reverse, pitch min/max, CAD switcher, Contacts picker, low-pass, Assist button steps.

**Debug footer chrome** (not catalog feel fields): Include system info checkbox, Copy button, Built (local) stamp. See [Hosts](#hosts).

## Preset state machine

1. Built-in **Slow / Default / Fast** are immutable.
2. Editing feel while a built-in or named preset is active switches identity to **New Preset**; autosave writes that scratch profile.
3. Dropdown **select loads**. **Save as…** is separate; enabled on New Preset; after save, the named preset is selected.
4. Further edits after Save as… dirty to New Preset again.
5. **Delete** removes a named user preset only, applies Default, persists — no confirm.
6. **One dirty bit → one coalesced flush** shared by Options and Debug.

## Hosts

| Host    | Skin                                                                                                                                                                                             | Shared                |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------- |
| Options | Colossal AddGroup rhythm; sliders; General = Show debug → Feel → Save as… → Delete; op description labels (**Gesture(s)** from style table + live **Keymapping(s)** from Cities camera bindings) | Catalog + editor      |
| Debug   | 560px floating chrome (drag, opacity, close, gear); numeric Sensitivity + Deadband; Reset on Feel row; Copy/build footer                                                                         | Same catalog + editor |

### Debug Copy footer

After Orbit, Debug always shows:

1. **Include system info** (persisted `IncludeSystemInfoInCopy`, default on) + **Copy**.
2. Dimmed `Built (local): yyyy-MM-dd HH:mm:ss` (or `Built (local): ?` if stamp missing).

Copy paste contract:

- `TrackpadCameraControl.Rewrite: {assembly identity}`
- `Built (UTC): …`
- When Include is on: `--- System ---` (OS, Model), `--- Input devices ---`, `--- Assemblies ---` (Unity, ICities, CitiesHarmony.API, 0Harmony — not this mod again).
- Off Mac, input enumeration fail-softs with `(macOS input enumeration unavailable on this host)`.

## Acceptance

- A contributor can implement both hosts from this shard without opening shipping UI sources.
- Section order and labels match [parity with shipping](./parity-with-shipping.md).
- Catalog tests assert shared inventory + per-host visibility/kind; hosts do not each own a field list.
- Debug Copy paste identifies the rewrite tree vs shipping.
