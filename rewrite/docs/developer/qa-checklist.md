# QA checklist (rewrite parity)

Side-by-side pass/fail matrix: shipping `TrackpadCameraControl` vs rewrite `TrackpadCameraControl.Rewrite`. Unit and fixture tiers cannot prove Harmony timing or hardware Option+drag — run this in Cities: Skylines after [Local MVP install](./local-mvp-install.md).

**Session defaults:** Cities Harmony on; **only one** camera mod enabled; city loaded; game focused; Maps+ seeds; Default feel.

Fill shipping and rewrite columns on the same machine / OS when possible. Player-visible UI must stay at [UI parity](../glossary/ui-parity.md) and Maps+ dynamics must match (L11). Source identity with shipping is not a pass (L13).

## Session platform

| Field                 | Value                             |
| --------------------- | --------------------------------- |
| macOS version         |                                   |
| Mac model             |                                   |
| Trackpad              | Built-in / Magic Trackpad / other |
| Chip                  | Apple silicon / Intel             |
| Shipping commit / asm |                                   |
| Rewrite commit / asm  |                                   |
| Result                | Pass / Fail / Partial             |
| Notes                 |                                   |

## Parity matrix

Check each row for **shipping** and **rewrite**. Fail the row if either side regresses or they diverge.

### Maps+ chords

| Check                                                                                        | Shipping | Rewrite |
| -------------------------------------------------------------------------------------------- | -------- | ------- |
| Two-finger drag pans; does not also vanilla-zoom                                             | ☐        | ☐       |
| Pinch zooms                                                                                  | ☐        | ☐       |
| Two-finger twist rotates heading (no pitch); hard-handoffs leftover orbit coast              | ☐        | ☐       |
| Option (`⌥`)+two-finger drag orbits (yaw and pitch); twist ignored while Option owns contact | ☐        | ☐       |
| Gestures respond within ~5 s of city load (Debug off)                                        | ☐        | ☐       |
| Mouse wheel still vanilla-zooms                                                              | ☐        | ☐       |

### Gates

| Check                                                                           | Shipping | Rewrite |
| ------------------------------------------------------------------------------- | -------- | ------- |
| Options open: two-finger scrolls Options; city does not pan/orbit from the mod  | ☐        | ☐       |
| Pointer over Debug / popup: two-finger scrolls UI, not city camera              | ☐        | ☐       |
| RequireGameFocus: unfocused game does not apply mod camera                      | ☐        | ☐       |
| Debug gear opens Options on Trackpad Camera Control; Options stacks above Debug | ☐        | ☐       |

### Feel

| Check                                                                            | Shipping | Rewrite |
| -------------------------------------------------------------------------------- | -------- | ------- |
| Options Sensitivity order / labels match (General → Zoom → Pan → Rotate → Orbit) | ☐        | ☐       |
| Debug panel field order and labels match shipping                                | ☐        | ☐       |
| Slow / Default / Fast gains match; dirty edits → New Preset                      | ☐        | ☐       |
| Sensitivity hot-applies and autosaves; Options and Debug stay in sync            | ☐        | ☐       |
| No Pitch min/max controls; orbit pitch stops at vanilla **0°–90°**               | ☐        | ☐       |
| Reset to factory restores Default feel; panel position preserved                 | ☐        | ☐       |

### Orbit latch

| Check                                                                              | Shipping | Rewrite |
| ---------------------------------------------------------------------------------- | -------- | ------- |
| Release Option while fingers still down: orbit latches until lift                  | ☐        | ☐       |
| Lift after Option-orbit: short coast then stop (middle-click-like), not a teleport | ☐        | ☐       |
| While latched, pan / zoom / rotate do not steal the contact                        | ☐        | ☐       |

### Vanilla suppress

| Check                                                                                      | Shipping | Rewrite |
| ------------------------------------------------------------------------------------------ | -------- | ------- |
| Precise trackpad pan without vanilla scroll-zoom                                           | ☐        | ☐       |
| Middle-click drag orbit still vanilla while mod on                                         | ☐        | ☐       |
| Edge pan / keyboard camera still work                                                      | ☐        | ☐       |
| Disable the active mod: trackpad pan fights or vanilla-zooms again; middle-click unchanged | ☐        | ☐       |

### Selection rotate

| Check                                                                                | Shipping | Rewrite |
| ------------------------------------------------------------------------------------ | -------- | ------- |
| Place / relocate ghost: two-finger twist rotates **ghost**, not camera yaw           | ☐        | ☐       |
| Click-selected building only: two-finger twist yaws **camera** (no object spin)      | ☐        | ☐       |
| Option-orbit never re-homes look-at to selection / ghost / Relocate-click / old cell | ☐        | ☐       |
| Relocate → pan away → Option-orbit keeps current look-at                             | ☐        | ☐       |
| Escape cancel after relocate twist does not leave the old-cell building spun         | ☐        | ☐       |

## Setup reminders

- [ ] Only one of shipping / rewrite enabled (see [Local MVP install](./local-mvp-install.md))
- [ ] Cities Harmony enabled
- [ ] City loaded (not menus-only)
- [ ] Session platform row filled
- [ ] Cold-boot dual-cursor / Steam overlay quirks noted as external if seen — not a rewrite blocker by itself

## Optional flag builds (not ship parity)

Run only when deliberately compiling experimental modules ([Feature flags](./feature-flags.md)). Do not count Contacts builds as ship evidence.

| Check                                                                           | Notes      |
| ------------------------------------------------------------------------------- | ---------- |
| `EnableAssistChrome`: pads/buttons drive shared apply                           | Future     |
| `EnableCadGestureStyle`: three-finger orbit only if capture emits honest counts | Future; L4 |
| `EnableContactsCapture`: dedicated plan required                                | Unfinished |

## Related

- [Harnesses and testing](./harnesses-and-testing.md)
- [Settings schema](./settings-schema.md)
- Features guide north star: greenfield redesign lessons (L1–L13)
