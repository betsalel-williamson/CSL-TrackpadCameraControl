# QA Round 2 — Debug UI stack

> **For agentic workers:** One bug per subagent. **Pause for human in-game verification after each fix** before starting the next task. Do not batch fixes.

**Goal:** Close five in-game QA findings on the Debug panel stack without regressing translucency, keymapping labels, deadband tuning, or numeric validation work.

**Stack base (trunk → tip before Round 2):** `fix/first-load-init` → … → `feat/debug-panel-translucency` → `feat/debug-deadband-tunables` (PRs #26–#32).

**Round 2 branches** (each stacks on the previous; one PR each after verification):

| #   | Branch                               | PR title (draft)                                           | Finding                                                                                           |
| --- | ------------------------------------ | ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| 1   | `fix/debug-panel-behind-options`     | fix: keep Debug panel behind Options when menu open        | Enabling Debug from Options or keymapping refresh must not steal foreground from Options          |
| 2   | `fix/debug-preset-dropdown`          | fix: show feel preset dropdown list in Debug panel         | Preset dropdown click does not open item list                                                     |
| 3   | `fix/debug-preset-name-alignment`    | fix: left-align preset name field (LTR/RTL)                | Name field text centered instead of start-aligned                                                 |
| 4   | `fix/keymapping-label-copy`          | fix: use Keymapping(s) label instead of vanilla in op copy | User-facing binding lines say Keymapping(s), not “vanilla”                                        |
| 5   | `fix/verify-op-heading-nomenclature` | fix: restore op heading / locale nomenclature              | Confirm section labels & locale strings were not corrupted (Zoom/Pan/Rotate/Orbit, binding lines) |

**After Round 2 (rebase v1 QA docs):**

| #   | Branch              | Notes                                                                                                               |
| --- | ------------------- | ------------------------------------------------------------------------------------------------------------------- |
| 6   | `docs/v1-launch-qa` | Rebase onto `fix/verify-op-heading-nomenclature`; fold Round 2 pass/fail rows into `docs/developer/qa-checklist.md` |

## Global constraints

- Each task: own branch, own subagent, build + `dotnet test` (EnableCitiesRefs=false).
- Install locally when Cities DLL not locked: `./scripts/install-mod-local.sh`.
- Commit when a task is complete and tests pass; push + open/update PR only after user verifies in game (unless user asks earlier).
- Do not amend unrelated WIP on other branches.

---

## Task 1 — Debug panel z-order behind Options

**Symptom:** Toggling **Show debug panel** in Options (or keymapping label refresh/rebuild) brings Debug on top of Options. Options should stay foreground.

**Likely causes:**

- `OnPanelMouseDown` → `BringToFront()` (correct on user click, wrong if called indirectly).
- New `UIPanel` from `EnsureCreated` / `ProcessPendingUiRebuild` defaults above Options.
- No re-apply of z-order after `OnKeymappingLabelsChanged` rebuild.

**Approach:**

- Add `ApplyPanelStackOrder()` using `GameUiContext.Default.IsMenuOrOptionsOpen()` (or `UIView.HasModalInput()`).
- When Options/menu open: `SendToBack()` on Debug root (or set `zOrder` below Options panel); when closed and user focuses Debug, allow `BringToFront()` on intentional interaction only.
- Call after `EnsureCreated`, `ApplyVisibility`, `ProcessPendingUiRebuild`, and when enabling Debug from `OptionsSettingsUi`.
- **Do not** `BringToFront` on keymapping-driven rebuild.

**Verify in game:**

1. Open Options → mod tab → enable Show debug panel → Options stays on top, Debug visible behind.
2. Open Keymapping, change a camera binding → return to game → Options still on top if still open.
3. Close Options → click Debug title bar → panel comes to front.

**Files (expected):** `mod/TuningPanelHost.cs`, `mod/TuningPanelHost.Focus.cs`, maybe `mod/GameUiContext.cs`, `mod/OptionsSettingsUi.cs`.

- [ ] Task 1 implemented
- [ ] User verified in game
- [ ] PR opened / updated

---

## Task 2 — Preset dropdown list

**Symptom:** Feel preset dropdown in Debug panel does not show other presets on click.

**Hypotheses:** Dropdown list parent clipped by panel; wrong `listPosition`; missing modal layer; dropdown not child of interactive root.

**Verify:** Click dropdown → list visible; select Slow/Default/Fast applies preset.

- [ ] Task 2 — **blocked until Task 1 verified**

---

## Task 3 — Preset name field alignment

**Symptom:** Name text field centered; should follow game LTR/RTL (start-aligned).

**Approach:** Set `UITextField` / label horizontal alignment from Colossal RTL APIs if available; mirror Cities Options text fields.

- [ ] Task 3 — **blocked until Task 2 verified**

---

## Task 4 — Keymapping(s) copy

**Symptom:** Op description lines use “vanilla zoom/orbit” — confusing.

**Approach:** Change `VanillaCameraKeyLabels.FormatVanillaActionLine` user strings to **Keymapping(s):** prefix or `Middle Mouse · W: Keymapping(s) orbit` pattern per UX; update client docs/tests. Keep internal `VanillaCamera*` type names in code if desired.

- [ ] Task 4 — **blocked until Task 3 verified**

---

## Task 5 — Nomenclature / label integrity

**Symptom:** Possible corrupted section or binding labels (“neighborhood names” — verify **nomenclature**: op titles, locale strings, duplicated/mangled lines).

**Approach:** Audit `VanillaCameraKeyLabels`, `AddOpHeading`, Options `OpHeading*` labels; compare to intended Zoom/Pan/Rotate/Orbit copy; fix any corruption; add regression test for label format if missing.

- [ ] Task 5 — **blocked until Task 4 verified**

---

## Task 6 — Rebase v1 launch QA docs

**Branch:** `docs/v1-launch-qa` onto tip of Task 5.

- Add Round 2 rows to `docs/developer/qa-checklist.md`.
- Rebase any v1 launch QA content; resolve doc conflicts with deadband/keymapping schema docs.

- [ ] Task 6 — **blocked until Task 5 verified**

---

## Stack commands (maintainer)

```bash
# After Task 1 verified on fix/debug-panel-behind-options:
gh stack add fix/debug-preset-dropdown   # Task 2
# … repeat through docs/v1-launch-qa
gh stack submit
```

**Current stack tip:** `feat/debug-deadband-tunables` (includes uncommitted WIP: numeric validation, deadband rename — commit or fold before stacking Round 2).
