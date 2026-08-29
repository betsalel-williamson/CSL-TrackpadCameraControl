#!/usr/bin/env bash
# CI validate: choose and run docs / csharp / native gates.
# main (or FORCE_FULL=1 / tooling paths): all gates.
# PRs: only gates for paths changed vs base (GITHUB_BASE_REF or origin/main).
#
# Safe: never evals git-derived strings. Writes only allowlisted KEY=value to GITHUB_OUTPUT.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"

DOCS=0
CSHARP=0
NATIVE=0

mark_all() {
  DOCS=1
  CSHARP=1
  NATIVE=1
}

path_is() {
  local path="$1"
  shift
  local pat
  for pat in "$@"; do
    case "$path" in
      $pat) return 0 ;;
    esac
  done
  return 1
}

select_scopes() {
  if [[ "${FORCE_FULL:-0}" == "1" || "${GITHUB_REF:-}" == "refs/heads/main" ]]; then
    mark_all
    return
  fi

  local base="${GITHUB_BASE_REF:-}"
  if [[ -n "$base" ]]; then
    base="origin/$base"
  elif git rev-parse --verify --quiet origin/main >/dev/null; then
    base="origin/main"
  else
    mark_all
    return
  fi

  # Ensure base exists for fork PRs (workflow should fetch it).
  if ! git rev-parse --verify --quiet "$base" >/dev/null; then
    mark_all
    return
  fi

  local merge_base path
  merge_base="$(git merge-base "$base" HEAD)"
  while IFS= read -r path; do
    [[ -z "$path" ]] && continue
    if path_is "$path" \
      ".github/*" \
      ".husky/*" \
      "infra/*" \
      "scripts/bootstrap-dev.*" \
      "scripts/hooks-*" \
      "scripts/ci-validate.sh" \
      "scripts/ensure-tool-path.sh" \
      "commitlint.config.mjs" \
      "package.json" \
      "package-lock.json" \
      "Makefile"; then
      mark_all
      return
    fi
    if path_is "$path" \
      "docs/*" \
      "README.md" \
      "docs/mdcp.config.json" \
      ".prettierrc.json" \
      ".prettierignore" \
      ".changeset/*"; then
      DOCS=1
    fi
    if path_is "$path" \
      "mod/*" \
      "src/*" \
      "tests/*" \
      ".csharpierignore" \
      ".config/dotnet-tools.json" \
      "TrackpadCameraControl.sln" \
      "scripts/lint-staged-csharpier.sh"; then
      CSHARP=1
    fi
    if path_is "$path" \
      "native/*" \
      ".clang-format" \
      "scripts/format-native.sh" \
      "scripts/lint-staged-clang-format.sh"; then
      NATIVE=1
    fi
  done <<EOF
$(git diff --name-only "$merge_base" HEAD)
EOF
}

select_scopes

if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
  {
    printf 'docs=%s\n' "$DOCS"
    printf 'csharp=%s\n' "$CSHARP"
    printf 'native=%s\n' "$NATIVE"
  } >>"$GITHUB_OUTPUT"
fi

printf '==> validate scopes docs=%s csharp=%s native=%s\n' "$DOCS" "$CSHARP" "$NATIVE"

if [[ "$DOCS" -eq 0 && "$CSHARP" -eq 0 && "$NATIVE" -eq 0 ]]; then
  # Fail closed on PRs / feature branches so required status checks cannot go green with no gates.
  # Full runs on main (or FORCE_FULL) already mark_all above.
  if [[ "${FORCE_FULL:-0}" != "1" && "${GITHUB_REF:-}" != "refs/heads/main" ]]; then
    echo "==> error: no validate scopes matched this change set (fail closed)"
    echo "    Add a path scope in scripts/ci-validate.sh or touch a known docs/mod/native/infra path."
    exit 1
  fi
  echo "==> nothing to validate for this change set"
  exit 0
fi

if [[ "$CSHARP" -eq 1 ]]; then
  echo "==> csharpier check"
  npm run format:csharp:check
  echo "==> dotnet test"
  dotnet test TrackpadCameraControl.sln --nologo --verbosity minimal
fi

if [[ "$NATIVE" -eq 1 ]]; then
  echo "==> clang-format check"
  npm run format:native:check
fi

if [[ "$DOCS" -eq 1 ]]; then
  echo "==> docs"
  npm run docs
fi

echo "==> validate ok"
