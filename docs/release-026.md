# Release 0.2.6

Published on 2026-09-12 with explicit user authorization to publish code and the five supplied gameplay clips to GitHub and Thunderstore.

## Changes and validation

- Native bonus-stamina lightning, outline, textured fill, masks and rounded modifiers; zero real bonus displays above the main bar, existing bonus extends below it. Only the gain pulses.
- Simultaneous hunger reductions and poison additions use a shared projected status row. Full and partial hunger recovery are retained alongside harm.
- See [implementation evidence](native-bonus-mixed-hud-fix.md).
- Runtime/package version 0.2.6. Release DLL SHA256: `8AE11E865FE1D6C0DF8746467F3A0EEAF81B9F276351EDC31BB301117AFD8BA7`.
- Release build: zero errors/warnings; 88 offline checks passed.
- Final isolated engine run PID 37468: mixed HUD, bonus positioning, native-shaped layout, tool cards and loaded-food checks passed; exit 0; temporary launch file cleaned up.
- User recordings demonstrate the actual UI. The engine smoke tests use synthetic/native-shaped geometry and do not independently establish all-item or multiplayer compatibility.

## GIF selection

Original speed, complete game view and HUD, no audio. Paused menus and non-demonstration tails are excluded. Source recordings remain unchanged and local. All five final GIF contact sheets were inspected.

| GIF | Source start | Duration | Frames | Dimensions |
| --- | --- | --- | --- | --- |
| sports-drink.gif | 0 s | 3.2 s | 32 | 960x600 |
| sports-drink-existing-bonus.gif | 0 s | 4.8 s | 48 | 960x600 |
| first-aid-kit.gif | 0 s | 2.2 s | 22 | 960x600 |
| special-item.gif | 0 s | 0.8 s | 8 | 960x600 |
| remedy-fungus.gif | 0 s | 2.8 s | 28 | 960x598 |

The special-item clip loops only the short interval with its description visible. No freeze frames or invented UI were added. Encoding uses 10 fps and 128 colors; each new GIF is under 10 MB.

Both READMEs show: energy drink, sports drink, sports drink with existing bonus, poisonous mushroom, poisonous berry, first aid kit, safe mushroom, special item, Remedy Fungus, item information. The bandage demo is replaced in both pages; its old asset remains available for historical links.

## Publication

Public ZIP contains manifest, bilingual README, changelog, existing cover, MIT license and the tested DLL only. GIFs are hosted in the public GitHub repository and Thunderstore uses fixed-commit URLs.

- GIF asset commit: `c2b4644e7073a084c239e917222b2d3788ef895f`; all five new raw URLs returned HTTP 200 with `image/gif` without authentication.
- Both README sequences verified: ten matching demos, first aid kit replaces bandage.
- Local package validation passed; five malformed-package regression cases were rejected.
- Candidate ZIP SHA256: `82171C96E3EA82E54EC80D11A6CEB24284DF8946C0947AC2444BAD1CF6012C9E`.

- GitHub code/README commit `fd66afc` pushed to main. Public README retrieval confirms the new GIFs and no bandage embed.
- Thunderstore returned **Success!** and listed the package in PEAK under the existing Mods, Items, Quality Of Life and Client Side categories.
- Public listing displays `Rachel560lu-PeakItemInsight-0.2.6` and matching install/manual download links. All ten GIFs were loaded with valid dimensions, in the expected order.
- Re-downloaded the public ZIP without installing it: archive hash matches the candidate byte-for-byte; DLL and archive verification passed.
- Release page: <https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/v/0.2.6/>.
