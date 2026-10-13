# 0.2.14 release evidence

Date: 2026-10-13 (Asia/Singapore). User authorized merging PR #6 and publishing a Thunderstore update.

PR #6 changes one heartbeat check in `src/Detection/RuntimeDriver.cs`: it checks for available character data instead of reading `CharacterData.userID`, which PEAK 2.6.b changed from `ulong` to `string`. The published 0.2.13 source cannot compile against the installed 2.6.b assembly (`CS0019`). The PR change compiles against that assembly.

The exact PR change was tested in a normal Steam PEAK 2.6.b session. The plugin loaded, the game reached Airport and Level_2, and its current-process diagnostic log recorded `characterDataReady=True`, `Hover` and `Held` `PREVIEW` events, and no plugin error. The actual preview layout was not independently captured in a screenshot. The temporary test DLL was removed after the game exited. Local acceptance details remain in the separate PR #6 verification report and were not packaged.

Release 0.2.14 changes only the plugin version string in code beyond PR #6. It retains the 0.2.13 UI, effect calculations, settings and dependency. The Release build against PEAK 2.6.b completed with zero warnings and zero errors; 138 offline verifier checks passed. Build DLL SHA256: `3AAD07A7F760D917FE440D80CBF36AFD56117FB23E3A968057CE9A554461AA8A`. The release build uses `IncludeSourceRevisionInInformationalVersion=false` and `EnableSourceLink=false` to keep the binary reproducible when release evidence is committed after compilation; a clean rebuild with these flags produced the same hash.

The package allowlist contains the manifest, README, changelog, icon, MIT license and plugin DLL. It excludes game assemblies, logs, saves and local configuration. Archive validation passed for `PeakItemInsight-0.2.14.zip` with SHA256 `6AFEB3D6E08F525D88D908D7FE8E4977F00E72A5638D9612284E3FBDBD24E131`. GitHub Release `v0.2.14` is published with the same SHA256 asset: https://github.com/Rachel560lu/peak-item-insight/releases/tag/v0.2.14. Thunderstore upload result and remote archive hash remain pending.
