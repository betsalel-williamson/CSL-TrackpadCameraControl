# macOS gesture conflicts

Platform-specific notes for the v1 macOS AppleKit backend. Shipped gesture style is [Maps+](../glossary/maps-plus-preset.md); feel tuning is in [feel presets](./feel-presets.md).

## Mission Control and Spaces

By default, three-finger swipes drive Mission Control and desktop Spaces. Maps+ avoids that conflict: orbit uses Option (`⌥`)+two-finger, not three fingers.

A future [CAD](../glossary/cad-preset.md) three-finger orbit style would likely need Mission Control / Spaces remapped to **four fingers** in System Settings → Trackpad — another reason v1 ships Maps+ only.

## Accessibility three-finger drag

If Pointer Control → Trackpad Options enables three-finger drag for moving windows, macOS already claims three fingers (and often moves Mission Control to four). That only matters if three-finger orbit ships later — Maps+ does not use three fingers for orbit.

## Two-finger scroll vs mouse wheel

Two-finger drag is also the system scroll gesture. While the mod is enabled and [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) is present, [vanilla camera suppress](../glossary/vanilla-camera-suppress.md) suppresses vanilla scroll-zoom for **precise trackpad** input so two-finger pan does not also zoom. A real **mouse wheel** still vanilla-zooms. Edge pan and keyboard camera stay.

If pan still zooms on the trackpad, confirm Cities Harmony is subscribed and the mod is enabled. macOS Mission Control / Spaces are separate OS reservations this mod cannot override.

When Options or another menu is open, or the cursor is over an active popup, two-finger scroll goes to the UI — not the city camera.

## Force click and secondary click

One-finger tools and two-finger secondary click should keep working for building and UI. If a gesture steals clicks, turn that op off in Options.
