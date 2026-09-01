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
4. If **Show debug panel** is on, the floating **Debug** panel comes back after reload (see [Debug UI across auto-reload](#debug-ui-across-auto-reload)). Footer **Built (UTC)** and **asm** must change after each rebuild.

## Debug UI across auto-reload

When Cities reloads the assembly it runs `OnDisabled` then `OnEnabled` while the city stays loaded (`OnLevelLoaded` does not fire again).

**Current behavior:** `OnEnabled` calls `TuningPanelHost.EnsureCreated()` and `ApplyVisibility()`. With **Show debug panel** on and a city UI view available, the floating Debug panel is recreated immediately — no city reload and no Options toggle required. Footer **Built (UTC)** / **asm** update with the new compile. On the main menu or early boot (no `UIView`), `EnsureCreated` fails soft and the panel appears on the next successful create path.

## Fallbacks

If auto-reload does not fire:

1. Main menu → Content Manager → disable **Trackpad Camera Control** → enable again.
2. Full game restart.
3. If the DLL was locked during copy: disable the mod first, rebuild, then enable.

## Related

- [Local MVP install](./local-mvp-install.md) — first-time Managed path / Harmony
- [Harnesses and testing](./harnesses-and-testing.md) — unit tests without the game
- [QA checklist](./qa-checklist.md) — manual gesture pass after reload
- Capture log: `tail -f "${TMPDIR:-/tmp}/trackpad-camera-control.log"`
- Upstream: [Advanced Mod Setup → Automate](https://skylines.paradoxwikis.com/Advanced_Mod_Setup#Automate)
