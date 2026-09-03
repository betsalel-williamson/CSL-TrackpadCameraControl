# Greenfield redesign lessons

Lessons from reviewing the shipping as-built mod and the greenfield redesign report. They are the **north star for every shard in this `rewrite/docs` tree** and for later implementation. Root `docs/` remains the as-built guide until cutover.

## L1 — Flexibility is a tick consumer, not a field

A settings field is flexible only if the **tick path reads it** (resolve, apply, or gates). Schema rows and Options labels without a consumer are ceremony.

**Shipping failure mode:** gesture binding fields were seeded and echoed in UI copy while resolve used hardcoded Maps+ heuristics plus an orbit-trigger enum.

**Rewrite rule:** a style binding table is the single source of truth for resolve. Maps+ ships as **seed data** that reproduces current chords (parity), not as parallel hardcoding beside the table.

## L2 — Feel ≠ gesture style

Keep the ADR 0003 split. Feel profiles (gains, inverts, deadbands, enables) are hot and real today—preserve that. Gesture style (Maps+ / future CAD) is orthogonal; feel copy must not rewrite bindings. Player “preset” language means a feel profile; a style switcher stays compile-gated.

## L3 — One gesture contract across capture and mod

Dual frame types plus copy bridges caused drift (including unit mismatches). Capture fills one primitive/frame contract; policy consumes it; units are documented once.

## L4 — Capture honesty gates multi-finger styles

Forcing a constant two-finger count on the AppKit path makes three-finger orbit / CAD seeds dead on the ship backend. Backends must emit honest finger counts, or style rows may only claim what capture can express. Platform backends and the parity matrix state this explicitly.

## L5 — No false feel knobs

Persisting orbit pitch min/max (and similar) while apply clamps to vanilla constants is theater. Pitch clamp is an apply constant—omit it from the feel blob and Options/Debug. Prefer deleting half-wired knobs over shipping them.

## L6 — Delete useless redirection

Alias APIs, dual capture factories, runtime feature-flag mirrors of `#if`, tick-path no-op filters when a module is off, Assist chrome with no production caller, and unused selection parameters add weight without seams.

Every type earns a second implementation or a real test seam. Compile flags **omit** modules from the DLL—no empty objects on the tick path. No alias hops.

## L7 — One write amplification path for settings

Feel edits must not double-flush XML. One live blob; one dirty bit; coalesced autosave. Options and Debug share one editor API.

## L8 — Three planes, narrow Harmony

```text
Capture (OS → primitives) → Policy (gates + session + style resolve) → Apply (feel math → camera / selection)
```

Harmony only: precise trackpad scroll suppress buffers, and deferred orbit velocity flush after vanilla damp. Do not statically cache focus, menu, over-UI, selection, or camera pose—re-query each tick (state ownership).

## L9 — Compile-time modules for unfinished surfaces

CAD, Contacts, and Assist stay behind `Enable*` **compile** symbols. The ship DLL has no stub UI or no-op filters for them. Future seeds belong in schema only when resolve will consume them with the module on.

## L10 — Tests: behavior over implementation; know the blind spots

Hand-built or injected frames prove resolve and apply—not capture fill. Fakes that integrate orbit inside velocity add encode bugs as success; queue/flush contracts and in-game order matter. Tautological tests create false greens.

Use golden Maps+ fixtures (tier A), capture-session coverage per primitive (tier B), and in-game UI + dynamics A/B (tier C). Static analysis (settings graph, leak pairing, dead aliases) is lint—not end-to-end proof.

## L11 — Parity vs elegance

Internal architecture may be greenfield-simple. **Player-visible UI and Maps+ dynamics stay 1:1** with shipping: same Options/Debug order, labels, control kinds, grouping rhythm, feel-preset interactions, and Maps+ chords/outcomes. Refuse **player-visible** cleanup that changes those. The style table must be **seeded** so behavior matches—not merely elegant and empty.

1:1 is [UI parity](../glossary/ui-parity.md) and dynamics parity — not source identity with the prototype. Do not keep shipping classes to “protect” L11. Freeze the contract; rewrite the internals ([ADR 0005](./adr/0005-ux-parity-not-source-parity.md)).

## L12 — Doc and code contract alignment

Root `docs/` remains as-built until cutover; this tree describes the **target** that still yields parity. Do not claim a “live binding table” unless resolve consumes it (ADR 0004 — style table-driven resolve). Every config field in the settings schema names its tick consumer or is marked chrome-only, XML alias, or non-field.

## L13 — Source independence

The shipping mod is a **black-box oracle** for labels, layout rhythm, numeric feel defaults, and Maps+ outcomes. It is not a paste buffer. Rewrite units must have one job (catalog vs editor vs Options host vs Debug host vs pure apply vs Cities adapter). A rewrite file that is a namespace-renamed shipping file is a failed design, even if the player cannot tell.

Do not copy unfinished prototype experiments (IPC, Contacts, CAD, Assist chrome, file loggers, legacy XML) into the rewrite “to tidy later.” Compile-omit or omit entirely (L6, L9).

## Authoring gate

If a sentence implies remappable gestures on ship, tunable pitch, runtime feature flags, tick-path Contacts low-pass on ship, or that UI 1:1 means copying shipping C#—rewrite it to match L1–L13.
