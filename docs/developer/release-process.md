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

## Pre-ship checklist

1. `npm run version-packages` merged on `main` when user-facing version changes.
2. `dotnet test TrackpadCameraControl.sln` green.
3. `npm run docs` PASS.
4. Default install (no `--dev`) — Options/Debug show product semver only.
5. [In-game parity checklist](./in-game-parity-checklist.md) tier C signed off.

## Related

- [Logging](./logging.md)
- [Local MVP install](./local-mvp-install.md)
