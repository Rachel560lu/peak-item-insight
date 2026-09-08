# 0.1.9 — Injury pulse and held preview

## Implemented

- Pure `PreviewTargetSelection` chooses a hovered world identity before a held instance; it does not inspect effect availability. `PreviewTargetGate` keys on both source and instance.
- `HeldResolver` reads only the local character's `data.currentItem`; no backpack enumeration, simulated input or item-use calls. It suppresses the held target during `consuming`, `isUsingPrimary`, `isUsingSecondary`, or positive `castProgress`; an unrelated hovered target remains eligible.
- Runtime selects one target before prediction, passes the same preview to panel/HUD, clears on target changes, and invalidates context on scene/character/GUI changes. `[准星]` / `[手持]` identifies the source. State hashing now includes live cooking/use and special-character state.
- `StatusRecoveryOverlays` owns independent hunger/injury instances of the accepted clipped renderer. Native images are located by status type, with separate diagnostics. No net-capacity or gold rectangles are re-enabled in live gameplay.
- Native colour/size/activity and player/item state are not changed. Other statuses and unsupported effects retain existing textual behaviour; this does not extend the effect provider's coverage.

## Automated evidence

- Build: 0 warnings, 0 errors. 54 offline checks PASS: original math/hover coverage, 13 additional selection/injury cases and a compiled runtime selection-before-prediction check; updated routing/hide checks target the paired renderer.
- Unity smoke PID 56068 started 2026-09-08 20:05:39 +08. START version 0.1.9, FIRST_FRAME, repeated Pretitle/Title heartbeats, `SMOKE_HUNGER_PASS`, `SMOKE_RECOVERY_ROUTING_PASS`, and `SMOKE_UI_PASS` recorded before normal destruction. Launcher exit 0; temporary steam_appid.txt removed.
- Screenshot `session-56068-20260908T120548793.png` visually inspected: synthetic injury orange segment has 25% green coverage, synthetic hunger has 40%, and panel correctly labels the synthetic item as held. Separate old smoke fixtures are not live gameplay output.
- Paired smoke checks use the production selector and renderer with synthetic inputs: hover food vs held bandage, hover tool, fallback, busy/cancel, empty hands, full/partial/zero/harm, simultaneous recovery, missing one binding, source label, hide/dispose/rebind, and original UI preservation.
- These tests do NOT drive a real player's hand or verify the native injury sprite hierarchy in a level. The resolver compiles against the installed game APIs, but real use transitions need manual acceptance.
- Final build/dist/test-profile DLL SHA-256 all match: `3191AF5279D0CB0A74A85B6CC3EDACF16809D58145CAE3D94B36DBEDBF052E81`; tested runtime MVID `d2105279-c82f-441f-827c-f9e80afde2ed`.
- Previous test DLL and current save backed up in `backups/pre-019-20260908-200539`. Save hash before/after automatic self-test matches `306D8A3C08C8D1E6B7939E2229FE7B68E5D8C6FBE542D0CF2E004BDE88D5518D`. Only the isolated profile was deployed; daily profile untouched. Evidence copied to `backups/recovery-held-019-evidence`.

## Real-world acceptance still pending

Use the isolated launcher with `-WorldTest`; require the final PID's 0.1.9 START and ongoing heartbeat (direct Steam/r2modman launch remains an unresolved separate route).

1. Existing injury + hover suitable treatment: orange/green pulse only on the recoverable injury area. Partial recovery uses the predicted ratio.
2. Pick up the item and look at the floor: panel says Held and recovery preview persists. Do not consume just to prove hand detection.
3. While holding treatment, hover food/tool: only the hovered item's effects appear. Look away to restore held preview; empty hands/no target must hide all.
4. Recheck hunger food, pause/disable, slot switching and scene re-entry. If using an item normally, check use suppression, cancellation, repeated uses and exhaustion without stale recovery.
5. Actual treatment results and unknown/conditional effects are separate accuracy checks, not established by UI smoke results.

On failure inspect `TARGET`, `PREVIEW source=...`, `HUNGER_PULSE`, `INJURY_PULSE`, and `*_PULSE_UNBOUND` in the new PID's trace. Do not reuse earlier-version evidence.
