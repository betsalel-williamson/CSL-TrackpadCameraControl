# Gesture frame (human template)

Aligns with `docs/features/ipc-gesture-primitives.md` and `shared/protocol/gesture-frame.md`.

| Field              | Notes                                |
| ------------------ | ------------------------------------ |
| magic / version    | `TCPF` / `1`                         |
| timestampNs        | monotonic nanoseconds                |
| fingerCount        | int                                  |
| phase              | began / changed / ended / cancelled  |
| centroidDeltaX / Y | normalized                           |
| pinchScaleDelta    | relative distance change             |
| rotateDelta        | radians                              |
| modifiers          | Option, Shift, Command, Control bits |
