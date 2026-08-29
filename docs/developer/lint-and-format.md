# Lint and format

Phase-1 contributor tooling keeps **C#**, **native C**, and **docs** consistent. CI fails on format or docs-lint drift. Game assembly builds are out of scope until Harmony/CS1 refs land.

## Prerequisites

| Tool | Purpose |
| --- | --- |
| Node.js 18+ | MDCP docs compile / check / fix |
| .NET SDK 8+ | Local CSharpier tool (`dotnet tool restore`) |
| clang-format | Native C style (LLVM/Xcode or `apt install clang-format`) |

On macOS, Xcode’s `clang-format` is enough if it is on `PATH` (the `format:native` script also probes the default Xcode toolchain path).

## Commands

From the repository root:

```bash
npm install
dotnet tool restore

npm run format          # docs + csharp + native
npm run format:check    # csharpier + clang-format verify (no write)
npm run docs            # compile then check (markdownlint required)
```

Individual targets: `format:docs`, `format:csharp`, `format:native` (and matching `:check` scripts).

## What CI runs

GitHub Actions (`.github/workflows/ci.yml`) on push/PR to `main`:

1. **Docs** — `npm ci` then `npm run docs` (`--require-lint`)
2. **Code format** — CSharpier check on `mod/`, clang-format dry-run on `native/**/*.{c,h}`

Format before opening a PR; CI will fail if style drifts.

Commit messages and releases: see [commits and releases](./commits-and-releases.md).
