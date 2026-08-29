#!/usr/bin/env bash
# lint-staged helper: format staged C# files with csharpier.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"
dotnet tool restore >/dev/null
dotnet tool run csharpier format "$@"
