#!/usr/bin/env bash
# Phase 3 static-analysis gates for the rewrite tree.
# Contract: rewrite/docs/developer/static-analysis-and-quality.md (L1/L6/L10).
set -euo pipefail

export PYTHONUNBUFFERED=1

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
cd "${REPO_ROOT}"

SEMGREP_CONFIG="${SCRIPT_DIR}/semgrep/rewrite.yml"
FAILED=0

echo "== sa:rewrite (repo=${REPO_ROOT}) =="

run_semgrep() {
  if ! command -v semgrep >/dev/null 2>&1; then
    echo "semgrep: CLI not found."
    echo "  Install: pip install semgrep"
    echo "  Or: python3 -m pip install --user semgrep && export PATH=\"\$HOME/.local/bin:\$PATH\""
    return 2
  fi
  echo "-- semgrep (${SEMGREP_CONFIG}) --"
  # ERROR severity fails the gate; WARNING (Ensure* heuristic) is reported only.
  if semgrep scan --config "${SEMGREP_CONFIG}" --error --severity ERROR \
    --metrics off \
    rewrite/mod; then
    echo "semgrep: PASS (ERROR rules)"
    # Still surface warnings without failing.
    semgrep scan --config "${SEMGREP_CONFIG}" --severity WARNING \
      --metrics off \
      rewrite/mod || true
    return 0
  else
    echo "semgrep: FAIL" >&2
    return 1
  fi
}

run_settings_graph() {
  echo "-- settings-field-graph --"
  python3 "${SCRIPT_DIR}/settings_field_graph.py" --repo-root "${REPO_ROOT}"
}

run_leak_pairing() {
  echo "-- native-leak-pairing --"
  python3 "${SCRIPT_DIR}/native_leak_pairing.py" --repo-root "${REPO_ROOT}"
}

if ! run_semgrep; then
  FAILED=1
fi
if ! run_settings_graph; then
  FAILED=1
fi
if ! run_leak_pairing; then
  FAILED=1
fi

if [[ "${FAILED}" -ne 0 ]]; then
  echo "sa:rewrite: FAIL" >&2
  exit 1
fi
echo "sa:rewrite: PASS"
exit 0
