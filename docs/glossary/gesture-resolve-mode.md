# Gesture resolve mode

How multiple camera ops from one trackpad frame are combined:

| Mode        | Behavior                                                        |
| ----------- | --------------------------------------------------------------- |
| Concurrent  | Apply every enabled op that clears its threshold (default)      |
| SessionLock | First winning op at gesture Began owns the session until Ended  |
| PrimaryOnly | Single op per frame by fixed priority: Orbit > Zoom > Yaw > Pan |

[Orbit latch](./orbit-latch.md) still strips pan and zoom while orbit is latched, even in Concurrent mode.
