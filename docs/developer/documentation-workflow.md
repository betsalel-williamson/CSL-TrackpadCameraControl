# Documentation workflow

This repo uses [MDCP](https://github.com/betsalel-williamson/mdcp) (MarkDown Context Protocol).

## Commands

From the repository root:

```bash
npm install
npm run docs:compile
npm run docs:check
npm run docs          # compile then check (markdownlint required via --require-lint)
npm run format:docs   # prettier / markdownlint auto-fix via mdcp fix
```

For C# and native format tooling, see [lint and format](./lint-and-format.md).

## Rules

- Edit shards under `docs/**/` only — never hand-edit `docs/_build/`.
- Update each guide’s `index.md` when adding shards.
- After cross-links, run `npm run docs:check` so fragments match compiled slugs.
- No implementation code in durable shards — contracts and acceptance criteria only.
- Planning and backlogs stay in GitHub Issues, not durable docs.

## Guides

| Guide | Holds |
| --- | --- |
| `docs/features/` | Capabilities, architecture, ADRs, settings contracts |
| `docs/client/` | Player install, presets, Options, OS gesture conflicts |
| `docs/developer/` | Layout, docs tooling, templates, work items |
| `docs/glossary/` | Shared terms |
