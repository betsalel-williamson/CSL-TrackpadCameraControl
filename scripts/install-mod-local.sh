#!/usr/bin/env bash
# Build TrackpadCameraControl.dll and copy into the local CS1 Mods folder (macOS).
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
      echo "  --symlink  Link Mods/TrackpadCameraControl.dll to bin/Release output (dev loop)."
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

dotnet build "${ROOT}/mod/TrackpadCameraControl.csproj" -c Release -p:CitiesManaged="${MANAGED}"
mkdir -p "${DEST}"
if [[ "${SYMLINK}" == 1 ]]; then
  ln -sf "${DLL_SRC}" "${DEST}/TrackpadCameraControl.dll"
  echo "Symlinked → ${DEST}/TrackpadCameraControl.dll"
else
  cp -f "${DLL_SRC}" "${DEST}/"
  echo "Installed → ${DEST}/TrackpadCameraControl.dll"
fi
PREVIEW="${ROOT}/mod/PreviewImage.png"
if [[ -f "${PREVIEW}" ]]; then
  cp -f "${PREVIEW}" "${DEST}/PreviewImage.png"
fi
API_DLL="${ROOT}/mod/bin/Release/net35/CitiesHarmony.API.dll"
if [[ -f "${API_DLL}" ]]; then
  cp -f "${API_DLL}" "${DEST}/"
fi
# CitiesHarmony.Harmony.dll is provided by the Cities Harmony workshop mod — do not copy it.
if [[ -f "${DEST}/PreviewImage.png" ]]; then
  echo "Preview → ${DEST}/PreviewImage.png"
fi
if [[ "${SYMLINK}" == 1 ]]; then
  echo "Dev loop: dotnet build mod/TrackpadCameraControl.csproj -c Release, then disable/enable mod in Content Manager."
  echo "See docs/developer/mod-reload-during-development.md"
else
  echo "Restart Cities: Skylines, or use the Content Manager reload loop (see mod-reload-during-development.md)."
fi
echo "Capture: in-process AppKit (default). No companion process."
echo "Options → Trackpad Camera Control: AppKit vs Contacts (legacy), plus sensitivities."
echo "Inspect: tail -f \"\${TMPDIR:-/tmp}/trackpad-camera-control.log\""
