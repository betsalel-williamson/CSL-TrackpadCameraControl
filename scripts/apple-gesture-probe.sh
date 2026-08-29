#!/usr/bin/env bash
# Headless AppKit gesture logger (macOS). Not the in-game capture path.
set -euo pipefail
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
exec dotnet run --project "$root/src/AppleGestureProbe" -- "$@"
