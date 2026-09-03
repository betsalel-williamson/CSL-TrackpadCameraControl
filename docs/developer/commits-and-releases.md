# Commits and releases

## Git hooks (husky)

Hooks are installed by `npm install` / bootstrap (`prepare` → husky). Same entrypoints run from husky and npm:

| Hook         | Command                    | What it does                                                                                  |
| ------------ | -------------------------- | --------------------------------------------------------------------------------------------- |
| `pre-commit` | `npm run hooks:pre-commit` | **lint-staged** (cheap): Prettier / CSharpier / clang-format on **staged** files              |
| `commit-msg` | commitlint                 | Conventional Commits subject                                                                  |
| `pre-push`   | `npm run hooks:pre-push`   | Every branch: `docs` (same compile+check as CI docs gate). On **`main`**: also `format:check` |

Emergency skip: `HUSKY=0 git commit …` or `HUSKY=0 git push …`. Prefer fixing the failure over skipping.

## CI: PR subset vs main full suite

| Event                        | Behavior                                                                                                                                                                                 |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Pull request** into `main` | **commitlint** always; one **validate** job runs only the gates touched by the PR (docs / C# / native / infra), or all gates if tooling paths change. **Fail closed** if no scopes match |
| **Push to `main`**           | Full docs + C# + native format                                                                                                                                                           |

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

This package uses Changesets for **version + CHANGELOG** and **GitHub Releases** (source archives for beta testers). Players will eventually install from **Steam Workshop**. This project does **not** publish to the npm registry.

The in-game **product** version (`Mod.OptionsTitle`, e.g. `Trackpad Camera Control (macOS) 0.2.0`) is `package.json` `"version"` stamped into `BuildInfo.ProductVersion` / InformationalVersion at **MSBuild** time. After `npm run version-packages` bumps `package.json`, the next mod build picks up the new semver — no separate sync step.

`AssemblyVersion` is `Major.Minor.*` (build/revision change every compile) so Cities can auto-reload during development — see [mod reload during development](./mod-reload-during-development.md). That identity is **not** the storefront / Options product version.

## Release workflow

On push to `main`, `.github/workflows/release.yml`:

1. **Version** — opens/updates the Changesets version PR when changesets are pending.
2. **GitHub Release** — when there is nothing left to version, runs `changeset tag` and creates a GitHub Release (source zip/tar only).

Beta install from a release: [Local MVP install](./local-mvp-install.md).

After a Release (or Workshop publish), follow [Community and marketing](./community-and-marketing.md) for soft vs public announcement — do not splash Reddit/Workshop until public-splash readiness there is met.

Branch protection and merge policy: [GitHub project controls](./github-project-controls.md).
