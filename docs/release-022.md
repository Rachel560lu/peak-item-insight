# 0.2.2 publication preparation

Date: 2026-09-11. Status: published. Thunderstore returned “Success! The package is listed in 1 community: PEAK”.

Listing: https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/

- Publisher: Rachel560lu; version: 0.2.2; categories: Mods, Items, Quality Of Life, Client Side.
- Archive: `dist/package-843dcdca3bd544a1afb993357b3f7aa3/PeakItemInsight-0.2.2.zip`.
- ZIP SHA256: `C81B10B9CC03AD64938713DB7F101EB5B8C9C0ADAB59A39A25B3CD5182C29402`.
- Six allowlisted archive entries, matching DLL, UTF-8 documents, 256x256 PNG and MIT LICENSE passed local validation. Four deliberately invalid ZIPs were correctly rejected.
- Official Markdown preview rendered both languages, headings and configuration table. Published package README deliberately has no broken private GIF URLs.

- User explicitly authorized publishing the latest version and selected MIT licensing.
- User reported in-game acceptance and supplied poisonous-food, safe-food and item-card demonstrations. This is qualitative acceptance, not all-item/post-consumption/multiplayer certification.
- `scripts/verify.ps1`: Release build 0 warnings/errors, 81 passed, 0 failed.
- Rebuilt `bin/Release/netstandard2.1/PeakItemInsight.dll` and installed test-profile DLL both match the prior tested SHA256 `AE223C26B485397450E33CF510329CD2CC03E739D30694C9AE2E76028F907F01`.
- Prior loaded-asset and synthetic evidence remains in `docs/poison-preview-022.md`; earlier timestamps and limitations are not rewritten as new tests.
- Old `dist/PeakItemInsight.dll` was 0.1.9 (SHA256 `3191AF5279D0CB0A74A85B6CC3EDACF16809D58145CAE3D94B36DBEDBF052E81`). Preserve it in a unique local backup before replacing the packaging input with the tested 0.2.2 DLL.
- Player-facing manifest, README and changelog updated to 0.2.2. Source repository and distributed plugin use MIT; this does not automatically make the private repository public or license game assets.

## Still open

- Complete: authenticated existing Team Rachel560lu; server confirmed successful upload and listing.
- GIFs remain in the repository README. No publicly accessible image host is configured, so the upload README does not use broken private-repository URLs. Public demo hosting requires a destination choice.
- Fresh-profile standard mod-manager startup remains unverified; retain this limitation in the public candidate rather than inventing a passing result.

No GitHub visibility change, Git push, game launch or gameplay mutation is part of these packaging checks.
