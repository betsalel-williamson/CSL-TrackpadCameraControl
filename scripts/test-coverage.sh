#!/usr/bin/env bash
# Run unit tests with Coverlet coverage, then print a class-level summary.
# No coverage % gate — visibility only (blind spots vs over-tested surfaces).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
export PATH="${HOME}/.dotnet:${PATH}"

mkdir -p TestResults
rm -f TestResults/coverage.cobertura.xml

dotnet test tests/TrackpadCameraControl.Tests/TrackpadCameraControl.Tests.csproj \
  --nologo --verbosity minimal \
  -p:CollectCoverage=true \
  -p:CoverletOutput="${ROOT}/TestResults/coverage"

COBERTURA="${ROOT}/TestResults/coverage.cobertura.xml"
if [[ ! -f "$COBERTURA" ]]; then
  # Coverlet may append the extension when CoverletOutput has no trailing slash.
  if [[ -f "${ROOT}/TestResults/coverage" ]]; then
    COBERTURA="${ROOT}/TestResults/coverage"
  else
    echo "error: cobertura output not found under TestResults/" >&2
    exit 1
  fi
fi

dotnet tool restore >/dev/null
dotnet tool run reportgenerator \
  "-reports:${COBERTURA}" \
  "-targetdir:${ROOT}/TestResults/coverage-report" \
  "-reporttypes:TextSummary;Html" \
  "-title:TrackpadCameraControl" \
  >/dev/null

echo ""
echo "==> coverage summary (class-level — look for piled-on helpers vs blind spots)"
cat "${ROOT}/TestResults/coverage-report/Summary.txt"
echo ""
echo "HTML report: ${ROOT}/TestResults/coverage-report/index.html"
echo "Cobertura:   ${COBERTURA}"
