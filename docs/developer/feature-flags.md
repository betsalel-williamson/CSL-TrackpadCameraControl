# Feature flags

Maintainer contract for product-surface gates. Same positive `Enable*` names appear in docs, schema notes, MSBuild properties, and `FeatureFlags` const mirrors. All three default **off** for ship.

These are **compile-time** flags: gated UI and behavior are wrapped in `#if ENABLE_*` so that code is **not compiled into** the shipping DLL when off.

Planning context: [AppleKit Maps+ feel surface design](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md). Durable player and capability contracts live in [`docs/features/`](../features/index.md) and [`docs/client/`](../client/index.md) — this shard is the flag inventory only.

## Flags

| Doc / property name     | Compile symbol             | When off (ship)                                                              | When on                                                        |
| ----------------------- | -------------------------- | ---------------------------------------------------------------------------- | -------------------------------------------------------------- |
| `EnableCadGestureStyle` | `ENABLE_CAD_GESTURE_STYLE` | Maps+ / AppleKit only; **CAD is future** — no player switcher                | Experimental: CAD / three-finger orbit as a compiled-in choice |
| `EnableContactsCapture` | `ENABLE_CONTACTS_CAPTURE`  | AppleKit only; no backend picker; no [low-pass](../glossary/low-pass.md)     | Contacts interpreter + picker; LP UI and processing            |
| `EnableAssistChrome`    | `ENABLE_ASSIST_CHROME`     | No Assist nudge buttons; no [button step](../glossary/button-step.md) fields | Assist chrome + Btn fields                                     |

There is **no** separate low-pass flag. LP rides `EnableContactsCapture` / `ENABLE_CONTACTS_CAPTURE`.

## How to turn a flag on (local / experimental builds)

From the worktree, pass MSBuild properties when building the mod (or set them in `mod/TrackpadCameraControl.csproj`):

```bash
dotnet build mod/TrackpadCameraControl.csproj -p:EnableContactsCapture=true
```

Multiple flags: `-p:EnableCadGestureStyle=true -p:EnableAssistChrome=true`.

Default ship and `./scripts/install-mod-local.sh` leave all three **false** (symbols undefined).

## Rules

- Call sites use `#if ENABLE_*` / `#endif` (not runtime `if (FeatureFlags…)`), so off builds omit that code.
- `FeatureFlags` const mirrors exist for tests and documentation; they track the same symbols.
- Schema may retain GesturePreset, CaptureBackend, button steps, and low-pass fields while flags are off.
- Turning a flag on for development does not change factory feel defaults; it only compiles in the gated surface.
