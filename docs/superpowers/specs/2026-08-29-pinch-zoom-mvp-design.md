# Pinch → Zoom MVP — Design

**Date:** 2026-08-29  
**Status:** Approved  
**Scope:** End-to-end proof that trackpad pinch zooms the CS1 camera on macOS

## Goal

In Cities: Skylines I on macOS, a trackpad **pinch** changes camera **zoom** with a local mod install. No Options UI.

## Locked decisions

| Concern       | Choice                                                        |
| ------------- | ------------------------------------------------------------- |
| Gesture       | Pinch → zoom only                                             |
| Proof bar     | End-to-end in-game                                            |
| Settings      | In-memory `ModSettings`; no Options UI                        |
| Dev transport | Separate TrackpadBridge over local IPC                        |
| Deploy target | In-process capture in the mod DLL (seam now; implement later) |

## Architecture

Shared core: `GestureFrame` → `GestureBindingResolver` (pinch→Zoom) → `CameraApplicator` → `CameraController`.

Two backends behind `IGestureSource`:

| Path                | Role                                                                                                   |
| ------------------- | ------------------------------------------------------------------------------------------------------ |
| Dev / bridge        | Separate process streams primitives over a Unix domain socket; crash isolation and easy restart        |
| Deploy / in-process | Same Multitouch logic loaded in-process; Workshop-shaped packaging. MVP ships a disconnected stub only |

## Components

| Piece                    | Responsibility                                                      |
| ------------------------ | ------------------------------------------------------------------- |
| TrackpadBridge (macOS)   | MultitouchSupport → pinch scale delta → socket frames               |
| Shared protocol          | Fixed binary `GestureFrame` layout                                  |
| `IpcGestureSource`       | Connect, read, reconnect; fail soft                                 |
| `InProcessGestureSource` | Stub: disconnected                                                  |
| Resolver / applicator    | Zoom only, using `ZoomSensitivity` / `InvertZoom` / `PinchDeadband` |

## Fail-soft

Missing bridge, socket errors, or MultitouchSupport load failure: mod stays enabled; vanilla input works.

## Acceptance

- Pinch with the game focused zooms the camera.
- One-finger tools remain usable.
- Quitting or missing the bridge does not disable the mod or break vanilla input.

## Out of scope

Pan, orbit, yaw, Options UI, working in-process capture, Windows/Linux backends, Steam Workshop packaging.
