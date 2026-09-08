<p align="center">
  <img src="docs/assets/peak-item-insight.jpg" alt="PEAK Item Insight — mountain, crosshair and status bar / 山峰、准星与状态条" width="192" height="192">
</p>

<h1 align="center">PEAK Item Insight</h1>

<p align="center">使用前，看见变化。<br>See what changes before you use an item.</p>

<p align="center"><a href="#中文">中文</a> · <a href="#english">English</a></p>

## 中文

为 PEAK 制作的非官方 BepInEx 物品提示 Mod。准星对准或手持物品时，预览已识别的效果，并查看简短的使用说明。

**当前版本：0.1.9 · 实验版 · 尚未在 Thunderstore 发布。** 饥饿恢复闪烁已有玩家实机确认；伤势恢复和手持预览已实现并通过自动化／合成场景检查，但仍待实机验收。普通 Mod 管理器的全新安装、启动流程也尚未完成验证。

### 功能与预览方式

- **准星优先，手持补充：**有悬停目标时预览该物品；没有目标时预览当前手持物品。面板标明来源。
- **原生 HUD 恢复预览：**饥饿或伤势区域中，预计能恢复的部分在原色与健康绿色之间柔和闪烁，周期约 1.4 秒。
- **完整／部分／零恢复：**饥饿 30、食物减少 10，则黄色区域的三分之一闪烁；饥饿 5、食物可减少 10，则整段闪烁；没有饥饿时，不显示饥饿恢复闪烁。伤势使用相同逻辑。
- **效果与用途：**显示已识别的即时状态变化、游戏内操作提示及已知用途；物品实例提供数据时，还显示剩余次数和烹饪状态。毒素、额外精力等目前仅在面板显示，不使用同样的原生 HUD 闪烁。
- **切换与清除：**空手看地板会隐藏预览；手持物品看地板则保留手持预览。正在使用的物品暂停预览，使用后重新计算。

预览只是提示，不会真正使用物品、修改角色状态或发送游戏网络事件。

### 安装与安全测试

开发环境为 Windows、PEAK 2.4.b、BepInEx 5.4.23.3；其他游戏版本、平台、多人角色及其他 Mod 的兼容性尚未验证。

1. 正常退出 PEAK，备份存档，在 Mod 管理器中新建独立测试 profile，保留日常配置。
2. 安装依赖 `BepInEx-BepInExPack_PEAK-5.4.2403`。
3. 将构建出的 `PeakItemInsight.dll` 放入该 profile 的 `BepInEx/plugins/PeakItemInsight/`，只保留一份 DLL。
4. 从管理器启动 Modded PEAK，并确认**本次运行**的日志包含 `PEAK Item Insight 0.1.9 loaded`。这是预期流程，不代表本实验版已通过全新环境启动验收。

详细故障排查与卸载方法见[玩家说明](thunderstore/README.md)。本仓库不包含游戏 DLL、BepInEx 安装包、存档或编译产物；克隆源码并不等于安装 Mod。

### 配置

成功加载后，配置位于测试 profile 的 `BepInEx/config/dev.rachel.peakiteminsight.cfg`。建议关闭游戏后编辑。

| 配置项 | 默认值 | 用途 |
| --- | --- | --- |
| `General / Enabled` | `true` | 总开关 |
| `General / HoverDelaySeconds` | `0.12` | 预览延迟，范围 0–1 秒 |
| `UI / PanelScale` | `1` | 面板缩放，范围 0.5–2 |
| `UI / OffsetX`, `OffsetY` | `32`, `-90` | 相对屏幕中心的面板位置 |
| `Debug / ShowDebugIds` | `false` | 显示物品 ID，便于排查 |

### 准确性与已知限制

这不是完整的物品安全图鉴。仅预测已识别的即时主操作效果；随机、延迟、条件性、特殊角色和多次使用物品的效果仍可能不完整。**未显示中毒效果，不等于食物无毒。** 请留意面板中的不完整效果警告。

部分物品名称和操作仍为英文；中文面板目前根据系统语言选择，未完全跟随游戏语言。“客户端只读”的设计不等于多人兼容性已经通过验证。

日志保存在本地，不会由此插件自动上传。报告问题时附上游戏／Mod 版本、物品、悬停或手持、使用前状态和复现步骤；分享日志前请遮盖用户名、个人路径及房间信息。

### 构建、验证与发布

需要 .NET SDK、合法安装的 PEAK 及对应 BepInEx 依赖。游戏程序集路径可通过以下参数覆盖：

```powershell
dotnet build -c Release -p:PeakManagedDir="X:\path\to\PEAK_Data\Managed"
```

产物位于 `bin/Release/netstandard2.1/PeakItemInsight.dll`。当前项目的 Harmony 引用及部分工具／测试脚本仍含开发机路径；换电脑时需要调整自己的 BepInEx 路径，不能仅改 `PeakManagedDir` 就假定全部可用。

- `scripts/verify.ps1`：构建、核心逻辑及 API／只读行为检查，当前记录 **54 项通过、0 项失败**。它依赖开发环境的 `.dotnet-sdk` 和游戏／BepInEx 路径，不是独立 CI。
- `scripts/start-isolated-test.ps1 -SmokeTest -ExitAfterSmoke`：**开发机专用**标题页合成 UI 测试，不是通用安装方式。要求游戏关闭、既有测试 profile junction 正确；临时添加 `steam_appid.txt`，正常退出后清理。不要运行中中断；若被强制中断，应先关游戏，再核对并仅移除它生成且未变化的临时文件。
- 验证需同一游戏进程中的版本／MVID、首帧、持续心跳及 `SMOKE_HUNGER_PASS`、`SMOKE_RECOVERY_ROUTING_PASS`、`SMOKE_UI_PASS`。仅有启动日志或合成截图不代表真实物品链路已验收。
- `scripts/package.ps1`：使用本地 `dist/PeakItemInsight.dll` 和记录的哈希组装、校验候选 ZIP；不构建、不安装、不启动游戏，也不上传。`dist/` 不入库，新克隆需要先准备并验证对应 DLL，不能绕过证据哈希检查。

[验证计划](docs/verification-plan.md) · [验证结果](docs/verification-results.md) · [饥饿预览](docs/hunger-pulse-018.md) · [伤势与手持验收](docs/recovery-held-019.md) · [发布清单](docs/release-checklist.md) · [更新日志](CHANGELOG.md)

许可证尚未决定；仓库当前未授予开源许可。本项目与 PEAK 开发商无隶属或官方背书关系。

---

## English

An unofficial BepInEx item-insight mod for PEAK. Hover over or hold an item to preview recognized effects and read concise usage information.

**Current version: 0.1.9 · Experimental · Not published on Thunderstore.** Hunger recovery pulses have player-confirmed in-game evidence. Injury recovery and held previews are implemented and covered by automated/synthetic checks, but still await real-game acceptance. Clean installation and startup through a standard mod manager also remain unverified.

### Features and preview behavior

- **Hover first, held fallback:** preview the hovered item when one exists; otherwise preview the currently held item. The panel labels the source.
- **Native HUD recovery preview:** the recoverable part of the hunger or injury segment gently pulses between its original color and healthy green, on an approximately 1.4-second cycle.
- **Full, partial and zero recovery:** with hunger at 30 and food that removes 10, one third of the yellow segment pulses. With hunger at 5 and food that removes 10, the whole segment pulses. At zero hunger, there is no hunger recovery pulse. Injury recovery follows the same logic.
- **Effects and usage:** see recognized immediate status changes, in-game prompts and known usage notes. Remaining uses and cooking state appear when exposed by the item instance. Poison, extra stamina and other effects remain in the information panel, without equivalent native HUD pulses.
- **Switching and clearing:** empty hands plus no target hides the preview. Looking at the floor while holding an item intentionally keeps a held preview. The item being used pauses its preview and is recalculated afterward.

Previews are informational: the plugin does not invoke item-use actions, mutate player status or send gameplay network events.

### Installation and safe testing

Development testing used Windows, PEAK 2.4.b and BepInEx 5.4.23.3. Compatibility with other game versions, platforms, multiplayer roles and other mods has not been established.

1. Close PEAK normally, back up your saves and create a separate test profile in your mod manager. Preserve your everyday profile.
2. Install `BepInEx-BepInExPack_PEAK-5.4.2403`.
3. Place the built `PeakItemInsight.dll` in that profile's `BepInEx/plugins/PeakItemInsight/` directory. Keep only one copy.
4. Launch Modded PEAK through the manager and confirm the **current session's** log includes `PEAK Item Insight 0.1.9 loaded`. This is the intended workflow, not a claim of clean-install certification.

See the [player guide](thunderstore/README.md) for troubleshooting and removal. Game assemblies, BepInEx installers, saves and compiled binaries are not included in this repository; cloning the source does not install the mod.

### Configuration

After a successful load, configuration is generated at `BepInEx/config/dev.rachel.peakiteminsight.cfg` in the test profile. Edit it while the game is closed.

| Setting | Default | Purpose |
| --- | --- | --- |
| `General / Enabled` | `true` | Master switch |
| `General / HoverDelaySeconds` | `0.12` | Preview delay, 0–1 seconds |
| `UI / PanelScale` | `1` | Panel scale, 0.5–2 |
| `UI / OffsetX`, `OffsetY` | `32`, `-90` | Panel position relative to screen center |
| `Debug / ShowDebugIds` | `false` | Include item IDs for troubleshooting |

### Accuracy and known limitations

This is not a complete item-safety guide. Only recognized immediate primary-use effects are predicted; random, delayed, conditional, special-character and multi-use effects may be incomplete. **An absent poison prediction does not prove food is safe.** Read the panel's incomplete-effect warnings.

Some item names and actions remain in English. Chinese panel selection currently follows the system language rather than fully following the game language. A client-side, read-only design is not proof of multiplayer compatibility.

Logs stay local and are not automatically uploaded by this plugin. Reports should include game/mod versions, the item, hover versus held, pre-use state and reproduction steps. Redact usernames, personal paths and room details before sharing logs.

### Building, verification and releases

Requires a .NET SDK, a legitimate PEAK installation and matching BepInEx dependencies. Override the game assembly directory as needed:

```powershell
dotnet build -c Release -p:PeakManagedDir="X:\path\to\PEAK_Data\Managed"
```

Output: `bin/Release/netstandard2.1/PeakItemInsight.dll`. The project's Harmony reference and some tools/test scripts still contain development-machine paths. On another machine, adjust those BepInEx paths too; overriding `PeakManagedDir` alone does not make every build and test portable.

- `scripts/verify.ps1`: builds and checks core logic, game APIs and read-only behavior. The recorded result is **54 passed, 0 failed**. It depends on the development environment's `.dotnet-sdk` and game/BepInEx paths; it is not standalone CI.
- `scripts/start-isolated-test.ps1 -SmokeTest -ExitAfterSmoke`: **development-machine-specific** synthetic UI tests at the title screen, not a general installer. It requires a closed game and the existing test-profile junction. It adds a temporary `steam_appid.txt`, exits normally and cleans up. Do not interrupt it while running. If forcibly interrupted, close the game, then verify and remove only the unchanged temporary file it created.
- Verification requires one game process's version/MVID, first frame, repeated heartbeats and `SMOKE_HUNGER_PASS`, `SMOKE_RECOVERY_ROUTING_PASS`, `SMOKE_UI_PASS`. Startup logs or synthetic screenshots alone do not establish real-item acceptance.
- `scripts/package.ps1`: assembles and validates a candidate ZIP from local `dist/PeakItemInsight.dll` against the recorded hash. It does not build, install, launch or upload. Since `dist/` is excluded from Git, a fresh clone must first prepare and verify the corresponding DLL; do not bypass the evidence hash check.

[Verification plan](docs/verification-plan.md) · [Results](docs/verification-results.md) · [Hunger preview](docs/hunger-pulse-018.md) · [Injury/held acceptance](docs/recovery-held-019.md) · [Release checklist](docs/release-checklist.md) · [Changelog](CHANGELOG.md)

Licensing is undecided; this repository does not currently grant an open-source license. Not affiliated with or endorsed by the PEAK developers.
