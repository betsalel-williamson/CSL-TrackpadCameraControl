# GitHub project controls Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans or superpowers:subagent-driven-development. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Land idempotent OpenTofu + Makefile GitHub controls, harden Release publish, and fail-closed CI Validate per the approved design.

**Architecture:** `infra/github/` declares repo merge policy, main ruleset, Actions workflow permissions, collaborators, and `npm-publish` environment. Makefile wraps init/import/plan/apply. Release splits version (no npm auth) vs publish (environment + OIDC). `ci-validate.sh` fails closed and scopes `infra/**`.

**Tech Stack:** OpenTofu, integrations/github provider, Make, `gh`, GitHub Actions, Changesets action v2

## Global Constraints

- Solo maintainer: no required PR approvals; fork-based contributors only
- Squash-only; strict up-to-date; required checks `Commitlint`, `Validate`
- Actions default `read`; `can_approve_pull_request_reviews = true`
- Never destroy the GitHub repository
- Idempotent `make check` / `make apply`

---

### Task 1: OpenTofu stack + Makefile

**Files:**

- Create: `infra/github/versions.tf`, `providers.tf`, `variables.tf`, `main.tf`, `outputs.tf`, `Makefile`, `README.md`
- Modify: `.gitignore`

- [ ] **Step 1:** Add OpenTofu gitignore entries and full `infra/github/` config + Makefile (`check`/`apply`/`status` with auto-import)
- [ ] **Step 2:** `tofu init` and verify plan runs (may show create/update before apply)
- [ ] **Step 3:** Commit

### Task 2: CI Validate fail-closed

**Files:**

- Modify: `scripts/ci-validate.sh`

- [ ] **Step 1:** Add `infra/*` to tooling/full paths; fail closed when no scopes on non-main PRs
- [ ] **Step 2:** Commit

### Task 3: Release workflow split

**Files:**

- Modify: `.github/workflows/release.yml`

- [ ] **Step 1:** Split `version` (no env/npm) and `publish` (`environment: npm-publish`, OIDC + optional env `NPM_TOKEN`)
- [ ] **Step 2:** Commit

### Task 4: Developer docs

**Files:**

- Create: `docs/developer/github-project-controls.md`
- Modify: `docs/developer/repository-layout.md`, `commits-and-releases.md`, `contributor-setup.md`, `docs/developer/index.md` if present

- [ ] **Step 1:** Write docs + cross-links
- [ ] **Step 2:** Commit

### Task 5: Apply controls

- [ ] **Step 1:** `make apply` (or plan + apply) against live repo
- [ ] **Step 2:** `make check` exits 0
- [ ] **Step 3:** Commit any lockfile / leftover fixes
