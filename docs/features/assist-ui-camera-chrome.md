# Assist UI camera chrome

## Intent

Give players an on-screen Assist / tuning panel for [feel presets](../glossary/feel-preset.md) and per-op tunables beside the city view. Optional **chrome** (pads / nudge buttons) proves the same camera pipeline gestures use when `EnableAssistChrome` is on.

## End-user outcomes

- Show or hide the in-game Assist / tuning panel (hot).
- Tune [Sensitivity](../glossary/sensitivity.md), reverse, enables, pitch limits, and feel presets (mirrored in Options).
- When `EnableAssistChrome` is on: drive pan, zoom, yaw, and orbit from pads and buttons through the shared apply path.

## Form

One floating panel while a city is loaded:

1. Feel preset row (Slow / Default / Fast, Save as… / Load, Reset to factory).
2. Per-op sections with meaning + activation, then enable / reverse / Sensitivity (and orbit pitch min/max). Multi-column layout.
3. Chrome pads/buttons only when `EnableAssistChrome` is on.
4. Closable; development defaults may keep the panel on.

## Control contract (chrome when flagged on)

- Assist chrome emits the same **camera ops** as trackpad gestures.
- Pads use [Sensitivity](../glossary/sensitivity.md); optional [low-pass](../glossary/low-pass.md) only under Contacts.
- Buttons use [button step](../glossary/button-step.md), skip low-pass, and are not multiplied by Sensitivity.
- Ops flow through the shared apply path (inverts, per-op enables, pitch clamp).
- Assist UI does **not** write the camera through a second path and does **not** synthesize OS Multitouch frames.

## Options

| Setting           | Default (development) | Default (ship) | Hot |
| ----------------- | --------------------- | -------------- | --- |
| Assist UI enabled | On                    | Off            | yes |

See [settings and hot configuration](./settings-and-hot-configuration.md). Cities Options mirrors tunables but does not host chrome.

## Acceptance criteria

- With Assist UI on, the floating panel shows feel presets and tunables while a city is loaded.
- When `EnableAssistChrome` is on, pads/buttons move the camera through the shared apply path.
- Turning Assist UI off removes the panel immediately (no restart).
- Disabled camera ops do not fire from chrome.
- Gestures and Assist UI share one apply path in the same session.

## Non-goals

- Replacing trackpad gestures as the primary input.
- Assist chrome inside the Cities Options page.
- Shipping Assist chrome while `EnableAssistChrome` is off.
