# Debug UI camera chrome

## Intent

Give players an on-screen **Debug** panel (debug menu / floating tuning surface) for [feel presets](../glossary/feel-preset.md) and per-op tunables beside the city view. Optional **chrome** (pads / nudge buttons) proves the same camera pipeline gestures use when `EnableAssistChrome` is on.

## End-user outcomes

- Show or hide the in-game Debug panel (hot).
- Tune [Sensitivity](../glossary/sensitivity.md) sliders, pitch limits, and feel presets (mirrored in Options). Product UI has no Enable-per-op or Reverse controls.
- When `EnableAssistChrome` is on: drive pan, zoom, yaw, and orbit from pads and buttons through the shared apply path.

## Form

One floating panel while a city is loaded:

1. Window title: mod name + version. The entire title bar is the drag handle.
2. Feel preset row (Slow / Default / Fast / **New Preset** when dirty, Save as… last in the dropdown, Load on select, Reset to factory).
3. Sections in order **General → Zoom → Pan → Rotate → Orbit**. Layout rhythm (contract): prior content → horizontal rule → section title (indented) → rows (further indented). Label + Sensitivity control share one row.
4. Chrome pads/buttons only when `EnableAssistChrome` is on.
5. Closable; development defaults may keep the panel on.

## Control contract (chrome when flagged on)

- Debug chrome emits the same **camera ops** as trackpad gestures.
- Pads use [Sensitivity](../glossary/sensitivity.md); optional [low-pass](../glossary/low-pass.md) only under Contacts.
- Buttons use [button step](../glossary/button-step.md), skip low-pass, and are not multiplied by Sensitivity.
- Ops flow through the shared apply path (schema inverts when set, pitch clamp, pan city-bounds clamp).
- Debug UI does **not** write the camera through a second path and does **not** synthesize OS Multitouch frames.

## Options

| Setting (product label) | Default (development) | Default (ship) | Hot |
| ----------------------- | --------------------- | -------------- | --- |
| Debug UI enabled        | On                    | Off            | yes |

Schema may retain an Assist-named field; the product label is **Debug**. See [settings and hot configuration](./settings-and-hot-configuration.md). Cities Options mirrors tunables but does not host chrome.

## Acceptance criteria

- With Debug UI on, the floating panel shows feel presets and tunables while a city is loaded; title bar drag moves the whole panel.
- A change in the Debug panel appears immediately in Options (and the reverse); every change autosaves.
- When `EnableAssistChrome` is on, pads/buttons move the camera through the shared apply path.
- Turning Debug UI off removes the panel immediately (no restart).
- Gestures and Debug UI share one apply path in the same session.

## Non-goals

- Replacing trackpad gestures as the primary input.
- Debug chrome inside the Cities Options page.
- Shipping chrome while `EnableAssistChrome` is off.
- Re-enabling Enable-per-op or Reverse on the product UI this pass.
