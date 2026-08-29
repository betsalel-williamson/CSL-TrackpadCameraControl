# Contributor setup

Automated bootstrap for docs, format, and commit tooling. **macOS is the primary path**; Linux is supported for CI-like machines; Windows is documented but not fully automated yet.

## One command (macOS / Linux)

From the repository root:

```bash
./scripts/bootstrap-dev.sh --install-tools
```

Or via npm (after Node is available):

```bash
npm run bootstrap
# with host-tool installs:
npm run bootstrap:install
```

What it does:

1. Checks **Node.js 18+**, **npm**, **.NET SDK 8+**, **clang-format**
2. With `--install-tools`: installs missing pieces when possible (Homebrew Node / clang-format on macOS; official `dotnet-install` to `~/.dotnet`; apt clang-format on Linux)
3. Runs `npm install` (or `npm ci` with `--ci`) and `dotnet tool restore` (CSharpier)
4. Points git hooks at `.husky` (commitlint)
5. Smoke-runs `format:check` and `docs` (skip with `--skip-verify`)

Check only (no package install):

```bash
./scripts/bootstrap-dev.sh --check
```

## Prerequisites (manual)

| Tool | macOS | Linux | Windows |
| --- | --- | --- | --- |
| Node.js 18+ | Homebrew `node`, or [nodejs.org](https://nodejs.org) | Distro / nvm | [nodejs.org](https://nodejs.org) or WSL |
| .NET SDK 8+ | `dotnet-install` → `~/.dotnet`, or Homebrew `dotnet` | Same install script | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| clang-format | Xcode toolchain or `brew install clang-format` | `apt install clang-format` | LLVM/clang-format, or WSL |

Game / Cities: Skylines assemblies are **not** required for Phase 1 contributor tooling.

## Windows notes

- Preferred: **WSL2 (Ubuntu)** and the same `./scripts/bootstrap-dev.sh --install-tools` flow.
- Native PowerShell automation is not shipped yet; install Node, .NET SDK, and clang-format yourself, then `npm install` and `dotnet tool restore` from the repo root.
- Commit hooks need Git for Windows + `npm install` so husky’s `prepare` runs.

## After bootstrap

| Command | Purpose |
| --- | --- |
| `npm run docs` | Compile + check MDCP docs |
| `npm run format` / `format:check` | Format or verify C# / C / docs |
| `npm run changeset` | Add a release note for releasable work |

Related: [lint and format](./lint-and-format.md), [commits and releases](./commits-and-releases.md).
