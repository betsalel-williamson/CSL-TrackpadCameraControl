# Gesture frame (human template)

Aligns with `docs/features/ipc-gesture-primitives.md`. Implement binary layout under `shared/protocol/` later.

| Field | Notes |
| --- | --- |
| timestamp | monotonic |
| fingerCount | int |
| centroidDeltaX / Y | normalized |
| pinchScaleDelta | relative |
| rotateDelta | radians or degrees — pick one in protocol and document |
| modifiers | Option, Shift, Command, Control bits |
| phase | began / changed / ended / cancelled |
