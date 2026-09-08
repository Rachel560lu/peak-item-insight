# 0.1.9 release checklist

Status: local package candidate only. Packaging completion does not authorize uploading or establish gameplay compatibility.

## Prepared

- Player-facing English/Chinese README, limitations, installation intentions, configuration, troubleshooting and removal.
- Curated CHANGELOG, original 256×256 PNG icon and manifest with an explicitly experimental description.
- Dependency `BepInEx-BepInExPack_PEAK-5.4.2403` verified on its Thunderstore version page on 2026-09-09. Retained the declared version rather than silently changing the test runtime.
- Allowlisted ZIP assembly and post-archive validation, with a checksum/validation report outside the archive.
- Package uses the evidence-pinned 0.1.9 DLL; no game DLLs, save files, logs, test launcher, SDK or machine-specific configuration are bundled.

## Blocking public release

- [ ] User decides license and author/owner attribution. Do not invent a license grant. No LICENSE is bundled until decided.
- [ ] User chooses publisher Team and feedback/contact URL; website_url intentionally remains empty (allowed by package format).
- [ ] Standard mod-manager installation and Modded startup succeed in a fresh profile without the custom developer launcher or temporary steam_appid.txt. Require current-session plugin version, first frame and ongoing runtime evidence. The prior intermittent loading issue remains unresolved.
- [ ] Real-game injury full/partial/zero recovery, hover/held fallback, hover priority, use-in-progress and post-use refresh pass with screenshots/current-session logs.
- [ ] Real-game hunger regression, target clearing, pause/resume and scene transitions pass with the packaged DLL.
- [ ] For effects advertised as supported, compare actual post-use states with predictions, including multi-use items. Document exceptions before broad coverage claims.
- [ ] Review the final player README, icon, package name and any third-party notices after choosing licensing. Replace pending publisher/license text before publication.
- [ ] Obtain explicit user approval to publish. Select the PEAK community, never upload private development backups or evidence.

Multiplayer and other-mod compatibility are untested. Either complete those tests or retain explicit unsupported/untested wording; do not advertise verified multiplayer compatibility.

## Local packaging

Run `scripts/package.ps1` from PowerShell. It renders the original geometric icon, reads the release material allowlist, checks the DLL against `thunderstore/release-evidence.json`, creates a ZIP under a new unique `dist/package-*` directory and validates the archive after writing. It never starts PEAK, changes an installed profile, or uploads. The evidence pin is a regression guard, not a signature or independent proof of test completion.

Run `scripts/verify-package.ps1 -ZipPath <candidate.zip>` to re-check an existing candidate. A pass is a local format/content check, not Thunderstore server approval or the completion of the gates above. Run `scripts/verify.ps1` separately for code regressions; synthetic UI evidence is recorded in `docs/recovery-held-019.md`.

## Packaging verification — 2026-09-09

- `scripts/verify.ps1`: build succeeded with 0 warnings/errors; 54 checks passed, 0 failed. Rebuilt DLL hash equals the pinned/tested DLL.
- Candidate: `dist/package-49ac2657cd8d41c2ab2ddd19694613e4/PeakItemInsight-0.1.9.zip`.
- ZIP SHA256: `7D71872B3F6D4D165EA0CAEF6C80B349DA2E6D65A7F5A5749B85A5466AC8F80C`.
- Post-archive validator passed: exactly five allowlisted entries, root filenames, manifest/version/dependency, UTF-8 documentation, decodable 256×256 PNG and evidence-matching DLL. No license included pending the user's decision.
- Icon visually inspected at native size.
- `scripts/test-package-validation.ps1 -ZipPath <candidate.zip>` accepted the valid candidate and rejected all four deliberately invalid copies: missing README, replaced DLL, extra private-log file and invalid PNG. Test ZIPs are isolated under `dist/validator-tests-*`, never in the candidate folder.
- No game launch, profile modification, save modification or public upload occurred during packaging. Gameplay/public-release gates above remain open.

## Future updates

Published package versions cannot be overwritten. Upload a higher numeric major.minor.patch version with the same Team/name for code or packaged README changes. Keep 'experimental' in prose rather than adding a `-beta` suffix to version_number. Re-run tests, update evidence hashes and rebuild before every release.

Sources: [package format](https://wiki.thunderstore.io/mods/creating-a-package), [BepInEx layout](https://wiki.thunderstore.io/mods/packaging-your-mods), [updating](https://wiki.thunderstore.io/mods/updating-a-package), [pinned dependency](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/v/5.4.2403/).
