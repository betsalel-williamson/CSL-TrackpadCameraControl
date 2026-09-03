# Rewrite mod assembly

`TrackpadCameraControl.Rewrite` — parallel playtest DLL (does not overwrite shipping `TrackpadCameraControl`).

**Quarantine:** These sources are a clone-and-strip experiment. Do not extend them as the v1 path. Rebuild from [UI parity](../docs/glossary/ui-parity.md) and [ADR 0005](../docs/features/adr/0005-ux-parity-not-source-parity.md) after the recovery spec is approved.

## Layout

| Folder      | Plane / role                                                             |
| ----------- | ------------------------------------------------------------------------ |
| `Host/`     | IUserMod, LoadingExtension, Pipeline, Harmony Patcher                    |
| `Capture/`  | GestureFrame contract, AppKit source/mapper, inject seam                 |
| `Policy/`   | Gates, session, **style binding table** resolve                          |
| `Apply/`    | Feel math → camera / selection (pitch clamp 0°–90°)                      |
| `Settings/` | Trimmed feel + chrome; StyleTable seed                                   |
| `Ui/`       | Cloned Options/Debug builders (quarantined; not the target feel catalog) |

## Build

```bash
dotnet build rewrite/mod/TrackpadCameraControl.Rewrite.csproj -c Release -p:EnableCitiesRefs=false
./scripts/install-mod-local.sh --rewrite   # when Cities Managed DLLs are present
```
