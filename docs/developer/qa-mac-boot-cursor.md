# QA — Mac cold-boot OS cursor (dual cursor)

Investigation checklist for the **macOS hardware cursor** appearing over (or beside) the Cities **in-game cursor** after cold launch until **Cmd-Tab** (or similar) forces a focus cycle.

This is **independent of trackpad gestures** in the sense that it is a focus/cursor-ownership problem on Mac + Unity 5.6 / CS1 — but our mod already attempts boot activation, so we own the mitigation attempt and the evidence trail.

**Related code:** [`mod/GameFocusActivation.cs`](../../mod/GameFocusActivation.cs), called from [`mod/LoadingExtension.cs`](../../mod/LoadingExtension.cs) `OnLevelLoaded`.  
**Log file:** `${TMPDIR}/trackpad-camera-control.log` (also printed when capture starts).  
**Parent checklist:** [qa-checklist.md](./qa-checklist.md) Setup → cold boot row.

---

## Symptom (pass / fail)

**Fail (bug present):** After cold boot → load city, the **macOS arrow** (system cursor) is visible in the city view at the same time as, or instead of, the game’s tool cursor. One-finger tools mis-click or feel “off” until you Cmd-Tab out and back (or otherwise cycle focus).

**Pass:** Only the in-game cursor is usable for tools; no floating OS arrow over the city after load (without alt-tab).

**Not this bug:** Gestures not arming, Options cursor (menus should show a normal cursor), Debug panel chrome.

---

## Session record (fill every attempt)

| Field                                     | Value            |
| ----------------------------------------- | ---------------- |
| Date (UTC)                                |                  |
| macOS                                     |                  |
| Mac model                                 |                  |
| Steam overlay                             | On / Off         |
| Launch path                               | Steam / other    |
| Mod Built (UTC) / asm (Debug footer Copy) |                  |
| Branch / commit                           |                  |
| Bug reproduced?                           | Yes / No / Flaky |
| Alt-tab clears it?                        | Yes / No / N/A   |
| Log excerpt attached?                     | Yes / No         |

Paste Debug **Copy** (Include system info) under the table when filing a PR note or issue.

---

## Reliable reproduction (cold boot)

Do this the same way each time so “can’t reproduce” vs “still broken” means something.

### A. Full cold boot (preferred)

1. **Quit Cities completely** (not just main menu — process gone).
2. Optionally quit **Steam** entirely if testing overlay-off (see matrix below).
3. Confirm the Mods DLL you intend to test is installed:
   - `~/Library/Application Support/Colossal Order/Cities_Skylines/Addons/Mods/TrackpadCameraControl/TrackpadCameraControl.dll`
   - Or rebuild: `./scripts/install-mod-local.sh` from the branch under test.
4. **Steam overlay variable** (record On or Off in the session table):
   - Steam → Cities: Skylines → Properties → **Enable the Steam Overlay while in-game**.
5. Launch Cities from Steam; wait until main menu is fully up.
6. **Load a city** (or New Game → city appears). Do **not** Cmd-Tab during load.
7. As soon as the city is interactive, **look at the cursor** over the map (bulldoze / road tool helps):
   - Fail = OS hardware arrow visible / tools mis-click.
   - Pass = only game cursor; tools click correctly.
8. If Fail: **Cmd-Tab** away and back once. Note whether the OS cursor disappears (expected for this bug).
9. Open Debug (if needed) → **Copy** with system info → paste into session record.
10. Capture log evidence (next section).

### B. Soft reload (weaker — may not reproduce)

Disable/enable the mod in Content Manager or rely on assembly auto-reload. Useful for log spam, **not** a substitute for A when claiming “fixed.”

### C. Control: overlay off

Repeat **A** with Steam overlay **disabled**, full quit of Steam + game between runs. Compare Yes/No in the session table. This is the primary test for the overlay hypothesis.

---

## Log evidence (what to collect)

```bash
LOG="${TMPDIR:-/tmp}/trackpad-camera-control.log"
rg -n 'focus activate|focus cursor follow-up|gestures armed|mod enabled' "$LOG" | tail -40
```

### Expected lines (builds with diagnostic logging)

| Line pattern                                     | Meaning                                                         |
| ------------------------------------------------ | --------------------------------------------------------------- |
| `focus activated on level load …`                | `TryActivate` ran after city load                               |
| `keyWindow=0/1`                                  | Whether `makeKeyAndOrderFront` found main/key `NSWindow`        |
| `unityCursorVisible=0/1`                         | Unity `Cursor.visible` right after activate                     |
| `cgCursorVisible=0/1/?`                          | `CGCursorIsVisible` (OS hardware cursor); `?` = P/Invoke failed |
| `focus cursor follow-up rem=…`                   | Periodic re-hide while follow-up armed                          |
| `unityWasVisible=1` on follow-up                 | Vanilla (or something) set Unity cursor visible again           |
| `focus activate: no main/key NSWindow yet`       | Key-window path missed at load time                             |
| `focus activate failed` / `AppKit dlopen failed` | Activation did not run                                          |

### Interpreting “still broken” with logs

| Observation                                      | Conclusion                                                                        |
| ------------------------------------------------ | --------------------------------------------------------------------------------- |
| No `focus activated` line                        | Activation never ran (wrong build, load path, or early failure)                   |
| `focus activated` + bug still present            | Mitigation insufficient — something else keeps OS cursor (overlay, Unity, timing) |
| `cgCursorVisible=1` throughout follow-up         | OS cursor still reported visible despite hide attempts                            |
| `cgCursorVisible=0` but user still sees OS arrow | Visual ≠ CG API, or second cursor source (overlay)                                |
| Bug gone only with overlay Off                   | Strong evidence Steam overlay is involved                                         |

---

## Fix attempts (ledger)

Update this table when you try something new. Do not claim fixed without **Reproduction A** Pass on the same machine that previously Failed.

| ID  | Attempt                                                                          | In build?                                                | Result              | Evidence                                                                                     |
| --- | -------------------------------------------------------------------------------- | -------------------------------------------------------- | ------------------- | -------------------------------------------------------------------------------------------- |
| F0  | None (stock CS1 + mod without boot activate)                                     | historical                                               | Fail (reported)     | QA / first-run notes                                                                         |
| F1  | `activateIgnoringOtherApps` + `Cursor.visible = false` once on `OnLevelLoaded`   | shipped earlier (`boot-focus-activation`)                | Fail still possible | Prior QA                                                                                     |
| F2  | F1 + `makeKeyAndOrderFront` on main/key window + ~45-frame Unity re-hide         | PR #37 (`121d38f`)                                       | **Fail** reproduced | Built `2026-09-02T19:06:14Z` / asm `0.2.9741.21787`; log had `focus activated on level load` |
| F3  | F2 + `NSCursor.hide` when `CGCursorIsVisible`, ~180-frame follow-up, richer logs | local WIP on `fix/mac-boot-cursor` (commit if validated) | **TBD**             | Collect log patterns above                                                                   |
| F4  | Steam overlay **Off** (no code change)                                           | N/A                                                      | **TBD**             | Session table Overlay=Off                                                                    |
| F5  | Overlay Off + F3 together                                                        | TBD                                                      | **TBD**             |                                                                                              |

### What is not working (as of F2)

- One-shot AppKit activate does run on level load but **does not** reliably clear the dual cursor.
- Unity `Cursor.visible = false` alone is **insufficient** on cold boot for the hardware cursor.
- Short Unity-only follow-up (~45 frames) was **not** enough on the reproducing machine.

### What is left to try

1. **F4 — Steam overlay off** (no code): primary environmental test.
2. **F3 — NSCursor.hide + diagnostics** (code): already prototyped; needs cold-boot Pass/Fail + log paste.
3. **Delay activate** until first focused frame / N seconds after `OnLevelLoaded` (race with load UI / overlay).
4. **Re-arm on `Application.isFocused` rising edge** after load (overlay steals then returns focus).
5. **`CGDisplayHideCursor` / associate mouse** (more aggressive; higher risk of stuck hidden cursor in menus — gate carefully).
6. **Harmony / observe** game or Steam cursor show calls (research-heavy; only if F3–F5 fail).
7. **Document-as-external** if overlay-off is the only reliable Pass: player-facing note + QA “known interaction with Steam overlay.”

**Out of scope / avoid:** Hiding cursor every frame forever; fighting Options/menu cursor; claiming a full cure without Reproduction A.

---

## Validation checklist (copy into PR)

### Preflight

- [ ] Correct DLL installed; Debug footer Built/asm matches the attempt (F2 vs F3)
- [ ] Session table filled (especially **Steam overlay On/Off**)
- [ ] Log path known; old log tailed or noted so new lines are visible

### Reproduce broken (baseline)

- [ ] Reproduction **A** with overlay **On** → **Fail** (or note if flaky)
- [ ] Cmd-Tab clears symptom → Yes/No
- [ ] Log shows `focus activated` (or note absence)

### Try mitigation

- [ ] Overlay **Off** cold boot (F4) → Pass / Fail
- [ ] F3 build cold boot overlay On → Pass / Fail + paste `focus activated` / `follow-up` / `cg=` lines
- [ ] F3 + overlay Off (F5) → Pass / Fail

### Safety

- [ ] Options / pause menu still show a usable cursor
- [ ] After alt-tab, game remains playable
- [ ] Gestures still arm (`gestures armed` in log)

### Decision

- [ ] Root cause narrowed (overlay / Unity / timing / unknown)
- [ ] Next ledger row filled (F3–F7…)
- [ ] Player docs updated only after a stable Pass or a clear “known external” ruling

---

## Quick commands

```bash
# Install branch under test
./scripts/install-mod-local.sh

# Watch focus/cursor lines live
tail -f "${TMPDIR:-/tmp}/trackpad-camera-control.log" | rg 'focus |gestures armed'

# After a run, summarize
rg -n 'focus activate|focus cursor follow-up' "${TMPDIR:-/tmp}/trackpad-camera-control.log" | tail -30
```
