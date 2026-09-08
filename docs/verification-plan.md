# Verification and release gates

The user's target is crosshair previews for stamina-affecting items and concise usage/effect cards for other items. A successful build or a loader message is not acceptance.

## Evidence audit (2026-09-06)

- 0.1.3–0.1.5 logged startup but no first frame. This does NOT prove Interaction.LateUpdate is unused. Withdraw that earlier claim.
- The running 0.1.5 game PID 49184 started at 17:51:36, parent Steam PID 38352; the BepInEx log last changed at 17:51:34. Follow-up reproduced the cause in PID 39244: plugin START and DRIVER_AWAKE, then DRIVER_DESTROY/PLUGIN_DESTROY without a frame. Player.log explicitly reports RestartAppIfNecessary returned true and shutdown for Steam relaunch. Steam then created PID 42096. Logs from the original process did not describe the replacement game.
- A temporary development steam_appid.txt, per Steamworks documentation, prevented that replacement. PID 54848 produced FIRST_FRAME and repeated heartbeats through Pretitle and Title; subsequent PID 5780 passed production-renderer geometry probes. Steam launch's separate failure to inject is not fully explained; the tested development launcher is the reproducible path.
- Real Unity logs live at LocalLow/LandCrab/PEAK, not LocalLow/Landfall Games/PEAK. Previous exception checks missed them.
- DLL hashes verify deployment only, not which process loaded them.
- Reference uses its own persistent Update tracker, chiefly for HELD items. Its existence does not prove our crosshair path works.

## Gate A: offline checks (agent)

1. Compile against installed game assemblies; inspect required API signatures with Cecil, without running game code.
2. Test production pure math: hunger recovery, poison increase, clamping, fully healthy no-change, mixed effects cancelling, overfull status totals, invalid inputs. Never sum per-frame held/cancel actions into an exact instant result.
3. Test production hover timing: enter/delay/show/leave, A→B immediately clears A, same prefab at different world identities, disabled/re-enabled, null/destroyed target, retry after render failure.
4. Test session log validation: require current PID, version, first frame and recent heartbeat in the SAME session; reject old logs/startup-only records, relaunch PID mismatch, exception-only sessions.
5. Inspect preview code for gameplay mutation calls; do not invoke Use/RunAction/RPC/SetStatus or instantiate gameplay prefabs to predict results.

## Gate B: engine smoke test (agent, title screen only)

Use the actual Steam executable with explicit absolute Doorstop target. Record final PID, process start, managed assembly path/version, scene and frame count in a separate flushed per-PID diagnostic file. Require at least two heartbeats in that PID, no errors, and successful synthetic UI show/hide with no gameplay objects created or saves entered. A startup log or installed-driver log alone fails.

If a game is already running, request normal window closure and verify exit before replacing its DLL. Do not force terminate it. Do not ask the user to enter another round merely to debug loading.

## Gate C: complete feature coverage (agent + engine)

| Requirement | Acceptance |
| --- | --- |
| Food with hunger | Correct current/projected status, beneficial ghost, no player mutation |
| Full state | Explicit no-change message; no silent empty result |
| Poison + nutrition | Both per-status changes visible even when net stamina change is zero |
| Extra stamina / timed / random | Dedicated supported preview or explicit partial/unknown warning; never present unknown as safe |
| FakeItem and Item | World identity preserved; active child paths respected; instance data not fabricated from a prefab |
| Tools | Localized name, available controls, known usage; unknown effects identified |
| Durability | OptionableIntItemData.Value is remaining uses; -1/HasData=false is not zero |
| Lifecycle | Leave, A→B, scene/HUD recreation, pause, spectating and disable clear correctly; no orphan overlay |
| Presentation | Font covers Chinese, canvas visible, no click interception, fits screen |

Full feature coverage is not yet demonstrated; unsupported complex item actions are release blockers for an all-items accuracy claim, even if an MVP can label them explicitly.

## Gate D: final user visual acceptance

Only after A+B pass and C gaps are resolved or explicitly agreed: one compact run for food, floor, tool, floor; one harmful/mixed effect example and one no-change case. Record screenshots plus session events. Numerical/engine tests cannot prove the final on-screen appearance and must not be described as doing so.

## Reporting

Record each gate as PASS / FAIL / NOT RUN with evidence paths. Never infer PASS from silence. Keep existing saves and daily profiles untouched; backups do not isolate save writes. Do not claim all objectives complete while a gate remains untested.
