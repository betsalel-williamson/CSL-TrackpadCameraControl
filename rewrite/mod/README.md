# Rewrite mod assembly

`TrackpadCameraControl.Rewrite` — parallel playtest DLL (does not overwrite shipping `TrackpadCameraControl`).

## Layout

| Folder      | Plane / role                                                  |
| ----------- | ------------------------------------------------------------- |
| `Host/`     | IUserMod, LoadingExtension, Pipeline, Harmony Patcher         |
| `Capture/`  | GestureFrame contract, AppKit source/mapper, inject seam      |
| `Policy/`   | Gates, session, **style binding table** resolve               |
| `Apply/`    | Feel math → camera / selection (pitch clamp 0°–90°)           |
| `Settings/` | Trimmed feel + chrome; StyleTable seed                        |
| `Ui/`       | Options + Debug 1:1 with shipping (CAD/Contacts/Assist gated) |

## Build

```bash
dotnet build rewrite/mod/TrackpadCameraControl.Rewrite.csproj -c Release -p:EnableCitiesRefs=false
./scripts/install-mod-local.sh --rewrite   # when Cities Managed DLLs are present
```
