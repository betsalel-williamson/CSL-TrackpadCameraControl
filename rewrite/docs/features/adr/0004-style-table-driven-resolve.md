# ADR 0004 — Style table-driven resolve

## Status

Accepted (rewrite target)

## Context

Shipping resolve mixed hardcoded Maps+ heuristics with an orbit-trigger enum while Options still showed binding-like fields. That failed the flexibility test: a settings field is flexible only if the tick path reads it ([greenfield redesign lessons](../greenfield-redesign-lessons.md) L1). ADR 0002 once framed Maps+/CAD as Options presets that seed bindings; ADR 0003 split **feel** from **gesture style** for product language. The rewrite still needs one durable rule for **how chords become ops**.

## Decision

1. **Binding table is the single source of truth for Policy resolve.** Capture emits primitives; Policy maps primitives + session state through the style binding table into an op set. Resolve must not keep a parallel hardcoded Maps+ chord path beside the table.

2. **Maps+ ships as seed data for parity.** Factory / default style rows reproduce the shipping Maps+ chords (two-finger pan, pinch zoom, two-finger rotate, Option+two-finger orbit, latch and rotate-owned-contact rules as session policy). Parity is proven by seeded behavior, not by elegance of an empty table ([parity with shipping](../parity-with-shipping.md), lesson L11).

3. **CAD and other styles** may add seeds only when `EnableCadGestureStyle` (or successor) is compiled on **and** Capture can express the claimed finger counts ([platform backends](../platform-backends.md), lesson L4). Off modules omit seeds and UI from the ship DLL.

4. **Feel profiles do not rewrite bindings.** Changing Slow / Default / Fast or Sensitivity never mutates style table rows (ADR 0003).

## Consequences

- Docs and schema may call the binding table “live” only because resolve consumes it — matching this ADR.
- ADR 0002’s “presets that seed bindings” product framing remains historical; **table-driven resolve + Maps+ seed** is the rewrite contract (this ADR). Feel-vs-style language stays with [ADR 0003](./0003-feel-profiles-and-product-flags.md).
- Tests inject frames against the table (tier A golden Maps+ fixtures); they do not re-encode Maps+ heuristics in test doubles that bypass resolve.
- Authoring gate: any sentence that implies remappable gestures on ship without a table consumer is wrong; ship remapping of style rows is not a v1 player surface unless a compiled style switcher exists.
