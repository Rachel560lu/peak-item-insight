# Release 0.2.5

Status: published to GitHub and Thunderstore with user authorization on 2026-09-12.

## Runtime changes and evidence

- Runtime and package version: **0.2.5**, code and documentation update.
- Tested DLL SHA256: `F963AE469375E72CD88665931E650642E0F3A55A5DA43904CA184CB47593C5CC`.
- See [UI fix evidence](ui-fixes-025.md) for bonus-stamina geometry, localized descriptions, real Piton/Remedy Fungus components and Unity assertions.
- Latest recorded engine run: PID 38980, `SMOKE_BEGINNER_TOOLS_PASS`, `ASSET_EFFECT_SUMMARY foods=74 loadedItems=194 consumedPoisonPassed=6 descriptions=4`, `SMOKE_UI_PASS`, exit 0.
- 81 offline checks passed. Hidden smoke screenshots are black and are not visual acceptance evidence.
- User-provided gameplay recordings demonstrate the interface; they are not hash-pinned proof of all-item or multiplayer compatibility.
- Promote the tested build output to the package staging DLL explicitly: the build script does not update `dist`.

## Demonstration refresh

Four new recordings are trimmed at original speed, with the complete item card and native HUD retained. No invented overlays or gameplay edits; GIFs contain no audio. Original recordings remain local and unchanged. PDF was a typo; no PDF is produced.

| Display order | GIF | Source start | Duration |
| --- | --- | --- | --- |
| 1 | energy-drink.gif | 3.8 s | 6.0 s |
| 2 | poisonous-mushroom.gif | 0 s | 2.8 s |
| 3 | poisonous-food.gif (new berry recording) | 0.4 s | 5.3 s |
| 4 | bandage.gif | 0 s | 3.3 s |
| 5 | non-poisonous-food.gif | Existing asset preserved | Unchanged |
| 6 | item-description.gif | Existing static card preserved | Unchanged |

Encoding: 960 px wide; energy drink uses 8 fps / 96 colors, poisonous berry 10 fps / 96 colors, mushroom and bandage 10 fps / 128 colors. Menus are excluded. GIF timing is quantized to centiseconds. Both READMEs use the same demo order and bilingual captions. Thunderstore links will be pinned to the public GitHub asset commit.

## Packaging and publication

The archive contains only `manifest.json`, `README.md`, `CHANGELOG.md`, `icon.png`, `LICENSE` and `BepInEx/plugins/PeakItemInsight/PeakItemInsight.dll`. No videos, saves, logs, SDKs or game assemblies are distributed. The existing cover and MIT license are retained.

- Re-ran the production build and all 81 offline checks before packaging; zero warnings/errors, DLL still matches the tested hash above.
- Validated ZIP: `PeakItemInsight-0.2.5.zip`, SHA256 `14E26331AEFE41FD10AD7C11D2740C5F67DC19C5B1CE980122EA5CF935EACB6A`.
- Package validator accepted the candidate and rejected five malformed variants.
- Both README demo sequences passed an exact six-file order check.
- All six fixed-commit raw GIF URLs returned HTTP 200 / `image/gif` without authentication. Asset commit: `59c4d3576ab85633872b9402eea37ed00a74d0ae`.
- Four refreshed GIFs contain 48 / 28 / 53 / 33 frames respectively, at 6.01 / 2.80 / 5.30 / 3.30 seconds. Final contact sheets confirm menus are excluded and card/HUD stay visible. Existing safe-food and tool-card assets are unchanged.

Thunderstore upload returned **Success** for the PEAK community. The public listing shows `Rachel560lu-PeakItemInsight-0.2.5` and 0.2.5 manager/manual download links. All six demonstration images were confirmed loaded on the public legacy listing, in the requested order, with their expected dimensions.

Published version: <https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/v/0.2.5/>. The new-site homepage briefly retained the previous package in its cache immediately after upload; the legacy listing already served the new version and updated README.

- Downloaded the public 0.2.5 archive again without installing it: archive SHA256 matches the candidate byte-for-byte; extracted DLL hash and package validation pass.
- The new-site Versions page confirms 0.2.5 as the newest release, with matching manager/manual download links.
