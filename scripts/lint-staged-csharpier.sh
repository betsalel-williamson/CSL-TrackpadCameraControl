#!/usr/bin/env bash
# lint-staged helper: format staged C# files with csharpier.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"
if [[ -x "$HOME/.dotnet/tools/csharpier" ]]; then
  export PATH="$HOME/.dotnet/tools:$PATH"
fi
if command -v csharpier >/dev/null 2>&1; then
  csharpier format "$@"
  exit 0
fi
dotnet tool restore >/dev/null
dotnet tool run csharpier format "$@"
