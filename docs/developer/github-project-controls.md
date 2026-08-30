# GitHub project controls

Declarative controls for this public repository: squash-only `main`, required CI, and fork-based contributors.

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

## Versioning vs player distribution

| Path                                 | Role                                                                 |
| ------------------------------------ | -------------------------------------------------------------------- |
| Changesets + Release **Version** job | Semver + `CHANGELOG.md` via version PR                               |
| Release **GitHub Release** job       | Tag + GitHub Release (source archive for beta testers)               |
| Local Mods install                   | Deploy those archives via [Local MVP install](./local-mvp-install.md) |
| **Steam Workshop**                   | Future community subscribe path (upload automation separate)         |
| npm registry                         | **Not used**                                                         |

`.github/workflows/release.yml` versions via Changesets, then tags and creates a GitHub Release. It does not publish to npm.

Beta install: [Local MVP install](./local-mvp-install.md). Player path overview: `docs/client/install-and-first-run.md`. Announce and channel plan: [Community and marketing](./community-and-marketing.md). Contributing: [CONTRIBUTING.md](../../CONTRIBUTING.md).

## Stacked PRs

Stacked PRs are available on GitHub (public preview). Install the CLI helper once:

```bash
gh extension install github/gh-stack
```

`make check` verifies the stacks API and installs `gh-stack` if missing. Branch rules still govern what reaches `main`.
