# Rewrite from UX contract, not source clone — Design

**Date:** 2026-09-03  
**Status:** Proposed (awaiting maintainer approval before deleting cloned sources)  
**Scope:** Recover the `rewrite/` tree from a failed clone-and-strip pass. Keep player-visible Options/Debug look-and-feel and Maps+ dynamics. Replace internals with a simpler model.

**MDCP work item**

| Field                | Value                                                                                                                                     |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| **WORK_ITEM**        | Address the rewrite failure: “1:1” was implemented as copying shipping C#, not as UX/interaction parity. Advise and lock a recovery plan. |
| **WORK_ITEM_LOOKUP** | [`docs/developer/work-item-tracking.md`](../../developer/work-item-tracking.md); rewrite contracts under `rewrite/docs/`                  |

Related durable contracts (updated with this work): rewrite features _Parity with shipping_, lesson L13, [ADR 0005](../../../rewrite/docs/features/adr/0005-ux-parity-not-source-parity.md).

## Diagnosis

The shipping mod is a working prototype that accumulated capture experiments (IPC, Contacts), dual gesture modules, ceremonial settings, and two independently grown Options/Debug UIs. The greenfield rewrite was supposed to keep **what the player sees and does**, and throw away **how the prototype happened to be coded**.

What shipped in `rewrite/mod` instead:

- Folder names that look like three planes, wrapping files that still match shipping types, sizes, and control-flow.
- ~12% fewer C# lines after deleting IPC/Contacts/CAD/legacy XML — not a new system.
- Options and Debug still two large, parallel ColossalUI builders (~4.6k lines in `Ui/` alone).
- Camera apply, selection context, QA dump, numeric-field helpers copied with light edits.
- “Refuse cleanup that changes Options/Debug order, labels, or feel math” (L11) was read as “do not rewrite those classes.”

That is a failing grade for a greenfield exercise. Availability of the prototype overpowered design. “1:1” was treated as genetic copy, not as an interface contract.

**What still counts as learning (keep as contracts and tests, not as types to extend):**

- Three-plane tick story (Capture → Policy → Apply).
- Style binding table as resolve source of truth; Maps+ as seed data.
- Feel ≠ gesture style; schema v1 without legacy migration.
- Standard logging to the game log; compile-omit unfinished modules.
- Golden Maps+ fixtures as behavior oracles.

**What does not count:** any rewrite file whose structure is the shipping file with a namespace change.

## Redefinition: what “1:1” means

| 1:1 **is**                                                                                   | 1:1 **is not**                                                                         |
| -------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------- |
| Same section order, labels, control kinds, grouping rhythm                                   | Same C# types, file splits, or method bodies                                           |
| Same feel-preset interactions (dropdown load, dirty → New Preset, Save as… / Delete / Reset) | Same `ModOptions` / `TuningPanelHost` implementation                                   |
| Same Sensitivity slider contract (0.1×–2×, three decimals, hot apply)                        | Same slider thumb workarounds copied line-for-line                                     |
| Same Maps+ chords and camera/selection outcomes                                              | Same hardcoded resolve beside a table, or a table that merely wraps the old heuristics |
| A player cannot tell the surfaces apart                                                      | `diff` against `mod/` is small                                                         |

Shipping `mod/` is a **black-box oracle**. Agents may **observe** it (play, screenshot, record labels, extract numeric constants into tests). They may **not** paste it into `rewrite/`.

Allowed citations from shipping (as numbers/strings in tests or seed tables, with a comment that they came from playtest/oracle):

- Feel factory defaults and Slow/Fast multipliers
- Maps+ chord seed (finger count, modifiers → op)
- Harmony patch target method names
- AppKit selector / event names required to talk to the OS

Forbidden: copying UI builders, settings stores, QA chrome, camera wrappers, or “we will clean it later” `#if` experiments.

## Approaches considered

### A — Keep editing the clone

Continue deleting leftover prototype bits inside the current `rewrite/mod` files.

- **Pro:** Shortest calendar path to an in-game binary that already looks like shipping.
- **Con:** The instinct that failed will keep winning. File-level identity with shipping will not dissolve by refactor. Extension cost stays high (two UIs, god objects, copied camera/selection).
- **Verdict:** Reject. This is how the intern exercise already failed.

### B — Quarantine the clone; rebuild from the UX contract (recommended)

Freeze current `rewrite/mod` as a failed experiment. Rebuild the assembly from:

1. The locked UX/interaction contract (this spec + _Parity with shipping_).
2. Lessons L1–L13 as architecture constraints.
3. Tests as oracles (tier A golden fixtures first).
4. Shipping only as a black-box for labels, layout rhythm, and dynamics.

Salvage **contracts and tests**, not cloned types. Re-implement Style table / ModLog / schema v1 from their shards if the files are entangled with cloned hosts.

- **Pro:** Matches the original rewrite intent. Forces a simpler model (one feel catalog, one editor API, thin Cities adapters). Makes future CAD/Contacts/Assist additive modules instead of more `#if` copies.
- **Con:** Temporary loss of a playable rewrite DLL until the new tree exists. Must not “save time” by copying UI files back in.
- **Verdict:** **Accept.** Only this addresses the fundamental failure.

### C — Hybrid: keep Policy/Apply clones, rewrite only UI

Delete `Ui/` and `ModOptions`, build a schema-driven feel catalog; leave `CameraApplicator` / selection / capture copies.

- **Pro:** Less work than B; UI was the largest clone mass.
- **Con:** Leaves the same camera/selection god objects and the same copy instinct for the next feature. The failure was not “UI files are big”; it was “prototype availability overpowered design.”
- **Verdict:** Reject as the recovery strategy. A later slice may _re-derive_ apply math from tests, but must not keep shipping types as the design.

## Target architecture (simple model)

End-user value is unchanged: trackpad camera fluency with hot feel tuning. Internals change.

```text
OS trackpad
  → Capture (one primitive frame)
  → Policy (gates + session + style table → op set)
  → Apply (pure feel math → camera/selection adapters)

Hot settings blob
  → Feel editor API (one)
  → Feel catalog (sections, labels, control kinds)
       → Options host (Colossal groups)
       → Debug host (floating panel chrome)
```

### Units (each has one job)

| Unit                      | Does                                                                                                    | Does not                  |
| ------------------------- | ------------------------------------------------------------------------------------------------------- | ------------------------- |
| **Feel catalog**          | Ordered sections and fields: id, player label, control kind (slider / dropdown / button), value mapping | Colossal or Debug drawing |
| **Feel editor**           | Preset load/dirty/Save as…/Delete/Reset; Sensitivity writes; one dirty bit; coalesced autosave          | Widget layout             |
| **Options host**          | Map catalog → native Options groups                                                                     | Duplicate preset state    |
| **Debug host**            | Map catalog → in-game panel chrome (drag, opacity, close, gear)                                         | Duplicate field list      |
| **Style table + resolve** | Chord → op from seed rows                                                                               | Feel numbers; UI          |
| **Feel math (pure)**      | Op + feel → camera/selection deltas; tested without Unity                                               | GameObject / Harmony      |
| **Cities adapters**       | Read/write `CameraController`, selection ghost, input gates                                             | Policy decisions          |
| **Capture backend**       | Fill the primitive frame from AppKit                                                                    | Pan/zoom/orbit decisions  |
| **Harmony**               | Precise-trackpad scroll suppress; deferred orbit flush                                                  | UI, settings, capture     |

Debug and Options **look** different (panel vs Options page) and **interact** the same on shared feel fields. That is one catalog, two skins — not two copies of the product.

QA system-info / clipboard dumps are **not** on the v1 ship surface unless a later work item proves they are required for Workshop support. They must not ride along because the prototype had them.

### Error handling and lifecycle

Unchanged from system architecture: fail soft if capture or Harmony is missing; re-query focus/menu/selection each tick; compile-omit unfinished modules (no stubs on the tick path).

### Testing

| Tier  | Oracle                                                                                                                                                                                  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **A** | Golden Maps+ frames through resolve + **pure** apply (no Unity). Catalog tests: section order and labels match the UX contract table. Preset dirty/Save as…/Delete model as unit tests. |
| **B** | Capture-session coverage per primitive, including honest finger count.                                                                                                                  |
| **C** | In-game **visual and interaction** A/B vs shipping (checklist). Not a code-review against `mod/`.                                                                                       |

A rewrite file that is a near-copy of a shipping file is a **test failure of the design**, even if behavior matches.

## Recovery plan (execute only after this spec is approved)

Do not start from “open `TuningPanelHost.cs` and tidy it.” Start from the contract, then empty sources, then TDD.

### Phase 0 — Stop the clone path (docs + process)

Already started by this spec and ADR 0005.

- Treat current `rewrite/mod` C# as **quarantined reference**, not a base to extend.
- Close the clone-and-strip audit (R7 in-game sign-off of the clone is **not** the success path).
- Agent rule: if a change would be easier by copying a shipping file, stop and design the unit instead.

**Gate:** Maintainer agrees this spec’s diagnosis and chooses approach B.

### Phase 1 — Lock the UX inventory (docs, no product C#)

Extract a **feel surface inventory** into rewrite client/feature shards (labels, section order, control kinds, preset state machine). Source the inventory from **playing shipping** and from _Settings and hot configuration_ / _Parity with shipping_ — not from shipping method names.

Same pass: Maps+ dynamics inventory already in _Parity with shipping_ stays the gesture oracle.

**Gate:** `npm run docs:rewrite`. A new contributor can build Options/Debug from the inventory without opening `mod/*.cs`.

### Phase 2 — Oracle tests first

Move or rewrite tests so they assert:

- Style-table Maps+ seeds (keep existing golden fixtures if they hit the table, not cloned helpers).
- Pure feel math (numeric outcomes).
- Preset dirty model.
- Catalog order/labels.

Delete tests that only lock cloned implementation details.

**Gate:** `dotnet test` on rewrite tests is green on **new** units even before Cities UI exists (hosts faked).

### Phase 3 — Replace the assembly (empty the clone)

Delete quarantined cloned sources. Scaffold Host + empty planes + schema + catalog + editor. The rewrite DLL may not be playable for a stretch; shipping `mod/` remains the playable product.

**Gate:** The tree compiles; no file remains whose body is a shipping clone. `diff`-similarity against `mod/` is high on purpose.

### Phase 4 — Schema-driven feel UI

Implement catalog + editor + Options host + Debug host. Match inventory labels and interactions. No second field list.

**Gate:** Headless catalog tests pass. Tier C UI rows on the in-game checklist are the human gate later.

### Phase 5 — Policy and apply from contracts

Re-implement resolve from ADR 0004; re-implement apply as pure functions + thin adapters. Selection-aware rotate is policy + adapter, not a 500-line Cities dump unless that dump is the only legal way to query the game — even then, isolate queries behind a small port.

**Gate:** Tier A fixtures pass on the new apply.

### Phase 6 — Capture and Harmony from contracts

AppKit backend fills the primitive frame. Harmony is the two documented patches only. Inject/E2E remains a **test seam**, not a second capture product.

**Gate:** Tier B session tests; then maintainer tier C dynamics A/B.

### Phase 7 — Cutover readiness

Only after tier C **visual/interaction** sign-off: install script, Workshop folder name, root docs cutover. Until then shipping `mod/` stays as-built.

## Process gates (so the failure cannot recur)

1. **Read L11 + L13 + ADR 0005** before any rewrite C# change.
2. **No paste from `mod/` into `rewrite/`.** Reviewers reject PRs that are namespace-renamed shipping files.
3. **One unit, one job.** A file that both draws ColossalUI and owns preset persistence is out of bounds.
4. **Every settings field names a tick or editor consumer** (L1). No ceremonial knobs.
5. **Unfinished experiments stay out of the ship DLL** (L9). Do not copy them “for later.”
6. **LOC is a heuristic, not a trophy.** Prefer fewer concepts (one catalog, one editor, one resolve table) over a 12% line shave on a clone.

## Non-goals (this recovery)

- Changing player-visible Options/Debug order, labels, or Maps+ chords.
- Shipping CAD, Contacts, or Assist on v1.
- Rewriting the **shipping** `mod/` in place (cutover later).
- In-game playtest of the **cloned** rewrite DLL as proof the architecture succeeded.

## Approval

Maintainer: confirm approach **B** and Phase 0–1 docs. Do not implement Phase 3 deletion until that approval is explicit.
