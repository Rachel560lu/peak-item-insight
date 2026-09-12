# Native bonus stamina and mixed food preview

Development changes, 2026-09-12. Promoted into the [0.2.6 release](release-026.md); see that record for final package and publication evidence.

## Changes

- `ExtraStaminaOverlay` rebuilds the original `extraBar` visual tree at its original parent/anchors. It uses the original outline, lightning icon, textured fill, masks and Image subclasses, including ProceduralImage's UniformModifier. It does not instantiate game/HUD controllers or bundle extracted game assets.
- The independent wrapper uses the game's settled 45x45 dimensions. The frame and icon stay steady; existing bonus stays steady and only the predicted gain pulses. Native Graphic.enabled flags are saved/restored while replacement visuals are active, preventing duplicate outlines. Native geometry, layout scripts, tweens and player data remain untouched.
- Placement follows the real `ExtraBefore`: zero places the entire preview above the main outline; positive values extend the lower bonus row. Bounds include the outline, lightning and gain fill, with a gap of 20% of the main outline height. A lower native anchor that overlaps the main row is offset downward on the preview copy. Every frame starts from native anchors, preventing accumulated offsets. Returning to zero restores suppressed native graphics immediately.
- `StatusProjector` captures unchanged numeric statuses alongside effect endpoints. Decorative badge widths never enter the capacity sum; the separately displayed petrification amount is excluded from the main-row sum.
- `StatusGhostLayout.BuildProjected` handles reductions as well as additions. Zero hunger has no yellow badge in the projected phase. Partial hunger restoration retains only the predicted after width, with native visibility threshold/minimum width/stretch padding.
- The mixed renderer includes a green capacity fill and uses one phase for the complete projected row. Separate recovery overlays are disabled for this route; recovery-only items retain the accepted existing renderer.
- Timed poison retains the existing item-only cumulative estimate, including its delay/duration text. It is not an immediate-effect claim and does not include natural recovery, other buffs or environmental exposure.

## Original asset audit (read-only)

Installed PEAK's `PEAK_Data/level3` contains StaminaBar controller path ID 18022:

| Reference | RectTransform path ID | Serialized layout |
| --- | --- | --- |
| fullBar | 16339 | 600x100 reference container |
| extraBar | 16485 | 45x45; left/top anchors; y=-177.5 |
| extraBarStamina (Back) | 15935 | x=46; vertical stretch; height sizeDelta=-12, visible height=33 |
| extraBarOutline | 15902 | x=40; vertical stretch; white Image outline |

The icon is an Image, component path ID 19497. Back contains a Mask and a nested textured Fill. Outline's shadow and stamina glow have ProceduralImage + UniformModifier components. Copying only a flat Image or only its sprite is insufficient. ContentSizeFitter and gameplay scripts are deliberately not copied.

Current DLL's `BarAffliction.ChangeAffliction` activates badges only above 1%, sizes them from `fullBar.sizeDelta.x * value`, then applies `minAfflictionWidth`. `Character.GetMaxStamina` is `max(0, 1 - statusSum)`; extra stamina is a separate row.

## Verification evidence

- Release build: zero errors, zero warnings.
- Offline verification: **88 passed, 0 failed**, including 10,000 randomized mixed layouts, zero/partial hunger, existing poison, unchanged injury, equal net changes, native badge threshold/minimum/padding and no gameplay mutation calls.
- First hidden title smoke, PID 38664, failed because Title does not load the original in-level HUD. This was a test fixture limitation, not recorded as successful original-asset rendering.
- Second isolated smoke, PID **50972**, returned **0**, auto-exited and removed its temporary steam_appid.txt. Passed `SMOKE_MIXED_HUD_PASS`, `SMOKE_NATIVE_BONUS_LAYOUT_PASS`, `SMOKE_BONUS_GEOMETRY_PASS`, existing food/poison asset checks and cleanup.
- Mixed renderer tests assert actual positioned hunger/poison widths and green recovery coverage at peak, plus native visibility at original phase, for hover and held sources.
- Original-layout test uses the audited 45px wrapper and -12px vertically stretched fill (33px visible height), verifies UniformModifier serialization, scaled parent/nonzero pivot, cap handling and restored flags. It does not claim original sprite visual acceptance.
- `SMOKE_NATIVE_BONUS_ASSETS_PENDING` remains explicit: Title cannot load original in-level sprites/HUD for this test. Hidden screenshot is black; **not visual acceptance**.
- Placement follow-up smoke, PID **56660**, returned **0** and cleaned up. Actual synthetic rendered bounds passed zero-above, positive-below, overlap clearance, depletion-to-zero, repeated-frame stability, hover/held and native visibility restoration. Offline verification remains **88 passed, 0 failed**.
- Current deployed test DLL SHA256: `177A5AD09DCA8BD17A3474B5FE754DAC35075FDE2D6BA9DE631ECB7D07237765`. Previous DLL retained in the repo's ignored `backups/bonus-placement-*` directory.

## Remaining in-level acceptance

Use PeakItemInsight-Test, not the daily profile:

1. With zero extra stamina, hover and hold a Granola Bar: original lightning icon and white outline above the main row, with gained green fill pulsing and a clear gap. Move away/switch items: no stale copy.
2. With five hunger, preview a poisonous Green Crispberry: original phase has yellow; projected phase has no yellow and has purple poison. Repeat with more hunger to check partial restoration.
3. Repeat with existing bonus stamina: preview switches below the main row; existing amount stays steady, new amount pulses. Eat the item: original row takes over without duplicate icons or double count. Exhaust extra stamina and preview again: the preview returns above.
4. Confirm original native UI after cancel, consumption, scene change and mod disable; include a petrification state because it shares the extra-row container.

Do not treat the cumulative poison endpoint as the toxin amount immediately after eating. Compare the model with the appropriate timed endpoint, or explicitly allow natural decay when comparing gameplay observations.
