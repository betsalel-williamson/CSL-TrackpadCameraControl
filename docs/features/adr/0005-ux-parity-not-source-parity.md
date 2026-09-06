# ADR 0005 — UX parity is not source parity

## Status

Accepted

## Context

The greenfield ship tree exists so internals can stay simple while players still see the v1 Maps+ feel surface. Lesson L11 froze **player-visible** Options/Debug order, labels, and feel math. Implementers read that freeze as a license to copy prototype C# into `mod/` (namespace and folder changes, same types). Prototype experiments then rode along until a later strip pass. That produced a parallel clone, not a simpler system.

“1:1” in product language means the **interface and interactions**, not genetic identity with the prototype sources (now under `bootstrap/`).

## Decision

1. **Parity is a UX and dynamics contract.** Options and Debug must match the v1 section order, control kinds, labels, grouping rhythm, feel-preset interactions, and Sensitivity behavior. Maps+ chords and camera/selection outcomes must match. Proof is golden fixtures plus in-game visual/interaction checklist — not a small `diff` against bootstrap sources.

2. **`bootstrap/mod/` is an oracle, not a paste buffer.** Ship types, file splits, and control flow must be designed from feature shards (planes, style table, feel catalog). Copying bootstrap UI builders, settings stores, QA chrome, or camera/selection dumps into the ship tree is out of bounds, including “copy then tidy.”

3. **One feel catalog, two hosts.** Options and Debug share one field inventory and one editor API (dirty/preset/autosave). They must not each own a parallel ColossalUI product definition.

4. **Unfinished prototype surfaces stay omitted** from the v1 ship DLL (CAD, Contacts, Assist, IPC, file loggers, legacy XML). They are not copied forward as `#if` souvenirs.

## Consequences

- Lesson L13 (source independence) applies with L11: freeze the player-facing contract; keep internals greenfield.
- A near-copy of a bootstrap file in `mod/` is an architecture defect even if behavior matches.
- Allowed oracle extraction: numeric feel defaults, Maps+ seed chords, Harmony target names, AppKit event names — recorded as test/seed data, not as transplanted classes.
- Recovery from the clone experiment follows the design spec _Rewrite from UX contract, not source clone_ (session spec under `docs/superpowers/specs/`). Do not extend quarantined clone sources as the path to v1.
