# Lint and format

Phase-1 contributor tooling keeps **C#**, **native C** (if present), and **docs** consistent. CI fails on format or docs-lint drift. Game assembly builds are out of scope until Harmony/CS1 refs land.

**C# pin:** in-game mod is **net35** + **LangVersion 9**; TrackpadCapture is **netstandard2.0** + **LangVersion 9** — see [contributor setup](./contributor-setup.md).

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

| Event            | Gates                                                                                                           |
| ---------------- | --------------------------------------------------------------------------------------------------------------- |
| **PR → main**    | **commitlint** (Ubuntu); one **validate** job on **macOS** for docs/format/`dotnet test` (tooling → full suite) |
| **Push to main** | Full docs + C# + native format on **macOS**                                                                     |

Validate runs on **macOS** so AppKit/IOKit QA paths and Darwin-only assertions match the product host. Pure formatting helpers stay cross-platform; Mac hardware probes fail soft or assert only under `OSPlatform.OSX`.

`dotnet test` (CI csharp scope) includes native-resource leak pairing — see [harnesses and testing](./harnesses-and-testing.md).

Locally, husky **pre-commit** formats staged files; **pre-push** runs the full format + docs gates only on `main` — see [commits and releases](./commits-and-releases.md).
