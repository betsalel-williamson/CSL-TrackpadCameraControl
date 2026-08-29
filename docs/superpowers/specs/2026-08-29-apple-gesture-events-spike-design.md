# Apple native gesture events — Spike

**Date:** 2026-08-29  
**Status:** Spike (not a shipping backend)  
**Scope:** Log AppKit gesture payloads beside the existing contact interpreter. Do not bind camera ops.

## Goal

Learn whether Apple-classified trackpad events carry enough **movement data** for a second macOS interpreter, while keeping the contact/`GestureFrame` path as the portable one for other OSes.

## Locked decisions

| Concern          | Choice                                                                                                       |
| ---------------- | ------------------------------------------------------------------------------------------------------------ |
| Product contract | Unchanged. Contacts remain the cross-OS primitive stream.                                                    |
| Spike shape      | Standalone AppKit probe under `native/mac/` (not in-mod, not TrackpadBridge).                                |
| Permissions      | Default run is **window-local** (`src=view` / `src=local`). No Accessibility.                                |
| Maps+ mapping    | Pan = two-finger scroll. Orbit = same scroll + Option (live orbit modifier). Zoom = magnify. Twist = rotate. |
| CAD swipe        | Log only. Do not treat `swipeWithEvent:` as continuous orbit.                                                |
| Binding          | Probe does not emit `GestureFrame` or call the resolver.                                                     |
| TDD              | Throwaway prototype; no production C# for this slice.                                                        |

## Operations → Apple event

| Operation   | Maps+                      | CAD                           |
| ----------- | -------------------------- | ----------------------------- |
| Pan         | Two-finger scroll          | Two-finger scroll             |
| Orbit       | Two-finger scroll + Option | Three-finger swipe (log only) |
| Zoom        | `magnifyWithEvent:`        | `magnifyWithEvent:`           |
| Twist / yaw | `rotateWithEvent:`         | `rotateWithEvent:`            |

## Questions the probe must answer

1. **Scroll:** Are `scrollingDeltaX` / `scrollingDeltaY` precise 2D values while two-finger dragging? Does `hasPreciseScrollingDeltas` stay true? Does `momentumPhase` continue after lift?
2. **Orbit modifier:** When Option is held during two-finger scroll, is `.option` set on **that** scroll event (`modifierFlags`)?
3. **Magnify:** Is `magnification` a continuous scalar during pinch, or a one-shot flag?
4. **Rotate:** Is `rotation` continuous degrees during two-finger twist?
5. **Swipe:** Are `deltaX` / `deltaY` only −1 / 0 / +1? Does swipe fire at all with default Mission Control settings?
6. **Separation:** During two-finger pan, do magnify/rotate stay quiet, or do they chatter beside scroll?
7. **Delivery (default):** With the probe focused, do `src=view` and `src=local` fire? (Enough to judge payload quality.)
8. **Delivery (optional):** `APPLE_GESTURE_PROBE_TAP=1` plus Accessibility: with Cities focused, does `src=tap` still see the same types? Not required for this spike.

## Log line

One stderr line per event, fields only when valid for that type:

```text
apple src=view|local|tap type=scroll|magnify|rotate|swipe|begin|end|gesture|smart
      phase=… momentum=… sdx=… sdy=… dx=… dy=… mag=… rot=… mods=opt,shift,cmd,ctrl precise=0|1
```

`mods` uses the same names as the wire modifier bits (Option, Shift, Command, Control). Missing movement fields are omitted, not zero-filled, so a swipe of `dx=-1` is not confused with a quiet scroll.

## How to run

From the spike worktree (or this branch):

```bash
./scripts/apple-gesture-probe.sh
```

Keep that terminal visible. A window titled **Apple Gesture Probe** appears.

1. Click the probe window so it is frontmost.
2. Gesture **on that window** (not over the terminal, Dock, or the game):
   - two-finger pan
   - Option + two-finger pan
   - pinch
   - two-finger twist
   - three-finger swipe
3. Read stderr. Expect `src=view` and `src=local` on the same gesture. No Accessibility prompt.

Optional (not needed for questions 1–7): `APPLE_GESTURE_PROBE_TAP=1 ./scripts/apple-gesture-probe.sh` listens while another app is focused; that path may ask for Accessibility.

## Pass bar

Spike is done when the log answers questions 1–7 with the probe focused (including “swipe never fired”). That evidence decides whether a later Apple-native **backend** is worth designing — it does not ship the backend.

## Out of scope

- `IGestureSource` / wire format / Options switch
- In-process Unity `NSEvent` monitor inside the mod
- Consuming or suppressing events (Harmony vanilla suppress unchanged)
- Windows / Linux
- Fluid `trackSwipeEvent` CAD orbit
