#!/usr/bin/env bash
# Shared pre-push entry (husky + manual: npm run hooks:pre-push).
# Mirrors CI: code format + docs compile/check with lint.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"
echo "==> pre-push: format:check"
npm run format:check
echo "==> pre-push: docs"
npm run docs
