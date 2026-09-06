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
2. Prefer moving `Addons/Mods/TrackpadCameraControl` aside so Content Manager shows the **Workshop** row only.
3. `./scripts/stage-workshop-update.sh` — backs up, then writes **1.0.x** into the subscribed Steam folder (`…/workshop/content/255710/3796575080`) and into the newest open Update staging package (`PreviewImage.png` beside `Content/`). Optional `--install`.
4. In Cities → Workshop row → **Update**, wait for **Committing changes**.
5. If using the folder-icon flow: open Update → folder icon first, then re-run the script so staging matches Steam.

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
