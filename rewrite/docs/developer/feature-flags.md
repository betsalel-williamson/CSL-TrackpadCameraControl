# Feature flags

Maintainer contract for unfinished product surfaces. Flags are **compile-time only**: gated code is wrapped in `#if` / `#endif` with positive `Enable*` symbols so off builds **omit** that code from the rewrite DLL (greenfield redesign lessons L6, L9).

## Flags

| Property / doc name     | Compile symbol             | When off (ship)                               | When compiled on                                      |
| ----------------------- | -------------------------- | --------------------------------------------- | ----------------------------------------------------- |
| `EnableCadGestureStyle` | `ENABLE_CAD_GESTURE_STYLE` | Maps+ seeds only; no style switcher           | Future — CAD / three-finger orbit choice              |
| `EnableContactsCapture` | `ENABLE_CONTACTS_CAPTURE`  | AppKit only; no backend picker; no low-pass   | Unfinished — Contacts + filter UI; not a ship QA path |
| `EnableAssistChrome`    | `ENABLE_ASSIST_CHROME`     | No Assist pads/buttons; no button-step fields | Future — chrome nudges through the shared apply path  |

There is no separate low-pass flag. Low-pass stays tied to Contacts.

## Rules

- Call sites use `#if ENABLE_*` only. Do **not** introduce a runtime `FeatureFlags` facade, const mirror class, or tick-path `if (flag)` that leaves empty objects on the ship DLL.
- Off modules leave **no** stub UI, no-op filters, or dead tick subscribers.
- Schema may add module-gated rows only when resolve / apply / chrome will consume them with the module on — see [Settings schema](./settings-schema.md).
- Turning a flag on for local experiments does not change factory feel defaults; it only compiles in the gated surface.
- Default ship and rewrite local install leave all three **false** (symbols undefined).

## How to enable (local experiments)

Pass MSBuild properties when building the rewrite mod project (property names match the table). Multiple flags may combine. Do not treat `EnableContactsCapture` as a supported playtest recipe without a dedicated validation plan.
