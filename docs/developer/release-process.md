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

Official Content Manager shape (folder icon on the Update dialog):

```text
WorkshopStagingArea/<guid>/
  PreviewImage.png          ← beside Content/ (thumbnail)
  Content/
    TrackpadCameraControl.dll
    TrackpadCameraControl.Gestures.dll
    CitiesHarmony.API.dll   ← if present in the live Mods folder
```

`PreviewImage.png` must **not** live inside `Content/` (copying the whole Mods folder into `Content/` is the usual mistake).

1. `./scripts/install-mod-local.sh` — release build into live Mods (product semver).
2. In Cities: Content Manager → Mods → **Workshop** row → **Update** → folder icon (creates/opens staging).
3. `./scripts/stage-workshop-update.sh` — backs up the current staging package under `WorkshopStagingArea/_backups/`, then replaces staging `Content/` DLLs and root `PreviewImage.png` from the live Mods folder. Use `--install` to do step 1 first; `--staging PATH` to target a specific staging folder; `--restore` to put the newest backup back.
4. Back in Cities → **Update**, wait for **Committing changes**. Do not cancel mid-upload.
5. Optional: temporarily move the local Mods copy out before Update if Content Manager shows duplicate rows.

Paste-ready title / description / tags: [Workshop storefront](./workshop-storefront.md).

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
