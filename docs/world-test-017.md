# Real-item validation, 0.1.7

This session tests the actual crosshair -> item -> snapshot -> native HUD chain. Synthetic UI results from 0.1.6 do not count.

## Preparation

- Saved `quicksave.peak` to `backups/world-test-017`; initial SHA-256: `6E2B06D03D94552531E4E1B5D3C57637A939CCB941269E94F4C23481FE4EA86F`.
- Built 0.1.7 against installed PEAK 2.4.b: 0 warnings/errors; 28 offline checks passed.
- Started via `scripts/start-isolated-test.ps1 -WorldTest`; PID 49908.
- Agent entered offline Airport through the visible UI, not Continue Adventure.
- Computer-use API supports short key presses but has no held movement or relative-look operation; user assistance requested only to move and aim at a real food. Agent reads evidence and leaves viewpoint alone while user operates.

## Evidence captured

- `HOVER`: actual interaction type, world ID, resolved item ID.
- `WORLD_PREDICTION`: item, frame, current total status, native max capacity, each predicted status and warnings.
- `WORLD_ACTION`: real prefab/instance components, active flags and action triggers/amounts.
- `WORLD_HUD`: native full width, pixel offset, actual native endpoint, predicted endpoint, rendered segment width and visibility.
- `WORLD_OBSERVED`: significant status changes and held/consuming transitions, sampled passively. These are observations, not automatic causal proof of consumption.
- `HIDE`: preview cleared on leaving a target.

Use `scripts/read-world-test.ps1` to inspect only the current game PID's evidence.

## Corrected HUD rule

Installed `Character.GetMaxStamina()` computes `max(0, 1-statusSum)`. Installed `StaminaBar.Update()` targets `max(0, maxStamina * fullBar.sizeDelta.x + staminaBarOffset)` and lerps the actual native width toward it. 0.1.7 applies this same pixel offset, omitted in the previous implementation. Compare stationary native width after the animation settles; do not treat transient lerp differences as arithmetic failure.

## Acceptance

1. Food hover resolves to the expected real food and active actions.
2. HUD sample corresponds to that same item and current snapshot; actual visible segment matches expected width/position, or explicit no-change message appears at zero Hunger.
3. Looking away emits HIDE and leaves no overlay.
4. For nonzero effect, pick up and consume once normally in the isolated offline session; compare immediately-before-use and after-use snapshots with the same cooking/use state. Account for natural drift and delayed effects. Never call RunAction or mutate character statuses from the preview code to manufacture success.

## Real-world results so far

- PID 49908 resolved real `Item` instances in the beach suitcase: Energy Drink (27), Sports Drink (71), and RopeSpool (65). This is no longer just an airport/synthetic test.
- User screenshot `C:/Users/midor/AppData/Local/Temp/codex-clipboard-dba680ea-adc5-4157-8e19-a249e21144fb.png` visibly confirms the Energy Drink panel. Its English title, raw `drink` prompt, and overlap with the native pickup label remain presentation issues.
- Energy Drink prediction at frame 17419: Hot 0 -> 0, Drowsy 0 -> 0. Native full/current/projected widths are all 600 pixels; the main ghost is correctly inactive for these *recognized* immediate effects. This does not validate the drink's unsupported affliction effects or a nonzero ghost.
- Sports Drink prediction at frame 17442: extra stamina 0 -> 0.3, Hot 0 -> 0. Need a sustained hover screenshot to validate its separate extra-stamina lane. The current lane is placed below the main bar and may fall below the screen; this is a code-based risk, not yet visually confirmed.
- At latest inspection: 6 predictions, 93 HUD samples, 6 hide events, 0 diagnostic ERROR entries. No consumption/state-change sample yet. Counts alone are not an end-to-end pass.
- `drawnWidth=100 visible=False` is a stale inactive RectTransform width, not evidence of a visible 100-pixel ghost. Diagnostics should report visible width separately.
- Cosmetic `Action_PlayAnimation` currently triggers the partial-effect warning. Energy Drink also has a genuinely unsupported active `Action_ApplyAffliction`; do not remove its warning merely by whitelisting animation.

Current status: real-item detection and visible information panel confirmed; nonzero native HUD geometry and prediction-versus-consumption agreement remain pending. Do not mark complete before those observations are present.

## Later launch regression: no panel on Trail Mix

- User screenshot `C:/Users/midor/AppData/Local/Temp/codex-clipboard-14774139-6a20-4009-9861-0aa57c696115.png` shows native Trail Mix pickup text but neither custom panel nor ghost.
- The successful PID 49908 ended at 2026-09-06 18:44:11 +08, with DRIVER_DESTROY and PLUGIN_DESTROY. The screenshot report is associated with a later running PEAK process, PID 56232, started at 22:24:55 +08; not a continuation of the successful session.
- PID 56232 has no session trace. Profile LogOutput.log is still dated 18:41; current Player.log has no BepInEx/PeakItemInsight/Doorstop text. Plugin initialization is not evidenced in this launch, so hover/render validation cannot proceed.
- Read-only process inspection confirms Steam is its parent and the command line includes `--doorstop-enabled true` and the absolute test-profile preloader path. Game-local WINHTTP.dll is loaded. The profile junction and deployed DLL still exist. Therefore do not claim the user omitted modded launch options or the loader DLL is absent.
- Exact reason the loader did not reach our plugin remains undetermined. Do not equate missing initialization evidence with a proven specific Steam-restart or hover-code failure.
- Next gate: after a normal exit, use the previously successful isolated launcher, then require the *new PID's* START, FIRST_FRAME and live HEARTBEAT before asking for any world-item tests. Do not reuse old-session evidence.

## Verified relaunch after normal exit

- On user confirmation of exit, no PEAK process remained. Backed up the current quicksave again under `backups/launch-check-*`; SHA-256 still matches the preparation hash. Did not change the deployed DLL.
- Build and profile DLL SHA-256 both: `E8F6867BD721ABF36BECDAB48FF14066AC02AF60A479C306EEB2BFEA23125065`.
- Launched `scripts/start-isolated-test.ps1 -WorldTest` at 22:34:51 +08, PID 41524. Launcher exec session 43505 waits for normal exit and temporary app-ID cleanup.
- New PID trace `session-41524-20260906T143500130.log` confirms START version 0.1.7, DRIVER_AWAKE, FIRST_FRAME, then sustained HEARTBEAT runtime=True in Pretitle and Title through 22:35:33 +08. No ERROR or DESTROY marker at inspection. BepInEx reports successful chainloader startup and persistent driver tick.
- Startup gate PASS for this PID only. This is not a claim that the Steam/r2modman startup failure has been permanently fixed, nor that nonzero world-item HUD preview has passed.
