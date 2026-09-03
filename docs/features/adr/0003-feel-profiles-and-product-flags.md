# ADR 0003 — Feel profiles and product flags

## Status

Accepted (docs contract for this branch)

## Context

ADR 0002 treated Maps+ and CAD as Options **presets that seed** bindings and feel. Playtesting and the AppleKit feel-surface design need a clearer product model: players tune **how camera ops feel** (sensitivities, reverse, pitch limits) separately from **which gesture style** activates orbit. Shipping Maps+/AppleKit only must not delete unfinished CAD, Contacts, or Assist chrome paths—those stay behind feature flags. UI and docs also need one canonical name for scale fields (**Sensitivity**), and scroll must distinguish precise trackpad pan from mouse-wheel zoom while never driving the mod camera over menus or popups.

## Decision

1. **Gesture style vs feel preset.** **Gesture style** is the orbit activation model (Maps+ / AppleKit: `⌥`+two-finger; CAD: three-finger when enabled). A **feel preset** (feel profile) is Slow / Default / Fast, Save as… / Load, and Reset to factory—sensitivities, reverse flags, pitch limits, and related feel fields—not a gesture-style seed.

2. **Ship surface and flags.** Ship Maps+ / AppleKit. Gate unfinished surfaces with positive `Enable*` flags, default **off**:
   - `EnableCadGestureStyle` — CAD / three-finger orbit as a player choice
   - `EnableContactsCapture` — Contacts interpreter, backend picker, and low-pass for that path
   - `EnableAssistChrome` — Assist nudge buttons and Btn sensitivity fields

3. **Sensitivity.** Canonical term is **Sensitivity** (synonyms: speed, scale, drag scale). Product numeric fields (sensitivities, button steps, deadbands, pitch) are **> 0** where required and round/display/apply to **three decimal places** (`RoundGain`).

4. **Scroll and UI gates.** Precise trackpad scroll → pan (suppress vanilla zoom when applying world pan). Mouse wheel → vanilla zoom (not mod pan). No mod camera ops when Options/menus are open or the pointer is over active popups; two-finger then scrolls UI.

## Consequences

- Player-facing “preset” means feel profile; Maps+/CAD switching is gesture style and is hidden while `EnableCadGestureStyle` is off.
- ADR 0002’s “presets that seed bindings” framing is superseded for product language where it conflicts; see [ADR 0002](./0002-gesture-presets-as-seeds.md).
- Flagged-off code may remain unused until flags turn on; docs, schema, and code share the same `Enable*` names.
- Vanilla suppress and input gates must follow precise-vs-wheel and menu/popup policy, not blanket scroll suppression.
