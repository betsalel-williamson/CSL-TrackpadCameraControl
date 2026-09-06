# Release process (rewrite)

**Audience:** Maintainers shipping the `TrackpadCameraControl.Rewrite` DLL for beta playtests and eventual cutover.

## Version sources

| Surface                                | Value                                              | When                                   |
| -------------------------------------- | -------------------------------------------------- | -------------------------------------- |
| Options / Content Manager title        | `package.json` semver (`BuildInfo.ProductVersion`) | Always                                 |
| `FileVersion` / `InformationalVersion` | Same semver                                        | Release and dev                        |
| `AssemblyVersion`                      | `Major.Minor.*` (wildcard build/revision)          | Always — Cities auto-reload during dev |

Product semver bumps ride the monorepo Changesets flow — see root [commits and releases](../../../docs/developer/commits-and-releases.md). There is no separate `rewrite/package.json`.

## Dev vs release build identity

MSBuild property `DevBuildIdentity` (default **true** for local builds):

| `DevBuildIdentity` | Debug panel title                              | Debug footer (UTC/local build time) |
| ------------------ | ---------------------------------------------- | ----------------------------------- |
| `true` (dev)       | Assembly identity `Major.Minor.Build.Revision` | Shown                               |
| `false` (release)  | Product semver only                            | Hidden                              |

Release CI / packaging should build with:

```bash
dotnet build rewrite/mod/TrackpadCameraControl.Rewrite.csproj -p:DevBuildIdentity=false
```

Options and Content Manager always show semver only — never assembly wildcard identity.

## Deploy path

Local install for parity QA:

```bash
./scripts/install-mod-local.sh --rewrite
```

Deploy folder: `Mods/TrackpadCameraControl.Rewrite` (parallel to shipping `TrackpadCameraControl`).

## Pre-ship checklist

1. `npm run version-packages` merged on `main` when user-facing version changes.
2. `dotnet test TrackpadCameraControl.sln` green (rewrite + shipping).
3. `npm run sa:rewrite` PASS.
4. `npm run docs:rewrite` PASS.
5. Release build with `-p:DevBuildIdentity=false`.
6. [In-game parity checklist](./in-game-parity-checklist.md) tier C signed off.

## Related

- [Logging](./logging.md)
- [Local MVP install](./local-mvp-install.md)
- Root [release process](../../../docs/developer/release-process.md) (Workshop / GitHub Releases)
