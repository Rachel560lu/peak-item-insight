# PEAK Item Insight

A client-only BepInEx mod for PEAK. Aim at an item to preview direct status changes or read concise usage and resource information.

## MVP

- **0.1.9 HUD:** hunger and injury recovery pulse the recoverable part of their native badges toward healthy green (1.4-second cycle). Full/partial/zero recovery is handled independently; poison and extra stamina remain in the information panel.
- **Preview source:** hovered item first, otherwise the currently held item. The panel labels Hover/Held. Looking away while holding an item now intentionally shows that item; empty hands plus no hovered item hides everything. Held previews pause during item use and recalculate afterward.

- Hovered items are detected without picking them up.
- Consumables with direct `Action_ModifyStatus` actions show before/after status bars.
- Items without direct status changes show their in-game prompts and known usage notes.
- Remaining uses and cooking state are shown when exposed by the item instance.
- The mod never calls `Use`, mutates player state, or sends network events.

## Build

The project defaults to the local PEAK install at:

`D:\SteamLibrary\steamapps\common\PEAK\PEAK_Data\Managed`

Override it when needed:

```powershell
dotnet build -c Release -p:PeakManagedDir="X:\path\to\PEAK_Data\Managed"
```

Copy `bin/Release/netstandard2.1/PeakItemInsight.dll` into a BepInEx profile's `BepInEx/plugins/PeakItemInsight/` directory.

## Controls and configuration

Configuration is generated at `BepInEx/config/dev.rachel.peakiteminsight.cfg`.

- `Enabled`: master switch
- `HoverDelaySeconds`: delay before opening a preview
- `PanelScale`: UI scale
- `OffsetX`, `OffsetY`: position relative to screen center
- `ShowDebugIds`: show the stable numeric item ID

## Accuracy policy

The diagnostic build previews recognized immediate primary-use status actions, hunger restoration and extra stamina. Unknown, random, timed, special-character and conditional effects are incomplete and explicitly labelled. This build must not be advertised as an all-items safety guide.

## Verification

Run `scripts/verify.ps1` to build against the installed game, run pure production-logic tests and inspect API/mutation contracts. See [verification-plan](docs/verification-plan.md) and [verification-results](docs/verification-results.md) for evidence and remaining release gates.

`scripts/start-isolated-test.ps1 -SmokeTest` is a local test launcher, not a general installer. It requires the existing test-profile junction and a closed game. It adds a temporary `steam_appid.txt` using Steam's documented development workflow, runs UI probes at the title screen, waits for normal exit, then removes that temporary file. Do not interrupt the launcher while the game is running. If the launcher is forcibly terminated, check for and remove only its unchanged `steam_appid.txt` after closing the game.

Startup-only BepInEx logs are insufficient. Per-process evidence is written under the test profile's `BepInEx/InsightDiagnostics`; require the final game's PID, matching version/MVID, first frame, repeated heartbeat and smoke results in one session. UI smoke screenshots use synthetic data and do not establish real-item coverage.

For the hunger renderer self-test with normal automatic exit, run `scripts/start-isolated-test.ps1 -SmokeTest -ExitAfterSmoke`. Require `SMOKE_HUNGER_PASS` and `SMOKE_UI_PASS` before the normal destruction markers. Evidence and remaining acceptance checks: [hunger pulse 0.1.8](docs/hunger-pulse-018.md).

For 0.1.9 also require `SMOKE_RECOVERY_ROUTING_PASS`. See [0.1.9 verification](docs/recovery-held-019.md) for tests and outstanding real-world acceptance.

## Release preparation

Player-facing package documentation lives in `thunderstore/`, separately from these development notes. Run `scripts/package.ps1` to assemble and validate a local candidate from the evidence-pinned DLL. This does not build, install, launch, or upload anything. See [release checklist](docs/release-checklist.md) for unpassed public-release gates.
