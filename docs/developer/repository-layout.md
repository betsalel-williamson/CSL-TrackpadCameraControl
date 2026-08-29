# Repository layout

Target layout (phase 1 has docs only; later phases fill code trees):

```text
CSL-TrackpadCameraControl/
  docs/                 # MDCP shards (source of truth for intent)
  mod/                  # C# Cities: Skylines I mod (CitiesHarmony Patcher for vanilla camera suppress)
  src/TrackpadCapture/  # Multitouch → GestureFrame (compiled into the mod DLL; also used by optional bridge)
  src/TrackpadBridge/   # Optional IPC console host (not the playtest path)
  src/AppleGestureProbe/ # Spike: C# AppKit gesture logger (macOS, not a backend)
  tests/                # xUnit unit + headless e2e
  native/               # Retired C helper notes / stubs (no shipping bridge)
  shared/protocol/      # GestureFrame wire layout
  templates/            # Copy-paste scaffolds for quick development
  scripts/              # bootstrap-dev, install, e2e smoke helpers
  infra/github/         # OpenTofu + Makefile for GitHub project controls
  .changeset/           # Pending release notes (Changesets)
  .github/workflows/    # Docs, format, commitlint, release CI
  TrackpadCameraControl.sln
  README.md
  LICENSE
  package.json          # docs + format orchestration
```

## Naming

| Surface            | Name                                                            |
| ------------------ | --------------------------------------------------------------- |
| Display / Workshop | Trackpad Camera Control                                         |
| GitHub repo        | CSL-TrackpadCameraControl                                       |
| npm workspace name | csl-trackpad-camera-control (local tooling only; not published) |

Search keywords: trackpad, touchpad, multitouch, pinch, camera, Cities Skylines, orbit, pan, zoom (plus Mac/Windows as backend tags, not product identity).
