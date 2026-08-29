# GitHub project controls — Design

**Date:** 2026-08-29
**Status:** Approved (revised: Workshop distribution; no npm publish)
**Scope:** Declarative GitHub controls (OpenTofu + Makefile), Changesets version-only Release, CI Validate gate hardening, stacked PR verification for `betsalel-williamson/CSL-TrackpadCameraControl`

## Goal

Encode durable GitHub project controls so a maintainer can run one idempotent command anytime: initialize if needed, detect drift, and converge — without recreating the repository.

**Maintainer-only merge (OSS, single maintainer):** Do **not** use required PR approvals. Rely on the **contributor / fork model**: outsiders open PRs from forks and never receive repository **Write**. Only the maintainer can merge to `main`. Accidental Write collaborators are control drift.

## Security review → remediation map

| Finding                                       | Severity | Remediation in this design                                                                                            |
| --------------------------------------------- | -------- | --------------------------------------------------------------------------------------------------------------------- |
| Zero reviews → any Write user can merge       | High     | Access model: no Write for outsiders; authoritative collaborator inventory; document forks                            |
| Actions default `write` + create/approve PRs  | Medium   | Repo default **`read`**; keep create-PR flag for Changesets; Release elevates only via job `permissions`              |
| Merge to `main` ≈ npm publish via `NPM_TOKEN` | Medium   | **No npm publish.** Changesets = version + CHANGELOG only; player distribution is **Steam Workshop** (automation TBD) |
| Validate can exit 0 with no scopes            | Medium   | Fail closed on PRs when no scopes match; add `infra/**` (+ related) to scopes                                         |
| Controls docs-only / local state              | Medium   | Land `infra/github/` + Makefile `check`/`apply`; gitignore state; commit provider lockfile                            |

## Decisions

| Concern          | Choice                                                                  | Rationale                                                        |
| ---------------- | ----------------------------------------------------------------------- | ---------------------------------------------------------------- |
| Approach         | Hybrid OpenTofu + `infra/github/Makefile`                               | IaC + idempotent operator surface                                |
| Who can merge    | Fork contributors; no casual Write collaborators                        | Solo self-merge without a second reviewer                        |
| Collaborators    | Authoritative intended set in OpenTofu                                  | Personal repo collaborator ≈ Write                               |
| Reviews          | `required_approving_review_count = 0`                                   | Solo-maintainer compatible                                       |
| Merge methods    | Squash only                                                             | Linear history                                                   |
| Up to date       | Strict required checks + `allow_update_branch`                          | Must merge/`update` from `main` before land                      |
| Required checks  | `Commitlint`, `Validate`                                                | Existing CI names                                                |
| Validate gate    | Fail closed + `infra/**` in scopes                                      | Status check must mean something                                 |
| Ruleset          | Active on `~DEFAULT_BRANCH`; no force-push                              | Protect `main`                                                   |
| Actions defaults | `read` + `can_approve_pull_request_reviews = true`                      | Least privilege; Changesets can still open version PRs           |
| Publish          | Changesets version PR + `changeset tag` GitHub Release (source); no npm | Beta testers use Release source + install script; Workshop later |
| State            | Local tfstate gitignored                                                | Solo v1                                                          |
| Auth             | `gh auth token` → `GH_TOKEN`                                            | Admin required for apply                                         |

## Access model (contributor check)

```text
Outside contributor  →  fork + PR  →  no repo Write  →  cannot merge
Maintainer (owner)   →  squash-merge after Commitlint + Validate (strict)
Version PR           →  Release “version” job (Changesets)
GitHub Release       →  tag + source archive for beta (`changeset tag`)
Player installs      →  Release source + install-mod-local.sh (Workshop later)
```

## Layout

```text
infra/github/
  Makefile
  versions.tf
  providers.tf
  variables.tf
  main.tf
  outputs.tf
  README.md
.github/workflows/release.yml   # Changesets version PR only
docs/developer/
  github-project-controls.md
```

Gitignore: `.terraform/`, `*.tfstate`, `*.tfstate.*`. Commit `.terraform.lock.hcl`.

## OpenTofu resources

1. **`github_repository` (imported)** — `prevent_destroy`; squash-only; `allow_update_branch`; `delete_branch_on_merge`; squash title `PR_TITLE` / message `PR_BODY`.
2. **`github_repository_ruleset` `main-protection`** — PR + squash-only; 0 approvals; required `Commitlint` + `Validate` with `strict_required_status_checks_policy`; `non_fast_forward`.
3. **`github_workflow_repository_permissions`** — `default_workflow_permissions = "read"`; `can_approve_pull_request_reviews = true`.
4. **`github_repository_collaborators`** — authoritative intended users (no unexpected Write).

Variables: owner, repo, check contexts, `maintainer_usernames`.

## Release workflow

`.github/workflows/release.yml`:

1. Workflow-level `permissions: contents: read`; jobs elevate as needed.
2. **Job `version`** — `changesets/action` with `version-script` only.
3. **Job `github-release`** — when no pending changesets: `publish-script: npx changeset tag`, `create-github-releases: true` (source zip/tar on the Release; no npm).

Package is `"private": true` with Changesets `privatePackages.version/tag` enabled so tagging works without a registry.

Beta install docs: download Release source → `./scripts/install-mod-local.sh`. Steam Workshop upload is a separate future effort.

## CI companion

`scripts/ci-validate.sh`:

- On `pull_request` (or when not `main` / not `FORCE_FULL`): if docs/csharp/native all 0 after scoping → **exit 1** with a clear message (fail closed).
- Add path scopes that mark tooling/full or at least run a gate: `infra/**` (and ensure `.github/**` continues to force full validate).

## Makefile operator model (idempotent)

**`make check` (default):** preflight → init → auto-import if needed → `tofu plan -detailed-exitcode` (0 sync / 2 drift / 1 error) → stacks + `gh-stack` → collaborator inventory match → remind Workshop (not npm) distribution.

**`make apply`:** converge OpenTofu (idempotent).

**`make status`:** raw `gh api` for merge settings, rulesets, Actions perms, collaborators, stacks.

Never destroy the GitHub repository.

## Docs

`docs/developer/github-project-controls.md`: access model, Make targets, Actions create-PR flag, versioning vs Workshop distribution, stacks.

Update: `commits-and-releases.md`, `contributor-setup.md` (forks), `repository-layout.md`.

## Out of scope

- Required PR approvals / CODEOWNERS-as-merge-gate (solo incompatible)
- Steam Workshop upload automation (future)
- npm registry publish / Trusted Publishing
- Merge queue
- Remote OpenTofu state
- Org-level policies
- Automating stack creation

## References

- [repository_ruleset](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/repository_ruleset)
- [workflow_repository_permissions](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/workflow_repository_permissions)
- [repository_collaborators](https://registry.terraform.io/providers/integrations/github/latest/docs/resources/repository_collaborators)
- [Stacked PRs preview](https://github.blog/changelog/2026-07-30-stacked-pull-requests-are-now-in-public-preview/)
