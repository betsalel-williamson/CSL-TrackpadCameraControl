# GitHub project controls — Design

**Date:** 2026-08-29  
**Status:** Approved  
**Scope:** Declarative repo controls for `betsalel-williamson/CSL-TrackpadCameraControl` via OpenTofu + a Makefile; stacked PR tooling verification

## Goal

Encode durable GitHub project controls so a maintainer can run one idempotent command anytime: initialize if needed, detect drift from desired state, and (optionally) converge — without recreating the repository or requiring careful “did I already set this?” bookkeeping.

## Decisions

| Concern                     | Choice                                                                            | Rationale                                                                |
| --------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| Approach                    | Hybrid: OpenTofu owns durable settings; Makefile is the only operator entrypoint  | IaC for drift; Make for init/auth/status without extra shell scripts     |
| Reviews                     | No required approvals (`required_approving_review_count = 0`)                     | Solo / self-merge                                                        |
| Merge methods               | Squash only (repo + ruleset); disable merge commit and rebase                     | Linear history; matches Conventional Commits + Changesets                |
| Up to date with `main`      | `strict_required_status_checks_policy = true` + `allow_update_branch = true`      | PR must include latest `main` before merge; UI “Update branch” available |
| Required checks             | `Commitlint`, `Validate` (CI job names)                                           | Existing `.github/workflows/ci.yml` PR jobs                              |
| Branch protection mechanism | Repository **ruleset** on `~DEFAULT_BRANCH`, not classic branch protection        | Current GitHub preference; supports merge-method allowlist               |
| Force push                  | `non_fast_forward = true` on the ruleset                                          | Protect `main` history                                                   |
| Actions → create PRs        | `can_approve_pull_request_reviews = true`; `default_workflow_permissions = write` | Unblocks `changesets/action` Release version PRs                         |
| Stacked PRs                 | No repo toggle; verify Stacks API + ensure `gh-stack` extension                   | Feature is platform preview; this repo already returns stacks list       |
| State                       | Local `infra/github/*.tfstate` (gitignored)                                       | Solo maintainer; no remote backend in v1                                 |
| Auth                        | `GH_TOKEN` from `gh auth token` in Makefile                                       | Reuse existing `gh` login; fail fast if missing admin                    |

## Layout

```text
infra/github/
  Makefile              # check (default), apply, status, import helpers
  versions.tf
  providers.tf
  variables.tf
  main.tf
  outputs.tf
  README.md             # points at Make targets
docs/developer/
  github-project-controls.md
```

Update `docs/developer/repository-layout.md` to mention `infra/github/`.

Gitignore OpenTofu artifacts: `.terraform/`, `*.tfstate`, `*.tfstate.*`, `.terraform.lock.hcl` may be **committed** (provider pins) — lockfile committed, state not.

## OpenTofu resources

1. **`github_repository` (imported)** — manage merge-related attributes only; `lifecycle { prevent_destroy = true }`. Desired:
   - `allow_squash_merge = true`
   - `allow_merge_commit = false`
   - `allow_rebase_merge = false`
   - `allow_update_branch = true`
   - `delete_branch_on_merge = true`
   - `squash_merge_commit_title = "PR_TITLE"`
   - `squash_merge_commit_message = "PR_BODY"`
2. **`github_repository_ruleset` (created)** — name e.g. `main-protection`; `target = "branch"`; `enforcement = "active"`; conditions `ref_name.include = ["~DEFAULT_BRANCH"]`; rules:
   - `pull_request` with `allowed_merge_methods = ["squash"]`, `required_approving_review_count = 0`
   - `required_status_checks` with contexts `Commitlint` and `Validate`, `strict_required_status_checks_policy = true`
   - `non_fast_forward = true`
3. **`github_workflow_repository_permissions`** — `default_workflow_permissions = "write"`, `can_approve_pull_request_reviews = true`

Variables: `github_owner`, `github_repository` (defaults for this repo), optional override for check context names.

## Makefile operator model (idempotent)

**Default: `make check`**

1. Preflight: `tofu`, `gh` on PATH; `gh auth status`; export `GH_TOKEN=$(gh auth token)`.
2. `tofu init` when providers/plugins missing (always safe to re-run).
3. If state has no repository / workflow-permissions resources: auto-import existing objects (skip if already present).
4. `tofu plan -detailed-exitcode`:
   - exit **0** — in sync; print OK
   - exit **2** — drift; print summary; tell operator to run `make apply`; Make exits 2
   - exit **1** — error
5. Read-only side checks: `GET /repos/{owner}/{repo}/stacks` succeeds; install `github/gh-stack` only if missing.

**`make apply`** — same preflight/init/import; then `tofu apply` (interactive confirm unless `APPLY_AUTO=1`). Re-runnable; second apply is a no-op when converged.

**`make status`** — raw `gh api` snapshot of merge settings, rulesets, Actions workflow permissions, stacks (no OpenTofu).

**Invariant:** Never destroy the GitHub repository. Imports and applies only adjust settings and rulesets.

Everyday loop: `make check` → if exit 2, review → `make apply` → `make check`.

## Stacked PRs

- Document using `gh stack` / GitHub UI for layered PRs targeting the branch below.
- Makefile `check`/`status` only verify availability; they do not create stacks.
- Existing branch protections and required checks still govern what reaches `main`.

## Docs

`docs/developer/github-project-controls.md` covers: policy summary, Make targets, how Release depends on Actions create-PR permission, pointer to stacks docs / `gh-stack`.

Cross-link from `commits-and-releases.md` (Release PR creation) and `repository-layout.md`.

## Out of scope

- Required PR approvals / code owners
- Merge queue
- Remote OpenTofu state backend
- Managing `NPM_TOKEN` or other secrets
- Classic branch protection API
- Org-level Actions / ruleset policies
- Automating stack creation for feature work

## References

- [GitHub repository rulesets (Terraform)](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/repository_ruleset)
- [github_workflow_repository_permissions](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/workflow_repository_permissions)
- [Stacked pull requests public preview](https://github.blog/changelog/2026-07-30-stacked-pull-requests-are-now-in-public-preview/)
- Existing CI job names in `.github/workflows/ci.yml` (`Commitlint`, `Validate`)
