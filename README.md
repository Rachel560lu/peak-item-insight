<p align="center">
  <img src="docs/assets/item-insight-header.png" width="920" alt="PEAK Item Insight Mod — scouts inspecting food with a stamina preview / 童军查看食物与精力条预览" />
</p>

<h1 align="center">PEAK Item Insight</h1>

<p align="center"><strong>See what changes before you use it.</strong><br />
Preview recovery and poison on your stamina bar. Know what you're holding.</p>

<p align="center"><strong>使用前，看见变化。</strong><br />
精力条预览恢复与毒性，极简提示说明物品用途。</p>

<p align="center">
  <a href="https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/"><img src="https://img.shields.io/badge/Download-Thunderstore-8cdb65?style=for-the-badge" alt="Download on Thunderstore" /></a>
  <a href="https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/"><img src="https://img.shields.io/thunderstore/dt/Rachel560lu/PeakItemInsight?style=for-the-badge&amp;color=8cdb65&amp;cacheSeconds=300" alt="Thunderstore total downloads" /></a>
  <a href="https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/versions"><img src="https://img.shields.io/thunderstore/v/Rachel560lu/PeakItemInsight?style=for-the-badge&amp;color=345b63&amp;label=version&amp;cacheSeconds=300" alt="Latest Thunderstore package version" /></a>
  <img src="https://img.shields.io/badge/BepInEx-5-345b63?style=for-the-badge" alt="BepInEx 5" />
</p>

<p align="center">
  <a href="https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/">Download</a> ·
  <a href="#中文">简体中文</a> ·
  <a href="#english">English</a> ·
  <a href="https://github.com/Rachel560lu/peak-item-insight/issues">Report an issue</a>
</p>

## 实机演示 / Gameplay demo

能量饮料与运动饮料：查看简洁的效果提示和精力条变化。

Energy and sports drinks: compact effect summaries and stamina previews before use.

![能量饮料与运动饮料 / Energy and sports drinks](docs/assets/demos/drinks-029.gif)

混合坚果：原装风格的额外精力条闪烁预览。

Trail mix: bonus stamina preview with native-style UI.

![混合坚果额外精力 / Trail mix bonus stamina](docs/assets/demos/trail-mix-029.gif)

## 中文

### 下载与安装

1. 打开 [Thunderstore 下载页](https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/)，点击 **Install with App**；也可以在 Thunderstore Mod Manager 或 r2modman 中选择 **PEAK**，搜索 **PeakItemInsight**。
2. 安装 Mod 及管理器提示的 **BepInExPack_PEAK** 依赖。
3. 点击 **Start modded** 启动游戏。

安装或更新前请退出游戏。首次使用 Mod，建议备份存档并新建一个管理器配置。

### 怎么使用

- **准星对准物品：**效果提示显示在第一个空物品栏上方；物品栏全满时显示在右侧背包栏上方。
- **拿在手里：**没有瞄准其他物品时，显示手持物品的预览。
- **恢复效果：**预计恢复的饥饿、伤势区域闪绿光，只标出实际可恢复的部分。
- **额外精力：**使用原生闪电和白框；当前没有额外精力时预览在上方，已有时在下方衔接，仅新增部分闪烁。
- **有害效果：**毒素、孢子等效果以相应颜色闪烁提示；持续效果以箭头、最终预计变化量和状态图标简洁展示。
- **移开准星：**空手时隐藏预览，手持物品时切回手持预览。

预览不会自动使用物品。没有饥饿或伤势时，对应的恢复区域不会闪烁。提示若显示“未知”，请不要当作“无毒”；持续效果数值是预估值。

### 调整显示

首次启动后，退出游戏，在管理器的 **Config editor** 中找到 **dev.rachel.peakiteminsight**：

- **Language：**跟随游戏，或选择中文／英文。
- **MinimalScale / MinimalOffsetX / MinimalOffsetY：**调整极简模式大小与位置。
- **AnimateRecovery：**开启或关闭闪烁；关闭后使用固定颜色预览。

默认直接使用极简界面，无需选择模式。

纯即时物品显示本次使用效果；持续效果物品显示从当前状态到效果结算后的预计变化，与精力条预览一致。加速、临时无限精力保留短标签。问号代表风险尚未确认。

### 遇到问题？

- **完全没有提示：**确认从 **Start modded** 启动，并且管理器中已启用本 Mod。
- **有效果提示但没有恢复闪烁：**确认角色有对应的饥饿或伤势，且物品能恢复它。
- **效果提示太小或挡住视线：**调整效果提示大小与位置。
- **其他问题：**[提交反馈](https://github.com/Rachel560lu/peak-item-insight/issues)，附上物品名称、问题截图和出现问题的操作步骤。

卸载时，退出游戏后在管理器中禁用或卸载 **PeakItemInsight** 即可。

## English

### Download and install

1. Open the [Thunderstore page](https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/) and choose **Install with App**, or select **PEAK** in Thunderstore Mod Manager or r2modman and search for **PeakItemInsight**.
2. Install the mod and its **BepInExPack_PEAK** dependency when prompted.
3. Click **Start modded** to launch the game.

Close the game before installing or updating. For your first modded run, back up your saves and use a separate manager profile.

### How to use

- **Aim at an item:** effects appear above the first empty inventory slot, or above the backpack slot when all three slots are full.
- **Hold an item:** when you are not aiming at another item, see the held item's preview.
- **Recovery:** the recoverable hunger or injury segment pulses green, showing only the amount you can recover.
- **Bonus stamina:** native lightning and outline appear above the main bar when you have none, or extend the lower bonus bar when you do. Only the gain pulses.
- **Harmful effects:** poison, spores and other effects pulse in their corresponding colors. Timed effects show an arrow, the estimated final change and a status icon.
- **Look away:** the preview disappears with empty hands or switches back to your held item.

Previews do not use items for you. There is no recovery pulse when you have no corresponding hunger or injury. Treat “Unknown” as unknown, not safe; timed effects are estimates.

### Customize the display

After the first launch, close the game and find **dev.rachel.peakiteminsight** in your manager's **Config editor**:

- **Language:** follow the game, or choose Chinese / English.
- **MinimalScale / MinimalOffsetX / MinimalOffsetY:** resize and reposition the minimal display.
- **AnimateRecovery:** turn pulsing on or off; off uses a steady preview color.

The minimal interface is always enabled; no mode selection is needed.

Instant-only items show effects per use. Timed-effect items show the final change from your current state, matching the stamina preview; speed boosts and temporary infinite stamina keep short labels. A question mark means the risk is not confirmed.

### Need help?

- **No preview at all:** launch with **Start modded** and check that the mod is enabled.
- **Effects visible but no recovery pulse:** check that you have hunger or injury the item can restore.
- **Effect text too small or in the way:** adjust its size and position.
- **Something else:** [report an issue](https://github.com/Rachel560lu/peak-item-insight/issues) with the item name, a screenshot and the steps that caused it.

To uninstall, close the game and disable or remove **PeakItemInsight** in your mod manager.

---

[MIT License](https://github.com/Rachel560lu/peak-item-insight/blob/main/LICENSE) · Rachel560lu · Unofficial PEAK mod / 非官方 PEAK Mod
