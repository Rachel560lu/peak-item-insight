# PEAK Item Insight

Preview recognized item effects before using them. An experimental, unofficial PEAK mod.

**0.1.9 is a testing candidate, not a fully validated stable release.** Hunger recovery has player-confirmed in-game evidence. Injury recovery and held-item behavior have automated/synthetic coverage but still need real-game acceptance. Clean mod-manager installation and startup remain unverified; an intermittent mod-loading problem in the development environment is unresolved.

## What you see

- Aim at an item to see its recognized effects and available usage information.
- When nothing is targeted, the currently held item becomes the preview source. A hovered item takes priority. The panel indicates Hover/Held.
- For hunger or injury recovery, only the recoverable portion of the existing native HUD status segment pulses toward healthy green. The unchanged portion keeps its original appearance. This is a prediction, not an actual heal.
- Example: hunger is 30 and the food removes 10: one third of the hunger segment pulses. If hunger is 5 and the food removes 10, the entire hunger segment pulses. With no hunger, no hunger recovery pulse is expected.
- Empty hands plus no target clears the preview. Looking away while holding an item intentionally keeps a held-item preview. The used item's preview pauses during use and recalculates afterward.
- Poison, extra stamina and other recognized effects appear as text; they do not yet have equivalent native HUD pulses.

## 中文说明

这是非官方实验版物品提示 Mod。准星对准物品时显示其已识别效果；没有悬停目标时，改为预览手持物品。

饥饿和伤势预览直接显示在原有状态条上：**只有预计能恢复的那一段，在原色和健康绿色之间柔和闪烁**。例如饥饿 30、食物减饥饿 10，则黄色区域的三分之一闪烁；饥饿 5、食物可减 10，则整段黄色闪烁。没有对应负面状态时，不会出现恢复闪烁。预览不会真正使用物品或治疗角色。

空手看地板会清除预览；手持物品看地板仍显示手持预览。使用中的物品暂停预览，使用后重新计算。毒素、额外精力等目前仅提供已识别的文字信息。

**限制：**并非完整物品图鉴或无毒保证。随机、持续、条件性及特殊角色效果可能不完整；多次使用物品的单次预测仍需逐项核对。部分名称/动作仍为英文，中文界面选择目前依赖系统语言。饥饿闪烁已有实机确认，0.1.9 的伤势和手持预览仍待实机验收。普通管理器启动兼容性尚未完成验证。

## Installation for testers

Development testing used Windows, PEAK 2.4.b and BepInEx 5.4.23.3. Compatibility with other game builds, platforms, other mods and multiplayer roles has not been established.

1. Close PEAK normally and back up your saves. Create a separate test profile in your mod manager; do not overwrite your everyday profile.
2. Install the declared dependency, `BepInEx-BepInExPack_PEAK-5.4.2403`, in that profile.
3. Import this local ZIP if your manager supports local package imports. Alternatively, with a working BepInEx profile, copy only `BepInEx/plugins/PeakItemInsight/PeakItemInsight.dll` from the ZIP into the matching profile directory. Keep only one copy of the plugin.
4. Start the modded game from the manager. This is the intended standard workflow, **not yet a certified clean-install path for this candidate**. Check that the current session's BepInEx log says `PEAK Item Insight 0.1.9 loaded` before testing previews.

This package does not contain BepInEx itself, game assemblies, saves, or the developer's custom local launcher. If normal startup does not load the mod, report it; do not replace game files with undocumented workarounds.

## Configuration

After successful loading, edit `BepInEx/config/dev.rachel.peakiteminsight.cfg` in your test profile while the game is closed:

| Section / setting | Default | Purpose |
| --- | --- | --- |
| General / Enabled | true | Enable previews |
| General / HoverDelaySeconds | 0.12 | Preview delay, 0–1 seconds |
| UI / PanelScale | 1 | Panel scale, 0.5–2 |
| UI / OffsetX | 32 | Horizontal panel offset from screen centre |
| UI / OffsetY | -90 | Vertical panel offset from screen centre |
| Debug / ShowDebugIds | false | Include item IDs for troubleshooting |

## Accuracy and safety

Recognized immediate effects are estimates based on current item/player state. Random, delayed, conditional and special-character effects are incomplete. Remaining-use behavior needs item-by-item validation. A missing poison effect must **not** be interpreted as proof that food is safe. Read incomplete-effect warnings.

The plugin is designed to read state and draw UI; it does not call item-use actions, change player status, or send gameplay network events. That design does not replace multiplayer compatibility testing. Use a solo test session first.

## Troubleshooting and removal

- No panel anywhere: verify the current run loaded the plugin, `Enabled` is true, and there is only one installed DLL. A historical log is not proof of loading in the current process.
- Panel but no pulse: check for actual hunger/injury and a recognized recovery effect. Zero recovery does not pulse. Poison/extra stamina are text-only in this build.
- Overlapping or small panel: adjust `PanelScale`, `OffsetX` and `OffsetY`.
- To report a problem, include mod/game versions, item and cooking state, hover versus held, status before use, reproduction steps and a screenshot. Supply relevant excerpts from `BepInEx/LogOutput.log` and, when needed, `BepInEx/InsightDiagnostics`. Redact personal paths, usernames and any room/account details before sharing. Diagnostics are written locally and are not automatically uploaded by this plugin.
- To remove it, close PEAK, then disable/uninstall only PeakItemInsight in the manager, or remove its plugin folder from the test profile. No save reset is required by this plugin. Do not remove shared BepInEx dependencies needed by other mods.

Publisher/contact details and licensing are pending release review. This candidate is not a declaration of an open-source license. Not affiliated with or endorsed by the PEAK developers.
