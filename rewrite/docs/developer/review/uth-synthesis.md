# Under-the-hood system review (2026-09-03)

**Audience:** Contributors judging whether the as-built `rewrite/` tree matches the under-the-hood redesign contracts.

**Procedure:** Project skill `system-architecture-review` (`.agents/skills/system-architecture-review/`). Specialist lanes ran in parallel; findings live in sibling shards.

**Overall verdict: Conditional**

The stack split is real (gesture library owns AppKit; mod consumes frames). Style-table resolve, FeelMath purity, one catalog / two hosts structure, fake-per-layer tests, and layer-import lint are strengths. Architecture is **not done**: Host/gates remain prototype-shaped; Options/Debug skins are checkbox/stub chrome (not UX parity); Feel dirty path can double-flush; Unity adapters still sit under `Policy/`.

## Keep

- ADR 0006 boundary: AppKit only in `rewrite/src`; no AppKit in `rewrite/mod`
- Style binding table as resolve SOT; Maps+ seed; FeelMath without Unity usings
- FeelCatalog + FeelEditor as shared inventory/write API (hosts must not own field lists)
- Tier A goldens with camera-port fake only; `FakeOsGestureSource` as OS-only stand-in
- `layer_import_lint` inside `sa:rewrite`

## P0 (before calling architecture / UX done)

1. **Options + Debug skins** — map catalog kinds through FeelEditor (not checkbox-for-all; Debug needs floating chrome + numeric Sensitivity).
2. **Host redesign** — rewrite Host/gate control flow from contracts (not ~80% shipping clones); strip dead mouse-rotate Harmony path; remove per-tick inject/capture hot-swap.

## P1

1. Single coalesced dirty flush (stop immediate envelope write on every New Preset dirty).
2. Move `GameModifierKeys` / `GameUiContext` out of pure `Policy/` (or tighten lint so FQN exemptions are not a forever carve-out).
3. Pass one FeelEditor into both hosts; wire `showDebugPanel`.
4. SA doc/root alignment (scripts README + Semgrep scan roots).

## P2

- Delete `CameraApplicator` / `CameraControllerZoom` / `ModSettingsStore` aliases
- Bound inject queue; library-only frame validity tests
- Selection-port goldens; retire stale clone-era audit shards as historical only

## Specialist shards

- [Architecture / Host](./uth-architecture-host.md)
- [Gesture library / Capture](./uth-gesture-library.md)
- [Policy / Apply](./uth-policy-apply.md)
- [Feel / Settings](./uth-feel-settings.md)
- [UI hosts](./uth-ui-hosts.md)
- [Tests / SA](./uth-tests-sa.md)

## Related

- Features _Under the hood_, ADR 0005 / ADR 0006
- Closed clone experiment: [about this review guide](./about-this-guide.md)
