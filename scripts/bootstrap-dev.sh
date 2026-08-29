#!/usr/bin/env bash
# Contributor / agent bootstrap for Trackpad Camera Control tooling.
# Focus: macOS. Linux supported for checks + package install. Windows: see docs.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

INSTALL_TOOLS=0
CHECK_ONLY=0
SKIP_VERIFY=0
NPM_CI=0

usage() {
  cat <<'EOF'
Usage: scripts/bootstrap-dev.sh [options]

  (default)     Check prerequisites, install npm + local .NET tools, enable husky
  --install-tools
                Attempt to install missing host tools (macOS: Homebrew / dotnet-install;
                Linux: apt hints + dotnet-install). Never runs sudo brew without brew present.
  --ci          Use npm ci (clean install from lockfile) instead of npm install
  --check       Only verify tools and report; do not install packages
  --skip-verify Skip post-install docs/format smoke checks
  -h, --help    Show this help

Examples:
  ./scripts/bootstrap-dev.sh
  ./scripts/bootstrap-dev.sh --install-tools
  npm run bootstrap
EOF
}

log() { printf '==> %s\n' "$*"; }
warn() { printf 'warn: %s\n' "$*" >&2; }
die() { printf 'error: %s\n' "$*" >&2; exit 1; }

have() { command -v "$1" >/dev/null 2>&1; }

os_family() {
  case "$(uname -s)" in
    Darwin) echo darwin ;;
    Linux) echo linux ;;
    MINGW*|MSYS*|CYGWIN*) echo windows ;;
    *) echo unknown ;;
  esac
}

ensure_xcode_clang_format_on_path() {
  local xcf="/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin"
  if [[ -x "$xcf/clang-format" ]]; then
    case ":$PATH:" in
      *":$xcf:"*) ;;
      *) export PATH="$xcf:$PATH" ;;
    esac
  fi
}

ensure_dotnet_on_path() {
  if [[ -x "$HOME/.dotnet/dotnet" ]]; then
    export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
    case ":$PATH:" in
      *":$HOME/.dotnet:"*) ;;
      *) export PATH="$HOME/.dotnet:$PATH" ;;
    esac
  fi
}

node_major() {
  node -p "process.versions.node.split('.')[0]" 2>/dev/null || echo 0
}

check_node() {
  if ! have node; then
    echo "missing: Node.js 18+"
    return 1
  fi
  local major
  major="$(node_major)"
  if [[ "$major" -lt 18 ]]; then
    echo "missing: Node.js 18+ (found $(node -v))"
    return 1
  fi
  echo "ok: Node $(node -v)"
  return 0
}

check_npm() {
  if ! have npm; then
    echo "missing: npm (usually ships with Node.js)"
    return 1
  fi
  echo "ok: npm $(npm -v)"
  return 0
}

check_dotnet() {
  ensure_dotnet_on_path
  if ! have dotnet; then
    echo "missing: .NET SDK 8+ (for csharpier)"
    return 1
  fi
  echo "ok: dotnet $(dotnet --version 2>/dev/null || echo unknown)"
  return 0
}

check_clang_format() {
  ensure_xcode_clang_format_on_path
  if ! have clang-format; then
    echo "missing: clang-format"
    return 1
  fi
  echo "ok: $(clang-format --version 2>/dev/null | head -1)"
  return 0
}

install_node_darwin() {
  if have brew; then
    log "Installing Node.js via Homebrew"
    brew install node@22 || brew install node
  else
    die "Node.js missing and Homebrew not found. Install Node 18+ from https://nodejs.org or install Homebrew first."
  fi
}

install_dotnet() {
  log "Installing .NET SDK 8 to \$HOME/.dotnet"
  local script
  script="$(mktemp)"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$script"
  bash "$script" --channel 8.0 --install-dir "$HOME/.dotnet"
  rm -f "$script"
  ensure_dotnet_on_path
  have dotnet || die "dotnet install finished but 'dotnet' is not on PATH"
}

install_clang_format_darwin() {
  ensure_xcode_clang_format_on_path
  if have clang-format; then
    return 0
  fi
  if [[ -d /Applications/Xcode.app ]]; then
    warn "Xcode is installed but clang-format is not on PATH; probing toolchain failed."
  fi
  if have brew; then
    log "Installing clang-format via Homebrew"
    brew install clang-format
  else
    die "clang-format missing. Install Xcode, or Homebrew + clang-format, or LLVM."
  fi
}

install_clang_format_linux() {
  if have clang-format; then
    return 0
  fi
  if have apt-get; then
    log "Installing clang-format via apt (may prompt for sudo)"
    sudo apt-get update
    sudo apt-get install -y clang-format
  else
    die "clang-format missing. Install the LLVM clang-format package for your distro."
  fi
}

install_missing_tools() {
  local os
  os="$(os_family)"
  case "$os" in
    darwin)
      check_node || install_node_darwin
      check_npm || die "npm still missing after Node install"
      check_dotnet || install_dotnet
      check_clang_format || install_clang_format_darwin
      ;;
    linux)
      if ! check_node || ! check_npm; then
        die "Install Node.js 18+ (e.g. NodeSource, nvm, or distro package), then re-run."
      fi
      check_dotnet || install_dotnet
      check_clang_format || install_clang_format_linux
      ;;
    windows)
      die "Native Windows bootstrap is not automated yet. Use Git Bash/WSL and see docs/developer/contributor-setup.md"
      ;;
    *)
      die "Unsupported OS '$(uname -s)'. See docs/developer/contributor-setup.md"
      ;;
  esac
}

print_tool_report() {
  local os ok=0
  os="$(os_family)"
  log "Platform: $os ($(uname -s) $(uname -m))"
  check_node || ok=1
  check_npm || ok=1
  check_dotnet || ok=1
  check_clang_format || ok=1
  return "$ok"
}

install_project_deps() {
  ensure_dotnet_on_path
  ensure_xcode_clang_format_on_path

  if [[ "$NPM_CI" -eq 1 ]]; then
    log "npm ci"
    npm ci
  else
    log "npm install"
    npm install
  fi

  log "dotnet tool restore (csharpier)"
  dotnet tool restore

  if [[ -d .git ]]; then
    log "Ensuring husky hooks path"
    git config core.hooksPath .husky || warn "could not set core.hooksPath (non-fatal)"
  fi
}

verify_smoke() {
  ensure_dotnet_on_path
  ensure_xcode_clang_format_on_path
  log "Smoke: format:check"
  npm run format:check
  log "Smoke: docs"
  npm run docs
}

# --- args ---
while [[ $# -gt 0 ]]; do
  case "$1" in
    --install-tools) INSTALL_TOOLS=1 ;;
    --check) CHECK_ONLY=1 ;;
    --ci) NPM_CI=1 ;;
    --skip-verify) SKIP_VERIFY=1 ;;
    -h|--help) usage; exit 0 ;;
    *) usage; die "unknown option: $1" ;;
  esac
  shift
done

log "Trackpad Camera Control — developer bootstrap"
os="$(os_family)"
if [[ "$os" == windows ]]; then
  warn "Windows: automated host-tool install is limited; prefer WSL2 or follow contributor-setup.md"
fi

if [[ "$INSTALL_TOOLS" -eq 1 ]]; then
  install_missing_tools
fi

if ! print_tool_report; then
  if [[ "$CHECK_ONLY" -eq 1 ]]; then
    die "Prerequisite check failed. Re-run with --install-tools on macOS/Linux, or install tools manually."
  fi
  die "Missing prerequisites. Re-run: ./scripts/bootstrap-dev.sh --install-tools"
fi

if [[ "$CHECK_ONLY" -eq 1 ]]; then
  log "Check-only: all prerequisites present"
  exit 0
fi

install_project_deps

if [[ "$SKIP_VERIFY" -eq 0 ]]; then
  verify_smoke
fi

log "Bootstrap complete."
printf '\nNext:\n'
printf '  npm run docs\n'
printf '  npm run format:check\n'
printf '  npm run changeset   # for releasable changes\n'
printf '  See docs/developer/contributor-setup.md\n'
