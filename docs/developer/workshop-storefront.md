# Workshop storefront

Paste-ready **Steam Workshop** and **Content Manager** title, description, and search keywords for Trackpad Camera Control. How to ship a Release and Share from a Mac: [Release process](./release-process.md). Announcement sequencing stays in [Community and marketing](./community-and-marketing.md). Naming identity: [Repository layout](./repository-layout.md).

## Naming policy

| Layer                                   | Value                                                                             |
| --------------------------------------- | --------------------------------------------------------------------------------- |
| **Core display name** (durable)         | Trackpad Camera Control                                                           |
| **v1 Workshop / Content Manager title** | Trackpad Camera Control (macOS)                                                   |
| **Folder / assembly / repo**            | `TrackpadCameraControl` / `CSL-TrackpadCameraControl` — never rename for platform |

`(macOS)` is a **temporary discoverability tag**, not product identity. When another OS backend ships, **update the same Workshop item**, drop the tag from the title, and keep the Workshop ID. Do not publish a second item.

## Title

```text
Trackpad Camera Control (macOS)
```

In-game `IUserMod.Name` / Options title uses the same core + tag, then the assembly version:

```text
Trackpad Camera Control (macOS) 1.0.0
```

## Short description

Use for `IUserMod.Description` and as the Workshop subtitle / first skim line:

```text
macOS trackpad camera — pan, pinch zoom, orbit. Middle-mouse orbit still works. Windows/Linux not supported.
```

## Workshop long description

Paste into the Workshop description field (adjust version notes as needed):

```text
Version 1.0.0 — first public macOS release.

macOS-only multitouch camera for Cities: Skylines I. Pan, pinch zoom, rotate, and Option-orbit from a laptop trackpad. A real mouse still works alongside (wheel zoom, middle-mouse orbit).

What you get
• Maps+ gestures: two-finger pan, pinch zoom, Option (⌥) + two-finger orbit, two-finger rotate for yaw / selection
• Sensitivities and feel presets hot-editable in Options
• Vanilla scroll-zoom suppressed on the precise trackpad path while enabled (mouse wheel still zooms)

Requires
• Cities: Skylines I
• Cities Harmony (required for vanilla camera suppress)
  https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402
• macOS trackpad (AppKit capture)

Getting started
1. Subscribe to Cities Harmony and enable it in Content Manager
2. Subscribe to this mod and enable it
3. Load a city, click the game window so it is focused
4. Two-finger drag to pan, pinch to zoom, Option (⌥)+two-finger to orbit
5. Options → Trackpad Camera Control to change Sensitivity or Slow / Default / Fast

If gestures do nothing: keep the game focused; turn off Mission Control / three-finger swipes stealing the trackpad (player guide on GitHub).

Compatibility (tested platforms)
• AppKit gesture APIs are old; we only claim what we (or you) have playtested.
• Maintainer matrix lives in the GitHub QA checklist — update that table when you confirm a pass.
• Not yet a full OS matrix — older macOS that still runs CS1 may work but is unproven until reported.

Share your setup
• Leave a Workshop comment or open a GitHub issue with: macOS version, Mac model (or chip), built-in vs Magic Trackpad, and which gestures work (pan / pinch / orbit / rotate).
• That helps us expand the known-good list for everyone.

Not supported
• Windows and Linux in this release — the mod may appear in Content Manager, but trackpad gestures will not work. No “coming soon” promise here.

Scope
• Trackpad gesture input only — not a full camera suite (saved views, zoom-limit overhaul, free-cam).

Conflicts
• macOS Mission Control / system gestures can steal multitouch — see the player guide OS conflict notes on GitHub if gestures do nothing

Known issues (not caused by this mod)
• On Mac, Cities/Steam/Unity sometimes show the macOS arrow instead of (or on top of) the in-game cursor after launch, or after Shift-Tab to the Steam overlay and back. That handoff is a pre-existing Steam/Paradox launcher/Unity Mac quirk — this mod does not own or fix it in v1.
• Workaround: Shift-Tab out of the Steam overlay back into the game (or Cmd-Tab once) so the in-game cursor returns. Gestures still work when the game window is focused.

Source / issues
https://github.com/betsalel-williamson/CSL-TrackpadCameraControl
```

## Tags and SEO keywords

Primary (Workshop tags + description body + README):

`trackpad` · `touchpad` · `mac` · `macos` · `macbook` · `camera` · `pinch` · `orbit` · `pan` · `zoom` · `gesture` · `laptop` · `multitouch`

Secondary (description / README; Workshop tag limits may not fit all):

`middle mouse` · `mmb` · `Cities Skylines` · `CSL`

Do **not** add `windows` or `linux` as capability tags until those backends ship.

## Share dialog (not description body)

Set these in Content Manager **Share** / **Update** ([Release process](./release-process.md)):

| Field          | v1                                                                                       |
| -------------- | ---------------------------------------------------------------------------------------- |
| Required items | [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) only |
| Preview        | `PreviewImage.png` in the local Mods folder (512×512 PNG)                                |
| Visibility     | Hidden or friends-only until splash readiness, then public                               |

Do not mark [Skyve](https://steamcommunity.com/sharedfiles/filedetails/?id=2881031511) as required.

## Tested platforms (public claim)

Keep the Workshop **Compatibility** blurb honest: claim only rows that appear in [QA checklist — Known good platforms](./qa-checklist.md). Before the first public splash, complete at least one maintainer Session platform row and copy that into the known-good table.

Invite players (Workshop comments or GitHub issues) to report:

- macOS version (name + number)
- Mac model or chip (Apple silicon / Intel)
- Built-in trackpad vs Magic Trackpad
- Which of pan / pinch / Option-orbit / two-finger rotate worked

Fold solid reports into the QA known-good table and refresh the Workshop description when the claim changes.

## Rename later (same Workshop item)

When a second OS ships:

1. Retitle Workshop / Content Manager to `Trackpad Camera Control` (drop `(macOS)`).
2. Rewrite the short description first line for the new platforms.
3. Update this shard and `IUserMod` strings in the same release.
4. Change note example: `Title no longer macOS-only — <platform> trackpad now supported.`
5. Keep the existing Workshop ID; never create a duplicate item.
