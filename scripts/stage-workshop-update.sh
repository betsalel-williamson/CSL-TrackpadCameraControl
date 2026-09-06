#!/usr/bin/env bash
# Fill the open Content Manager Update staging folder from live Mods.
# Does NOT publish to Steam by itself — you must click Update in Cities after.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

LIVE="${CITIES_MODS:-$HOME/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}/TrackpadCameraControl"
STAGING_ROOT="${CITIES_WORKSHOP_STAGING:-$HOME/Library/Application Support/Colossal Order/Cities_Skylines/WorkshopStagingArea}"
BACKUPS="$STAGING_ROOT/_backups"
LOCAL_MODS_DIR="$(dirname "$LIVE")"

usage() {
  cat <<'EOF'
Usage: ./scripts/stage-workshop-update.sh [--install]

  1) In Cities: Workshop row → Update → folder icon (opens staging)
  2) Run this script
  3) Back in Cities: click Update → wait for "Committing changes"

  Writes live Mods DLLs into staging Content/, PreviewImage.png beside Content/.
  Never writes into steamapps/workshop (that only fakes a local version).

  --install   build+install live Mods first (keep Addons/Mods clear when updating)
EOF
}

case "${1:-}" in
  -h | --help) usage; exit 0 ;;
  --install) bash "$ROOT/scripts/install-mod-local.sh" ;;
  "") ;;
  *) usage >&2; exit 2 ;;
esac

for f in TrackpadCameraControl.dll TrackpadCameraControl.Gestures.dll PreviewImage.png; do
  [[ -f "$LIVE/$f" ]] || {
    echo "Missing $LIVE/$f — run with --install (then move the Mods folder aside before Update)." >&2
    exit 1
  }
done

# Prefer newest staging folder (the one the Update dialog just opened).
STAGING=""
best_m=0
shopt -s nullglob
for d in "$STAGING_ROOT"/*/; do
  [[ "$(basename "${d%/}")" == _backups ]] && continue
  [[ -d "${d}Content" ]] || continue
  m=$(stat -f '%m' "${d%/}")
  if [[ "$m" -ge "$best_m" ]]; then
    best_m=$m
    STAGING="${d%/}"
  fi
done

[[ -n "$STAGING" ]] || {
  echo "No open Update staging under:" >&2
  echo "  $STAGING_ROOT" >&2
  echo "In Cities: Content Manager → Workshop item → Update → folder icon, then re-run." >&2
  exit 1
}

CONTENT="$STAGING/Content"
mkdir -p "$CONTENT"

# Warn if a local Mods copy will compete with the Workshop row.
if [[ -f "$LIVE/TrackpadCameraControl.dll" ]]; then
  echo "Note: local Mods copy exists at:"
  echo "  $LIVE"
  echo "Official Update flow: move that folder aside so only the Workshop row remains."
  echo
fi

BACKUP="$BACKUPS/$(basename "$STAGING")/$(date +%Y%m%d-%H%M%S)"
mkdir -p "$BACKUP/Content"
[[ -f "$STAGING/PreviewImage.png" ]] && cp -f "$STAGING/PreviewImage.png" "$BACKUP/"
for f in "$CONTENT"/*; do
  [[ -f "$f" ]] || continue
  cp -f "$f" "$BACKUP/Content/"
done

cp -f "$LIVE/PreviewImage.png" "$STAGING/PreviewImage.png"
rm -f "$CONTENT/PreviewImage.png" "$STAGING/.DS_Store" "$CONTENT/.DS_Store"
cp -f "$LIVE/TrackpadCameraControl.dll" "$CONTENT/"
cp -f "$LIVE/TrackpadCameraControl.Gestures.dll" "$CONTENT/"
[[ -f "$LIVE/CitiesHarmony.API.dll" ]] && cp -f "$LIVE/CitiesHarmony.API.dll" "$CONTENT/"

echo "Backup → $BACKUP"
echo "Stage  → $STAGING"
ls -la "$STAGING/PreviewImage.png" "$CONTENT"
echo
echo "Now in Cities: click Update and wait for Committing changes / Workshop page."
echo "Do not click Share on a local 1.0.2 row — that creates a second Workshop item."
