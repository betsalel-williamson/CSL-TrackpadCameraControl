# Local MVP install (rewrite)

Deploy the rewrite into the **same** Cities Mods folder and Content Manager name as shipping. Last install wins. Player-facing install for the shipping mod stays under repo-root client docs until cutover.

## Deploy

From the repository root:

`./scripts/install-mod-local.sh --rewrite`

(Short form: `-r`.) Overwrites **`Mods/TrackpadCameraControl`** — the same path as `./scripts/install-mod-local.sh` (shipping). Content Manager shows one row: **Trackpad Camera Control (macOS)**. Which tree is loaded is the Debug footer / Copy assembly identity (`TrackpadCameraControl.Rewrite` vs `TrackpadCameraControl`), not a second checkbox.

The folder must contain **`TrackpadCameraControl.dll` and `TrackpadCameraControl.Gestures.dll`**. Missing the library is a Content Manager load failure. A shipping install removes the gesture library DLL so it does not linger beside the shipping mod.

| Path                                       | Role                                                                 |
| ------------------------------------------ | -------------------------------------------------------------------- |
| `./scripts/install-mod-local.sh`           | Shipping → Mods/`TrackpadCameraControl` (replaces rewrite)           |
| `./scripts/install-mod-local.sh --rewrite` | Rewrite → same folder, same Content Manager name (replaces shipping) |

Requires Cities: Skylines Managed assemblies (override with `CitiesManaged` / `CITIES_MODS` as with shipping).

## A/B — last install wins

Do **not** keep a parallel `TrackpadCameraControl.Rewrite` folder. Switch trees by reinstalling, then restart Cities (or rely on Automate reload when `AssemblyVersion` changes):

1. `./scripts/install-mod-local.sh --rewrite` — play rewrite; confirm Copy shows `TrackpadCameraControl.Rewrite`.
2. `./scripts/install-mod-local.sh` — play shipping; confirm Copy shows `TrackpadCameraControl` and Gestures.dll is gone.

Cities Harmony stays enabled for either path.

## In game

1. Enable **Cities Harmony**.
2. Enable **Trackpad Camera Control (macOS)** (the single local row).
3. Load a city; keep the game focused; exercise Maps+ chords.
4. Confirm Debug Copy assembly matches the tree you just installed.
5. Record results on the [QA checklist](./qa-checklist.md) parity matrix.

Capture remains in-process AppKit for ship-shaped builds. Do not use Contacts or bridge socket paths for rewrite parity QA unless a compile-flag experiment is explicitly under test ([Feature flags](./feature-flags.md)).

Capture remains in-process AppKit for ship-shaped builds. Do not use Contacts or bridge socket paths for rewrite parity QA unless a compile-flag experiment is explicitly under test ([Feature flags](./feature-flags.md)).

## Related

- [Harnesses and testing](./harnesses-and-testing.md)
- [QA checklist](./qa-checklist.md)
- [Repository layout](./repository-layout.md)
