# GitHub project controls (OpenTofu)

Declarative settings for this repository. Operator entrypoint is **Make** (not raw `tofu`).

## Prerequisites

- [OpenTofu](https://opentofu.org/) (`tofu`)
- [GitHub CLI](https://cli.github.com/) (`gh`) logged in as a user with **admin** on the repo

```bash
gh auth status
cd infra/github
make check
```

## Targets

| Target                 | Behavior                                                                                    |
| ---------------------- | ------------------------------------------------------------------------------------------- |
| `make check` (default) | Init/import if needed, `tofu plan` (exit 0 sync / 2 drift), stacks + `gh-stack` side checks |
| `make apply`           | Converge desired state (`APPLY_AUTO=1` for non-interactive)                                 |
| `make status`          | Live `gh api` snapshot (no OpenTofu)                                                        |

State files are gitignored. Provider lockfile (`.terraform.lock.hcl`) is committed after first `tofu init`.

## Manual follow-ups

1. Move any repo-level `NPM_TOKEN` to Environment secret `npm-publish` / `NPM_TOKEN`, or prefer [npm Trusted Publishing](https://docs.npmjs.com/trusted-publishers) for `release.yml` + environment `npm-publish`.
2. Outside contributors must use **forks** — do not grant Write collaborators on this personal repo.
