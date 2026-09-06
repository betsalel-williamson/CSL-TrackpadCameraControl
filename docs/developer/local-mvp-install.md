# Local MVP install

Deploy the primary mod into Cities Mods. Last install wins. Player-facing steps stay in the client guide (`docs/client/install-and-first-run.md`).

## Deploy

From the repository root:

`./scripts/install-mod-local.sh`

Overwrites **`Mods/TrackpadCameraControl`**. Content Manager shows one row: **Trackpad Camera Control (macOS)**. Options and Debug titles show **product semver** (`1.0.0`) by default.

The folder must contain **`TrackpadCameraControl.dll` and `TrackpadCameraControl.Gestures.dll`**. Missing the library is a Content Manager load failure.

| Path                                         | Role                                                                     |
| -------------------------------------------- | ------------------------------------------------------------------------ |
| `./scripts/install-mod-local.sh`             | Primary → Mods/`TrackpadCameraControl` (product semver in Options/Debug) |
| `./scripts/install-mod-local.sh --dev`       | Same, with assembly build/revision + Built footer for reload QA          |
| `./scripts/install-mod-local.sh --bootstrap` | Historical prototype under `bootstrap/` → same Mods folder               |
| `./scripts/install-mod-local.sh --rewrite`   | Alias for primary (kept for older docs/scripts)                          |

Requires Cities: Skylines Managed assemblies (override with `CitiesManaged` / `CITIES_MODS`).

## A/B — last install wins

Do **not** keep a parallel `TrackpadCameraControl.Rewrite` folder. Switch trees by reinstalling, then restart Cities (or rely on Automate reload when `AssemblyVersion` changes):

1. `./scripts/install-mod-local.sh` — play primary; Options/Debug show product semver.
2. `./scripts/install-mod-local.sh --bootstrap` — play prototype; confirm Gestures.dll is gone.

Cities Harmony stays enabled for either path.

## In game

1. Enable **Cities Harmony**.
2. Enable **Trackpad Camera Control (macOS)** (the single local row).
3. Load a city; keep the game focused; exercise Maps+ chords.
4. Confirm Options / Debug titles show product semver (use `--dev` only when checking assembly reload identity).
5. Record results on the [QA checklist](./qa-checklist.md) parity matrix.

Capture remains in-process AppKit for ship-shaped builds. Do not use Contacts or bridge socket paths for parity QA unless a compile-flag experiment is explicitly under test ([Feature flags](./feature-flags.md)).

## Related

- [Harnesses and testing](./harnesses-and-testing.md)
- [QA checklist](./qa-checklist.md)
- [Repository layout](./repository-layout.md)
- [Release process](./release-process.md) (dev vs release build identity)
