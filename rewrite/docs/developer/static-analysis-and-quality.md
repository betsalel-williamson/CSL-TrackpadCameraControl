# Static analysis and quality

Lint and structural gates for the rewrite tree. These catch ceremony, resource bugs, and **layer-import** violations early; they do **not** replace tier A–C behavior proof ([Harnesses and testing](./harnesses-and-testing.md), greenfield redesign lessons L10).

## Semgrep

Run Semgrep rules over `rewrite/mod` and `rewrite/src` (and matching tests when rules need fixtures). Intended classes:

| Rule class                      | Intent                                                                          |
| ------------------------------- | ------------------------------------------------------------------------------- |
| Dead alias / wrapper types      | Ban one-hop facades with a single implementation and no test seam (L6)          |
| Runtime feature-flag facade     | Ban types that mirror `#if Enable*` for tick-path branching — compile omit only |
| Tick-path empty modules         | Ban constructing no-op filters / chrome when the compile module is off          |
| Settings field without consumer | Flag schema / settings members never read by resolve, apply, gates, or chrome   |

Phase 3 gates live under `rewrite/scripts/` (`npm run sa:rewrite`). Rule IDs and allowlists: [scripts README](../../scripts/README.md) and `rewrite/scripts/semgrep/rewrite.yml`. This shard is the contract.

## Settings read/write graph

Maintain a machine-checkable graph: every live settings field is either

- **read** by a named tick consumer (resolve, gates, apply), or
- classified **chrome**, **alias**, or **module-gated** in [Settings schema](./settings-schema.md),

and every **write** goes through the single live-blob editor path (one dirty bit, coalesced autosave — L7).

Fail the gate when:

- A persisted field has no reader
- Feel edits flush XML twice
- Options and Debug write through divergent apply layers
- A **non-field** (pitch min/max, unused hysteresis) reappears on the live blob

## Native leak pairing

In-process capture pins unmanaged resources. Pair acquires with releases in the same ownership scope:

| Acquire                               | Release                                                   |
| ------------------------------------- | --------------------------------------------------------- |
| GCHandle alloc                        | Free; types that store GCHandle fields must be disposable |
| CoreFoundation string / object create | CFRelease (or documented transfer)                        |
| Multitouch device start               | Device stop                                               |
| AppKit local event monitor add        | Monitor remove                                            |

An explicit `native-leak-ok:` marker with a reason may skip process-lifetime or ownership-transfer cases. Do not use it to silence a real leak. Pairing analysis is not a runtime leak detector — early-return paths still need review.

## Dead-alias ban

Every public type must earn a second implementation or a real test seam. Ban:

- Alias APIs that only forward to one concrete type
- Dual capture factories that always return the same backend on ship
- Tick-path subscribers that no-op when a module is compile-omitted

Prefer deleting redirection over documenting it (L6).

## Dispose order

Mod disable / runtime teardown must release in a defined order so Harmony, capture, and UI do not touch freed state:

1. Unhook or idle the per-frame tick entry
2. Dispose capture / gesture source (monitors, devices, GCHandles)
3. Clear session queues and latches
4. Remove Harmony patches
5. Drop the live runtime instance reference
6. Flush or abandon the settings dirty bit per store policy (no write after dispose of dependents)

Construction on enable is the reverse ownership: settings live blob → runtime → Harmony → capture arm on city load. Document ownership in types; static analysis should flag disposable fields never disposed on the disable path.

## Layer-import lint

Fail the gate when stack layers violate the import matrix in features _Under the hood_ / ADR 0006:

- `rewrite/src` contains `ICities`, `Colossal`, `HarmonyLib`, or CSL Feel/Maps+ product types
- `rewrite/mod` contains AppKit / Multitouch P/Invoke
- A “pure” Policy / Apply / Feel file gains UnityEngine / ICities / Harmony / AppKit usings

Csproj `Reference` sets must match: the gesture library never lists Cities managed DLLs.

## Related

- [State ownership](./state-ownership.md)
- [Feature flags](./feature-flags.md)
- [Settings schema](./settings-schema.md)
