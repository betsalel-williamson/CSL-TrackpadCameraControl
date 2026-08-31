# Community and marketing

Maintainer playbook for **where** Trackpad Camera Control builds buzz, **when** to announce, and **how** to stay honest about platform and maturity. Player-facing install steps stay in the client guide (`docs/client/install-and-first-run.md`); packaging mechanics stay in [commits and releases](./commits-and-releases.md).

## Positioning (durable pitch)

Lead with the gap: CS1 camera orbit expects a middle mouse button; trackpad players have asked for map-app-style multitouch for years; no Workshop mod ships true multitouch camera control. Vanilla remaps and OS middle-click tools are partial.

Frame as a serious camera-**input** mod: learn continuous-input patterns from Joystick Camera Control; do not claim camera-suite features (saved views, zoom-limit overhaul, free-cam). Demo beats essay: short silent clips of pan / pinch / orbit sell the product.

Product names and search keywords: [repository layout](./repository-layout.md). Gap and relatives: `docs/features/prior-art-and-scope.md`. Personas: `docs/client/personas.md`.

## Channels (CS1 only)

This product targets **Cities: Skylines I**. Do not invest primary effort in CS2-only rooms.

| Priority | Channel | Role |
| --- | --- | --- |
| 1 — Home base | Steam Workshop (when published) | Canonical install + storefront. Title, tags, GIFs, required items, and change notes *are* the ongoing announcement. |
| 2 — Soft launch | Cities: Skylines Modding Discord | Early adopters, Harmony/compatibility feedback, Mac trackpad testers before a public splash. |
| 3 — Reach | Reddit `r/CitiesSkylines` + `r/CitiesSkylinesModding` | Discovery vs technical legitimacy. One post, Workshop or Release link, short demo GIF. |
| 4 — Niche | `r/macgaming`, Mac / laptop CS1 pockets in Discord | Underserved Mac trackpad audience; strongest early word-of-mouth while v1 is macOS-first. |
| 5 — Longevity | GitHub Releases + README keywords | Beta source archives and contributor discovery — not primary player discovery once Workshop exists. |
| Optional | Paradox Plaza CS1 modding, Simtropolis, Bluesky/X | Secondary; use when a thread already exists or someone asks. |

## Announcement sequence

Durable order — not a dated backlog:

1. **Soft** — Modding Discord (and a few known Mac CS1 players). Validate install friction (Harmony, permissions, OS gesture conflicts) before hype.
2. **Quiet distribute** — GitHub Release for betas today; Workshop item when packaging exists. Description must state **macOS-first** (and stub/unsupported elsewhere) so ratings are not ambushed.
3. **Public splash** — Reddit + Discord when the Maps+ (or CAD) gesture set feels shippable on a real trackpad session, with a 10–20s demo clip.
4. **Ongoing** — Workshop comments and change notes; short “fixed X conflict” posts. Support *is* marketing for utility mods.

## Public-splash readiness

Announce broadly only when all of the following are true for the advertised platform:

- Core gestures in the advertised preset work in a focused game session (pan, pinch zoom, orbit path for that preset).
- Client install docs match the real path (Release beta vs Workshop).
- Cities Harmony requirement and vanilla-camera behavior are stated up front.
- Unsupported OS / backends are labeled unsupported — not “coming soon” theater in the Workshop blurb.
- A short demo asset exists (GIF or muted video).

Soft channels may run earlier for bug finding; public Reddit/Workshop splash waits on readiness.

## Messaging do / don’t

| Do | Don’t |
| --- | --- |
| Name the middle-mouse / trackpad gap | Promise Windows or Linux before a backend ships |
| Say macOS-first when that is the truth | Bury Harmony or suppress behavior |
| Stay a camera-input mod | Reposition as a full camera suite |
| Lead with a gesture demo | Lead with architecture or bridge internals |
| Link client install + conflict notes | Dump contributor bootstrap into player posts |

## Copy hooks (reuse)

Use these as seeds for Workshop description, Discord soft-launch, and Reddit — keep voice consistent:

- **Headline gap:** Trackpad camera control for Cities: Skylines I — pan, orbit, and pinch zoom without a three-button mouse.
- **Why now:** True multitouch camera input, not only remapped middle-click.
- **Presets:** Maps+ (map-app defaults) and CAD (three-finger orbit); sensitivities hot-editable in Options.
- **Requires:** Cities: Skylines I, Cities Harmony (for vanilla camera suppress), supported trackpad backend for your OS.

## Distribution cross-links

| Audience | Install / deploy doc |
| --- | --- |
| Players (current / Workshop later) | `docs/client/install-and-first-run.md` |
| Beta testers / local prove-out | [Local MVP install](./local-mvp-install.md) |
| Versioning and Release jobs | [Commits and releases](./commits-and-releases.md) |
| Workshop vs GitHub Release roles | [GitHub project controls](./github-project-controls.md) |
