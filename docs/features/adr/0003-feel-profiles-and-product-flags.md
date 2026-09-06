# ADR 0003 — Feel profiles and product flags

## Status

Accepted (ported for rewrite target)

## Context

ADR 0002 treated Maps+ and CAD as Options **presets that seed** bindings and feel. Playtesting needs a clearer product model: players tune **how camera ops feel** (sensitivities, reverse, enables) separately from **which gesture style** maps chords to ops. Shipping Maps+ only must not pull unfinished CAD, Contacts, or Assist chrome into the ship DLL. Scroll must distinguish precise trackpad pan from mouse-wheel zoom while never driving the mod camera over menus or popups.

## Decision

1. **Feel ≠ gesture style.** **Gesture style** is the chord → op model (Maps+ seed on ship; CAD when compiled on). A **feel preset** (feel profile) is Slow / Default / Fast, Save as… / Load, and Reset to factory — sensitivities, reverse flags, enables, and related feel fields — not a gesture-style seed. Feel edits must not rewrite style bindings ([ADR 0004](./0004-style-table-driven-resolve.md)).

2. **Compile-time `Enable*` modules (omit when off).** Gate unfinished surfaces with positive `Enable*` **compile** symbols, default **off**. When off, the module is **absent from the ship DLL** (no stub UI, no tick-path no-op filters):
   - `EnableCadGestureStyle` — CAD / three-finger orbit as a player choice (**future**)
   - `EnableContactsCapture` — Contacts interpreter, backend picker, and low-pass (**unfinished**)
   - `EnableAssistChrome` — Assist nudge buttons and Btn sensitivity fields (**future**)

3. **Sensitivity.** Canonical term is **Sensitivity** (synonyms: speed, scale, drag scale). Product numeric fields (sensitivities, button steps, deadbands) are **> 0** where required and round/display/apply to **three decimal places**. Orbit pitch min/max is **not** a feel field — it is an Apply constant (**0°–90°**).

4. **Scroll and UI gates.** Precise trackpad scroll → pan (suppress vanilla zoom when applying world pan). Mouse wheel → vanilla zoom (not mod pan). No mod camera ops when Options/menus are open or the pointer is over active popups; two-finger then scrolls UI.

## Consequences

- Player-facing “preset” means feel profile; Maps+/CAD switching is gesture style and is hidden (and omitted) while `EnableCadGestureStyle` is off.
- ADR 0002’s “presets that seed bindings” framing stays historical; resolve ownership of the style table is [ADR 0004](./0004-style-table-driven-resolve.md).
- Docs, schema, and build share the same `Enable*` names; runtime feature-flag mirrors of `#if` are rejected (lesson L6).
- Vanilla suppress and input gates follow precise-vs-wheel and menu/popup policy, not blanket scroll suppression.
