# Platform backends

## Intent

Keep product language and Options **platform-neutral**. Isolate OS capture behind a shared backend interface and IPC primitive contract.

## Policy

| Layer | Stance |
| --- | --- |
| Features, client Outcomes, settings schema | No required OS brand in the capability story |
| First shipping backend | **macOS** trackpad via native multitouch bridge |
| Windows / Linux | Stubs with the same interface; contributor implementations welcome |

## Backend contract

A backend must:

- Stream [IPC gesture primitives](./ipc-gesture-primitives.md) while the game is focused (when configured).
- Avoid deciding pan vs orbit vs zoom (C# bindings own that).
- Fail soft and leave vanilla input alone when unsupported or disconnected.

## macOS (v1)

- Native helper loads the platform multitouch API, serves local IPC.
- Maps+ orbit modifier defaults to Option.
- Client notes for Mission Control / Accessibility live under the client guide’s platform conflicts shard.

## Windows / Linux (stubs)

- Compile-time or runtime “unsupported” path.
- Future: Precision Touchpad or equivalent contact streaming mapped to the same primitives.

## Acceptance

- High-level feature shards describe trackpads and presets, not “Mac-only product.”
- README and client install state which backends ship today without rewriting the capability contract.
