# GitHub project controls — Design

**Date:** 2026-08-29  
**Status:** Approved (revised for OSS single-maintainer access model)  
**Scope:** Declarative repo controls for `betsalel-williamson/CSL-TrackpadCameraControl` via OpenTofu + a Makefile; stacked PR tooling verification

## Goal

Encode durable GitHub project controls so a maintainer can run one idempotent command anytime: initialize if needed, detect drift from desired state, and (optionally) converge — without recreating the repository or requiring careful “did I already set this?” bookkeeping.

**Maintainer-only merge (OSS, single maintainer):** Do **not** use required PR approvals (incompatible with solo self-merge). Rely on GitHub’s **contributor / fork model**: outside contributors open PRs from forks and never receive repository **Write**. Only the maintainer (repo owner / trusted Write) can merge to `main`. Accidental Write collaborators are treated as control drift.

## Decisions

| Concern                | Choice                                                                                        | Rationale                                                                                                              |
| ---------------------- | --------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Approach               | Hybrid: OpenTofu owns durable settings; Makefile is the only operator entrypoint              | IaC for drift; Make for init/auth/status                                                                               |
| Who can merge          | **Access model**, not reviews: no Write for outside contributors; fork-based PRs only         | Solo maintainer can self-merge; strangers cannot                                                                       |
| Collaborator inventory | Authoritative empty (or owner-only) collaborator set + `make check` alert on unexpected Write | Personal repos: adding a collaborator ≈ granting Write                                                                 |
| Reviews                | `required_approving_review_count = 0`                                                         | Required; second reviewer unavailable                                                                                  |
| Merge methods          | Squash only (repo + ruleset); disable merge commit and rebase                                 | Linear history; Conventional Commits + Changesets                                                                      |
| Up to date with `main` | `strict_required_status_checks_policy = true` + `allow_update_branch = true`                  | PR must include latest `main` before merge                                                                             |
| Required checks        | `Commitlint`, `Validate`                                                                      | Existing CI job names                                                                                                  |
| Validate as a gate     | Fail closed on PRs when no path scopes match; include `infra/**` in scopes                    | Prevent “green” merges that skipped all gates                                                                          |
| Branch protection      | Repository **ruleset** on `~DEFAULT_BRANCH`                                                   | Supports merge-method allowlist                                                                                        |
| Force push             | `non_fast_forward = true`                                                                     | Protect `main` history                                                                                                 |
| Actions defaults       | `default_workflow_permissions = "read"`; `can_approve_pull_request_reviews = true`            | Least privilege by default; still allow Changesets to **create** version PRs (Release job elevates via `permissions:`) |
| Stacked PRs            | Verify Stacks API + ensure `gh-stack` extension                                               | Platform preview; no repo toggle                                                                                       |
| State                  | Local `infra/github/*.tfstate` (gitignored)                                                   | Solo maintainer; no remote backend in v1                                                                               |
| Auth                   | `GH_TOKEN` from `gh auth token` in Makefile                                                   | Fail fast if missing admin                                                                                             |
| npm publish secrets    | Document environment / Trusted Publishing as follow-up; not required for v1 IaC               | Merge≈publish remains a residual risk until env gate lands                                                             |

## Access model (contributor check)

```text
Outside contributor  →  fork + PR  →  no repo Write  →  cannot merge
Maintainer (owner)   →  merge squash to main after Commitlint + Validate (strict)
```

- Do **not** add casual collaborators on this personal repository (GitHub only offers Write/`push` for non-owners).
- Document in `docs/developer/github-project-controls.md` and contributor setup: use forks.
- OpenTofu: `github_repository_collaborators` asserts the intended set (empty of non-owner users, or explicitly listed maintainers). `make check` / `make status` fail or warn if live collaborators diverge.

## Layout

```text
infra/github/
  Makefile              # check (default), apply, status, import helpers
  versions.tf
  providers.tf
  variables.tf
  main.tf
  outputs.tf
  README.md
docs/developer/
  github-project-controls.md
```

Update `docs/developer/repository-layout.md` to mention `infra/github/`.

Gitignore: `.terraform/`, `*.tfstate`, `*.tfstate.*`. Commit `.terraform.lock.hcl`.

## OpenTofu resources

1. **`github_repository` (imported)** — merge-related attrs only; `lifecycle { prevent_destroy = true }`:
   - `allow_squash_merge = true`
   - `allow_merge_commit = false`
   - `allow_rebase_merge = false`
   - `allow_update_branch = true`
   - `delete_branch_on_merge = true`
   - `squash_merge_commit_title = "PR_TITLE"`
   - `squash_merge_commit_message = "PR_BODY"`
2. **`github_repository_ruleset`** — `main-protection`; `~DEFAULT_BRANCH`; `enforcement = active`:
   - `pull_request`: `allowed_merge_methods = ["squash"]`, `required_approving_review_count = 0`
   - `required_status_checks`: `Commitlint`, `Validate`; `strict_required_status_checks_policy = true`
   - `non_fast_forward = true`
3. **`github_workflow_repository_permissions`** — `default_workflow_permissions = "read"`, `can_approve_pull_request_reviews = true`
4. **`github_repository_collaborators`** — authoritative intended collaborators (no unexpected Write)

Variables: `github_owner`, `github_repository`, check context names, optional `maintainer_usernames` list.

## CI companion change (same effort)

`scripts/ci-validate.sh` (and docs): treat zero matched scopes on `pull_request` as failure; add `infra/**` to path scopes so IaC edits always run a gate.

## Makefile operator model (idempotent)

**Default: `make check`**

1. Preflight: `tofu`, `gh`; `gh auth status`; `GH_TOKEN=$(gh auth token)`.
2. `tofu init` when needed (safe to re-run).
3. Auto-import missing state objects (repo, workflow permissions); skip if present.
4. `tofu plan -detailed-exitcode` → 0 in sync / 2 drift (tell operator `make apply`) / 1 error.
5. Side checks: Stacks API OK; `gh-stack` installed if missing; collaborator list matches intended set.

**`make apply`** — same preflight; `tofu apply` (`APPLY_AUTO=1` for non-interactive). Idempotent.

**`make status`** — raw `gh api` snapshot (merge, rulesets, Actions perms, collaborators, stacks).

**Invariant:** Never destroy the GitHub repository.

Loop: `make check` → drift → `make apply` → `make check`.

## Stacked PRs

Document `gh stack` / UI; Makefile only verifies availability. Branch rules still govern `main`.

## Docs

`docs/developer/github-project-controls.md`: access model (forks vs Write), Make targets, Actions create-PR permission, stacks, residual publish risk.

Cross-link from `commits-and-releases.md`, `contributor-setup.md`, `repository-layout.md`.

## Out of scope (v1)

- Required PR approvals / CODEOWNERS (solo-maintainer incompatible)
- Merge queue
- Remote OpenTofu state
- Wiring npm Environment / OIDC Trusted Publishing (document only; follow-up)
- Classic branch protection API
- Org-level policies
- Automating stack creation

## Security review notes (2026-08-29)

Addressed in this revision: maintainer-only via access model; Actions default **read**; Validate fail-closed + `infra/**`. Residual: merge to `main` still triggers Release/`NPM_TOKEN` until an Environment or Trusted Publishing follow-up.

## References

- [GitHub repository rulesets](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/repository_ruleset)
- [github_workflow_repository_permissions](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/workflow_repository_permissions)
- [github_repository_collaborators](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/repository_collaborators)
- [Stacked PRs preview](https://github.blog/changelog/2026-07-30-stacked-pull-requests-are-now-in-public-preview/)
- `.github/workflows/ci.yml` job names: `Commitlint`, `Validate`
