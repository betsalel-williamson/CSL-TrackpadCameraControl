#!/usr/bin/env bash
# Build TrackpadCameraControl.dll and copy into the local CS1 Mods folder (macOS).
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
export PATH="${HOME}/.dotnet:${PATH}"

MANAGED="${CitiesManaged:-${HOME}/Library/Application Support/Steam/steamapps/common/Cities_Skylines/Cities.app/Contents/Resources/Data/Managed}"
MODS="${CITIES_MODS:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}"
DEST="${MODS}/TrackpadCameraControl"

if [[ ! -f "${MANAGED}/ICities.dll" ]]; then
  echo "ICities.dll not found at: ${MANAGED}" >&2
  echo "Set CitiesManaged to your Cities_Data/Managed (or Contents/Resources/Data/Managed) path." >&2
  exit 1
fi

dotnet build "${ROOT}/mod/TrackpadCameraControl.csproj" -c Release -p:CitiesManaged="${MANAGED}"
mkdir -p "${DEST}"
cp -f "${ROOT}/mod/bin/Release/netstandard2.0/TrackpadCameraControl.dll" "${DEST}/"
echo "Installed → ${DEST}/TrackpadCameraControl.dll"
echo "Start the bridge before playing: dotnet run --project src/TrackpadBridge"
