#!/usr/bin/env bash
# Build and run the C# Apple gesture-event spike probe (macOS). Logs to stderr.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
exec dotnet run --project "$root/src/AppleGestureProbe" -- "$@"
