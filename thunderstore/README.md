# PEAK Item Insight

See what changes before you use an item. 使用前，看见变化。

An unofficial, experimental PEAK mod by Rachel560lu. Version **0.2.2**.

## Features

- **Hover or hold:** aim at a pick-up-able item to preview it. With no hovered target, the held item is previewed instead. Hover takes priority.
- **Recovery on the native HUD:** the recoverable part of hunger or injury pulses toward healthy green. Hunger 30 and food -10: one third pulses. Hunger 5 and food -10: the whole hunger segment pulses. Zero recovery does not pulse.
- **Harm before consumption:** recognized poison, spores and other status increases have pulsing HUD previews, including a new poison segment when you are not already poisoned. Extra/infinite stamina have separate cues.
- **Timed poison:** the card shows recognized delay, duration and cumulative poison. You do not need to wait out the poison delay while hovering. Totals are item-only estimates, not guaranteed final states.
- **Readable item cards:** game-style icons and fonts, item type, separate poison/spore warnings, signed effects and useful charges. Chinese/English follows the game or your setting.
- Empty hands plus no target clears the preview. Looking away while holding an item keeps the held preview. The used item's preview pauses during use and refreshes afterward.

## 中文

这是 Rachel560lu 制作的非官方实验版 Mod，范围是**可拾取物品**。准星对准物品时显示预览；没有悬停目标时，预览手持物品。

- 饥饿／伤势预计恢复的部分，在原色与健康绿色之间闪烁；完整、部分、零恢复分别处理。
- 已识别的毒素、孢子和其他状态增加也有原生 HUD 闪烁预览；当前没有中毒时，也会显示新增毒素区域。
- 持续中毒显示延迟、持续时间与累计估计量；无需悬停到毒性延迟结束才出现预览。
- 简介卡显示图标、类别、毒性／孢子风险、单次效果与有用的剩余次数，不再重复显示“准星／手持”或“烹饪：否”。
- 空手看地板会清除预览；手持看地板保留预览；使用期间暂停，使用后重新计算。

**累计毒素是物品自身效果的估计，不包含自然恢复、环境、已有增益和完整游戏时间步进。未知不等于无毒。** 并非所有物品、烹饪变种和特殊角色均已适配；多人及其他 HUD Mod 的兼容性尚未验证。

## Installation / 安装

Development testing used Windows and PEAK 2.4.b. User testing confirmed working previews; a completely fresh standard mod-manager startup has not yet been independently verified.

1. Close PEAK and back up saves. Use a separate mod-manager profile for the first run.
2. Install PeakItemInsight and dependency `BepInEx-BepInExPack_PEAK-5.4.2403` through your manager. For a local candidate ZIP, use the manager's local-import option.
3. Start **Modded PEAK**. Check the current BepInEx log for `PEAK Item Insight 0.2.2 loaded`.
4. Manual installation: with BepInEx working, copy the included `BepInEx/plugins/PeakItemInsight/PeakItemInsight.dll` into the matching profile path. Keep only one plugin DLL.

首次使用请备份存档，并使用独立 profile。安装本 Mod 及所需 BepInEx 后，从管理器启动 Modded PEAK；日志应显示本次运行加载了 0.2.2。不要同时保留多个版本的 DLL。

## Configuration / 配置

After the first successful load, close the game before editing `BepInEx/config/dev.rachel.peakiteminsight.cfg`.

| Setting | Default | Purpose / 用途 |
| --- | --- | --- |
| General / Enabled | true | Enable previews / 总开关 |
| General / HoverDelaySeconds | 0.12 | Hover delay / 悬停延迟 |
| UI / Language | Auto | Auto, Chinese or English / 自动、中、英 |
| UI / PanelScale | 1 | Card scale / 卡片大小 |
| UI / OffsetX, OffsetY | 32, -90 | Card position / 卡片位置 |
| UI / ShowDetails | false | Before/after values / 变化前后数值 |
| UI / BackgroundOpacity | 0.88 | Background opacity / 背景透明度 |
| UI / AnimateRecovery | true | Pulse; false uses steady tint / 关闭后为固定着色 |
| UI / RecoveryStrength | 1 | Recovery overlay opacity / 恢复预览强度 |
| Debug / ShowDebugIds | false | Item IDs / 诊断 ID |

## Accuracy and compatibility

The mod reads current item/player data and draws UI; it does not consume items, change gameplay status or send gameplay network actions. Preview-owned UI copies and temporary native-image visibility changes are restored when the preview clears.

Recognized immediate and timed effects are supported, but target-dependent tools, random area effects, special characters, environmental interactions and some conditional/cooking variants remain incomplete. Mushroom mapping must be available for mapping-dependent effects. **No poison preview is not proof of safety: check the card's risk and incomplete-effect warnings.** Environmental thorns and spore clouds are not targetable-item features of this mod.

81 offline checks pass, with separate loaded-asset/synthetic HUD coverage for six audited poison-food variants. Player-supplied recordings demonstrate poisonous food, safe-food recovery and item cards. These checks do not certify every food, multiplayer or compatibility with other mods.

## Troubleshooting / 问题反馈

- No card: confirm the current session loaded 0.2.2, `Enabled` is true and only one DLL is installed.
- Card but no recovery pulse: check that the relevant status exists and the item can reduce it. Zero recovery deliberately has no pulse.
- Poison warning but no poison pulse: record the item/variant, existing status and whether it is hovered or held; this needs investigation, not an assumption that the food is safe.
- Overlap or tiny text: adjust `PanelScale`, `OffsetX` and `OffsetY`.
- For a report, include mod/game versions, item and cooking state, hover/held, reproduction steps and a screenshot. Share relevant BepInEx log excerpts only after removing personal paths, usernames and room/account details. Diagnostics stay local; the mod does not automatically upload them.

## Removal / 卸载

Close PEAK, then disable/uninstall PeakItemInsight in the manager, or remove only its plugin folder. No save reset is required by this mod. Leave shared BepInEx dependencies used by other mods intact.

## License and credits

Copyright (c) 2026 Rachel560lu. Released under the **MIT License**, included in the package. Game assets and dependencies retain their respective owners' rights and are not relicensed by this notice. Not affiliated with or endorsed by the PEAK developers.
