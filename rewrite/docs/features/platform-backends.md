# Platform backends

## Intent

Keep product language and Options **platform-neutral**. Isolate OS capture behind one shared primitive/frame contract. Capture runs **in-process** in the mod DLL.

## Policy

| Layer                                      | Stance                                                                                        |
| ------------------------------------------ | --------------------------------------------------------------------------------------------- |
| Features, client Outcomes, settings schema | No required OS brand in the capability story                                                  |
| First shipping backend                     | **macOS AppKit** — only validated Capture path                                                |
| Windows / Linux                            | Stubs; not supported in v1                                                                    |
| Contacts (MultitouchSupport)               | **Omitted from ship DLL** unless `EnableContactsCapture` is compiled on; unfinished even then |
| TrackpadBridge socket host                 | Dev experiment only; not playtest                                                             |

## Backend contract

A backend must:

- Emit the shared Capture primitive contract while the game is focused (when configured).
- Emit an **honest finger count** for the active contact set (lesson L4). Forcing a constant two-finger count makes three-finger style seeds dead on that backend.
- Avoid deciding pan vs orbit vs zoom (Policy style resolve owns that).
- Fail soft when unsupported or disconnected (do not crash the game). Precise trackpad scroll suppress applies while the mod is on; mouse wheel and middle-mouse orbit remain vanilla — see [vanilla camera suppress](./vanilla-camera-suppress.md).

Style rows may only claim finger counts Capture can express on the active backend ([parity with shipping](./parity-with-shipping.md)).

## macOS (v1)

- **AppKit (ship):** in-process AppKit local monitor (scroll / magnify / rotate) → the same primitives. No Accessibility. Precise scroll deltas drive pan; non-precise (mouse wheel) are not mapped to pan. This is the **only** path playtested for v1.
- **Finger count honesty:** AppKit must report the real contact count available from the OS event path (or document a hard ceiling). Maps+ seed chords that need only two fingers remain valid; CAD three-finger seeds require honest counts **and** the CAD module compiled on.
- **Contacts:** omitted unless `EnableContactsCapture` is on. Even when compiled, Contacts is not a ship QA path until a dedicated validation pass. No backend picker or low-pass on the default product surface.
- Maps+ orbit modifier defaults to Option (`⌥`).
- Client notes for Mission Control live under the client guide’s platform conflicts shard (when authored).

## Windows / Linux (stubs)

- Compile-time or runtime “unsupported” path.
- Future: Precision Touchpad or equivalent contact streaming mapped to the same primitives, with honest finger counts.

## Acceptance

- High-level feature shards describe trackpads and feel presets, not “Mac-only product.”
- README and client install state which backends ship today without rewriting the capability contract.
- Durable docs do not treat a C helper binary or socket host as the Capture path.
- Ship DLL without Contacts flag has no Contacts types on the tick path.
- Parity matrix / backend notes state AppKit finger-count honesty explicitly.
