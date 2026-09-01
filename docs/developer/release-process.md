# Release process

Maintainer checklist to ship a **GitHub Release** and the **Steam Workshop** item for Trackpad Camera Control. v1 is **macOS trackpad only** — Share from a Mac Steam install; do not list Windows or Linux as supported.

Paste-ready title, description, and tags: [Workshop storefront](./workshop-storefront.md). In-game pass/fail: [QA checklist](./qa-checklist.md). Versioning mechanics: [Commits and releases](./commits-and-releases.md). Player install: `docs/client/install-and-first-run.md`.

## End-to-end

1. **Prove it** — [QA checklist](./qa-checklist.md) on the advertised Mac; fill the session row and known-good table before a public splash.
2. **Version** — merge feature work to `main`; let Changesets open/update the version PR; merge that so Content Manager shows the public version (v1: **1.0.0**). Tag/Release archives follow [commits and releases](./commits-and-releases.md).
3. **Install locally** — `./scripts/install-mod-local.sh` copies the DLL and `PreviewImage.png` into the Mods folder ([local MVP install](./local-mvp-install.md)).
4. **Share (or Update)** — Content Manager → Mods → **Share** on a Mac (steps below). Hidden or friends-only until [public-splash readiness](./community-and-marketing.md).
5. **Record** — Workshop ID into storefront + client install; required item is Harmony; run Setup + first-run gestures from a **subscribe** install, not only the local DLL.
6. **Announce** — [Community and marketing](./community-and-marketing.md). Keep the **same Workshop item** when another OS ships ([storefront rename later](./workshop-storefront.md)).

## Workshop Share (Mac)

Cities Content Manager can reject some **deterministic** net35 assemblies. This repo’s mod project already sets non-deterministic builds. Assembly version still comes from `package.json` (Changesets) — do not switch to `1.0.*` wildcards.

### Preview image

Content Manager and Share read **`PreviewImage.png`** next to the DLL in the local Mods folder (`TrackpadCameraControl`). The thumbnail must read as a **silver trackpad with a hand operating it**.

| Rule      | v1 default                                       |
| --------- | ------------------------------------------------ |
| Filename  | `PreviewImage.png` (exact)                       |
| Shape     | Square **1:1** (other ratios get letterbox bars) |
| Size      | **512×512** (Share also accepts up to ~644×644)  |
| File size | Under **2 MB**                                   |

Replace the PNG in the repo and reinstall before Share. After the item exists, **Update** re-uploads the folder (including a new preview).

### Share dialog

1. Enable the local mod in Content Manager (Mods tab).
2. Click **Share** on **Trackpad Camera Control (macOS)**.
3. Paste title, description, and tags from [Workshop storefront](./workshop-storefront.md).
4. Confirm `PreviewImage.png` is the thumbnail (folder icon in the Share dialog).
5. **Required items:** [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402) only — not Skyve, not Patch Loader.
6. Visibility: hidden or friends-only until splash readiness; then public.
7. Share; record the Workshop ID.

Harmony must load before this mod. Players who already use Skyve get Harmony-first order automatically; everyone else enables Harmony, then this mod.

## After first publish

- [ ] Workshop ID in [storefront](./workshop-storefront.md) and client install
- [ ] Workshop page shows Harmony as a required item
- [ ] Subscribe on a clean Mac profile and re-run [QA](./qa-checklist.md) Setup + trackpad camera
- [ ] Later Updates use change notes; never a second Workshop item for another OS

## References

External guides this process follows. We adopt Share-from-Content-Manager, non-deterministic net35, Mods-folder deploy, and a square `PreviewImage.png`. We do **not** require Skyve or Patch Loader, and we do not use overlay-image `Files/` folders (that is another mod’s product).

| Guide                                                                                                 | What we use                                                                                                   |
| ----------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| [Publishing a mod on the Steam Workshop](https://city-skylines-modding.github.io/docs/guides/gui-008) | Enable local mod → Content Manager → Mods → **Share**; title, description, tags                               |
| [Paradox wiki — Modding](https://skylines.paradoxwikis.com/Modding)                                   | Content Manager is the share and manage path                                                                  |
| [Paradox wiki — Advanced Mod Setup](https://skylines.paradoxwikis.com/Advanced_Mod_Setup)             | Copy into the Mods folder; Workshop vs deterministic builds (keep Changesets versions, not `1.0.*`)           |
| [Cities Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2040656402)                   | Sole **required item**                                                                                        |
| [Skyve v4](https://steamcommunity.com/sharedfiles/filedetails/?id=2881031511)                         | Optional player load-order helper; Mac app needs Wine; **not** a required item                                |
| [Image Overlay Renewal](https://github.com/Mbyron26/ImageOverlayRenewal)                              | Pattern: short getting-started + Harmony link in README / Workshop copy — not their overlay `Files/` workflow |

In-repo: [commits and releases](./commits-and-releases.md), [Workshop storefront](./workshop-storefront.md), [QA checklist](./qa-checklist.md), [local MVP install](./local-mvp-install.md), [community and marketing](./community-and-marketing.md), [GitHub project controls](./github-project-controls.md).
