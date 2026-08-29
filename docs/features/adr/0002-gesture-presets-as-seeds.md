# ADR 0002 — Gesture presets as seeds

## Status

Accepted

## Context

Map-app-aligned gestures often avoid OS three-finger conflicts; CAD users expect three-finger orbit. Hardcoding either profile locks out the other audience. Experimentation requires mid-session changes. Preset names must not imply a single OS.

## Decision

Expose **Maps+** and **CAD** as Options **presets that seed** a fully editable binding and feel table. Every parameter lives in ModSettings and applies hot. Defaults exist only in the settings schema. OS-specific modifier key labels belong in client platform notes, not in the preset product name.

## Consequences

- Default experience is map-app-friendly; CAD is one click away.
- Custom overrides are first-class.
- Camera/gesture code must not embed feel literals.
- Client docs explain OS gesture conflicts when using CAD three-finger orbit.
