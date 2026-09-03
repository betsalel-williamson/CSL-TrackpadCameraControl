# QA Debug clipboard report (trim + assemblies)

## Goal

Make Debug panel **Copy** (with **Include system info** on) paste only what helps trackpad/mod troubleshooting and QA parity checks — not noise, not serials.

## Clipboard shape

```text
TrackpadCameraControl: <asm identity>
Built (UTC): …

--- System ---
OS: <SystemInfo.operatingSystem or Environment.OSVersion>
Model: <SystemInfo.deviceModel / hw.model>

--- Input devices ---
Keyboard: <model display> [×N]
Mouse: <model display> [×N]
Trackpad: <model display> [×N]

--- Assemblies ---
Unity: <Application.unityVersion when HAS_CITIES>
ICities: <…>
CitiesHarmony.API: <…>
0Harmony: <…>
```

## Keep

| Block      | Fields                                         | Why                                                                  |
| ---------- | ---------------------------------------------- | -------------------------------------------------------------------- |
| Header     | This mod asm identity, Built UTC               | Confirm which DLL is loaded + when built                             |
| System     | OS, Model only                                 | Platform matrix; OS version is the proxy for Apple framework DLL set |
| Input      | Keyboard / Mouse / Trackpad models             | Gesture source hardware                                              |
| Assemblies | Unity runtime + dependency DLLs (not this mod) | Catch mismatched Harmony / Unity / game binaries                     |

## Drop

- CPU, Memory (not actionable for trackpad QA)
- AppKit / IOKit / framework paths or CFBundle versions (same OS ⇒ same Apple libs; path-on-disk is unreliable under dyld shared cache)
- MultitouchSupport (Contacts off; revisit later if needed)
- Serial numbers (never on clipboard)
- Duplicate identical models as separate lines (use ×N)

## Input device model display (unchanged intent)

- Name + optional `VID:PID`, `rev`, transport, `built-in`
- Quantity via LocationID (in-memory only); never paste serial
- Noise filter (backlight, headset, etc.) stays

## Assemblies collection

- Resolve by **loaded** assembly name when possible (`AppDomain.CurrentDomain.GetAssemblies()`), else `missing`
- Prefer `GetName().Version`; fail soft
- Omit `UnityEngine` and `Assembly-CSharp` — Unity stamps them `0.0.0.0`; runtime is covered by `Unity:` line
- Skip any loaded assembly whose version is exactly `0.0.0.0`; keep `missing` for whitelist entries not loaded
- `Application.unityVersion` only under `#if HAS_CITIES`
- Do not dump all loaded mods — whitelist above only

## Non-goals

- Windows/Linux device or framework enumeration
- Apple framework binary version fingerprints
- Persisting the “Include system info” checkbox across sessions (session default on is fine)

## Acceptance

- Copy with checkbox on matches the shape above (no CPU/Memory)
- Copy with checkbox off is this-mod asm + Built (UTC) only
- Two identical mice → one `Mouse: … ×2` line
- Missing Harmony → `CitiesHarmony.API: missing` (or equivalent), no throw
- Docs (`debug-ui-camera-chrome`, `qa-checklist`) describe OS/Model + assemblies, not CPU/RAM
