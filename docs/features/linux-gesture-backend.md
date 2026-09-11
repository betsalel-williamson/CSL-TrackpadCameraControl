# Linux gesture backend

## Intent

Give Linux the same shipped ops as macOS (pan, zoom, rotate, orbit) through one in-process `IGestureSource` in the [gesture library](../glossary/gesture-library.md), with **no** extra process, **no** root, and **no** streaming-specific code path. Linux is a [platform backend](./platform-backends.md) decision only — product language, Options, and the [style binding table](../glossary/style-binding-table.md) do not change.

This shard is the Linux half of the remote-streaming story: a Sunshine host that exposes an [indirect touch device](../glossary/indirect-touch-device.md) is indistinguishable from a laptop touchpad, so [remote streaming gestures](./remote-streaming-gestures.md) reuses everything here.

## What Linux actually hands an application

libinput classifies devices and **pre-interprets touchpads**; it does not hand apps raw contacts. Two-finger motion is already scroll, pinch/rotate arrive as one gesture event, and three-or-more-finger motion is a swipe. Touchscreens get **no** gesture interpretation at all — raw touchpoints only.

| Mod op       | Linux source event                               | Notes                                                                     |
| ------------ | ------------------------------------------------ | ------------------------------------------------------------------------- |
| Pan          | Smooth scroll axis (finger-source scroll)        | Same shape as AppKit precise scroll; no finger count is reported          |
| Zoom         | Pinch gesture `scale`                            | Absolute multiplier vs gesture start — differentiate per frame            |
| Rotate       | Pinch gesture `delta_angle`                      | Rotation rides the **pinch** gesture; there is no separate rotate gesture |
| Orbit        | Modifier + scroll (Maps+) or swipe gesture (CAD) | Swipe is 3+ fingers; see the desktop-grab hazard below                    |
| Finger count | Gesture event detail; **absent** for scroll      | Scroll-derived pan declares 2 by contract                                 |

## Transport options

| Transport                                   | Reach                                                          | Verdict                                                                                       |
| ------------------------------------------- | -------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| **XInput2 2.4 gestures** (`libXi` P/Invoke) | X11 and XWayland; X server 21.1+, `xf86-input-libinput` 1.2.0+ | **Ship path.** Unprivileged, in-process, window-scoped, already how desktop apps get pinch    |
| Wayland `pointer-gestures-unstable-v1`      | Native Wayland clients only                                    | Not reachable — CS1 is an X11 Unity player; XWayland already bridges this protocol to XI 2.4  |
| `libinput` context directly                 | Everything                                                     | Needs a udev seat and `/dev/input/event*` read; root or `input` group. Opt-in power path only |
| Raw `evdev` + own MT protocol-B parser      | Everything                                                     | Same permission wall, and re-implements libinput's palm/jitter/gesture work. Reject           |

X server 21.1 shipped XI 2.4; `xf86-input-libinput` 1.2.0 implemented the gesture detection; XWayland implements the same events on top of `pointer-gestures-unstable-v1`. Pre-21.1 servers simply never report gestures — that is a fail-soft noop, not a crash.

## Mapping to the frame contract

The existing `GestureFrame` (48 bytes, `shared/protocol/gesture_frame.h`) already carries exactly what XI 2.4 delivers, so Linux needs a mapper, not a new contract.

| `GestureFrame` field | XI 2.4 / scroll source                                                              |
| -------------------- | ----------------------------------------------------------------------------------- |
| `phase`              | `XIGesturePinchBegin` / `Update` / `End`; End with the cancelled flag → `Cancelled` |
| `centroidDeltaX/Y`   | `delta_x` / `delta_y` (pinch) or scroll valuator delta (pan)                        |
| `pinchScaleDelta`    | `scale` differentiated against the previous update                                  |
| `rotateDelta`        | `delta_angle`                                                                       |
| `fingerCount`        | gesture `detail`; declared 2 for scroll-derived pan                                 |
| `modifiers`          | `mods.effective` mapped to the shared modifier flags                                |

## Policy

- **Own X connection.** Open a second `Display` and select gestures there. XI2 event masks are per client, per window, per device — selecting on the game window from our own connection cannot clobber what the Unity player selected on its own.
- **Select all three.** A client must select Begin, Update, and End together or it receives nothing.
- **Attribute by source device.** XI events carry `sourceid`. Scroll from a **touchpad-class** device feeds the mod; scroll from a **mouse-class** device stays vanilla. This is the Linux spelling of the precise-vs-wheel rule already in [suppress vanilla camera input](./vanilla-camera-suppress.md) — and it is what makes a plugged-in mouse coexist with a trackpad (or with a streamed [indirect touch device](../glossary/indirect-touch-device.md)) without a mode switch.
- **Honest finger count** (lesson L4): gesture events carry a real count; scroll does not. Report 2 for scroll-derived pan and say so, exactly as the AppKit ship path does. Never infer a count libinput did not give us.
- **Fail soft.** No X display, no `libXi`, no XI 2.4, or no gesture-capable device → noop source, mod stays enabled, Options and Debug open, vanilla wheel and middle-mouse orbit keep working.

## The recognizer is libinput, not us

This is the load-bearing property of the Linux design: **no layer we own ever converts contacts into frames.** libinput is the peer of AppKit's `magnify` / `rotate` / `scroll` — it owns contact tracking, palm and thumb detection, jitter filtering, finger-count hysteresis, and pinch-vs-swipe disambiguation, and it hands out begin/update/end gestures with `scale`, `delta_angle`, `dx/dy`, and a finger count. An `IGestureSource` that reads XI 2.4 is a mapper, the same size and risk as `AppleGestureMapper`.

Raw contacts are therefore not a Linux design option we are declining for taste; consuming them would mean re-implementing libinput inside a Workshop mod.

## Where the QA cost actually lands

It does not disappear — it moves from _implementing_ a recognizer to _disagreeing_ with one. libinput's classifier is not AppKit's, and each difference below is a playtest item, not a code item.

| Difference                                                                                                                                                                                                                    | Consequence for feel                                                                                                                                                                             |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Scroll first, pinch corrected.** libinput posts two-finger scroll immediately and only reclassifies to pinch inside a 300 ms window ("we may confuse a pinch for a scroll initially, allow ourselves to correct our guess") | A pinch can arrive as pan for up to 300 ms and then become zoom. Resolve needs a policy: suppress the latch until the window closes, or cancel the pan retroactively. AppKit never produces this |
| **Pinch entry threshold.** Both touches must move >= 1.5 mm in non-identical directions before pinch is allowed                                                                                                               | Slow or small pinches register as pan. A Slow feel preset cannot compensate for a gesture that was never classified                                                                              |
| **No dedicated rotate.** Rotation exists only as the pinch gesture's `delta_angle`                                                                                                                                            | Rotate-without-zoom must be teased out of a gesture whose entry condition is a distance change                                                                                                   |
| **Geometry is assumed when absent.** `scale` is a distance ratio and so resolution-free, but `angle` is `atan2` over resolution-normalized coordinates. With no declared resolution libinput assumes 69x50 mm                 | On a 19200x10800 virtual pad that is 278 units/mm in x against 216 in y — anisotropic, so a circular two-finger rotation reads as elliptical and every mm threshold applies to a fictional pad   |

Mitigation order: declare real axis resolution on any virtual pad first (it is the one item that silently skews everything else), then write the scroll-to-pinch correction policy, then tune feel.

## Known hazards

| Hazard                                                            | Consequence                                                                                                                                                  |
| ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Desktop environments take passive grabs on 3- and 4-finger swipes | A CAD-style three-finger orbit will fight workspace switching. Pinch is normally ungrabbed, which is why Maps+ (pinch + modifier) is the right Linux seed    |
| Only one gesture can exist at a time                              | No simultaneous pinch + swipe; resolve stays single-op, same as today                                                                                        |
| Gestures apply at the current cursor position                     | Focus/over-UI gates still decide whether the frame applies — unchanged policy                                                                                |
| Virtual touchpads often declare no axis resolution                | libinput falls back to a default ~69x55 mm size, so mm-based thresholds skew. Fixable host-side with a libinput quirks `AttrResolutionHint` / `AttrSizeHint` |
| Two-finger **rotate** has no dedicated event                      | Rotate must come off the pinch gesture's angle delta or it does not exist                                                                                    |

## Acceptance

- Linux ships the same Maps+ rows as macOS, or honestly reports the op as unavailable — no silent half-gesture.
- The mod never opens `/dev/input/*` on the ship path and never asks the player for group membership.
- A mouse plugged in alongside a touchpad keeps vanilla wheel zoom and middle-mouse orbit, with no player-visible mode.
- `docs/features/platform-backends.md` stops calling Linux a stub only after the XI 2.4 source is playtested on X11 **and** XWayland.
