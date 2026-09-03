#!/usr/bin/env bash
# Build TrackpadCameraControl.dll. MSBuild post-build deploys into the local
# CS1 Mods folder (Paradox Advanced Mod Setup → Automate).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"

REWRITE=0
if [[ $# -gt 0 ]]; then
  case "$1" in
    -h | --help)
      echo "Usage: $0 [--rewrite|-r]"
      echo "  Build + post-build copy into Mods (wiki Automate)."
      echo "  --rewrite | -r  Build/deploy rewrite/mod → Mods/TrackpadCameraControl.Rewrite"
      exit 0
      ;;
    --rewrite | -r)
      REWRITE=1
      shift
      ;;
    *)
      echo "Unknown option: $1" >&2
      echo "Usage: $0 [--rewrite|-r]" >&2
      exit 1
      ;;
  esac
fi

if [[ "${REWRITE}" -eq 1 ]]; then
  CSPROJ="${ROOT}/rewrite/mod/TrackpadCameraControl.Rewrite.csproj"
  DEST_REWRITE="${MODS:-${CITIES_MODS:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}}/TrackpadCameraControl.Rewrite"
  if [[ ! -f "${CSPROJ}" ]]; then
    echo "rewrite/mod is not buildable yet (missing ${CSPROJ})." >&2
    echo "Docs-first phase: see rewrite/README.md. Deploy target will be: Mods/TrackpadCameraControl.Rewrite" >&2
    exit 2
  fi
  # Phase 2+ will build and deploy here.
  echo "ERROR: --rewrite csproj exists but install path not wired yet." >&2
  exit 1
fi

MANAGED="${CitiesManaged:-${HOME}/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed}"
MODS="${CITIES_MODS:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}"
DEST="${MODS}/TrackpadCameraControl"

if [[ ! -f "${MANAGED}/ICities.dll" ]]; then
  echo "ICities.dll not found at: ${MANAGED}" >&2
  echo "Set CitiesManaged to your Cities_Data/Managed (or Contents/Resources/Data/Managed) path." >&2
  exit 1
fi

dotnet build \
  "${ROOT}/mod/TrackpadCameraControl.csproj" \
  -c Release \
  "-p:CitiesManaged=${MANAGED}" \
  "-p:CitiesMods=${MODS}"

mkdir -p "${DEST}"

echo "Build finished (post-build should have deployed to ${DEST})."
echo "Cities auto-reloads when AssemblyVersion changes — see mod-reload-during-development.md"
echo "Capture: in-process AppKit (default, mod DLL). Optional TrackpadBridge socket experiment in src/TrackpadBridge."
echo "Debug panel footer: Built (UTC) + asm identity confirm the loaded build."
if [[ -f "${DEST}/PreviewImage.png" ]]; then
  echo "Preview → ${DEST}/PreviewImage.png"
fi
echo "Inspect: tail -f \"\${TMPDIR:-/tmp}/trackpad-camera-control.log\""
