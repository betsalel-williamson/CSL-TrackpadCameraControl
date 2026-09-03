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

## v1.0.0 pre-release record

First public macOS tag. Automated suite is recorded here; in-game boxes in the lists below are the remaining splash gate. Copy those lists into the launch PR as you check them. After a full in-game pass, replace the known-good placeholder and refresh [Workshop storefront](./workshop-storefront.md) Compatibility.

### Session platform (this release)

| Field                | Value                                                                                         |
| -------------------- | --------------------------------------------------------------------------------------------- |
| macOS version        | 26.5.2 Tahoe                                                                                  |
| Mac model            | MacBook Air M2, 2022 (Mac14,2)                                                                |
| Trackpad             | Built-in                                                                                      |
| Chip                 | Apple silicon (M2)                                                                            |
| Mod version / commit | Assembly `0.2.0` on this branch until the Changesets version PR; tree from `main` (`bcdfd1d`) |
| Result               | Automated pass; in-game hardware pass still required before Workshop splash                   |
| Notes                | Coverlet ~47% line is expected — capture, Harmony timing, and UI stay session-tested          |

### Automated suite (2026-08-31)

| Gate                          | Result                                                                                     |
| ----------------------------- | ------------------------------------------------------------------------------------------ |
| `npm test`                    | **206 passed**, 0 failed, 0 skipped                                                        |
| Coverlet (mod assembly)       | Line 47.23% · Branch 46.82% · Method 54.94% — visibility only; no fail gate                |
| In-game inject smoke          | Not run this pass (needs a loaded city + `TRACKPAD_E2E_INJECT`)                            |
| Optional chrome / ghost flags | Not on the v1 product surface (`EnableAssistChrome` off; place/relocate covered when used) |

[Harnesses and testing](./harnesses-and-testing.md) lists what unit / headless e2e cannot prove (Harmony postfix order, hardware Option+drag, capture filling `GestureFrame`).

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
- [ ] Cold boot → load city: note Mac OS vs in-game cursor. Dual cursor / Steam overlay Shift-Tab cursor swap is a **known external issue** (deferred to v2) — see [qa-mac-boot-cursor.md](./qa-mac-boot-cursor.md). Not a mod blocker for v1.
- [ ] Fresh city load: pan, pinch, rotate, and Option-orbit work without opening Debug panel or Options
- [ ] Debug panel **Reset** restores Default preset while panel stays open
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
- [ ] Debug title-bar **gear** opens Options focused on **Trackpad Camera Control** (not last-used category); Options still stacks above Debug

## Optional (future flag builds — not v1 launch QA)

These rows apply only if you deliberately compile experimental flags. **Contacts** is unfinished; do not treat an `EnableContactsCapture` build as launch evidence.

- [ ] `EnableAssistChrome`: chrome **button** orbit steps; drag pad orbits like Option+drag
- [ ] Place/relocate ghost: Option-orbit does **not** snap Target to ghost; two-finger twist rotates ghost

## Related

- [Harnesses and testing](./harnesses-and-testing.md) — what unit / e2e prove vs what they miss
- [Local MVP install](./local-mvp-install.md) — install the mod DLL for playtest
- [Workshop storefront](./workshop-storefront.md) — public “tested on” claim + community invite
- [Release process](./release-process.md) — version, Share on Mac, preview, Harmony required item
- [Vanilla camera suppress](../glossary/vanilla-camera-suppress.md) — scroll / mouse-rotate policy
