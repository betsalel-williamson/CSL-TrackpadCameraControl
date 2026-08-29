# Repository layout

Target layout (phase 1 has docs only; later phases fill code trees):

```text
CSL-TrackpadCameraControl/
  docs/                 # MDCP shards (source of truth for intent)
  mod/                  # C# Cities: Skylines I mod scaffold (CitiesHarmony later)
  native/mac/           # TrackpadBridge (format-ready C seed)
  native/win/           # stub
  native/linux/         # stub
  shared/protocol/      # IPC frame schema (later)
  templates/            # Copy-paste scaffolds for quick development
  scripts/              # Contributor helpers (e.g. clang-format)
  .github/workflows/    # Docs + code-format CI
  TrackpadCameraControl.sln
  README.md
  LICENSE
  package.json          # docs + format orchestration
```

## Naming

| Surface | Name |
| --- | --- |
| Display / Workshop | Trackpad Camera Control |
| GitHub repo | CSL-TrackpadCameraControl |
| npm package (docs) | csl-trackpad-camera-control |

Search keywords: trackpad, touchpad, multitouch, pinch, camera, Cities Skylines, orbit, pan, zoom (plus Mac/Windows as backend tags, not product identity).
