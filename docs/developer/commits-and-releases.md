# Commits and releases

## Git hooks (husky)

Hooks are installed by `npm install` / bootstrap (`prepare` → husky). Same entrypoints run from husky and npm:

| Hook         | Command                    | What it does                                                                                         |
| ------------ | -------------------------- | ---------------------------------------------------------------------------------------------------- |
| `pre-commit` | `npm run hooks:pre-commit` | **lint-staged** (cheap): Prettier / CSharpier / clang-format on **staged** files                     |
| `commit-msg` | commitlint                 | Conventional Commits subject                                                                         |
| `pre-push`   | `npm run hooks:pre-push`   | On **`main`**: `format:check` + `docs`. On feature branches: skip expensive checks (PR CI validates) |

Emergency skip: `HUSKY=0 git commit …` or `HUSKY=0 git push …`. Prefer fixing the failure over skipping.

## CI: PR subset vs main full suite

| Event                        | Behavior                                                                                                                                     |
| ---------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Pull request** into `main` | **commitlint** always; one **validate** job runs only the gates touched by the PR (docs / C# / native), or all gates if tooling paths change |
| **Push to `main`**           | Full docs + C# + native format                                                                                                               |

## Conventional commits

Commit messages must follow [Conventional Commits](https://www.conventionalcommits.org/) (enforced by commitlint + husky `commit-msg`):

```text
feat: add orbit deadzone option
fix: correct Maps+ modifier docs
chore: bump prettier
docs: document lint and format
```

Types commonly used here: `feat`, `fix`, `docs`, `chore`, `ci`, `refactor`, `test`.

Local check of the latest commit:

```bash
npm run commitlint
```

## Changesets

User-facing or releasable changes get a **changeset** (temporary note under `.changeset/`). Do not link durable MDCP shards to pending changeset files.

```bash
npm run changeset           # interactively add a changeset
npm run changeset:status    # list pending changesets
npm run version-packages    # apply changesets → version + CHANGELOG (maintainers)
```

This package is **public** (`publishConfig.access` / changesets `access: public`). Publish when maintainers run a release; Workshop/mod DLL shipping is separate from the npm package version.
