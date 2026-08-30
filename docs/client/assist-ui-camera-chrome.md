# Assist UI camera chrome

The in-game **Assist / tuning panel** is the floating window for [feel presets](./feel-presets.md) and per-op tunables beside the city view. Tunables mirror Options → Trackpad Camera Control.

**Assist chrome** (drag pads and nudge buttons) is **not** on the shipped product surface: it appears only when `EnableAssistChrome` is on. With that flag off, the panel (when Assist UI is enabled) is for feel and Sensitivity tuning — not pads/buttons — and mouse/keyboard stay vanilla for those chrome paths.

## Turn the panel on or off

1. Open Options → Trackpad Camera Control, or use the floating panel itself.
2. Toggle **Assist UI** (panel visibility).
3. Return to the city view — no restart required.

Development builds may default Assist UI **on**. Shipping defaults turn it **off** so gesture-only players keep a clean viewport unless they opt in.

## How to use it (shipped surface)

1. Load a city — the floating panel appears when Assist UI is on.
2. Use the feel-preset row (Slow / Default / Fast, Save as… / Load, Reset) — see [feel presets](./feel-presets.md).
3. For each op, read the short meaning + activation text, then edit Enable, Reverse, and **Sensitivity** (Orbit also has Pitch min / max).
4. Close the panel if you want a clean view; reopen from Options or the remaining control.

With `EnableAssistChrome` off, there are no Btn fields, drag pads, or nudge buttons. CAD switcher and capture-backend / low-pass controls stay behind their own flags.

## Chrome (only when `EnableAssistChrome` is on)

When the flag is on:

1. For each op, use the **drag pad** for continuous motion or the **buttons** for one-shot steps.
2. Pads use [Sensitivity](../glossary/sensitivity.md); buttons use [button step](../glossary/button-step.md) (not multiplied by Sensitivity).
3. Disabled ops do not fire from chrome.

## Validate camera controls

1. With Assist UI on, change a Sensitivity or feel preset and confirm gestures respond immediately.
2. If chrome is flagged on: nudge each enabled axis from a pad and from a button, then confirm the camera moves.
3. If chrome moves the camera but trackpad gestures do not, the apply path is fine — check the backend and [OS gesture conflicts](./os-gesture-conflicts.md).

Assist UI does not replace installing or connecting a trackpad backend for gesture play.
