#!/usr/bin/env bash
# Build and run the Apple gesture-event spike probe (macOS). Logs to stderr.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root/native/mac"
make probe
exec ./AppleGestureProbe "$@"
