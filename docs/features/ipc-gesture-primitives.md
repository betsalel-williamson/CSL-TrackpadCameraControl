# IPC gesture primitives

## Intent

Define the contract between a platform backend and the CS1 mod so camera binding stays hot-configurable in C#.

## Frame model (logical)

Each frame describes **raw gesture primitives**, not camera operations:

| Field             | Meaning                                               |
| ----------------- | ----------------------------------------------------- |
| Timestamp         | Monotonic time for delta integration                  |
| Finger count      | Active contacts in the gesture                        |
| Centroid delta    | Normalized 2D motion of the contact centroid          |
| Pinch scale delta | Relative distance change between two primary contacts |
| Rotate delta      | Relative angle change between two primary contacts    |
| Modifier flags    | Primary / secondary / meta / control (OS-mapped)      |
| Gesture phase     | Began / changed / ended / cancelled                   |

## Rules

- Backend does **not** decide pan vs orbit vs zoom.
- Mod applies the live binding table from ModSettings.
- Frames are dropped or coalesced under backpressure; never block the OS contact callback.
- Transport is local to the user session (for example a Unix domain socket on macOS); no network.

## Acceptance

- A debug Options overlay can show last finger count and last resolved camera op.
- Changing orbit trigger in Options changes resolved ops without backend restart.
- Missing backend yields disconnected state, not exceptions that disable the mod.

## Wire layout

Concrete binary layout and enum names live in `shared/protocol/`. This shard is the durable contract; update it when the wire format changes.
