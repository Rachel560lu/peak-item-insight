# Native minimal item preview — 2026-09-12

Branch: `fix/native-minimal-item-preview`, based on published 0.2.7.
The settings-panel work remains stashed; it is not included here.

## Scope and design

Followed the user-specified [Karpathy guidelines](https://github.com/multica-ai/andrej-karpathy-skills/blob/main/skills/karpathy-guidelines/SKILL.md): reproduce the defect, limit changes to presentation, and use verifiable checks.

- Reuse `FontFallbackSwapper.instance.mainBaseFont` (`DarumaDropOne-Regular SDF`) and the game's Chinese fallback chain. Do not change shared font fallback tables or globally search arbitrary UI fonts. Do not display default-font frames while native resources are unavailable.
- Render increases/decreases with independently drawn clipped-tip triangles, numeric magnitude, and existing native status sprites/colors. Keep signed source facts unchanged; presentation alone removes the redundant sign.
- Increase base text size from 22 to 28. Align status icons in a column, measure content width, keep useful resource/use information, and retain the transparent background.
- Held previews read the same `currentSelectedSlot` used by `InventoryItemUI.SetSelected`; temporary held slots have priority. Hover previews instead read `Player.localPlayer.itemSlots` and choose the first empty ordinary slot (1, 2, 3), falling back to `GUIManager.backpack` when all three are occupied. A holstered item's lingering selection never anchors a different hovered item. Track native inventory animations without moving sideways because of invisible prompt padding.
- Keep existing scale/offset configuration. No new settings panel, no release/version change, no fabricated running/climbing duration calculations.

## Protected code

No changes to `RuntimeController`, `Plugin`, providers, effect projection/math, status layout, or stamina HUD overlay implementations. The new Zorro reference is only needed to read the native selected-slot option.

`git diff --exit-code` against the branch base was clean for those protected paths. Existing hunger, poison, injury, extra-stamina, native HUD routing and hide/restore regression tests remain passing.

## Evidence

- Baseline offline verification: 98 passed.
- The new native-font assertion failed against the old presentation code in isolated PID 60556 (`native main font, not default sans`). This reproduces the default-font regression independently of screenshots.
- Final offline verification: **101 passed, 0 failed**; build: 0 warnings/errors.
- Final isolated Unity probe: **PID 60824**, exit 0, log `session-60824-20260912T110936792.log` in the test profile's `BepInEx/InsightDiagnostics`.
- `SMOKE_MINIMAL_NATIVE_PASS`: native Daruma font, actual generated glyph Unicode matches original requested Chinese/English characters (including 剩余次数/剩余), up/down arrows, 16:9/16:10/21:9 anchor/clamp arithmetic.
- `SMOKE_MINIMAL_PASS`, `SMOKE_ENGLISH_PASS`, `SMOKE_UI_PASS` completed; existing HUD probes completed as part of the same run.
- Final build and isolated DLL SHA256: `D19A69A6A4E6454D5F23441FD7D986AF90B854F1AC53C2D96FAD136363788D1E`.
- Original test DLL backed up under `backups/native-minimal-20260912-185342/`.

## Remaining visual acceptance

Hidden-window screenshots and attempted offscreen captures were blank/background-only on this host. They are **not** visual-pass evidence; the unsuccessful extra rendering helper was removed. The assertions above validate actual fonts/glyphs and layout math, not a completed in-level visual comparison.

Use `scripts/start-isolated-test.ps1 -WorldTest` after confirming PEAK is stopped. Check:

1. Hold Scout Cookies in slots 1, 2 and 3: native rounded numbers, readable Chinese use counts, effects directly above that slot's item name.
2. Hover a different item while holding one, then look away: original target precedence and clearing remain unchanged.
3. Inspect hungry/poisonous food: down hunger triangle and up poison triangle, both native HUD effects unchanged.
4. Extra-stamina food with/without existing extra stamina and healing items: original stamina-bar position, pulse and values unchanged.
5. Long special-item text and narrow/wide screens: no clipping or overlap with visible controls; verify at the user's normal UI scale.

## Follow-up: hover anchor routing

The user accepted the native font/arrows in a visible session, then reported that holstering slot 1 left its highlight selected and caused a hovered bandage to appear above the cookies. Content already tracked `PreviewSource`, but positioning ignored it and always followed selection.

The follow-up changes only anchor selection in `MinimalPreviewPanel` and adds the pure `PreviewAnchor` routing helper. The native `GUIManager.UpdateItems` IL confirms that `items[i]` maps directly to `Player.itemSlots[i]`, with a separate `backpack` reference. `ItemSlot.IsEmpty()` reads the actual prefab state rather than decorative highlight state. Inventory contents, selected slots, font rendering and stamina overlays are not mutated.

- Offline verification: **104 passed, 0 failed**, build 0 warnings/errors. Coverage includes all eight occupancy patterns with different selected slots and temporary-held visibility, holstering, full inventory, dropping an item and hover/held transitions.
- Candidate SHA256: `E8E13E14FED3558B4BA11435045012C83ED13D122946C0FFF3A45DC0F868D003`.
- Previously accepted DLL backed up at `backups/preview-anchor-20260912-192552/`.
- Hidden isolated Unity probe: PID **63132**, exit 0, `session-63132-20260912T112623218.log`. Native font/glyph/arrow checks, minimal UI checks and the existing hunger/poison/extra-stamina HUD probes passed. This is not visual acceptance of the new in-level slot routing.
- In-level acceptance still needed: cookies in slot 1, holster, hover bandage -> slot 2; fill slot 2 -> slot 3; fill all -> backpack position (with and without an equipped backpack); look away and hold cookies -> own slot. Check that picking up or dropping an item updates the anchor immediately.

The candidate is installed only in `PeakItemInsight-Test`. GitHub and Thunderstore have not been updated.
