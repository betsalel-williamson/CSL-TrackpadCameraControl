# Feature flags

Maintainer contract for the product-surface gate. Same positive `Enable*` identifiers appear in docs, schema notes, and code (`FeatureFlags.*`). All three default **off** for ship.

Planning context: [AppleKit Maps+ feel surface design](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md). Durable player and capability contracts live in [`docs/features/`](../features/index.md) and [`docs/client/`](../client/index.md) — this shard is the flag inventory only.

## Flags

| Flag | When off (ship now) | When on |
| ---- | ------------------- | ------- |
| `EnableCadGestureStyle` | Maps+ / AppleKit only; no Maps+/CAD gesture-style switcher in Options or Assist | CAD / three-finger orbit as a player choice |
| `EnableContactsCapture` | AppleKit capture only; no backend picker; no [low-pass](../glossary/low-pass.md) UI or filtering | Contacts interpreter + backend picker; LP UI and processing for that path |
| `EnableAssistChrome` | No Assist nudge buttons; no [button step](../glossary/button-step.md) Sensitivity fields | Assist chrome pads/buttons + Btn fields |

There is **no** separate low-pass flag. LP rides `EnableContactsCapture`.

## Rules

- Builders for Options and the Assist / tuning panel consult these flags before exposing gated controls.
- Schema may retain GesturePreset, CaptureBackend, button steps, and low-pass fields while flags are off; product UI stays gated.
- Turning a flag on for development does not change factory feel defaults; it only reveals the gated surface.
