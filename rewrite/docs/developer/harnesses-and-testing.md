# Harnesses and testing

How contributors prove rewrite behavior without treating fakes as end-to-end truth (greenfield redesign lessons L10). Prefer behavior contracts over implementation snapshots.

## Tiers

| Tier                          | What it proves                                                                                         | Needs game? |
| ----------------------------- | ------------------------------------------------------------------------------------------------------ | ----------- |
| **A — Golden Maps+ fixtures** | Resolve and apply given known primitives (centroid, pinch, rotate, modifiers) match Maps+ parity seeds | No          |
| **B — Capture-session**       | Backend / session fill produces honest primitives (including finger counts the style table claims)     | No          |
| **C — In-game**               | UI 1:1, Harmony order, hardware chords, gates, suppress, selection rotate — side-by-side with shipping | Yes         |

Static analysis (settings graph, leak pairing, dead aliases, Dispose order) is **lint**, not a substitute for tiers A–C — see [Static analysis and quality](./static-analysis-and-quality.md).

## Tier A — fixtures

Hand-built or injected frames exercise policy resolve and apply math against fakes. Use golden Maps+ cases for pan, pinch zoom, two-finger rotate, and Option+two-finger orbit (including latch and hard handoff into rotate).

Fixtures prove **consumption** of correct primitives — not that capture emitted them.

## Tier B — capture-session

For every primitive the mod consumes, add session coverage from contact / AppKit samples → frame fields. Honest finger counts matter: a backend that always reports two fingers makes three-finger CAD seeds dead (L4).

Tier B is required when adding camera ops or style rows that claim new primitives.

## Tier C — in-game

Manual and scripted play after [Local MVP install](./local-mvp-install.md). Covers Harmony postfix timing, `HandleMouseEvents` order, Option+drag hardware, UI parity, and A/B vs shipping. Checklist: [QA checklist](./qa-checklist.md).

## Fake limits (blind spots)

| Fake / harness                                             | Does **not** prove                                                                 |
| ---------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| Hand-built frames (tier A)                                 | Capture fill of centroid / rotate / modifiers / finger count                       |
| Inject that bypasses the backend                           | Production Multitouch / AppKit sampling                                            |
| Fake that integrates angles inside the orbit-velocity seam | Real queue → damp → flush → integrate order (encodes dead Option-orbit as success) |
| Pinch-only inject smoke                                    | Pan / rotate / orbit hardware paths                                                |
| High line coverage on helpers                              | Harmony timing, UI 1:1, or feel parity                                             |

Orbit apply must queue pending deltas and flush after vanilla inertia damp. Fakes that write angles immediately in the velocity API are forbidden as sole proof.

## Language and targets

Rewrite mod code targets the same Cities Mono constraints as shipping (net35-safe surfaces in the mod). Shared capture libraries may use newer TFMs for host tests. Prefer Mono-safe BCL in mod assemblies.

## Related

- [QA checklist](./qa-checklist.md)
- [Settings schema](./settings-schema.md)
- [Feature flags](./feature-flags.md)
