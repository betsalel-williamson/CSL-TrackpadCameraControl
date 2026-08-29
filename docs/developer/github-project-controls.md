# GitHub project controls

Declarative controls for this public repository: squash-only `main`, required CI, fork-based contributors, and a gated npm publish path.

## Access model (who can merge)

| Actor               | How they contribute                 | Can merge to `main`?     |
| ------------------- | ----------------------------------- | ------------------------ |
| Outside contributor | Fork + pull request                 | No (no repository Write) |
| Maintainer (owner)  | Branch or PR; squash-merge after CI | Yes                      |

Do **not** add casual collaborators on this personal repository — GitHub only grants Write/`push` to non-owners. Accidental Write collaborators are treated as control drift (`make check` / OpenTofu collaborators resource).

Required PR approvals are **not** used (solo maintainer). Protection is the access model plus required checks.

## Operator commands

From `infra/github/`:

```bash
make check    # default: init/import, plan (exit 2 on drift), stacks side checks
make apply    # converge (APPLY_AUTO=1 for non-interactive)
make status   # live gh api snapshot
```

See [infra/github/README.md](../../infra/github/README.md).

## What OpenTofu manages

- Squash-only merges; update-branch; delete head branch on merge
- Ruleset `main-protection` on default branch: PR + squash; `Commitlint` + `Validate` with strict up-to-date; no force-push
- Actions workflow defaults: `read`; allow Actions to create version PRs
- Authoritative collaborator inventory
- Environment `npm-publish` (wait timer; protected-branch deploys only)

## Release / publish

`.github/workflows/release.yml`:

1. **Version** job — creates the Changesets version PR; no npm credentials.
2. **Publish** job — runs only when there are no pending changesets; uses Environment **`npm-publish`**. Prefer [npm Trusted Publishing (OIDC)](https://docs.npmjs.com/trusted-publishers) for workflow `release.yml` + environment `npm-publish`. Optional fallback: Environment secret `NPM_TOKEN` (not a repository secret).

Checklist:

1. `cd infra/github && make apply`
2. On npmjs.com → package → Trusted Publisher → GitHub Actions → repo + `release.yml` + `npm-publish`
3. Remove any leftover repository-level `NPM_TOKEN` after OIDC works

## Stacked PRs

Stacked PRs are available on GitHub (public preview). Install the CLI helper once:

```bash
gh extension install github/gh-stack
```

`make check` verifies the stacks API and installs `gh-stack` if missing. Branch rules still govern what reaches `main`.
