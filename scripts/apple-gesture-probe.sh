#!/usr/bin/env bash
# Headless AppKit gesture logger (macOS). Not the in-game capture path.
# Historical probe lives under bootstrap/ (primary ship capture is in-process Gestures).
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
exec dotnet run --project "$root/bootstrap/src/AppleGestureProbe" -- "$@"
