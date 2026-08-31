# Debug panel

The in-game **Debug** panel is the floating window for [feel presets](./feel-presets.md) and per-op tunables beside the city view. It shares the same live **ModSettings** as Options → Trackpad Camera Control: camera feel updates immediately from either surface; the Debug panel rebuilds its controls when settings change; every change **autosaves**.

Drag the panel by its **title bar** (mod name + version). The title bar uses native Cities chrome: a circular **close** control and a circular **gear** (Options). At rest those header buttons are translucent; on hover they strengthen. The gear opens vanilla **OPTIONS** (the game’s Options window). Closing the panel leaves a floating **Debug** reopen chip when Options still allows the panel.

## Turn the panel on or off

1. Open Options → Trackpad Camera Control, or use the floating panel itself.
2. Toggle **Show debug panel**.
3. Return to the city view — no restart required.

Turning **Show debug panel** **off** hides both the Debug panel and the floating Debug reopen chip. Factory default is **off** so gesture-only players keep a clean viewport; enable it from Options when you want the floating panel.

## How to use it (shipped surface)

1. Load a city — the floating panel appears when Debug is on.
2. Use the feel-preset **dropdown** (built-ins, your named presets, **Save as…**) — see [feel presets](./feel-presets.md).
3. Sections follow **General → Zoom → Pan → Rotate → Orbit**. Edit **Sensitivity**. Orbit pitch uses vanilla **0–90°** (not tunable here).
4. Close with the title-bar close control for a clean view; reopen from the floating Debug chip (when Show debug panel is on) or from Options. Use the gear to jump to vanilla OPTIONS.

There are no Enable-per-op or Reverse controls on the product surface. Pad/button chrome, CAD switcher, and capture-backend / low-pass controls stay behind their own flags and are not part of shipped play.

## Validate camera controls

1. With Debug on, change a Sensitivity or feel preset and confirm gestures respond immediately.
2. Change a value in Options and confirm the Debug panel rebuilds to match; change in Debug and confirm gestures update immediately — reopen Options to see slider positions catch up.
3. Turn **Show debug panel** off and confirm both the panel and the Debug reopen chip disappear.
4. If gestures do nothing after tuning, check the backend and [OS gesture conflicts](./os-gesture-conflicts.md).

The Debug panel does not replace installing or connecting a trackpad backend for gesture play.
