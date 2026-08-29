# Windows stub — full automation is macOS/Linux (bash) first.
# Prefer WSL2, then: ./scripts/bootstrap-dev.sh --install-tools
Write-Host @"
Trackpad Camera Control — Windows bootstrap stub

Preferred: WSL2 (Ubuntu), then run:
  ./scripts/bootstrap-dev.sh --install-tools

Native Windows (manual):
  1. Install Node.js 18+ and .NET SDK 8+
  2. Install clang-format (LLVM) or use WSL for format checks
  3. npm install
  4. dotnet tool restore
  5. npm run docs
  6. npm run format:check

See docs/developer/contributor-setup.md
"@
exit 1
