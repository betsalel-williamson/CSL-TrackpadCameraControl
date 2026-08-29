# Lint and format

Phase-1 contributor tooling keeps **C#**, **native C**, and **docs** consistent. CI fails on format or docs-lint drift. Game assembly builds are out of scope until Harmony/CS1 refs land.

## Prerequisites

Use the automated bootstrap when possible — see [contributor setup](./contributor-setup.md):

```bash
./scripts/bootstrap-dev.sh --install-tools
```

| Tool           | Purpose                                                     |
| -------------- | ----------------------------------------------------------- |
| Node.js 22.12+ | MDCP docs compile / check / fix; commitlint and lint-staged |
| .NET SDK 8+    | Local CSharpier tool (`dotnet tool restore`)                |
| clang-format   | Native C style (LLVM/Xcode or `apt install clang-format`)   |

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

GitHub Actions (`.github/workflows/ci.yml`). Third-party actions are **pinned by commit SHA** (with a version comment), not floating tags.

| Event            | Gates                                                                                                               |
| ---------------- | ------------------------------------------------------------------------------------------------------------------- |
| **PR → main**    | **commitlint**; one **validate** job runs docs and/or format gates for changed paths (tooling changes → full suite) |
| **Push to main** | Full docs + C# + native format                                                                                      |

Locally, husky **pre-commit** formats staged files; **pre-push** runs the full format + docs gates only on `main` — see [commits and releases](./commits-and-releases.md).
