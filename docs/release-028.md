# Release 0.2.8

User authorized GitHub and Thunderstore publication after accepting the isolated candidate, and requested that both READMEs replace all old demos with the supplied new energy-drink recording.

## Changes and validation

- Native rounded font and Chinese fallback glyphs; native status icons and up/down triangles.
- Hover information follows the first empty slot (1, 2, 3), then the backpack position when full; held information stays at its own slot.
- No settings panel. No changes to stamina overlay code, effect providers, status math or runtime target priority. Plugin.cs changes only the release version.
- See [implementation evidence](native-minimal-preview.md).
- 104 offline tests passed, build zero errors/warnings.
- Accepted development DLL SHA256: `E8E13E14FED3558B4BA11435045012C83ED13D122946C0FFF3A45DC0F868D003`; isolated Unity probe PID 63132 passed, and user subsequently accepted the visible test session.
- Release DLL SHA256: `896C1C825B1097F66031D5FC3FADF38A07A51F6184D1F76FE9AD2BD85C4D4A65`.
- Final release Unity probe PID 62688 exited 0, with native font/glyph/arrow, minimal UI and existing HUD probes passing (`session-62688-20260912T114426677.log`).
- ZIP validation passed; five malformed package fixtures were rejected. ZIP SHA256: `5CB0BEF5DBA14F41A8579C015886113D896E35A10858DB026E07BA0ABF442849`.

## Gameplay GIF

gameplay-028.gif: source 新能量饮料预览.mp4, 0–5 seconds, 50 frames at 10 fps, 960px wide, 128 colors, 9,321,837 bytes. Original speed and full HUD, silent, no invented overlays. Pause menu and the holstered tail are excluded. Both the original recording contact sheet and final GIF contact sheet were inspected.

Each current README embeds this single GIF. Historical demo assets remain available for older links; they are no longer embedded. The original recording is unchanged and is not included in the mod package.

Thunderstore uses the version-tagged public GitHub asset URL. The ZIP contains only manifest, README, changelog, cover, MIT license and release DLL.
