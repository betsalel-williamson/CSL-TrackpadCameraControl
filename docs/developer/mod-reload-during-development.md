# Mod reload during development

Fast iteration when changing mod code — without treating a full game restart as the default loop.

## What “reload” means in CS1

Cities: Skylines loads local mod DLLs from the Colossal **Mods** folder. This mod implements `IUserMod.OnEnabled` / `OnDisabled` — Harmony patches, gesture capture, and the Debug panel arm on enable and tear down on disable.

There is **no** in-process hot reload of a changed DLL while the assembly stays loaded. After you rebuild, the game must **load the new file** (disable → install → enable, or restart).

## Recommended dev loop (no full restart)

Use this when the game is already running and you changed C#:

1. **Exit to the main menu** (unload the city — mod disable/enable is reliable from the menu, not mid-city).
2. **Content Manager → Mods** → disable **Trackpad Camera Control**.
3. Rebuild and install:

   ```bash
   ./scripts/install-mod-local.sh
   ```

   One-time symlink (skip copy on every build):

   ```bash
   ./scripts/install-mod-local.sh --symlink
   ```

   After `--symlink`, later builds only need `dotnet build mod/TrackpadCameraControl.csproj -c Release -p:CitiesManaged=…` — the Mods folder DLL is a link to `mod/bin/Release/net35/TrackpadCameraControl.dll`.

4. **Content Manager** → enable the mod again (`OnEnabled` runs again; Harmony and capture reconnect).
5. **Load a city** and verify gestures / Debug panel.

**Confirm the new build loaded:** open the in-game **Debug** panel (Options → Show debug panel). The footer shows **Built (UTC)** from the assembly compile timestamp — it must change after each rebuild.

## When to restart the game

Restart Cities entirely if:

- Disable → enable did not pick up behavior (stale assembly or file lock).
- `install-mod-local.sh` failed to copy (DLL still open — disable the mod first, then reinstall).
- You changed Harmony patch targets or static init that only runs at process start.
- Capture or Options UI looks wedged after several reload cycles.

## File lock notes (macOS)

While the mod is **enabled**, the game may keep `TrackpadCameraControl.dll` open. **Disable the mod in Content Manager before** copying or replacing the DLL (non-symlink installs). Symlink installs still require disable before `dotnet build` overwrites the target if the linker path is locked.

## Related

- [Local MVP install](./local-mvp-install.md) — first-time Mods folder setup
- [Harnesses and testing](./harnesses-and-testing.md) — unit tests without the game
- [QA checklist](./qa-checklist.md) — manual gesture pass after reload
- Capture log: `tail -f "${TMPDIR:-/tmp}/trackpad-camera-control.log"`
