# Rewrite v1 audit (review guide)

**Audience:** Contributors reconciling why the first rewrite implementation failed and what contracts replace it.

**Purpose:** Record organized product feedback and specialist findings from the **clone-and-strip experiment**. That path is **closed**. Do not treat R7 in-game sign-off of the clone as architecture success.

**Current path:** Features ADR 0005 and lesson L13 — rebuild from [UI parity](../../glossary/ui-parity.md) and Maps+ dynamics, not from shipping C#. Recovery design lives in the session spec _Rewrite from UX contract, not source clone_ under repo-root `docs/superpowers/specs/`.

**Workflow (historical):**

1. Read [organized product feedback](./v1-product-feedback.md) (user scope vs prototype carryover).
2. The [v1 audit and cleanup plan](./v1-audit-plan.md) is closed after R6; do not continue it.
3. Specialist shards remain as findings (capture, settings, UI, release, tests).
