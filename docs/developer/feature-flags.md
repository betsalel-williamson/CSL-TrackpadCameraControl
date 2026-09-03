# Feature flags

Maintainer contract for product-surface gates. Same positive `Enable*` names appear in docs, schema notes, MSBuild properties, and `FeatureFlags` const mirrors. All three default **off** for ship.

These are **compile-time** flags: gated UI and behavior are wrapped in `#if ENABLE_*` so that code is **not compiled into** the shipping DLL when off.

Planning context: [AppleKit Maps+ feel surface design](../superpowers/specs/2026-08-29-applekit-feel-surface-design.md). Durable player and capability contracts live in [`docs/features/`](../features/index.md) and [`docs/client/`](../client/index.md) — this shard is the flag inventory only.

## Flags

| Doc / property name     | Compile symbol             | When off (ship)                                                              | Status when compiled on                                                                |
| ----------------------- | -------------------------- | ---------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| `EnableCadGestureStyle` | `ENABLE_CAD_GESTURE_STYLE` | Maps+ / AppKit only; **CAD is future** — no player switcher                  | **Future** — three-finger orbit choice; not a v1 validation target                     |
| `EnableContactsCapture` | `ENABLE_CONTACTS_CAPTURE`  | AppKit only; no backend picker; no [low-pass](../glossary/low-pass.md)       | **Unfinished** — Contacts + LP UI may compile in, but the path was **not** troubleshot |
| `EnableAssistChrome`    | `ENABLE_ASSIST_CHROME`     | No Assist nudge buttons; no [button step](../glossary/button-step.md) fields | **Future** — Assist chrome + Btn fields; not part of shipped play                      |

There is **no** separate low-pass flag. LP was tied to Contacts; with Contacts unfinished, ship builds have neither.

## How to turn a flag on (local experiments only)

From the worktree, pass MSBuild properties when building the mod (or set them in `mod/TrackpadCameraControl.csproj`):

```bash
dotnet build mod/TrackpadCameraControl.csproj -p:EnableAssistChrome=true
```

Multiple flags: `-p:EnableCadGestureStyle=true -p:EnableAssistChrome=true`.

Default ship and `./scripts/install-mod-local.sh` leave all three **false** (symbols undefined). **Do not** treat `EnableContactsCapture=true` or `TRACKPAD_CAPTURE_BACKEND=contacts` as a supported QA recipe — reopen Contacts only with a dedicated validation plan.

## Rules

- Call sites use `#if ENABLE_*` / `#endif` (not runtime `if (FeatureFlags…)`), so off builds omit that code.
- `FeatureFlags` const mirrors exist for tests and documentation; they track the same symbols.
- Schema may retain GesturePreset, CaptureBackend, button steps, and low-pass fields while flags are off.
- Turning a flag on for development does not change factory feel defaults; it only compiles in the gated surface.
