#!/usr/bin/env bash
# Shared pre-commit entry (husky + manual: npm run hooks:pre-commit).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"
npx --no lint-staged
