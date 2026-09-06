---
name: system-architecture-review
description: >-
  Use when asked for a system review, architecture audit, redesign review,
  under-the-hood review, layer/import boundary review, or a detailed specialist
  pass over a multi-layer codebase (mod vs library vs OS) after a major rewrite
  or before calling architecture complete.
---

# System architecture review

## Overview

A **system architecture review** judges whether the as-built tree matches the intended **stack layers**, **non-overlapping imports**, **single-responsibility units**, and **player/contract parity** — not whether files merely exist or look like a prior prototype.

**Core principle:** Review the **machine** (layers, imports, seams, tests/fakes). Do not treat source similarity to a prototype as a strength.

## When to use

- After a redesign / under-the-hood rebuild
- When the user asks for a detailed system or architecture review (like a prior specialist audit)
- Before claiming greenfield architecture is done
- When import/DLL boundaries or fake-per-layer tests are part of the contract

**Not for:** single-bug PR review, typo fixes, or pure docs-only wording passes (use normal code review / MDCP helpers instead).

## Baseline failures (why this skill exists)

Without this procedure, agents previously:

| Failure | Harm |
| ------- | ---- |
| Praised “structural parity with shipping” / copied UI builders | Treated clone as success |
| Equated LOC −12% with a simpler system | Missed concept duplication |
| Skipped DLL/using matrix | Layers leaked OS ↔ Cities ↔ Feel |
| One monolith review | Missed capture vs policy vs UI vs tests |
| Called “1:1” source identity | Misread UX parity |

## Procedure (required)

```text
1. Load contracts (docs) before code
2. Confirm stack layers + import matrix
3. Split specialist lanes (parallel OK)
4. Each lane: Strengths / Weaknesses / Critical improvements
5. Write MDCP review shards (durable findings)
6. Synthesis: pass / conditional / fail + prioritized P0–P2
7. Do not implement fixes in the same turn unless the user asks
```

### Step 1 — Contracts first

Read (as applicable):

- Features *Under the hood*, *Parity with shipping*, lessons L1–L13
- ADR 0005 (UX ≠ source), ADR 0006 (library vs mod)
- Developer harnesses (fake-per-layer) and static analysis (layer-import lint)

**Gate:** If contracts and code disagree, the review finding is “docs/code drift,” not “code is fine.”

### Step 2 — Stack + import matrix

For each layer, list **allowed** vs **forbidden** references. Fail the layer if usings/csproj violate the matrix.

| Layer | Must not import |
| ----- | --------------- |
| Native / AppKit backend | UnityEngine, ICities, Colossal, Harmony, Feel/Policy |
| Gesture library core | Cities, Feel UI; OS P/Invoke mixed into Unity types |
| CSL pure Policy / FeelMath / catalog / editor | UnityEngine, ICities, AppKit, Harmony |
| CSL Host / adapters / UI hosts | AppKit P/Invoke |

Run automated lint when present (`sa:rewrite` layer-import). Manual spot-check largest files.

### Step 3 — Specialist lanes

Use **one shard (or one subagent) per lane**. Parallelize.

| Lane | Scope |
| ---- | ----- |
| **Architecture / Host** | Lifecycle, tick pipeline, plane wiring, Harmony scope |
| **Gesture library / Capture** | Frame contract, backends, inject seam, OS-only fakes |
| **Policy / Apply** | Style table, session, FeelMath purity, Cities adapters |
| **Feel / Settings** | Catalog, editor, store, schema consumers, dirty model |
| **UI hosts** | Options/Debug share catalog; no second product; no QA dump ride-along |
| **Tests / SA** | Fake-per-layer, goldens, integration, layer-import lint |

### Step 4 — Shard template (every lane)

```markdown
# <Lane> audit (<target>)

**As-built date:** YYYY-MM-DD
**Scope:** …
**Contracts:** link or name shards/ADRs

## Strengths
- …

## Weaknesses
- … (cite file paths)

## Critical improvements (prioritized)
### P0 — …
### P1 — …
### P2 — …

## Verdict
Pass | Conditional | Fail — one sentence.
```

**Praise rule:** Only call something a strength if it advances the **target** architecture. Copied prototype structure is a **weakness**, even if behavior matches.

### Step 5 — Synthesis

Write an index/synthesis shard:

- Overall verdict
- P0 list (must fix before “architecture done”)
- What to keep
- Explicit non-goals (e.g. in-game tier C still human)

### Step 6 — Delivery

- MDCP shards under the project’s review guide (e.g. `rewrite/docs/developer/review/`)
- Update review guide index; run docs check
- Commit/push when in a delivery branch
- Summary to the user: verdict + top P0/P1 only (details in shards)

## Red flags — STOP

- “Looks 1:1 with shipping sources” as praise
- Reviewing only `diff` size or LOC
- One fake standing in for OS + Unity + game
- Skipping import matrix because “tests pass”
- Implementing a rewrite mid-review without being asked
- Leaving findings only in chat (no durable shard)

## Rationalizations

| Excuse | Reality |
| ------ | ------- |
| “Tests pass so architecture is fine” | Tests can pass on a clone; check layers and imports |
| “UI must look the same so keep the same classes” | UX parity ≠ source identity (ADR 0005) |
| “We’ll tidy imports later” | Overlapping DLLs are the failure mode — report now |
| “One big review is faster” | Specialist lanes catch different defects |
| “Thin hosts are fine forever” | Note as Conditional if catalog exists but chrome is stub |

## Related

- Superpowers `requesting-code-review` for task-level PR review
- MDCP parent skill for shard placement and docs check
