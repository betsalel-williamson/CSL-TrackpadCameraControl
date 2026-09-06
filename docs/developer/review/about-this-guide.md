# Review guide (historical)

**Audience:** Contributors reading closed architecture-review passes.

**Status:** Clone-era and under-the-hood review shards under this folder are **historical**. Path strings that say `rewrite/` describe the pre-cutover tree; the primary ship tree is now repo-root `mod/`, `src/`, and `tests/` (see [repository layout](../repository-layout.md)).

**Procedure for new reviews:** Use project skill **`system-architecture-review`**. That skill defines specialist lanes, the Strengths / Weaknesses / Critical template, import-matrix checks, and the rule that prototype clones are weaknesses — not strengths.

## Under-the-hood redesign (2026-09-03) — closed

- [Synthesis — Conditional](./uth-synthesis.md)
- [Architecture / Host](./uth-architecture-host.md)
- [Gesture library / Capture](./uth-gesture-library.md)
- [Policy / Apply](./uth-policy-apply.md)
- [Feel / Settings](./uth-feel-settings.md)
- [UI hosts](./uth-ui-hosts.md)
- [Tests / SA](./uth-tests-sa.md)

## Clone experiment (closed — historical)

Do not treat R7 in-game sign-off of the clone as architecture success. Findings remain context only.

- [Organized product feedback](./v1-product-feedback.md)
- [v1 audit and cleanup plan](./v1-audit-plan.md) (closed)
- Specialist clone-era shards (`architecture-audit.md`, `capture-audit.md`, …) describe the **quarantined clone**, not today’s ship tree.
