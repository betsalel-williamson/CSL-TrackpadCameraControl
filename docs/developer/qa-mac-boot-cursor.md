# QA — Mac cold-boot / Steam overlay cursor (deferred to v2)

**Status (2026-09-02):** **Do not fix in this mod for v1.** Investigation closed for Round 3; reopen for v2 if prioritized.

The dual / wrong cursor on Mac is a **pre-existing Steam + Paradox launcher + Unity/CS1** interaction, not something this mod should own. Round 3 attempts to harden `GameFocusActivation` were **reverted** after `NSCursor.hide` left the cursor **entirely hidden**.

**Related (shipped, unchanged):** original one-shot `GameFocusActivation.TryActivate` from earlier boot-focus work may still run on city load — it does not solve this and must not grow into aggressive OS cursor hiding.  
**Parent checklist:** [qa-checklist.md](./qa-checklist.md).

---

## Symptom

| Mode                     | What you see                                                                               |
| ------------------------ | ------------------------------------------------------------------------------------------ |
| Dual / wrong cursor      | macOS arrow over or instead of in-game tool cursor after launch/load until focus cycles    |
| Overlay cycle (reliable) | **Shift-Tab** → Steam overlay → **OS cursor**; Shift-Tab again → game → **in-game cursor** |
| Launcher                 | Same class of issue clicking Resume/Play in **Paradox launcher**                           |
| Flaky good path          | Sometimes in-game cursor is correct immediately when Steam overlay notification appears    |

**Confirmed:** Same Steam account, **two different Macs** (incl. Mac17,6 / macOS 26.6.2). Overlay **enabled**.

---

## Reproduction (for v2 — do not chase in v1)

### Steam overlay (most reliable demo)

1. Steam overlay **enabled** for Cities: Skylines.
2. In city view with **in-game cursor** active.
3. **Shift-Tab** → overlay → OS cursor.
4. **Shift-Tab** → back to game → in-game cursor.

This shows overlay and game disagree about cursor ownership; cold-boot dual cursor is the same family of bug.

### Cold boot (flaky)

1. Full quit Cities (and optionally Steam).
2. Launch via Steam → Paradox launcher → Play/Resume → load city.
3. Note whether OS cursor appears over the city without Shift-Tab.
4. Sometimes Pass immediately when overlay toast shows.

### Session record

| Field                    | Example / value                                                                 |
| ------------------------ | ------------------------------------------------------------------------------- |
| Date                     | 2026-09-02 (cursor investigation); platform reconfirmed 2026-09-03              |
| macOS / model            | 26.6.2 / Mac17,6                                                                |
| Steam overlay            | On                                                                              |
| Launch                   | Steam → Paradox launcher                                                        |
| Mod build                | Built 2026-09-03T01:11:24Z · asm 0.2.9741.32742 (also earlier 0.2.9741.22220)   |
| Dual cursor on load      | Intermittent                                                                    |
| Shift-Tab overlay ↔ game | Reproducible cursor swap                                                        |
| Second Mac               | Same account — also Mac14,2 / 26.5.2 (see [qa-checklist.md](./qa-checklist.md)) |
| Input (2026-09-03 Copy)  | Built-in trackpad; Magic Keyboard (BT); Logitech G500s (USB)                    |

---

## Fix ledger (closed for v1)

| ID  | Attempt                                                                   | Result                    | Notes                                            |
| --- | ------------------------------------------------------------------------- | ------------------------- | ------------------------------------------------ |
| F1  | `activateIgnoringOtherApps` + `Cursor.visible = false` on `OnLevelLoaded` | Insufficient              | Still fails / flaky; left as historical one-shot |
| F2  | + `makeKeyAndOrderFront` + short Unity re-hide                            | **Fail**                  | Still reproduced; `focus activated` logged       |
| F3  | + `NSCursor.hide` / CG visibility + longer follow-up                      | **Harmful**               | Cursor became **entirely hidden** — reverted     |
| F4  | Steam overlay Off                                                         | Not required for v1 close | Optional data for v2                             |
| —   | **Decision**                                                              | **Defer to v2**           | Do not ship further mod cursor ownership hacks   |

### What we learned

- Mitigation **does** run (`focus activated on level load`) and still does not own the real problem.
- Aggressive AppKit hide can **break** the game cursor worse than the original bug.
- Steam overlay Shift-Tab is a **clean repro** of OS vs in-game cursor handoff without blaming the mod.
- Paradox launcher path is in the same story.

### What is left for v2 (ideas only — not scheduled)

1. Document player workaround: Shift-Tab twice, or toggle overlay, if dual cursor appears.
2. Optional: detect Steam overlay / document “known with overlay.”
3. Research without `NSCursor.hide` spam (delayed activate, focus rising-edge only, never stack hides).
4. Consider whether **removing** even F1 one-shot is cleaner than keeping a no-op-looking activate.

---

## v1 product guidance

- **QA:** Treat dual cursor / overlay cursor swap as **environment known issue**, not a mod regression.
- **Workshop / mod description:** Paste-ready **Known issues** copy lives in [workshop-storefront.md](./workshop-storefront.md) — keep that text honest that this is Steam/Unity/Paradox Mac, not the mod.
- **First-run / support:** Point at Shift-Tab or focus cycle if tools mis-click; do not tell players the mod “fixes” Mac cursor.
- **Code:** No further `GameFocusActivation` expansion in Round 3 / v1 launch stack.
