# Mod reload during development

Fast iteration when changing mod code. Start from the known Cities workflow ([Advanced Mod Setup → Automate](https://skylines.paradoxwikis.com/Advanced_Mod_Setup#Automate)), then use this repo’s extras (product semver, Debug build stamp).

## What works (Paradox Automate)

1. **Post-build deploy** — a successful `dotnet build` of the mod project copies `TrackpadCameraControl.dll` (and `PreviewImage.png` when present) into the local Mods folder. You do not need a separate copy step for the default loop.
2. **Automatic reload** — `AssemblyVersion` is `Major.Minor.*` (from `package.json` major.minor). Each compile gets a new build/revision, so Cities can **reload the mod while the game is running** without a full reboot. `Deterministic` is `false` so wildcards are allowed.

Default Mods path (macOS):

`~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/TrackpadCameraControl`

Override with `CitiesMods=…` or `CITIES_MODS=…`. Skip deploy with `-p:SkipModDeploy=true` (tests already skip via `EnableCitiesRefs=false`).

## Product semver vs assembly identity

| Field                                             | Source                                                             | Purpose                                    |
| ------------------------------------------------- | ------------------------------------------------------------------ | ------------------------------------------ |
| Product version (Options / Content Manager title) | `package.json` → `BuildInfo.ProductVersion` / InformationalVersion | Stable Changesets semver (e.g. `0.2.0`)    |
| Assembly version                                  | `Major.Minor.*`                                                    | Changes every build so Cities auto-reloads |
| Built (UTC) + asm                                 | Debug panel footer                                                 | Confirm the new DLL loaded                 |

Do **not** put `1.0.*` wildcards on the **product** / InformationalVersion string — Share and storefront copy stay on Changesets semver.

## Recommended loop (game already running)

1. Keep Cities open (city loaded is fine for many changes; exit to main menu if reload looks stuck).
2. From the repo root:

   ```bash
   ./scripts/install-mod-local.sh
   # or
   dotnet build mod/TrackpadCameraControl.csproj -c Release
   ```

3. Watch Content Manager / play — the game should pick up the new assembly version.
4. Confirm **Debug** reappears if **Show debug panel** was on (OnEnabled recreates it after Destroy; `OnLevelLoaded` alone is not enough mid-city).
5. Footer **Built (UTC)** and **asm** must change after each rebuild.

## Debug UI across auto-reload

Cities calls `OnDisabled` then `OnEnabled` when the assembly version changes. Disable destroys the Debug panel. Enable must recreate it when a city UI view exists — waiting for the next `OnLevelLoaded` leaves Options “Show debug panel” checked with no panel.

## Fallbacks

If auto-reload does not fire:

1. Main menu → Content Manager → disable **Trackpad Camera Control** → enable again.
2. Full game restart.
3. If the DLL was locked during copy: disable the mod first, rebuild, then enable.

## Optional symlink

When you want the Mods folder to point at `bin/Release` without post-build copy:

```bash
./scripts/install-mod-local.sh --symlink
```

That sets `SkipModDeploy` and links the DLL. Prefer the default post-build copy unless you need the symlink.

## Related

- [Local MVP install](./local-mvp-install.md) — first-time Managed path / Harmony
- [Harnesses and testing](./harnesses-and-testing.md) — unit tests without the game
- [QA checklist](./qa-checklist.md) — manual gesture pass after reload
- Capture log: `tail -f "${TMPDIR:-/tmp}/trackpad-camera-control.log"`
- Upstream: [Advanced Mod Setup → Automate](https://skylines.paradoxwikis.com/Advanced_Mod_Setup#Automate)
