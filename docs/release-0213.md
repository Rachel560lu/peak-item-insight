# 0.2.13 release evidence

The user authorized GitHub and Thunderstore publication on 2026-09-14.

Changes: optional Minimal / Detailed descriptions; item icon beside the title; available cooking information; background transparency control; readable immediate/rate/duration/side-effect phrases; Turkish and Spanish localization; fewer hidden-settings refreshes and cached layouts; scoped settings-focus cleanup.

The formatter preserves numeric EffectFact data and existing HUD projections. Missing durations, side effects and absent risks are omitted. Opposing signs and distinct trigger times are not merged.

Verification: the final version build and 138 verifier checks passed. The final native UI probe confirmed font glyphs, layout reuse, settings pointer clicks and switch cleanup before packaging. Final hashes and the native probe result are recorded below.

In-game screenshots supplied by the user demonstrate earlier detailed-card rendering and prompted the prose update. They are not exhaustive numerical, all-item or multiplayer verification. Standard fresh-profile startup has not been independently reverified for this build.

A prior native offline session in PEAK 2.4.b failed to parse a GUID-form UserID, preventing Player.localPlayer initialization and causing native reconnect and item-switch exceptions. This release optimizes mod refresh work; it does not claim to repair native networking. Use Create room / Host for a solo retest if the native offline registration failure occurs.

The distributable allowlist contains only the manifest, player README, changelog, original package icon, MIT license and the tested plugin DLL. Game assemblies, logs, machine configuration and test launchers are excluded.

Final verification: version 0.2.13, MVID 8ad18fc2-cf23-4a77-8cc4-73ec235120ec, final native probe PID 11820 completed SMOKE_DETAILED_PASS, SMOKE_SETTINGS_PASS, SMOKE_SETTINGS_CLICK_PASS and SMOKE_UI_PASS. 138 verifier checks passed; zero build warnings/errors. DLL SHA256: 69DB24B27DD7758E9E2EF675F860111B0A3157EF229B7425A717C1FA6294F16C.
Package ZIP SHA256: CE694E650062C67393860CB76DCC810BD36E652F2DFF173906AFE7B646A7098B. Archive validation passed; the valid package was accepted and five intentionally invalid packages were rejected.

GitHub v0.2.13 published: https://github.com/Rachel560lu/peak-item-insight/releases/tag/v0.2.13. The publicly readable release asset digest matches the validated ZIP. Thunderstore upload is pending an available authenticated upload channel; the live Thunderstore package is still 0.2.12.
