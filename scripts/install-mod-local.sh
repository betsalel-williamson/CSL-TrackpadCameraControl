#!/usr/bin/env bash
# Build TrackpadCameraControl.dll. MSBuild post-build deploys into the local
# CS1 Mods folder (Paradox Advanced Mod Setup → Automate).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"

MODE=primary
if [[ $# -gt 0 ]]; then
  case "$1" in
    -h | --help)
      echo "Usage: $0 [--bootstrap|-b]"
      echo "  Build + post-build copy into Mods/TrackpadCameraControl (last install wins)."
      echo "  (default)         Build primary mod/ tree (post-cutover rewrite)"
      echo "  --bootstrap | -b  Build historical prototype under bootstrap/"
      echo "  --rewrite | -r    Alias for default (kept for older docs/scripts)"
      exit 0
      ;;
    --bootstrap | -b)
      MODE=bootstrap
      shift
      ;;
    --rewrite | -r)
      MODE=primary
      shift
      ;;
    *)
      echo "Unknown option: $1" >&2
      echo "Usage: $0 [--bootstrap|-b]" >&2
      exit 1
      ;;
  esac
fi

MANAGED="${CitiesManaged:-${HOME}/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed}"
MODS="${CITIES_MODS:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}"
DEST="${MODS}/TrackpadCameraControl"
LEGACY_REWRITE_DEST="${MODS}/TrackpadCameraControl.Rewrite"

if [[ "${MODE}" == "bootstrap" ]]; then
  CSPROJ="${ROOT}/bootstrap/mod/TrackpadCameraControl.csproj"
else
  CSPROJ="${ROOT}/mod/TrackpadCameraControl.Rewrite.csproj"
fi

if [[ ! -f "${CSPROJ}" ]]; then
  echo "Missing project: ${CSPROJ}" >&2
  exit 2
fi

if [[ ! -f "${MANAGED}/ICities.dll" ]]; then
  echo "ICities.dll not found at: ${MANAGED}" >&2
  echo "Set CitiesManaged to your Cities_Data/Managed (or Contents/Resources/Data/Managed) path." >&2
  exit 1
fi

dotnet build \
  "${CSPROJ}" \
  -c Release \
  "-p:CitiesManaged=${MANAGED}" \
  "-p:CitiesMods=${MODS}"

mkdir -p "${DEST}"
# Prior dual-folder QA left a second Content Manager row; last-install-wins uses one folder.
if [[ -d "${LEGACY_REWRITE_DEST}" ]]; then
  rm -rf "${LEGACY_REWRITE_DEST}"
  echo "Removed legacy Mods/TrackpadCameraControl.Rewrite"
fi

echo "Build finished (post-build should have deployed to ${DEST})."
echo "Cities auto-reloads when AssemblyVersion changes — see docs/developer/local-mvp-install.md"
if [[ "${MODE}" == "primary" ]]; then
  GESTURES="${DEST}/TrackpadCameraControl.Gestures.dll"
  if [[ ! -f "${GESTURES}" ]]; then
    echo "Missing ${GESTURES} — Cities cannot load the IUserMod without the gesture library." >&2
    exit 1
  fi
  echo "Gestures → ${GESTURES}"
  echo "Capture: in-process AppKit → style-table Policy → Apply (Mods/TrackpadCameraControl)."
  echo "Inspect: tail -f \"\${TMPDIR:-/tmp}/trackpad-camera-control-rewrite.log\""
else
  echo "Capture: bootstrap prototype (in-process AppKit). Optional TrackpadBridge under bootstrap/src/TrackpadBridge."
  echo "Inspect: tail -f \"\${TMPDIR:-/tmp}/trackpad-camera-control.log\""
fi
echo "Debug panel footer: Built (UTC) + asm identity confirm the loaded build."
if [[ -f "${DEST}/PreviewImage.png" ]]; then
  echo "Preview → ${DEST}/PreviewImage.png"
fi
