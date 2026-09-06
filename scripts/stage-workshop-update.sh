#!/usr/bin/env bash
# Sync the subscribed Steam Workshop mod (and open Update staging) from live Mods.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

LIVE="${CITIES_MODS:-$HOME/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}/TrackpadCameraControl"
STEAM_WS="${CITIES_WORKSHOP_CONTENT:-$HOME/Library/Application Support/Steam/steamapps/workshop/content/255710/3796575080}"
STAGING_ROOT="${CITIES_WORKSHOP_STAGING:-$HOME/Library/Application Support/Colossal Order/Cities_Skylines/WorkshopStagingArea}"
BACKUPS="$STAGING_ROOT/_backups"

usage() {
  cat <<'EOF'
Usage: ./scripts/stage-workshop-update.sh [--install] [--restore]

  Copies live Mods/TrackpadCameraControl into:
    1) Steam subscribed folder (…/workshop/content/255710/3796575080)
    2) Newest open Update staging folder, if any
         PreviewImage.png beside Content/; DLLs only in Content/

  Then in Cities: Workshop row → Update → wait for Committing changes.

  --install   build+install live Mods first
  --restore   restore newest Steam-folder backup
EOF
}

DO_INSTALL=0
RESTORE=0
case "${1:-}" in
  -h | --help) usage; exit 0 ;;
  --install) DO_INSTALL=1 ;;
  --restore) RESTORE=1 ;;
  "") ;;
  *) usage >&2; exit 2 ;;
esac

[[ "$DO_INSTALL" == 1 ]] && bash "$ROOT/scripts/install-mod-local.sh"

stamp() { date +%Y%m%d-%H%M%S; }

newest_staging() {
  local best="" best_m=0 d m
  shopt -s nullglob
  for d in "$STAGING_ROOT"/*/; do
    [[ "$(basename "${d%/}")" == _backups ]] && continue
    [[ -f "${d}Content/TrackpadCameraControl.dll" ]] || continue
    m=$(stat -f '%m' "${d%/}")
    if [[ "$m" -ge "$best_m" ]]; then
      best_m=$m
      best="${d%/}"
    fi
  done
  [[ -n "$best" ]] && echo "$best"
}

if [[ "$RESTORE" == 1 ]]; then
  latest="$(ls -td "$BACKUPS/steam-workshop"/*/ 2>/dev/null | head -1 || true)"
  latest="${latest%/}"
  [[ -d "$latest" ]] || {
    echo "No Steam workshop backup under $BACKUPS/steam-workshop" >&2
    exit 1
  }
  cp -f "$latest"/* "$STEAM_WS/"
  echo "Restored Steam workshop from $latest"
  ls -la "$STEAM_WS"
  exit 0
fi

for f in TrackpadCameraControl.dll TrackpadCameraControl.Gestures.dll PreviewImage.png; do
  [[ -f "$LIVE/$f" ]] || {
    echo "Missing $LIVE/$f — run with --install first." >&2
    exit 1
  }
done

[[ -d "$STEAM_WS" ]] || {
  echo "Steam workshop folder missing: $STEAM_WS" >&2
  echo "Subscribe to item 3796575080 and let Steam download it." >&2
  exit 1
}

# --- Steam subscribed folder (flat) ---
STEAM_BACKUP="$BACKUPS/steam-workshop/$(stamp)"
mkdir -p "$STEAM_BACKUP"
shopt -s nullglob
for f in "$STEAM_WS"/*; do
  [[ -f "$f" ]] || continue
  cp -f "$f" "$STEAM_BACKUP/"
done
cp -f "$LIVE/TrackpadCameraControl.dll" "$STEAM_WS/"
cp -f "$LIVE/TrackpadCameraControl.Gestures.dll" "$STEAM_WS/"
cp -f "$LIVE/PreviewImage.png" "$STEAM_WS/"
[[ -f "$LIVE/CitiesHarmony.API.dll" ]] && cp -f "$LIVE/CitiesHarmony.API.dll" "$STEAM_WS/"
rm -rf "$STEAM_WS/Content"

echo "Steam  → $STEAM_WS  (was backed up to $STEAM_BACKUP)"
ls -la "$STEAM_WS"
echo -n "Steam DLL version: "
strings "$STEAM_WS/TrackpadCameraControl.dll" | rg '^1\.0\.[0-9]+$' | head -1

# --- Open Update staging, if any ---
STAGING="$(newest_staging || true)"
if [[ -n "$STAGING" ]]; then
  CONTENT="$STAGING/Content"
  STAGE_BACKUP="$BACKUPS/$(basename "$STAGING")/$(stamp)"
  mkdir -p "$STAGE_BACKUP/Content"
  [[ -f "$STAGING/PreviewImage.png" ]] && cp -f "$STAGING/PreviewImage.png" "$STAGE_BACKUP/"
  for f in "$CONTENT"/*; do
    [[ -f "$f" ]] || continue
    cp -f "$f" "$STAGE_BACKUP/Content/"
  done

  cp -f "$LIVE/PreviewImage.png" "$STAGING/PreviewImage.png"
  rm -f "$CONTENT/PreviewImage.png" "$STAGING/.DS_Store" "$CONTENT/.DS_Store"
  cp -f "$LIVE/TrackpadCameraControl.dll" "$CONTENT/"
  cp -f "$LIVE/TrackpadCameraControl.Gestures.dll" "$CONTENT/"
  [[ -f "$LIVE/CitiesHarmony.API.dll" ]] && cp -f "$LIVE/CitiesHarmony.API.dll" "$CONTENT/"

  echo
  echo "Stage  → $STAGING  (backed up to $STAGE_BACKUP)"
  ls -la "$STAGING/PreviewImage.png" "$CONTENT"
else
  echo
  echo "No Update staging folder open (Steam folder still updated)."
fi

echo
echo "Next: Cities → Content Manager → Workshop item → Update → wait for Committing changes."
echo "Move Addons/Mods/TrackpadCameraControl aside if you see a duplicate local row."
