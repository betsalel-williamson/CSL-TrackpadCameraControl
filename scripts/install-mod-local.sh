#!/usr/bin/env bash
# Build TrackpadCameraControl.dll. Default: MSBuild post-build deploys into the local
# CS1 Mods folder (Paradox Advanced Mod Setup → Automate).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"

SYMLINK=0
while [[ $# -gt 0 ]]; do
  case "$1" in
    --symlink)
      SYMLINK=1
      shift
      ;;
    -h | --help)
      echo "Usage: $0 [--symlink]"
      echo "  Default: build + post-build copy into Mods (wiki Automate)."
      echo "  --symlink  Skip post-build copy; link Mods DLL to bin/Release output."
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      exit 1
      ;;
  esac
done

MANAGED="${CitiesManaged:-${HOME}/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed}"
MODS="${CITIES_MODS:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}"
DEST="${MODS}/TrackpadCameraControl"
DLL_SRC="${ROOT}/mod/bin/Release/net35/TrackpadCameraControl.dll"

if [[ ! -f "${MANAGED}/ICities.dll" ]]; then
  echo "ICities.dll not found at: ${MANAGED}" >&2
  echo "Set CitiesManaged to your Cities_Data/Managed (or Contents/Resources/Data/Managed) path." >&2
  exit 1
fi

BUILD_ARGS=(
  "${ROOT}/mod/TrackpadCameraControl.csproj"
  -c Release
  "-p:CitiesManaged=${MANAGED}"
  "-p:CitiesMods=${MODS}"
)

if [[ "${SYMLINK}" == 1 ]]; then
  BUILD_ARGS+=("-p:SkipModDeploy=true")
fi

dotnet build "${BUILD_ARGS[@]}"
mkdir -p "${DEST}"

if [[ "${SYMLINK}" == 1 ]]; then
  ln -sf "${DLL_SRC}" "${DEST}/TrackpadCameraControl.dll"
  PREVIEW="${ROOT}/mod/PreviewImage.png"
  if [[ -f "${PREVIEW}" ]]; then
    cp -f "${PREVIEW}" "${DEST}/PreviewImage.png"
  fi
  API_DLL="${ROOT}/mod/bin/Release/net35/CitiesHarmony.API.dll"
  if [[ -f "${API_DLL}" ]]; then
    cp -f "${API_DLL}" "${DEST}/"
  fi
  echo "Symlinked → ${DEST}/TrackpadCameraControl.dll"
  echo "See docs/developer/mod-reload-during-development.md"
else
  echo "Build finished (post-build should have deployed to ${DEST})."
  echo "Cities auto-reloads when AssemblyVersion changes — see mod-reload-during-development.md"
fi

echo "Capture: in-process AppKit (default). No companion process."
echo "Debug panel footer: Built (UTC) + asm identity confirm the loaded build."
echo "Inspect: tail -f \"\${TMPDIR:-/tmp}/trackpad-camera-control.log\""
