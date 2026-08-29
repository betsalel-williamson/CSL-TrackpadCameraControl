# csl-trackpad-camera-control

## 0.2.0

### Minor Changes

- 0e2e5da: C# TrackpadCapture + TrackpadBridge host (retire C), LangVersion 9 pin, xUnit/headless e2e, and in-game inject smoke harness.
- f23d9ad: Pinch → zoom MVP: macOS TrackpadBridge IPC, shared GestureFrame protocol, and C# camera pipeline with IGestureSource seam for a future in-process deploy path.
- f5b06ab: Add commitlint, husky, and changesets; publish the package as public npm (`publishConfig.access`).

### Patch Changes

- 9f8b233: Document optional Assist UI camera chrome (design, feature, and client guides).
- f8f834d: Upgrade @changesets/cli to v3 and align release workflow inputs with changesets/action v2.
- 93a7f2a: Pin GitHub Actions by commit SHA (checkout v7, setup-node v7, setup-dotnet v6, changesets/action v2) instead of floating version tags.
- f0244a9: Add macOS-first `scripts/bootstrap-dev.sh` (and npm `bootstrap*` scripts) so contributors can scriptably install and verify host + project tooling.
- 1d31a27: Add husky pre-commit (lint-staged) and pre-push (format:check + docs) hooks with shared npm scripts for consistent local gates.
- 23a5535: Require Node 22.12+, stop overwriting husky hooksPath, and simplify pre-push/CI scoping (no eval, no paths-filter fan-out).
