# Release process

**Audience:** Maintainers shipping the Trackpad Camera Control DLL for beta playtests and Workshop.

## Version sources

| Surface                                | Value                                              | When                                   |
| -------------------------------------- | -------------------------------------------------- | -------------------------------------- |
| Options / Content Manager title        | `package.json` semver (`BuildInfo.ProductVersion`) | Always                                 |
| Debug panel title (default)            | Same product semver                                | Default / release builds               |
| `FileVersion` / `InformationalVersion` | Same semver                                        | Release and dev                        |
| `AssemblyVersion`                      | `Major.Minor.*` (wildcard build/revision)          | Always — Cities auto-reload during dev |

Product semver bumps ride the monorepo Changesets flow on `main`.

## Dev vs release build identity

MSBuild property `DevBuildIdentity` (default **false** — player-facing semver only):

| `DevBuildIdentity` | Debug panel title                              | Debug footer (UTC/local build time) |
| ------------------ | ---------------------------------------------- | ----------------------------------- |
| `false` (default)  | Product semver only                            | Hidden                              |
| `true` (dev)       | Assembly identity `Major.Minor.Build.Revision` | Shown                               |

Local reload QA:

```bash
./scripts/install-mod-local.sh --dev
# or
dotnet build mod/TrackpadCameraControl.Rewrite.csproj -p:DevBuildIdentity=true
```

Options and Content Manager always show semver only — never assembly wildcard identity.

## Deploy path

```bash
./scripts/install-mod-local.sh
```

Deploy folder: `Mods/TrackpadCameraControl` (last install wins).

## Steam Workshop Update (same item)

Do **not** Share a second item. Update [Workshop item 3796575080](https://steamcommunity.com/sharedfiles/filedetails/?id=3796575080).

Patching `steamapps/workshop/content/…` only changes what the game loads locally. It does **not** upload to Steam. Content Manager can show **1.0.2** while the Workshop page stays on the old build until Update commits.

### Missing **Update** button

Workshop rows often hide Update (known CS1 issue). Steam launch option (exact spelling):

```text
--refreshWorkshop
```

Restart Cities, confirm **Update** on the Workshop row, finish the upload, then **remove** the launch option.

Local `Addons/Mods/TrackpadCameraControl` shows **Share** (unpublished local). Do **not** Share that — it can create a duplicate Workshop item. Move/delete the local Mods folder before updating.

### Staging layout

```text
WorkshopStagingArea/<guid>/
  PreviewImage.png          ← beside Content/ (thumbnail)
  Content/
    TrackpadCameraControl.dll
    TrackpadCameraControl.Gestures.dll
    CitiesHarmony.API.dll
```

`PreviewImage.png` must **not** live inside `Content/`.

### Steps

1. Quit Cities. If **Update** is missing, add Steam launch option `--refreshWorkshop` (exact spelling), then relaunch.
2. Keep `Addons/Mods/TrackpadCameraControl` **absent** so only the Workshop row appears. Do **not** click **Share** on a local row.
3. Stay subscribed to your own Workshop item.
4. `./scripts/install-mod-local.sh`, then **move that Mods folder aside** again.
5. Cities → Content Manager → Mods → Workshop row → **Update** → folder icon.
6. `./scripts/stage-workshop-update.sh` (fills that staging package; backs up under `_backups/`).
7. Click **Update**, wait for **Committing changes** / Workshop page. Confirm a new change note on Steam; remove `--refreshWorkshop`.

## Pre-ship checklist

1. Land user-facing work on `main` with a `.changeset/*.md` when the product version should bump.
2. Release workflow opens **`chore: version packages`** — merge it to bump `package.json` / `CHANGELOG.md`.
3. Same workflow then tags (`vX.Y.Z`) and creates the GitHub Release (no separate skipped job).
4. `dotnet test TrackpadCameraControl.sln` green (PR **Validate** check).
5. `npm run docs` PASS.
6. Default install (no `--dev`) — Options/Debug show product semver only.
7. [In-game parity checklist](./in-game-parity-checklist.md) tier C signed off.

## Related

- [Logging](./logging.md)
- [Local MVP install](./local-mvp-install.md)
