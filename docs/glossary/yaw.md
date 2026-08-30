# Yaw

**Orbit yaw** — the horizontal half of [orbit](./orbit.md): compass heading change from Option (`⌥`)+two-finger **scroll/drag**, via the shared middle-mouse velocity channel (with pitch). Not the twist gesture.

**Rotation** — two-finger **rotate** (twist) on the trackpad. Turns camera heading (`AngleX`) or, while **placing new** / **relocating**, the **ghost**. It must not use the orbit velocity channel. Starting a rotation **hard-handoffs**: leftover orbit coast (yaw and pitch velocity) yields so twist cannot pitch. A click-selected placed object does not steal rotation — then the camera rotates. With Option held, orbit owns the contact and rotation is ignored.
