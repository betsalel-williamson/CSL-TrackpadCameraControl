# Tests / SA audit (under-the-hood rewrite)

**As-built date:** 2026-09-03  
**Scope:** `rewrite/tests/**`, `rewrite/scripts/layer_import_lint.py`, `sa-rewrite.sh`  
**Contracts:** harnesses and testing, static analysis, _Under the hood_ fake rules  
**Verdict:** Conditional

## Strengths

- Fakes are one layer each: `FakeCameraController`, `FakeOsGestureSource`, `FakeGameUiContext` — no god-fake.
- Maps+ goldens use camera-port fake only; orbit stays queue-only until simulated vanilla flush (L10).
- FeelCatalogEditorTests cover section order, shared descriptors, dirty→New Preset, Save as / Delete / Reset.
- `layer_import_lint` is wired into `sa:rewrite` and currently PASS.
- Settings-field-graph and leak-pairing still run; shipping tests untouched.

## Weaknesses

- No automated proof that Options/Debug skins honor catalog kinds (descriptor equality ≠ UI parity).
- No test fails immediate double-flush on New Preset dirty.
- `rewrite/scripts/README.md` omits layer-import from the individual-command list.
- Semgrep roots differ between `sa-rewrite.sh` (mod) and `npm run sa:rewrite:semgrep` (mod+src).
- No selection-port golden; clone-era review shards (`ui-audit.md`, etc.) are stale vs as-built.

## Critical improvements

### P0

None for fake-per-layer / layer-import existence.

### P1

1. Host-mapping tests: Kind → control kind (fail checkbox-for-all). **Closed (feedback cycle 2026-09-03).**
2. Coalesced-flush regression test. **Closed.**
3. Document layer-import in scripts README; align Semgrep roots. (Follow-up.)

### P2

1. Optional `FakeSelectionContext` goldens.
2. Mark clone-era audit shards historical so they do not contradict FeelCatalog/OptionsHost as-built.
