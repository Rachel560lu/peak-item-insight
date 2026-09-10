# 0.2.3 documentation-only package

Date: 2026-09-11. Status: prepared; not uploaded yet.

The user authorized making the GitHub repository public and adding the gameplay GIFs to Thunderstore. Source, MIT license and demos were pushed in commit `1fec93acc0c3a149054df1003fdd34f40e414f15`.

## Version and evidence

- Thunderstore package version: 0.2.3. Plugin runtime version: 0.2.2.
- No C# or gameplay changes. Preserve the previously tested DLL byte-for-byte: SHA256 `AE223C26B485397450E33CF510329CD2CC03E739D30694C9AE2E76028F907F01`.
- Packaging explicitly separates package and plugin versions only for a documented `documentation_only` change. DLL hash verification remains mandatory; legacy evidence still defaults to matching versions.
- Add three commit-pinned public GIF URLs, source link and issue-tracker link to the player README. The item-card GIF is intentionally static because the supplied original is a screenshot.
- The 81 offline checks and gameplay acceptance belong to the unchanged 0.2.2 plugin; this is not new game testing. Fresh-profile and multiplayer limitations remain.

## Publication checklist

Local package validation passed: `dist/package-533da34961344440a9831b5f9a723977/PeakItemInsight-0.2.3.zip`, ZIP SHA256 `72F3135CD03427A08AAB7865FADE2D2DFE95A53CDE499A9761B80D312C0A697A`. Five invalid archives were rejected, including a private Windows-path case; legitimate HTTPS image URLs are accepted. The validator previously mistook the final `s:/` in `https://` for a drive prefix; the drive-prefix check now excludes preceding alphabetic characters.

GitHub is paused at its visibility-effects confirmation pending user response. Do not publish the Thunderstore package until the commit-pinned images are publicly accessible.

- [x] Commit and push current source, license and demo assets.
- [ ] Confirm public GitHub visibility.
- [ ] Verify all three GIFs load without private credentials and render in Thunderstore Markdown preview.
- [ ] Validate ZIP and upload as Rachel560lu-PeakItemInsight-0.2.3 in the PEAK community.
- [ ] Confirm listing version and all three rendered images.

Prior release: [0.2.2 record](release-022.md).
