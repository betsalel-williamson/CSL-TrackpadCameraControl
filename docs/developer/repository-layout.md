# Repository layout

Target layout (phase 1 has docs only; later phases fill code trees):

```text
CSL-TrackpadCameraControl/
  docs/                 # MDCP shards (source of truth for intent)
  mod/                  # C# Cities: Skylines I mod (CitiesHarmony Patcher for vanilla camera suppress)
                          # PreviewImage.png — Content Manager / Workshop thumbnail
  src/TrackpadCapture/  # Multitouch → GestureFrame (compiled into the mod DLL; also used by optional bridge)
  src/TrackpadCapture/  # Multitouch → GestureFrame (compiled into the mod DLL; also used by optional bridge)
  src/TrackpadBridge/   # Optional dev socket host (TrackpadBridge); playtest uses in-process capture
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

| Surface                 | Name                                                                        |
| ----------------------- | --------------------------------------------------------------------------- |
| Core display name       | Trackpad Camera Control                                                     |
| Display / Workshop (v1) | Trackpad Camera Control (macOS) — temporary tag; drop when another OS ships |
| GitHub repo             | CSL-TrackpadCameraControl                                                   |
| npm workspace name      | csl-trackpad-camera-control (local tooling only; not published)             |

Folder and assembly stay `TrackpadCameraControl` forever. Paste-ready title, description, and SEO tags: [Workshop storefront](./workshop-storefront.md). Search keywords: trackpad, touchpad, multitouch, pinch, camera, mac, macos, macbook, Cities Skylines, orbit, pan, zoom (Mac is a backend/discoverability tag, not durable product identity).
