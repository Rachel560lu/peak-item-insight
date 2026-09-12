# PEAK Item Insight

**See what changes before you use it. 使用前，看见变化。**

Preview recovery and poison on your stamina bar, and see what each item does.

在精力条上预览恢复与毒性，随时查看物品用途。

[Download / 下载](https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/) · [简体中文](#中文) · [English](#english) · [Report an issue / 问题反馈](https://github.com/Rachel560lu/peak-item-insight/issues)

## 实机演示 / Gameplay demos

**0.2.7：默认且仅保留极简界面。** 以原生字体和状态图标显示效果，精力条闪烁预览保持不变。以下已有录像展示旧版信息卡布局。

**0.2.7 uses the minimal interface exclusively.** Compact effects use game fonts and status icons; stamina-bar previews remain available. Existing recordings below show the previous card layout.

### 能量饮料 / Energy drink

从准星悬停到拿在手里，使用前查看状态变化。

Preview status changes before use, both while aiming and while holding the drink.

![能量饮料预览 / Energy drink preview](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/energy-drink.gif)

### 运动饮料：新增额外精力 / Sports drink: new bonus stamina

没有额外精力时，原生风格的预览条显示在主精力条上方。

With no bonus stamina, a native-style preview appears above the main stamina bar.

![运动饮料额外精力预览 / Sports drink bonus stamina preview](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/sports-drink.gif)

### 运动饮料：衔接已有精力 / Sports drink: extending an existing bonus

已有额外精力时，预览在下方衔接；已有部分保持稳定，仅新增部分闪烁。

With bonus stamina already available, the preview extends the lower bar. Your current amount stays steady while only the gain pulses.

![已有额外精力时的预览 / Preview with existing bonus stamina](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/sports-drink-existing-bonus.gif)

### 有毒蘑菇 / Poisonous mushroom

同时查看饥饿恢复和预计增加的毒素占用。

See hunger recovery alongside the expected poison increase.

![有毒蘑菇预览 / Poisonous mushroom preview](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/poisonous-mushroom.gif)

### 有毒莓果 / Poisonous berry

食用前查看毒性提示，紫色闪烁区域显示预计增加的毒素占用。

See poison warnings before eating. The purple pulse previews the expected poison on your stamina bar.

![有毒食物预览 / Poisonous food preview](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/poisonous-food.gif)

### 急救箱 / First aid kit

拿起急救箱，在使用前查看可恢复的伤势区域和物品效果。

Hold a first aid kit to preview recoverable injuries and read its effects before use.

![急救箱恢复预览 / First aid kit recovery preview](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/first-aid-kit.gif)

### 无毒蘑菇 / Safe mushroom

食用前查看可以恢复的饥饿区域。

Preview the hunger you can recover before eating.

![食物恢复预览 / Food recovery preview](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/non-poisonous-food.gif)

### 特殊道具 / Special items

查看童军道具的用途及使用代价。

Read what a special Scout item does and the cost of using it.

![特殊道具功能介绍 / Special item information](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/special-item.gif)

### 灵药菇 / Remedy Fungus

效果提示说明投掷或放下后的群体治疗，以及留在治疗云中的用法。

The card explains how to drop or throw it for group healing and stay within its healing cloud.

![灵药菇使用说明 / Remedy Fungus instructions](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/remedy-fungus.gif)

### 物品简介 / Item information

查看物品用途、效果与剩余使用次数。

See what an item does, its effects and remaining uses.

![物品效果提示 / Item compact effect information](https://raw.githubusercontent.com/Rachel560lu/peak-item-insight/c2b4644e7073a084c239e917222b2d3788ef895f/docs/assets/demos/item-description.gif)

## 中文

### 下载与安装

1. 打开 [Thunderstore 下载页](https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/)，点击 **Install with App**；也可以在 Thunderstore Mod Manager 或 r2modman 中选择 **PEAK**，搜索 **PeakItemInsight**。
2. 安装 Mod 及管理器提示的 **BepInExPack_PEAK** 依赖。
3. 点击 **Start modded** 启动游戏。

安装或更新前请退出游戏。首次使用 Mod，建议备份存档并新建一个管理器配置。

### 怎么使用

- **准星对准物品：**显示该物品的效果预览和效果提示。
- **拿在手里：**没有瞄准其他物品时，显示手持物品的预览。
- **恢复效果：**预计恢复的饥饿、伤势区域闪绿光，只标出实际可恢复的部分。
- **额外精力：**使用原生闪电和白框；当前没有额外精力时预览在上方，已有时在下方衔接，仅新增部分闪烁。
- **有害效果：**毒素、孢子等效果以相应颜色闪烁提示；持续毒性可查看延迟、持续时间和预计累计量。
- **移开准星：**空手时隐藏预览，手持物品时切回手持预览。

预览不会自动使用物品。没有饥饿或伤势时，对应的恢复区域不会闪烁。卡片若显示“未知”，请不要当作“无毒”；持续效果数值是预估值。

### 调整显示

首次启动后，退出游戏，在管理器的 **Config editor** 中找到 **dev.rachel.peakiteminsight**：

- **Language：**跟随游戏，或选择中文／英文。
- **MinimalScale / MinimalOffsetX / MinimalOffsetY：**调整极简模式大小与位置。
- **AnimateRecovery：**开启或关闭闪烁；关闭后使用固定颜色预览。

默认直接使用极简界面，无需选择模式。

极简模式中的饥饿、伤势等数值是物品本次效果；精力条只闪烁当前能实际改变的部分。毒性等持续效果旁会显示累计时长与延迟；问号代表风险尚未确认。

### 遇到问题？

- **完全没有提示：**确认从 **Start modded** 启动，并且管理器中已启用本 Mod。
- **有卡片但没有恢复闪烁：**确认角色有对应的饥饿或伤势，且物品能恢复它。
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

- **Aim at an item:** see its effect preview and compact effect information.
- **Hold an item:** when you are not aiming at another item, see the held item's preview.
- **Recovery:** the recoverable hunger or injury segment pulses green, showing only the amount you can recover.
- **Bonus stamina:** native lightning and outline appear above the main bar when you have none, or extend the lower bonus bar when you do. Only the gain pulses.
- **Harmful effects:** poison, spores and other effects pulse in their corresponding colors. Timed poison includes its delay, duration and estimated total.
- **Look away:** the preview disappears with empty hands or switches back to your held item.

Previews do not use items for you. There is no recovery pulse when you have no corresponding hunger or injury. Treat “Unknown” as unknown, not safe; timed effects are estimates.

### Customize the display

After the first launch, close the game and find **dev.rachel.peakiteminsight** in your manager's **Config editor**:

- **Language:** follow the game, or choose Chinese / English.
- **MinimalScale / MinimalOffsetX / MinimalOffsetY:** resize and reposition the minimal display.
- **AnimateRecovery:** turn pulsing on or off; off uses a steady preview color.

The minimal interface is always enabled; no mode selection is needed.

Minimal hunger/injury numbers describe this item's effect per use; the stamina pulse shows the amount your current state can actually change. Timed effects include duration and delay. A question mark means the risk is not confirmed.

### Need help?

- **No preview at all:** launch with **Start modded** and check that the mod is enabled.
- **Card but no recovery pulse:** check that you have hunger or injury the item can restore.
- **Effect text too small or in the way:** adjust its size and position.
- **Something else:** [report an issue](https://github.com/Rachel560lu/peak-item-insight/issues) with the item name, a screenshot and the steps that caused it.

To uninstall, close the game and disable or remove **PeakItemInsight** in your mod manager.

---

[MIT License](https://github.com/Rachel560lu/peak-item-insight/blob/main/LICENSE) · Rachel560lu · Unofficial PEAK mod / 非官方 PEAK Mod
