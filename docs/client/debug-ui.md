# Debug panel

The in-game **Debug** panel is the floating window for [feel presets](./feel-presets.md) and per-op tunables beside the city view. It shares the same live **ModSettings** as Options → Trackpad Camera Control: camera feel updates immediately from either surface; the Debug panel **prefers in-place updates** of fields and Gesture/Keymapping labels when settings change (full recreate only if structure cannot be refreshed); every change **autosaves**.

Drag the panel by its **title bar** (mod name + version). The title bar uses native Cities chrome: a circular **close** control and a circular **gear** (Options). At rest those header buttons are translucent; on hover they strengthen. Hover the panel for full opacity; move off and it softens so you can see through it. The gear opens the game’s **Options** window focused on **Trackpad Camera Control**. Closing the panel leaves a floating **Debug** reopen chip when Options still allows the panel.

## Turn the panel on or off

1. Open Options → Trackpad Camera Control, or use the floating panel itself.
2. Toggle **Show debug panel**.
3. Return to the city view — no restart required.

Turning **Show debug panel** **off** hides both the Debug panel and the floating Debug reopen chip. Factory default is **off** so gesture-only players keep a clean viewport; enable it from Options when you want the floating panel.

## How to use it (shipped surface)

1. Load a city — the floating panel appears when Debug is on.
2. Use the feel-preset **dropdown** (built-ins, your named presets, **New Preset** when dirty), **Save as…** when on New Preset, and **Delete** when a named user preset is active — see [feel presets](./feel-presets.md).
3. Sections follow **General → Zoom → Pan → Rotate → Orbit**. Op headings show Maps+ activation plus a **Keymapping(s):** line with live Cities binding labels (see [Options and hot tuning](./options-and-hot-tuning.md)). Edit **Sensitivity** and per-op **Deadband** (activation threshold — ignores gesture noise below the bound). Orbit pitch uses vanilla **0–90°** (not tunable here).
4. Close with the title-bar close control for a clean view; reopen from the floating Debug chip (when Show debug panel is on) or from Options. Use the gear to jump to Options → Trackpad Camera Control.

There are no Enable-per-op or Reverse controls on the product surface. Pad/button chrome, capture-backend, and low-pass controls are **not** part of shipped play (Contacts / Assist chrome are unfinished futures). There is no CAD / gesture-style switcher in v1.

## Validate camera controls

1. With Debug on, change a Sensitivity or feel preset and confirm gestures respond immediately.
2. Change a value in Options and confirm the Debug panel rebuilds to match; change in Debug and confirm gestures update immediately — reopen Options to see slider positions catch up.
3. Turn **Show debug panel** off and confirm both the panel and the Debug reopen chip disappear.
4. **Reset to factory** while the panel is open — preset returns to Default and the panel stays visible (no OPTIONS toggle needed).
5. If gestures do nothing after tuning, check the backend and [OS gesture conflicts](./os-gesture-conflicts.md).

The Debug panel is for tuning only. Gesture capture arms on city load; you do not need to open Debug for gestures to work.
