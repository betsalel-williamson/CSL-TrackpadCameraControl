# Local MVP install (rewrite)

Deploy the rewrite mod for A/B playtest beside the shipping build. Player-facing install for the shipping mod stays under repo-root client docs until cutover.

## Deploy

From the repository root, build and copy into the Cities Mods folder as **`TrackpadCameraControl.Rewrite`**:

`./scripts/install-mod-local.sh --rewrite`

(Short form: `-r`.)

| Path                                       | Role                                           |
| ------------------------------------------ | ---------------------------------------------- |
| `./scripts/install-mod-local.sh`           | Shipping → Mods/`TrackpadCameraControl`        |
| `./scripts/install-mod-local.sh --rewrite` | Rewrite → Mods/`TrackpadCameraControl.Rewrite` |

Requires Cities: Skylines Managed assemblies (override with `CitiesManaged` / `CITIES_MODS` as with shipping). Until `rewrite/mod` is buildable, `--rewrite` exits with a docs-first message and does not touch the shipping Mods folder — see [Repository layout](./repository-layout.md).

## A/B — enable only one mod

Content Manager must run **either** shipping **or** rewrite — not both:

| Enable                             | Disable                          |
| ---------------------------------- | -------------------------------- |
| Trackpad Camera Control (shipping) | TrackpadCameraControl.Rewrite    |
| TrackpadCameraControl.Rewrite      | Shipping Trackpad Camera Control |

Both enabled double-apply gestures, fight Harmony suppress, and invalidate parity runs. Cities Harmony stays enabled for either path.

## In game

1. Enable **Cities Harmony**.
2. Enable **only** the rewrite mod (or only shipping for the control side).
3. Load a city; keep the game focused; exercise Maps+ chords.
4. Record results on the [QA checklist](./qa-checklist.md) parity matrix.

Capture remains in-process AppKit for ship-shaped builds. Do not use Contacts or bridge socket paths for rewrite parity QA unless a compile-flag experiment is explicitly under test ([Feature flags](./feature-flags.md)).

## Related

- [Harnesses and testing](./harnesses-and-testing.md)
- [QA checklist](./qa-checklist.md)
- [Repository layout](./repository-layout.md)
