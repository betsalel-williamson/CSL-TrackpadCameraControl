#!/usr/bin/env bash
# Ensure contributor tools are on PATH (dotnet / clang-format) for hooks & scripts.
set -euo pipefail

if [[ -x "$HOME/.dotnet/dotnet" ]]; then
  export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
  case ":$PATH:" in
    *":$HOME/.dotnet:"*) ;;
    *) export PATH="$HOME/.dotnet:$PATH" ;;
  esac
fi

if ! command -v clang-format >/dev/null 2>&1; then
  XCODE_CF_BIN="/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin"
  if [[ -x "$XCODE_CF_BIN/clang-format" ]]; then
    export PATH="$XCODE_CF_BIN:$PATH"
  fi
fi
