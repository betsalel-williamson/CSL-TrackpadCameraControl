# Commits and releases

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
