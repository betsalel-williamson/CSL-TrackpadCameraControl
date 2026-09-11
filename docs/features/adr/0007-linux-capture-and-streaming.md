# ADR 0007 — Linux capture via XI 2.4, streaming via a host trackpad

## Status

Proposed

## Context

Linux is a stub ([platform backends](../platform-backends.md)) and players have asked whether Sunshine/Moonlight streaming can carry trackpad gestures. The naive reading is three parallel builds of work: a Linux backend, a streaming backend, and a product switch that tells the mod to stop reading the mouse while streaming.

Research into the upstream projects (Sunshine `307a608`, libvirtualhid `3636008`, moonlight-qt `e3fd29e`, moonlight-common-c `62e0663`) says otherwise:

- libinput interprets gestures **only** for indirect touch devices. Touchscreens deliver raw points and nothing else.
- Sunshine injects client touch into a virtual **touchscreen**; `libvirtualhid` already implements a virtual **trackpad** that Sunshine never creates.
- Linux has no semantic gesture-injection API, so any streaming design terminates in a uinput device regardless of what the wire carries. libinput's pinch `scale` and `angle` are closed-form functions of two touch positions, so a frame can be re-encoded as two contacts analytically rather than heuristically.
- Earlier prototyping on macOS already established that consuming raw contacts costs far more QA than consuming an OS-recognized gesture, and that the recognized frame is the better UX. That result transfers: the recognizer must stay upstream of us on every platform and every transport.
- X11 and XWayland expose libinput's gestures to ordinary unprivileged clients through XInput2 2.4 (X server 21.1+).

A virtual trackpad on the host is therefore indistinguishable from a laptop touchpad, and both reach the mod through the same XI 2.4 events.

## Decision

1. Linux capture is **XInput2 2.4 gesture + scroll events** read over the mod's own X display connection, in-process, unprivileged, mapped to the existing `GestureFrame`. `libinput`/`evdev` direct reads stay an opt-in power path, never the ship path.
2. Streaming support is **not a mod feature**. The mod ships one Linux `IGestureSource`; making streamed gestures arrive is upstream work on Sunshine (create and route to `lvh::Trackpad`) and Moonlight (stop discarding indirect contacts).
3. The streaming wire carries a **recognized gesture frame**, not contacts, and the host re-encodes it onto the virtual trackpad analytically. Contact passthrough stays a complement for generic multitouch apps, never the path this mod depends on.
4. Source-device attribution replaces any streaming mode switch: touchpad-class devices feed the mod, mouse-class devices stay vanilla. No Options row, no auto-detection of a streaming session.
5. Fidelity is proven locally first, by replaying recorded capture logs through a uinput trackpad and comparing the frames libinput reports against the frames that went in.

## Consequences

- One Linux backend covers native trackpads, streamed trackpads, and future virtual pads. No layer we own ever tracks contacts.
- libinput's classifier is not AppKit's, so QA shifts from implementing recognition to reconciling two recognizers — chiefly libinput's scroll-first, pinch-corrected 300 ms window and its 1.5 mm pinch entry threshold.
- The mod takes a hard dependency on the host exposing an indirect device — if upstream never lands it, streamed players get the documented noop, not a half-working path.
- Our upstream asks stay small and independently useful, which is the only version of them likely to merge.
- Three-finger (CAD) gestures are weaker on Linux than macOS because desktop environments passively grab 3- and 4-finger swipes; Maps+ (pinch + modifier) remains the Linux seed.
- Scroll-derived pan cannot report a true finger count on Linux, matching the existing AppKit ship-path caveat rather than inventing a number.

## Alternatives rejected

| Alternative                                               | Why not                                                                                                  |
| --------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| Semantic gesture frames on the wire as the primary path   | Host must re-synthesize contacts anyway, adding a second recognizer; keep it as the iPadOS-only fallback |
| Read `/dev/input/event*` from the mod                     | Needs group membership or root for a Workshop mod, and re-implements libinput's palm/jitter work         |
| A "streaming mode" product flag that disables mouse input | A second code path playtesting cannot cover, and unnecessary once devices are attributed                 |
| Out-of-process bridge receiving frames from the client    | [ADR 0001](./0001-native-multitouch-bridge.md) retired IPC; the host device route removes the need       |
