# 首次发布到 Thunderstore / First Thunderstore release

更新：0.2.2 已以 MIT 许可发布到 [Thunderstore](https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/)。下文是准备前的历史检查清单，最新状态以 [release-022.md](release-022.md) 为准。

2026-09-11：用户反馈实机测试可用，并提供有毒食物、无毒食物与物品简介演示。素材已放进仓库 README；未上传、未推送、未改变仓库可见性。

## 先准备，不要上传旧 ZIP

当前源码与测试 DLL 为 0.2.2，但 thunderstore/manifest.json、release-evidence.json 和旧 ZIP 仍为 0.1.9。现有打包工具会拒绝版本不一致，这是预期保护。

1. 确认发布者 Team 名称、反馈渠道和代码／二进制的许可条款。保持私有源码也可分发二进制，不必为发布而自动公开仓库；不要未经所有者决定添加开源许可证。
2. 将发布 manifest、玩家 README、CHANGELOG 和证据 pin 同步至最终 0.2.2 DLL，保存其校验值。先前核对版本的 SHA256 为 `AE223C26B485397450E33CF510329CD2CC03E739D30694C9AE2E76028F907F01`；若当前构建不同，先重新核对测试证据，不应仅改 hash 绕过检查。
3. 仓库 README 的 GIF 使用相对路径，适用于 GitHub 仓库内浏览。Thunderstore 玩家 README 应改用公开可访问的 HTTPS 素材地址，并实际预览。私有仓库的 raw 链接不是公开图床；不要把带访问令牌的链接写进 README。素材可以单独公开托管，无需公开全部源码。
4. 生成 ZIP：根目录须有 manifest.json、README.md、256×256 icon.png；DLL 放 BepInEx/plugins/PeakItemInsight/PeakItemInsight.dll，可附 CHANGELOG。不要打包存档、日志、测试脚本、视频原片或转换工具。
5. 在新 profile 导入候选 ZIP，只安装所需 BepInEx，从管理器正常启动 Modded PEAK。不要依赖开发机 junction 或临时 steam_appid.txt；确认 0.2.2 加载和真实毒食物／无毒食物／手持切换。

## 上传

打开 [上传页面](https://thunderstore.io/package/create/)，登录并选择 Team，选择 PEAK 社区和合适分类，上传候选 ZIP，核对 README 预览与依赖，最终确认 Submit。

官方要求与工具：[创建包](https://wiki.thunderstore.io/mods/creating-a-package)、[打包布局](https://wiki.thunderstore.io/mods/packaging-your-mods)、[Markdown 预览](https://thunderstore.io/tools/markdown-preview/)。

## 后续更新

已经上传的版本不能覆盖修改。代码或打包 README 更新都应增加三段版本号，保持同一个 Team 和 name，再上传。见[官方更新说明](https://wiki.thunderstore.io/mods/updating-a-package)。

## Demonstration provenance

- `梅子有毒.mp4` → `docs/assets/demos/poisonous-food.gif`: first 4.2 seconds, original speed.
- `蘑菇无毒预览.mp4` → `docs/assets/demos/non-poisonous-food.gif`: first 2.3 seconds, original speed.
- `物品简介.png` → `docs/assets/demos/item-description.gif`: static GIF; original PNG also retained in that folder.
- 1100 px wide, video GIFs at 10 fps; no synthetic UI or invented gameplay added. Only trailing pause menus trimmed. Audio omitted by the GIF format.
- Original Desktop files are unchanged. Repeatable conversion: `scripts/create-demo-gifs.ps1 -FfmpegPath <local-ffmpeg.exe> -SourceDir <source-folder>`.

User acceptance and recordings demonstrate specific working interactions, not exhaustive numerical post-consumption, all-item, all-character or multiplayer verification. The clips alone do not establish their DLL hash/version; keep that distinction from the build-pinned automated evidence.

The root README is updated for 0.2.2; the old Thunderstore distribution materials have deliberately not been silently promoted to a release package.
