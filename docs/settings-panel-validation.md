# Preview settings panel — local implementation

Uses Karpathy guidelines: change only source routing and add a separate native-window panel; no edits to preview typography, anchors, effect projection or stamina rendering.

## Behavior

- Text above inventory / 物品栏上方文字 and Stamina bar preview / 精力条闪烁预览 independently select Hover, Held, Both or Off. Defaults: Both.
- Both prefers hover, otherwise actual held item. Held ignores hovered items; holstered inventory selection is not a held item.
- First ready Airport visit opens the panel once per configuration. Title screen never shows the panel. Pause menu retains a top-right entry.
- Config entries InventoryTextSource / StaminaPreviewSource save through BepInEx; AirportSetupSeen records onboarding. No new dependencies.
- Native MenuWindow owns input and cursor lifecycle. Independent canvas uses the original font/fallback chain without changing shared assets.

## Validation

- Build: zero warnings/errors. 117 offline checks passed, including all 16 source combinations, missing targets and busy-item suppression.
- Isolated native smoke PID 35444: SMOKE_SETTINGS_PASS. Both languages have exact glyphs; window registration/open/close cleanup and unchanged shared font material verified.
- Existing mixed hunger/poison, bonus stamina geometry and native minimal UI smoke regressions included.
- Manual Airport/pause interactions, controller navigation, and visual comparison still require player acceptance. No public release made.

## Player acceptance

### Pause-entry click fix (2026-09-13)

- Entry now belongs to the native pause main page. Its raycaster sorts above native pause canvases, and the settings panel sorts above the entry. Native canvas sorting values are read, not changed.
- Detached entry is explicitly destroyed on cleanup. Other preview rendering code is untouched by this fix.
- Final smoke PID 55524 exited 0: `SMOKE_SETTINGS_CLICK_PASS` verifies top-hit raycasting and pointer-event dispatch against a synthetic pause blocker at sorting order 30000, then clicks Done and checks the pause object remains active.
- Cross-frame checks wait for rendered graphics before raycasting. Original synchronous probe returned no hit before graphics had rendered; it was corrected and rerun.
- Build zero warnings/errors; 117 offline checks passed. Real Airport pause interaction still needs manual acceptance; synthetic click coverage is not a claim of native in-level acceptance.

1. Title: no settings entry. Enter Airport: readable two-row panel; choices save; Done releases cursor and movement with no accidental item use.
2. Pause: top-right settings entry; open and close with Done or Escape. Resume normally.
3. Text Hover + bar Both: holster item, hover another; correct empty-slot text anchor. Held text is absent; held bar still previews on looking away.
4. Text Held + bar Hover: hover another while holding food; text stays with held item and bar previews hovered target.
5. Test Off independently; restart confirms persistence and no repeat onboarding.
6. Confirm prior fonts/icons/anchors and bonus/mixed status pulses remain unchanged. Leave scene with panel open; no stuck cursor or input block.
