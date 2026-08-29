# Harnesses and testing

How contributors validate Trackpad Camera Control without (and with) Cities: Skylines.

## Tiers

| Tier                     | What it proves                                                        | Needs game? | Where it runs |
| ------------------------ | --------------------------------------------------------------------- | ----------- | ------------- |
| **Unit** (xUnit)         | Frame layout, binding resolver, camera apply with fakes               | No          | Local + CI    |
| **Headless e2e**         | Gesture source → resolve → apply pipeline end-to-end with fake camera | No          | Local + CI    |
| **In-game inject smoke** | Synthetic frames into the loaded mod change camera zoom               | Yes         | Local only    |

Real Multitouch / trackpad hardware is **not** required for CI. Hardware pinch remains a manual check on macOS with the bridge host running (see [local MVP install](./local-mvp-install.md)).

## Unit tests

From the repository root (once the test project exists):

```bash
dotnet test tests/TrackpadCameraControl.Tests
```

Or the whole solution:

```bash
dotnet test
```

Expect coverage of resolver rules, wire/`GestureFrame` layout assumptions, and applicator behavior against a fake zoom seam — no Cities assemblies.

## Headless e2e

Same `dotnet test` invocation; headless cases live in the test project and exercise the pipeline with fake `IGestureSource` / zoom seams. CI should run `dotnet test` without downloading game DLLs.

## In-game inject smoke

Local-only. Assumes Cities: Skylines is running, the mod is installed and enabled, and inject mode is enabled (environment or file flag such as `TRACKPAD_E2E_INJECT=1`).

Forward-looking kickoff:

```bash
./scripts/e2e-ingame-smoke.sh
```

The smoke path queues synthetic pinch frames through an inject gesture source and checks that camera zoom moved. It does not synthesize OS Multitouch events.

## Language and BCL pin

Mod-loaded and shared capture libraries target **netstandard2.0** with **C# 9**. Prefer Mono-safe BCL surfaces that Unity/CS1 can load — see [contributor setup](./contributor-setup.md) and [lint and format](./lint-and-format.md).

## Related

- [Local MVP install](./local-mvp-install.md) — bridge host + local mod DLL
- Design decisions: `docs/superpowers/specs/2026-08-29-csharp-capture-tests-design.md`
