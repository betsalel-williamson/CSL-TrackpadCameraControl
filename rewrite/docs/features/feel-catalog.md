# Feel catalog

## Intent

One ordered inventory of player-facing feel controls. Options and Debug are **two hosts** over this catalog and one feel editor — not two product definitions ([under the hood](./under-the-hood.md), [ADR 0005](./adr/0005-ux-parity-not-source-parity.md)).

Source this inventory from [settings and hot configuration](./settings-and-hot-configuration.md) and from **playing shipping**, not from shipping UI class names.

## Section order

**General → Zoom → Pan → Rotate → Orbit** on both product surfaces ([UI parity](../glossary/ui-parity.md)).

## Fields (ship surface)

| Section | Field id            | Player label     | Control kind                       | Notes                                                      |
| ------- | ------------------- | ---------------- | ---------------------------------- | ---------------------------------------------------------- |
| General | `feelPreset`        | Feel preset      | Dropdown                           | Slow / Default / Fast / New Preset / named                 |
| General | `saveAs`            | Save as…         | Button                             | Enabled on New Preset                                      |
| General | `deletePreset`      | Delete           | Button                             | Named user presets only                                    |
| General | `resetFactory`      | Reset to factory | Button                             | Loads Default feel                                         |
| General | `sensitivity`       | Sensitivity      | Slider (Options) / numeric (Debug) | Master feel scale where product exposes it                 |
| General | `showDebugPanel`    | Show debug panel | Toggle                             | Chrome; hides reopen chip when off                         |
| Zoom    | `zoomSensitivity`   | Sensitivity      | Slider / numeric                   | 0.1×–2× Options contract; three decimals                   |
| Pan     | `panSensitivity`    | Sensitivity      | Slider / numeric                   | Same contract (X/Y as product surface requires)            |
| Rotate  | `rotateSensitivity` | Sensitivity      | Slider / numeric                   | Same contract                                              |
| Orbit   | `orbitSensitivity`  | Sensitivity      | Slider / numeric                   | Same contract; pitch clamp is apply constant — not a field |

Product UI does **not** expose Enable-per-op, Reverse, pitch min/max, CAD switcher, Contacts picker, low-pass, or Assist button steps on the ship DLL.

## Preset state machine

1. Built-in **Slow / Default / Fast** are immutable.
2. Editing feel while a built-in or named preset is active switches identity to **New Preset**; autosave writes that scratch profile.
3. Dropdown **select loads**. **Save as…** is separate; enabled on New Preset; after save, the named preset is selected.
4. Further edits after Save as… dirty to New Preset again.
5. **Delete** removes a named user preset only, applies Default, persists — no confirm.
6. **One dirty bit → one coalesced flush** shared by Options and Debug.

## Hosts

| Host    | Skin                                                            | Shared                |
| ------- | --------------------------------------------------------------- | --------------------- |
| Options | Colossal AddGroup rhythm; Sensitivity label + slider on one row | Catalog + editor      |
| Debug   | Floating panel chrome (drag, opacity, close, gear)              | Same catalog + editor |

## Acceptance

- A contributor can implement both hosts from this shard without opening shipping UI sources.
- Section order and labels match [parity with shipping](./parity-with-shipping.md).
- Catalog tests assert order and labels; hosts do not each own a field list.
