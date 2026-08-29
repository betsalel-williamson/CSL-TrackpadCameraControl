#!/usr/bin/env bash
# Format or check native C sources with clang-format.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"

if ! command -v clang-format >/dev/null 2>&1; then
  echo "clang-format not found on PATH (install LLVM or Xcode Command Line Tools)" >&2
  exit 1
fi

# Portable file list (macOS ships Bash 3 — no mapfile).
files=()
while IFS= read -r line; do
  files+=("$line")
done <<EOF
$(find native -type f \( -name '*.c' -o -name '*.h' \) | sort)
EOF

if [[ ${#files[@]} -eq 0 ]]; then
  echo "No native C/H files under native/"
  exit 0
fi

mode="${1:-format}"
case "$mode" in
  format)
    clang-format -i "${files[@]}"
    echo "Formatted ${#files[@]} file(s)"
    ;;
  check)
    clang-format --dry-run -Werror "${files[@]}"
    echo "clang-format check passed (${#files[@]} file(s))"
    ;;
  *)
    echo "Usage: $0 [format|check]" >&2
    exit 2
    ;;
esac
