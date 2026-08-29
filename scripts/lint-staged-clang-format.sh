#!/usr/bin/env bash
# lint-staged helper: format staged C/H files with clang-format.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
if ! command -v clang-format >/dev/null 2>&1; then
  echo "clang-format not found — run ./scripts/bootstrap-dev.sh --install-tools" >&2
  exit 1
fi
clang-format -i "$@"
