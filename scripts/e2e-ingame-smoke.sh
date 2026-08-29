#!/usr/bin/env bash
# In-game inject smoke: arm inject, install mod, wait for game to process a synthetic pinch.
# Prerequisites: Cities: Skylines running, mod enabled, city loaded (inject polls each frame).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck source=ensure-tool-path.sh
source "$ROOT/scripts/ensure-tool-path.sh"
cd "$ROOT"

echo "==> headless e2e (always)"
dotnet test TrackpadCameraControl.sln --nologo --filter "FullyQualifiedName~HeadlessPipelineE2e" --verbosity minimal

TMPDIR_VAL="${TMPDIR:-/tmp}"
FLAG="${TMPDIR_VAL%/}/e2e-inject.flag"
touch "$FLAG"

./scripts/install-mod-local.sh

MODS="${CITIES_MODS:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}"
DEST="${MODS}/TrackpadCameraControl"
mkdir -p "$DEST"
cp -f "$FLAG" "${DEST}/e2e-inject.flag"
rm -f "${DEST}/e2e-inject-result" "${DEST}/e2e-inject-request"

TIMEOUT_SEC="${E2E_INGAME_TIMEOUT:-90}"
PINCH="${E2E_INGAME_PINCH:-0.1}"

echo "==> inject armed. Waiting up to ${TIMEOUT_SEC}s for game to consume request…"
echo "==> Ensure CS1 is running with Trackpad Camera Control enabled and a city loaded."

echo -n "$PINCH" >"${DEST}/e2e-inject-request"

deadline=$((SECONDS + TIMEOUT_SEC))
while ((SECONDS < deadline)); do
  if [[ -f "${DEST}/e2e-inject-result" ]]; then
    result="$(tr -d '[:space:]' <"${DEST}/e2e-inject-result")"
    echo "==> PASS in-game inject: camera size reported as ${result}"
    rm -f "$FLAG" "${DEST}/e2e-inject.flag" "${DEST}/e2e-inject-request" "${DEST}/e2e-inject-result"
    exit 0
  fi
  sleep 1
done

echo "==> FAIL: no e2e-inject-result within ${TIMEOUT_SEC}s (is the game running with inject flag + city loaded?)" >&2
rm -f "${DEST}/e2e-inject-request"
exit 1
