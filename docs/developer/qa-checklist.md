# In-game QA checklists

Manual pass/fail lists for Trackpad Camera Control. Unit tests cannot prove Harmony postfix timing, `HandleMouseEvents` order, or hardware Option+drag — run these in Cities: Skylines after local install.

**Session defaults:** Cities Harmony on, this mod enabled, city loaded, game focused, Maps+ gesture style, Default feel preset.

Copy a section into a PR or commit note and check boxes as you go.

## Setup

- [ ] Cities Harmony subscribed and enabled
- [ ] Trackpad Camera Control enabled in Content Manager
- [ ] City loaded (not menus-only)
- [ ] Game window focused
- [ ] Maps+ / Default feel (Options or Debug panel)

## Trackpad camera

- [ ] Two-finger drag **pans**; does **not** also vanilla-zoom
- [ ] Pinch **zooms**
- [ ] Two-finger twist **yaws** (no extra pitch)
- [ ] Option (`⌥`)+two-finger drag **orbits** (yaw **and** pitch)
- [ ] With DefaultTool + a click-selected building: Option-orbit does **not** jump look-at to that building
- [ ] Release Option while fingers still down: orbit **latches** until lift
- [ ] Lift fingers after Option-orbit: short **coast** then stop (middle-click-like), not a teleport
- [ ] Pitch stops at **0°** looking down (not negative in normal play); top clamp **90°**
- [ ] Mouse wheel still **vanilla-zooms**

## Vanilla still works

- [ ] Edge pan moves the camera
- [ ] Keyboard camera keys move / rotate / zoom as vanilla
- [ ] Middle-click drag rotate still orbits when that binding is held (mod on)
- [ ] Disable this mod in Content Manager: trackpad pan fights or vanilla-zooms again; middle-click rotate unchanged

## UI gates

- [ ] Options open: two-finger scrolls Options; city does not pan/orbit from the mod
- [ ] Pointer over Debug panel (or another popup): two-finger scrolls/drags UI, not city camera

## Optional (only when flags are on)

- [ ] `EnableAssistChrome`: chrome **button** orbit steps; drag pad orbits like Option+drag
- [ ] Place/relocate ghost: Option-orbit follows ghost; twist rotates ghost, not leftover click-select

## Related

- [Harnesses and testing](./harnesses-and-testing.md) — what unit / e2e prove vs what they miss
- [Local MVP install](./local-mvp-install.md) — install the mod DLL for playtest
- [Vanilla camera suppress](../glossary/vanilla-camera-suppress.md) — scroll / mouse-rotate policy
