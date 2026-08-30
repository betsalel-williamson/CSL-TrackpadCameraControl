# Debug panel

The in-game **Debug** panel is the floating window for [feel presets](./feel-presets.md) and per-op tunables beside the city view. It shares the same live settings as Options → Trackpad Camera Control: change a value in either place and the other stays in sync; every change **autosaves**.

Drag the panel by its **title bar** (mod name + version).

## Turn the panel on or off

1. Open Options → Trackpad Camera Control, or use the floating panel itself.
2. Toggle **Debug** (panel visibility).
3. Return to the city view — no restart required.

Development builds may default Debug **on**. Shipping defaults turn it **off** so gesture-only players keep a clean viewport unless they opt in.

## How to use it (shipped surface)

1. Load a city — the floating panel appears when Debug is on.
2. Use the feel-preset **dropdown** (built-ins, your named presets, **Save as…**) — see [feel presets](./feel-presets.md).
3. Sections follow **General → Zoom → Pan → Rotate → Orbit**, each after a horizontal rule and section title. Edit **Sensitivity**. Orbit pitch uses vanilla **0–90°** (not tunable here).
4. Close the panel for a clean view; reopen from Options or the remaining control.

There are no Enable-per-op or Reverse controls on the product surface. Pad/button chrome, CAD switcher, and capture-backend / low-pass controls stay behind their own flags and are not part of shipped play.

## Validate camera controls

1. With Debug on, change a Sensitivity or feel preset and confirm gestures respond immediately.
2. Change the same value in Options and confirm the panel reflects it (and the reverse).
3. If gestures do nothing after tuning, check the backend and [OS gesture conflicts](./os-gesture-conflicts.md).

The Debug panel does not replace installing or connecting a trackpad backend for gesture play.
