# macOS gesture conflicts

Platform-specific notes for the v1 macOS backend. Product presets are described in [gesture presets](./gesture-presets.md).

## Mission Control and Spaces

By default, three-finger swipes drive Mission Control and desktop Spaces. The **CAD** preset uses three-finger drag for orbit, so you may need to:

- Remap Mission Control / Spaces swipes to **four fingers** in System Settings → Trackpad, or
- Stay on **Maps+**, which uses Option+two-finger for orbit instead

## Accessibility three-finger drag

If Pointer Control → Trackpad Options enables three-finger drag for moving windows, macOS already claims three fingers (and often moves Mission Control to four). CAD orbit then competes with window dragging — prefer Maps+ or disable system three-finger drag while playing.

## Two-finger scroll

Two-finger drag is also the system scroll gesture. While the mod is enabled and [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) is present, [vanilla camera suppress](../glossary/vanilla-camera-suppress.md) turns off vanilla scroll-zoom so two-finger pan does not also zoom. Edge pan and keyboard camera stay.

If pan still zooms, confirm Cities Harmony is subscribed and the mod is enabled. macOS Mission Control / Spaces (below) are separate OS reservations this mod cannot override.

## Force click and secondary click

One-finger tools and two-finger secondary click should keep working for building and UI. If a gesture steals clicks, turn that op off in Options or switch presets.
