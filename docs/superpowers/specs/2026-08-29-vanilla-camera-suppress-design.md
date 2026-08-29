# Vanilla camera suppress — Design

**Date:** 2026-08-29  
**Status:** Approved  
**Scope:** Harmony-suppress vanilla scroll-zoom and mouse-drag camera rotate while the mod is enabled; keep edge pan, keyboard, and gamepad

## Goal

Two-finger pan no longer fights Unity scroll-zoom. Players get trackpad Maps+/CAD feel while classic CS1 edge pan and keyboard camera keys still work. Turning the mod off restores full vanilla camera input (no Options checkbox in this slice).

## Locked decisions

| Concern           | Choice                                                                                    |
| ----------------- | ----------------------------------------------------------------------------------------- |
| Switch            | Mod on/off — no Options checkbox                                                          |
| Scroll-zoom       | Prefix-skip `CameraController.HandleScrollWheelEvent` while suppress enabled              |
| Mouse-drag rotate | Prefix-skip `CameraController.HandleMouseEvents` only while rotate-camera binding is held |
| Edge scrolling    | Keep — original `HandleMouseEvents` runs when rotate binding is not held                  |
| Keyboard          | Keep — no patch on `HandleKeyEvents`                                                      |
| Gamepad / analog  | Keep                                                                                      |
| Free-cam / follow | Keep — untouched                                                                          |
| Harmony missing   | Fail soft: log once; gestures still work; scroll fight may remain                         |
| CitiesHarmony API | `HarmonyHelper` from `IUserMod`; all `HarmonyLib` in static `Patcher`                     |

This supersedes the deferred “optional Harmony checkbox” in the full-gesture camera ops design. That checkbox remains out of scope.

## Architecture

```text
Mod.OnEnabled → VanillaCameraSuppress.Enabled = true
             → HarmonyHelper.DoOnHarmonyReady(Patcher.PatchAll)

Prefix HandleScrollWheelEvent → skip original if Enabled
Prefix HandleMouseEvents      → skip original if Enabled AND m_cameraMouseRotate.IsPressed()
                                else run original (edge pan)

Mod.OnDisabled → unpatch if Harmony installed
               → VanillaCameraSuppress.Enabled = false
               → existing pipeline shutdown
```

Harmony id: `com.betsalel.trackpadcameracontrol`.

Policy helpers on `VanillaCameraSuppress` are unit-tested without the game (`EnableCitiesRefs=false`). Patches compile only with `HAS_CITIES`.

## Acceptance

- Two-finger pan without vanilla scroll-zoom fight when Harmony is present and the mod is on.
- Edge pan and keyboard camera still move the camera.
- Disable the mod → full vanilla camera input returns.
- Missing Harmony → no crash; log once.

## Out of scope

- Options UI checkbox
- Patching `HandleKeyEvents`, free-cam, or ACME beyond don’t-break keyboard/edge
- In-process Multitouch deploy path
