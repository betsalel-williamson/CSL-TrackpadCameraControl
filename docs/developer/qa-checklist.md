# In-game QA checklists

Manual pass/fail lists for Trackpad Camera Control. Unit tests cannot prove Harmony postfix timing, `HandleMouseEvents` order, or hardware Option+drag — run these in Cities: Skylines after local install.

**Session defaults:** Cities Harmony on, this mod enabled, city loaded, game focused, Maps+ gesture style, Default feel preset.

Copy a section into a PR or commit note and check boxes as you go. With **Show debug panel** on, click **Copy** in the Debug footer (**Include system info** checked) to paste OS, Mac model, input device models, and loaded Unity/Harmony/game/mod assembly versions into the session platform table below. After a full pass, add a row to **Known good platforms** below (and update [Workshop storefront](./workshop-storefront.md) when the public claim changes).

## Session platform (fill every run)

| Field                                                | Value                             |
| ---------------------------------------------------- | --------------------------------- |
| macOS version (e.g. 15.1 Sequoia)                    |                                   |
| Mac model (e.g. MacBook Pro M2, 2023)                |                                   |
| Trackpad                                             | Built-in / Magic Trackpad / other |
| Chip                                                 | Apple silicon / Intel             |
| Mod version / commit                                 |                                   |
| Result                                               | Pass / Fail / Partial             |
| Notes (gestures that failed, Mission Control tweaks) |                                   |

AppKit APIs we use date to ~macOS 10.6; **practical support is “whatever still runs CS1 + a precise trackpad,” proven by this checklist** — not by an untested OS matrix.

## Known good platforms

Maintainer and community reports. Prefer Workshop comments or a GitHub issue titled `platform: …` so we can fold rows here.

| macOS                  | Hardware | Chip | Result | Source                                           |
| ---------------------- | -------- | ---- | ------ | ------------------------------------------------ |
| _(none published yet)_ |          |      |        | Add first maintainer pass before Workshop splash |

## Setup

- [ ] Cities Harmony subscribed and enabled
- [ ] Trackpad Camera Control enabled in Content Manager
- [ ] City loaded (not menus-only)
- [ ] Game window focused
- [ ] Cold boot → load city: no OS hardware cursor overlaying the in-game cursor; one-finger tools work without alt-tab first
- [ ] Fresh city load: pan, pinch, rotate, and Option-orbit work without opening Debug panel or Options
- [ ] Maps+ / Default feel (Options or Debug panel)
- [ ] Session platform row filled above

## Trackpad camera

- [ ] Two-finger drag **pans**; does **not** also vanilla-zoom
- [ ] Gestures respond within ~5 s of city load on cold boot (Debug panel off)
- [ ] Pinch **zooms**
- [ ] Two-finger twist **rotates** heading (no pitch; hard-handoffs leftover orbit coast)
- [ ] Option (`⌥`)+two-finger drag **orbits** (orbit yaw **and** pitch); twist ignored while Option owns contact
- [ ] With DefaultTool + a click-selected building: Option-orbit does **not** jump look-at to that building
- [ ] Relocate building → two-finger **pan away** → Option-orbit does **not** jump look-at to Relocate-click / ghost / old cell
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
- [ ] Place/relocate ghost: Option-orbit does **not** snap to ghost; two-finger twist rotates ghost, not leftover click-select

## Related

- [Harnesses and testing](./harnesses-and-testing.md) — what unit / e2e prove vs what they miss
- [Local MVP install](./local-mvp-install.md) — install the mod DLL for playtest
- [Workshop storefront](./workshop-storefront.md) — public “tested on” claim + community invite
- [Vanilla camera suppress](../glossary/vanilla-camera-suppress.md) — scroll / mouse-rotate policy
