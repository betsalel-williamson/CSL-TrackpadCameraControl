# Debug UI camera chrome

## Intent

Give players an on-screen **Debug** panel (debug menu / floating tuning surface) for [feel presets](../glossary/feel-preset.md) and per-op tunables beside the city view. Optional **chrome** (pads / nudge buttons) proves the same camera pipeline gestures use when `EnableAssistChrome` is on.

## End-user outcomes

- Show or hide the in-game Debug panel (hot).
- Tune [Sensitivity](../glossary/sensitivity.md) sliders, pitch limits, and feel presets (mirrored in Options). Product UI has no Enable-per-op or Reverse controls.
- When `EnableAssistChrome` is on: drive pan, zoom, yaw, and orbit from pads and buttons through the shared apply path.

## Form

One floating panel while a city is loaded:

1. Window title: mod name + version. The title bar is the drag handle and hosts native Cities circular **close** and **gear** (Options) controls. Header buttons are translucent at rest and stronger on hover. While the pointer is over the panel, the body and title bar are fully opaque; moving off softens both so the city shows through. Gear opens vanilla **OPTIONS**. Closing the panel (while Options still allows Debug) leaves a floating **Debug** reopen chip.
2. Feel preset row (Slow / Default / Fast / **New Preset** when dirty, Save as… last in the dropdown, Load on select, Reset to factory).
3. Sections in order **General → Zoom → Pan → Rotate → Orbit**. Label + Sensitivity control share one row.
4. Footer line: **Built (UTC)** and **asm** identity — confirm the latest compile loaded after auto-reload; **Copy** pastes build info to the clipboard. **Include system info (OS, devices)** (default on) adds OS + Mac model, connected keyboard/mouse/trackpad **models** (VID:PID / transport; duplicates as ×N; never serials), and dependency-critical **assembly** versions (Unity, Harmony, game, this mod) for QA. Both the Copy checkbox and title-bar dismiss state **persist across sessions** (schema 4).
5. Chrome pads/buttons only when `EnableAssistChrome` is on.
6. Closable via the native circular close control; development defaults may keep the panel on.

## Control contract (chrome when flagged on)

- Debug chrome emits the same **camera ops** as trackpad gestures.
- Pads use [Sensitivity](../glossary/sensitivity.md); optional [low-pass](../glossary/low-pass.md) only under Contacts.
- Buttons use [button step](../glossary/button-step.md), skip low-pass, and are not multiplied by Sensitivity.
- Ops flow through the shared apply path (schema inverts when set, pitch clamp, pan city-bounds clamp).
- Debug UI does **not** write the camera through a second path and does **not** synthesize OS Multitouch frames.

## Options

| Setting (product label) | Default (development) | Default (ship) | Hot |
| ----------------------- | --------------------- | -------------- | --- |
| Show debug panel        | On                    | Off            | yes |

Schema may retain an Assist-named field; the product label is **Show debug panel**. Turning it **off** hides both the Debug panel and the floating Debug reopen chip. See [settings and hot configuration](./settings-and-hot-configuration.md). Cities Options mirrors tunables but does not host chrome.

## Acceptance criteria

- With Show debug panel on, the floating panel shows feel presets and tunables while a city is loaded; title bar drag moves the whole panel 1:1 with the cursor; native circular close and gear appear in the title bar (translucent at rest, stronger on hover); hovering the panel shows it at full opacity; moving off softens it so the city shows through; gear opens vanilla OPTIONS.
- A change in the Debug panel appears immediately in Options (and the reverse); every change autosaves.
- When `EnableAssistChrome` is on, pads/buttons move the camera through the shared apply path.
- Turning Show debug panel off removes the panel and the floating Debug reopen chip immediately (no restart).
- After a mid-city mod auto-reload (assembly version change), with Show debug panel still on, the floating panel is recreated without requiring a city reload or toggling Options.
- Gestures and Debug UI share one apply path in the same session.

## Non-goals

- Replacing trackpad gestures as the primary input.
- Debug chrome inside the Cities Options page.
- Shipping chrome while `EnableAssistChrome` is off.
- Re-enabling Enable-per-op or Reverse on the product UI this pass.
