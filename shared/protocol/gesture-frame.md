# GestureFrame wire layout

Aligned with `docs/features/ipc-gesture-primitives.md`. Little-endian. Packed, no padding.

## Constants

| Name       | Value                 |
| ---------- | --------------------- |
| Magic      | `0x54435046` (`TCPF`) |
| Version    | `1`                   |
| Frame size | `48` bytes            |

## Header + body (48 bytes)

| Offset | Size | Type | Field                                                           |
| ------ | ---- | ---- | --------------------------------------------------------------- |
| 0      | 4    | u32  | magic                                                           |
| 4      | 2    | u16  | version                                                         |
| 6      | 2    | u16  | flags (reserved, 0)                                             |
| 8      | 8    | i64  | timestampNs (monotonic)                                         |
| 16     | 4    | i32  | fingerCount                                                     |
| 20     | 4    | i32  | phase (`0` began, `1` changed, `2` ended, `3` cancelled)        |
| 24     | 4    | f32  | centroidDeltaX                                                  |
| 28     | 4    | f32  | centroidDeltaY                                                  |
| 32     | 4    | f32  | pinchScaleDelta (relative; `0` = no change)                     |
| 36     | 4    | f32  | rotateDelta (radians)                                           |
| 40     | 4    | u32  | modifiers (bit0 Option, bit1 Shift, bit2 Command, bit3 Control) |
| 44     | 4    | u32  | reserved                                                        |

MVP uses `fingerCount`, `phase`, and `pinchScaleDelta`; other fields may be zero.
