# Parity with shipping

## Intent

Define what **must** match the shipping mod end-to-end, and what the clean-architecture rewrite **may** change. Internal elegance never excuses player-visible drift ([greenfield redesign lessons](./greenfield-redesign-lessons.md) L11).

## Must match (definition of done)

[UI parity](../glossary/ui-parity.md) is **look and interaction**, not copied C#. Internals must differ ([ADR 0005](./adr/0005-ux-parity-not-source-parity.md), lesson L13).

### UI 1:1

- Options → Trackpad Camera Control and in-game Debug panel: **same section order** (General → Zoom → Pan → Rotate → Orbit), same control kinds, same labels and grouping rhythm.
- Same feel preset model (Slow / Default / Fast / New Preset / named; Save as… / Delete / Reset).
- Same Sensitivity slider contract (0.1×–2× factory, three decimals).
- No Enable-per-op or Reverse on the product surface.
- No pitch min/max on Options/Debug (apply constant only).
- Ship surface omits CAD switcher, Contacts picker, low-pass, and Assist chrome fields when those modules are compiled off.
- Options and Debug share **one** field inventory and **one** editor API; they are two hosts, not two product definitions.

### Gesture and dynamics parity (Maps+)

- Two-finger pan, pinch zoom, two-finger rotate, Option (`⌥`)+two-finger orbit — same chords and outcomes as shipping.
- Orbit latch, rotate-owned contact, Concurrent default, hard handoff of orbit coast into rotate.
- Orbit from current look-at (no Target re-home); pitch **0°–90°**; no yaw angle clamp; pan clamped to unlocked game area.
- Selection-aware place/relocate ghost rotate vs camera yaw — same as [selection-aware gestures](./selection-aware-gestures.md).
- Precise-trackpad pan without vanilla zoom; mouse wheel zooms; middle-mouse orbit remains; menu/popup gates — same as [vanilla camera suppress](./vanilla-camera-suppress.md).
- Hot feel apply and persist behavior — same player-visible results as [settings and hot configuration](./settings-and-hot-configuration.md).

### Proof tiers (lesson L10)

| Tier | Proof                                                                 |
| ---- | --------------------------------------------------------------------- |
| A    | Golden Maps+ fixtures through style-table resolve + Apply             |
| B    | Capture-session coverage per primitive (honest finger count included) |
| C    | In-game UI + dynamics A/B against shipping                            |

## May differ

- Assembly / mod folder name under the rewrite tree.
- Internal plane split (Capture / Policy / Apply), type names, and folder layout — **must** differ from shipping sources (L13). Same player result with the same classes is a failed rewrite.
- Style chords implemented as a **seeded binding table** consumed by resolve ([ADR 0004](./adr/0004-style-table-driven-resolve.md)) instead of shipping’s hardcoded Maps+ heuristics — **player-visible chords must still match**.
- Feel UI implemented as a catalog + two hosts instead of parallel Options/Debug builders.
- Compile-time omission of unfinished modules (no stub objects on the tick path) vs shipping’s unused flagged paths.
- One dirty bit / one flush autosave path (same durable outcome; no double XML write).
- Removal of ceremonial fields that had no tick consumer (e.g. pitch in the feel blob).
- Omission of prototype QA dumps from the v1 ship surface unless a later work item requires them.

## Must not copy

- Shipping UI builders, settings stores, numeric-field stacks, camera/selection dumps, or QA chrome into `rewrite/` (including copy-then-tidy).
- Unfinished experiments (IPC, Contacts, CAD, Assist, file loggers, legacy XML schemas) as souvenirs.
- Shipping `mod/` as the implementation template. It is an oracle for labels, layout rhythm, constants, and dynamics only.

## Must not claim

- Remappable gestures on ship without a table consumer and a compiled style surface.
- Tunable orbit pitch on Options/Debug.
- Runtime feature-flag mirrors of `#if` modules.
- Contacts low-pass or Assist chrome on the default ship DLL.
- Finger-count-dependent styles on a backend that cannot emit honest counts ([platform backends](./platform-backends.md)).

## Acceptance

- A player moving from shipping to rewrite cannot tell the Maps+ gesture or Options/Debug feel surface apart, aside from any documented assembly/folder rename.
- A maintainer opening rewrite sources **can** tell them apart from shipping: fewer concepts, one catalog, thin adapters (lessons L1–L13).
- Tier A fixtures pass; tier C A/B signs off **visual and interaction** parity and dynamics — not source similarity.
- Diff against shipping docs is intentional target cleanup, not silent capability loss.
