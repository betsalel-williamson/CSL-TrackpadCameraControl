#!/usr/bin/env bash
# Sync Workshop Update staging from the live Mods install.
# PreviewImage.png beside Content/; DLLs only inside Content/.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

LIVE="${CITIES_MODS:-$HOME/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}/TrackpadCameraControl"
STAGING_ROOT="${CITIES_WORKSHOP_STAGING:-$HOME/Library/Application Support/Colossal Order/Cities_Skylines/WorkshopStagingArea}"
BACKUPS="$STAGING_ROOT/_backups"

usage() {
  cat <<'EOF'
Usage: ./scripts/stage-workshop-update.sh [--install] [--restore [BACKUP]]

  Open Content Manager → Update → folder icon first, then run this.

  --install   ./scripts/install-mod-local.sh first
  --restore   put the newest backup back (or BACKUP path)
EOF
}

DO_INSTALL=0
RESTORE=""
case "${1:-}" in
  -h | --help) usage; exit 0 ;;
  --install) DO_INSTALL=1; shift ;;
  --restore)
    RESTORE="${2:-latest}"
    [[ "${RESTORE}" != latest ]] && shift
    shift
    ;;
  "") ;;
  *) usage >&2; exit 2 ;;
esac

# Newest staging folder that already has our mod DLL (skip _backups).
pick_staging() {
  local d
  shopt -s nullglob
  for d in "$STAGING_ROOT"/*/; do
    [[ "$(basename "${d%/}")" == _backups ]] && continue
    if [[ -f "${d}Content/TrackpadCameraControl.dll" ]]; then
      echo "${d%/}"
      return 0
    fi
  done
  return 1
}

STAGING="$(pick_staging)" || {
  echo "No staging folder with Content/TrackpadCameraControl.dll under:" >&2
  echo "  $STAGING_ROOT" >&2
  echo "In Cities: Update → folder icon, then re-run." >&2
  exit 1
}
CONTENT="$STAGING/Content"

if [[ -n "$RESTORE" ]]; then
  if [[ "$RESTORE" == latest ]]; then
    RESTORE=""
    shopt -s nullglob
    for d in "$BACKUPS/$(basename "$STAGING")"/*/; do
      RESTORE="${d%/}"
    done
  fi
  [[ -n "$RESTORE" && -d "$RESTORE/Content" ]] || {
    echo "Backup not found: ${RESTORE:-<none>}" >&2
    exit 1
  }
  rm -f "$CONTENT"/*.dll "$CONTENT/PreviewImage.png"
  shopt -s nullglob
  for f in "$RESTORE/Content"/*; do
    [[ -f "$f" ]] || continue
    cp -f "$f" "$CONTENT/"
  done
  rm -f "$CONTENT/PreviewImage.png"
  [[ -f "$RESTORE/PreviewImage.png" ]] && cp -f "$RESTORE/PreviewImage.png" "$STAGING/PreviewImage.png"
  echo "Restored from $RESTORE"
  ls -la "$STAGING/PreviewImage.png" "$CONTENT"
  exit 0
fi

[[ "$DO_INSTALL" == 1 ]] && bash "$ROOT/scripts/install-mod-local.sh"

for f in TrackpadCameraControl.dll TrackpadCameraControl.Gestures.dll PreviewImage.png; do
  [[ -f "$LIVE/$f" ]] || {
    echo "Missing $LIVE/$f — run ./scripts/install-mod-local.sh first." >&2
    exit 1
  }
done

BACKUP="$BACKUPS/$(basename "$STAGING")/$(date +%Y%m%d-%H%M%S)"
mkdir -p "$BACKUP/Content"
[[ -f "$STAGING/PreviewImage.png" ]] && cp -f "$STAGING/PreviewImage.png" "$BACKUP/"
shopt -s nullglob
for f in "$CONTENT"/*; do
  [[ -f "$f" ]] || continue
  base="$(basename "$f")"
  [[ "$base" == PreviewImage.png || "$base" == .DS_Store ]] && continue
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
echo "In Cities: click Update and wait for Committing changes."
