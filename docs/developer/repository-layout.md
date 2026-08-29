# Repository layout

Target layout (phase 1 has docs only; later phases fill code trees):

```text
CSL-TrackpadCameraControl/
  docs/                 # MDCP shards (source of truth for intent)
  mod/                  # C# Cities: Skylines I mod (CitiesHarmony later)
  src/TrackpadCapture/  # Shared Multitouch → GestureFrame (netstandard2.0)
  src/TrackpadBridge/   # Dev IPC console host (dotnet run)
  tests/                # xUnit unit + headless e2e
  native/               # Retired C helper notes / stubs (no shipping bridge)
  shared/protocol/      # GestureFrame wire layout
  templates/            # Copy-paste scaffolds for quick development
  scripts/              # bootstrap-dev, install, e2e smoke helpers
  .changeset/           # Pending release notes (Changesets)
  .github/workflows/    # Docs, format, commitlint, release CI
  TrackpadCameraControl.sln
  README.md
  LICENSE
  package.json          # docs + format orchestration
```

## Naming

| Surface            | Name                        |
| ------------------ | --------------------------- |
| Display / Workshop | Trackpad Camera Control     |
| GitHub repo        | CSL-TrackpadCameraControl   |
| npm package (docs) | csl-trackpad-camera-control |

Search keywords: trackpad, touchpad, multitouch, pinch, camera, Cities Skylines, orbit, pan, zoom (plus Mac/Windows as backend tags, not product identity).
