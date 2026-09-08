# Hunger recovery pulse — 0.1.8

## Scope

Crosshair detection and effect prediction are retained. Live HUD drawing now handles hunger reductions only. The recoverable fraction `(before-after)/before` is clamped to 0..1 and drawn inside the actual Hunger BarAffliction fill, from the side toward the healthy fill. No second HUD bar or old net-capacity/gold rectangle is shown. Other recognized effects and extra stamina remain in the panel. A mixed poison-producing food is not safe merely because hunger pulses green.

A 1.4-second cosine opacity cycle starts on original yellow, reaches healthy green, and returns. The native shape clips our own fill; partial coverage has a second rectangular clip. Both are new UI children, ignored by layout. Original image/colour/size/activity, player statuses and item use logic are not written. Native minimum badge widths are respected by proportional coverage of the actual displayed rect. Native HUD hiding automatically hides the child; hover exit calls Hide; destruction/rebinding removes old child UI.

## Evidence

- Release build: zero warnings/errors; 40 automated checks passed. `scripts/verify.ps1` covers production math, hover gate, installed game fields, compiled runtime routing/hide path, and forbidden gameplay mutation calls. It is not proof of all game behaviours.
- Final renderer Unity smoke: PID 57820, 2026-09-07 09:34:58 +08. New session START identifies version 0.1.8; FIRST_FRAME and Pretitle/Title heartbeats are present.
- `SMOKE_HUNGER_PASS`: full/partial/zero coverage, yellow/green/yellow opacity, healthy-side mirroring, moved/scaled native bounds, hide, inactive source, inactive healthy reference, dispose/rebind and native colour/size preservation.
- `SMOKE_UI_PASS` followed by normal shutdown; launcher exit 0 and temporary steam_appid removal confirmed. Tests only entered title/loading UI and created synthetic UI; no level or save was loaded by the test.
- Screenshot `session-57820-20260907T013502832.png` visually inspected: synthetic yellow badge shows the expected green left 40% and unchanged yellow right 60%, with no green outside the badge. This tests stencil/rect clipping but not the game's real hunger sprite hierarchy.
- Earlier smoke PID 51228 also passed assertions but skipped its screenshot due a slow frame; screenshot timing was fixed and the final build rerun as PID 57820.
- Build, `dist/PeakItemInsight.dll`, and the test profile's only plugin DLL all have SHA-256 `D31F287A7BA12A038CBD3BC6479EA6A665A47AFC519990B68269BFBB0C85EE41`. Final runtime MVID: `1fb7bc1c-e496-4497-8c4b-913aed326182`.
- Save SHA-256 before/after these self-tests matches: `E9873B7F75DBE2DA04B5A9B1A7DA0DE7DB487DB515B2BB7950A7204E0A7335A5`. Prior DLL and current save backed up in `backups/pre-hunger-018-20260907-093304`; logs/screenshot copied into `backups/hunger-018-evidence`. Only the isolated test plugin was replaced; daily profile unchanged.

## Remaining real-world acceptance (not claimed passed)

1. Start using the isolated launcher with `-WorldTest`; require the new PID's START version 0.1.8, FIRST_FRAME and live heartbeat. Steam/r2modman direct startup remains a separately unresolved route.
2. With visible hunger, hover food with sufficient recovery: the entire native yellow hunger segment should cycle yellow/green in place; injury must not change.
3. With recovery smaller than hunger, only the proportional slice toward healthy stamina cycles; the rest stays yellow. No hunger/no improvement must not flash.
4. Look away, change targets, pause or disable: no lingering green overlay. Observe through at least one full 1.4-second cycle rather than judging a screenshot taken during the yellow phase.
5. Check `HUNGER_PULSE` for before/after, fraction, native/overlay widths, visibility and alpha. `HUNGER_PULSE_UNBOUND` means native image binding failed; do not report success. A synthetic renderer test cannot establish real-item numerical accuracy or all resolutions.

This iteration is ready for a focused in-game acceptance check after artifact hashes match. It is not a fully validated release of every original item-insight goal.
