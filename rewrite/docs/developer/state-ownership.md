# State ownership

Where the rewrite keeps data, and what must stay derived from the game each frame. Aligns with the three planes and narrow Harmony rule in greenfield redesign lessons (L7–L8).

## Sources of truth

| Layer               | Owns                                               | Examples                                                                             |
| ------------------- | -------------------------------------------------- | ------------------------------------------------------------------------------------ |
| **Disk (XML)**      | Durable player preferences and named feel profiles | Sensitivity gains, reverse flags, op enables, gate prefs, Debug QoL                  |
| **Live blob (RAM)** | Working copy of settings plus one dirty bit        | Load once; Options and Debug share one editor API; coalesced autosave (L7)           |
| **Per-frame game**  | Live simulation and UI                             | Focus, menus, pointer-over-UI, selection / place / relocate, camera pose             |
| **Session**         | Temporal gesture policy                            | Orbit latch, rotate-owned contact, pending orbit velocity queue, capture connect arm |
| **Harmony buffers** | Cross-patch snapshots for the current frame only   | Precise trackpad scroll vs mouse wheel; menu-or-over-UI for scroll suppress          |

One live blob, one dirty bit, one write amplification path — feel edits must not double-flush XML.

## Do not statically cache

Re-query from Colossal UI / Unity **each tick**. Do not mirror these in mod statics:

- Game window focus
- Menu / Options open
- Pointer over UI / popups
- Selection, relocate, placement tool state
- Camera position, angles, zoom (read and write through the camera seam only)

Static caches create drift (unfocused input, stale menu gates, wrong orbit look-at).

## Session vs disk

| Kind                                                | Storage          | Lifetime                                      |
| --------------------------------------------------- | ---------------- | --------------------------------------------- |
| Feel and gate prefs                                 | Disk + live blob | Across quit                                   |
| Debug panel dismissed / position / copy-system-info | Disk + live blob | Across quit (chrome, not tick math)           |
| Orbit latch / rotate-owned contact                  | Session only     | Until touch-up                                |
| Pending orbit velocity                              | Session queue    | Flushed on Harmony postfix after vanilla damp |
| Capture arm after city load                         | Session only     | Until connect succeeds or mod disables        |
| Focus / menu / over-UI / selection / pose           | Per-frame game   | Never persisted                               |

## Harmony buffers (not preferences)

Buffers exist so Harmony prefixes and the policy tick share the same frame snapshot. They are not settings and must not appear in the feel blob.

| Buffer                  | Written by           | Purpose                                                                |
| ----------------------- | -------------------- | ---------------------------------------------------------------------- |
| Precise trackpad scroll | Capture scroll path  | Suppress vanilla zoom only for precise trackpad; keep mouse-wheel zoom |
| Menu or over-UI         | Gates sync each tick | Leave scroll to UI; skip mod camera apply                              |

Policy decisions live in gates / policy — not inside the buffer type. Harmony stays narrow: suppress buffers and deferred orbit velocity flush only (L8).

## Related

- [Settings schema](./settings-schema.md)
- [Feature flags](./feature-flags.md)
- [Harnesses and testing](./harnesses-and-testing.md)
