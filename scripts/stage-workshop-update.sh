#!/usr/bin/env bash
# Sync Cities Workshop staging from the live local Mods install.
# PreviewImage.png sits beside Content/; DLLs go only inside Content/.
# Previous staging files are copied to WorkshopStagingArea/_backups/ first.
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"

MODS="${CITIES_MODS:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods}"
LIVE="${MODS}/TrackpadCameraControl"
STAGING_ROOT="${CITIES_WORKSHOP_STAGING:-${HOME}/Library/Application Support/Colossal Order/Cities_Skylines/WorkshopStagingArea}"
BACKUP_ROOT="${STAGING_ROOT}/_backups"

DO_INSTALL=0
STAGING=""
OPEN_UPDATE=0
RESTORE=""
SKIP_BACKUP=0

usage() {
  cat <<'EOF'
Usage: ./scripts/stage-workshop-update.sh [options]

  Copies the live Mods/TrackpadCameraControl build into an open Workshop
  staging folder (Content Manager → Update → folder icon).

  Layout written (official Update staging shape):
    <staging>/PreviewImage.png
    <staging>/Content/TrackpadCameraControl.dll
    <staging>/Content/TrackpadCameraControl.Gestures.dll
    <staging>/Content/CitiesHarmony.API.dll   (if present in live install)

  PreviewImage is never left inside Content/.

  Before replacing, copies the current staging files to:
    WorkshopStagingArea/_backups/<staging-id>/<timestamp>/

  --install | -i     Run ./scripts/install-mod-local.sh (release) first
  --staging PATH     Use this staging folder (must contain Content/)
  --open             Open the staged folder in Finder when done
  --no-backup        Skip the pre-replace backup
  --restore [PATH]   Restore a backup into the staging folder, then exit.
                     PATH may be omitted to use the newest backup for the
                     selected/auto staging folder.

  Without --staging, picks the newest WorkshopStagingArea/* folder that
  already contains Content/TrackpadCameraControl.dll.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -h | --help)
      usage
      exit 0
      ;;
    --install | -i)
      DO_INSTALL=1
      shift
      ;;
    --staging)
      STAGING="${2:-}"
      if [[ -z "${STAGING}" ]]; then
        echo "--staging requires a path" >&2
        exit 2
      fi
      shift 2
      ;;
    --open)
      OPEN_UPDATE=1
      shift
      ;;
    --no-backup)
      SKIP_BACKUP=1
      shift
      ;;
    --restore)
      if [[ $# -ge 2 && "${2}" != -* ]]; then
        RESTORE="${2}"
        shift 2
      else
        RESTORE="__LATEST__"
        shift
      fi
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage >&2
      exit 2
      ;;
  esac
done

find_staging() {
  local best=""
  local best_mtime=0
  local candidate content dll mtime
  if [[ ! -d "${STAGING_ROOT}" ]]; then
    return 1
  fi
  shopt -s nullglob
  for candidate in "${STAGING_ROOT}"/*/; do
    # Never treat the backup tree as a staging package.
    if [[ "$(basename "${candidate%/}")" == "_backups" ]]; then
      continue
    fi
    content="${candidate}Content"
    dll="${content}/TrackpadCameraControl.dll"
    if [[ ! -d "${content}" || ! -f "${dll}" ]]; then
      continue
    fi
    # Prefer the staging package whose Content DLL was touched most recently.
    mtime=$(stat -f '%m' "${dll}" 2>/dev/null || stat -c '%Y' "${dll}")
    if [[ "${mtime}" -ge "${best_mtime}" ]]; then
      best_mtime="${mtime}"
      best="${candidate%/}"
    fi
  done
  shopt -u nullglob
  if [[ -z "${best}" ]]; then
    return 1
  fi
  printf '%s\n' "${best}"
}

resolve_staging() {
  if [[ -n "${STAGING}" ]]; then
    return 0
  fi
  if ! STAGING="$(find_staging)"; then
    echo "No Workshop staging folder found under:" >&2
    echo "  ${STAGING_ROOT}" >&2
    echo "In Cities: Content Manager → Mods → your Workshop item → Update → folder icon," >&2
    echo "then re-run this script (or pass --staging PATH)." >&2
    exit 1
  fi
}

backup_staging() {
  local staging="$1"
  local content="${staging}/Content"
  local stamp staging_id dest
  stamp="$(date +%Y%m%d-%H%M%S)"
  staging_id="$(basename "${staging}")"
  dest="${BACKUP_ROOT}/${staging_id}/${stamp}"
  mkdir -p "${dest}/Content"

  if [[ -f "${staging}/PreviewImage.png" ]]; then
    cp -f "${staging}/PreviewImage.png" "${dest}/PreviewImage.png"
  fi
  if [[ -d "${content}" ]]; then
    # Copy whatever is currently staged (DLL set may differ by older uploads).
    find "${content}" -maxdepth 1 -type f ! -name '.DS_Store' -exec cp -f {} "${dest}/Content/" \;
  fi

  printf '%s\n' "${dest}"
}

restore_staging() {
  local staging="$1"
  local backup="$2"
  local content="${staging}/Content"
  local staging_id latest

  if [[ "${backup}" == "__LATEST__" ]]; then
    staging_id="$(basename "${staging}")"
    latest="$(
      find "${BACKUP_ROOT}/${staging_id}" -mindepth 1 -maxdepth 1 -type d 2>/dev/null \
        | sort \
        | tail -1 || true
    )"
    if [[ -z "${latest}" ]]; then
      echo "No backups found for staging ${staging_id} under ${BACKUP_ROOT}" >&2
      exit 1
    fi
    backup="${latest}"
  fi

  if [[ ! -d "${backup}" ]]; then
    echo "Backup path not found: ${backup}" >&2
    exit 1
  fi
  if [[ ! -d "${backup}/Content" ]]; then
    echo "Backup is missing Content/: ${backup}" >&2
    exit 1
  fi

  mkdir -p "${content}"
  rm -f "${content}/PreviewImage.png" "${staging}/.DS_Store" "${content}/.DS_Store"
  find "${content}" -maxdepth 1 -type f -name '*.dll' -delete
  find "${backup}/Content" -maxdepth 1 -type f ! -name '.DS_Store' -exec cp -f {} "${content}/" \;
  if [[ -f "${backup}/PreviewImage.png" ]]; then
    cp -f "${backup}/PreviewImage.png" "${staging}/PreviewImage.png"
  fi
  rm -f "${content}/PreviewImage.png"

  echo "Restored → ${staging}"
  echo "From     → ${backup}"
  ls -la "${staging}/PreviewImage.png" 2>/dev/null || true
  ls -la "${content}"
}

if [[ -n "${RESTORE}" ]]; then
  resolve_staging
  CONTENT="${STAGING}/Content"
  if [[ ! -d "${CONTENT}" ]]; then
    echo "Staging path has no Content/ directory: ${STAGING}" >&2
    exit 1
  fi
  restore_staging "${STAGING}" "${RESTORE}"
  if [[ "${OPEN_UPDATE}" == "1" ]] && command -v open >/dev/null 2>&1; then
    open "${STAGING}"
  fi
  exit 0
fi

if [[ "${DO_INSTALL}" == "1" ]]; then
  bash "${ROOT}/scripts/install-mod-local.sh"
fi

if [[ ! -d "${LIVE}" ]]; then
  echo "Live mod folder missing: ${LIVE}" >&2
  echo "Run ./scripts/install-mod-local.sh first (or pass --install)." >&2
  exit 1
fi

for required in TrackpadCameraControl.dll TrackpadCameraControl.Gestures.dll PreviewImage.png; do
  if [[ ! -f "${LIVE}/${required}" ]]; then
    echo "Live mod missing ${required} under ${LIVE}" >&2
    exit 1
  fi
done

resolve_staging

CONTENT="${STAGING}/Content"
if [[ ! -d "${CONTENT}" ]]; then
  echo "Staging path has no Content/ directory: ${STAGING}" >&2
  exit 1
fi

echo "Live  → ${LIVE}"
echo "Stage → ${STAGING}"

if [[ "${SKIP_BACKUP}" != "1" ]]; then
  BACKUP_PATH="$(backup_staging "${STAGING}")"
  echo "Backup → ${BACKUP_PATH}"
fi

# Preview beside Content/ (Workshop thumbnail), never inside Content/.
cp -f "${LIVE}/PreviewImage.png" "${STAGING}/PreviewImage.png"
rm -f "${CONTENT}/PreviewImage.png"
rm -f "${STAGING}/.DS_Store" "${CONTENT}/.DS_Store"

cp -f "${LIVE}/TrackpadCameraControl.dll" "${CONTENT}/TrackpadCameraControl.dll"
cp -f "${LIVE}/TrackpadCameraControl.Gestures.dll" "${CONTENT}/TrackpadCameraControl.Gestures.dll"
if [[ -f "${LIVE}/CitiesHarmony.API.dll" ]]; then
  cp -f "${LIVE}/CitiesHarmony.API.dll" "${CONTENT}/CitiesHarmony.API.dll"
fi

echo
echo "Staged layout:"
ls -la "${STAGING}/PreviewImage.png"
ls -la "${CONTENT}"

if [[ -f "${CONTENT}/PreviewImage.png" ]]; then
  echo "ERROR: PreviewImage.png still inside Content/ — aborting." >&2
  exit 1
fi

echo
echo "Back in Cities: confirm the Update preview, then click Update and wait for Committing changes."
echo "Revert staging: ./scripts/stage-workshop-update.sh --restore"
if [[ "${OPEN_UPDATE}" == "1" ]] && command -v open >/dev/null 2>&1; then
  open "${STAGING}"
fi
