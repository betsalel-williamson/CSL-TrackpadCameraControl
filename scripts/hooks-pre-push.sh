#!/usr/bin/env bash
# Shared pre-push entry (husky + manual: npm run hooks:pre-push).
# main: full format + docs. Feature branches: skip (PR CI validates).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"

branch="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || printf 'unknown')"
# Log only — never eval branch names.
printf '==> pre-push on branch %s\n' "$branch"

if [[ "${HOOKS_FORCE_FULL:-0}" == "1" || "$branch" == "main" ]]; then
  echo "==> pre-push: format:check"
  npm run format:check
  echo "==> pre-push: docs"
  npm run docs
  echo "==> pre-push: ok"
  exit 0
fi

echo "==> pre-push: skipping expensive checks on feature branch (CI validates the PR)"
exit 0
