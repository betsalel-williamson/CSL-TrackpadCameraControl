# ADR 0002 — Gesture presets as seeds

## Status

Superseded in part by [ADR 0003 — Feel profiles and product flags](./0003-feel-profiles-and-product-flags.md)

Player-facing **preset** now means a **feel profile** (Slow / Default / Fast, Save as… / Load), not Maps+/CAD as Options presets that seed bindings. Gesture-style seeding (Maps+ vs CAD orbit trigger) remains historical context; product flags and the feel-profile model live in ADR 0003.

## Context

Map-app-aligned gestures often avoid OS three-finger conflicts; CAD users expect three-finger orbit. Hardcoding either profile locks out the other audience. Experimentation requires mid-session changes. Preset names must not imply a single OS.

## Decision

Expose **Maps+** and **CAD** as Options **presets that seed** a fully editable binding and feel table. Every parameter lives in ModSettings and applies hot. Defaults exist only in the settings schema. OS-specific modifier key labels belong in client platform notes, not in the preset product name.

*(Superseded for player-facing “preset” meaning and Maps+/CAD as seeded Options presets — see ADR 0003. Gesture style as a distinct concern from feel remains valid.)*

## Consequences

- Default experience is map-app-friendly; CAD is one click away.
- Custom overrides are first-class.
- Camera/gesture code must not embed feel literals.
- Client docs explain OS gesture conflicts when using CAD three-finger orbit.
- **Update (ADR 0003):** CAD and related unfinished surfaces ship behind `Enable*` flags (default off); feel profiles replace “preset” as the player-facing product model.
