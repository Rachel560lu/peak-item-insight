# 0.2.1 持续毒性与卡片优化验证

日期：2026-09-10（Asia/Singapore）。开发测试构建，未发布。**全物品目标未完成，不能据此放行最终验收。**

## 已实现

- 普通 `Action_Consume` / `Action_ConsumeAndSpawn` 不再错误要求 `totalUses == 1`。`Action_ReduceUses` 耗尽触发独立处理；缺少次数证据不猜测最后一次。
- 即时、延迟、持续效果通过同一纯函数时间轴投影：按时间分段、同时间保留瞬时操作顺序、每段钳制上下限。
- 持续毒性使用 rate × active duration，延迟不乘进伤害。卡片展示累计量/延迟/持续时间，并标明估计范围。
- 状态增加从毒素/孢子专用扩展到所有有原生徽章的枚举状态。恢复沿原徽章绿闪；增加以 1.4 秒周期在原生行和预计行间淡入淡出，原色阶段不永久挪动原有徽章。
- 添加额外精力与无限精力的图像提示。隐藏的额外条只作为样式模板，不启用或修改其游戏组件。
- 支持持续寒冷/困倦、能量饮料的困倦变化、无限精力结束后的已知效果、清状态动作的实际排除列表、直接石化状态。
- 卡片移除“准星/手持”“烹饪：否”和通用食用说明；保留毒性/孢子、效果、剩余次数和实际已烹饪状态。风险未知绝不伪装成安全。
- 局内诊断新增实际使用次数、效果时间参数、风险和未支持组件名称。

## 验证层级

1. `scripts/verify.ps1`：74 passed / 0 failed，Release 构建 0 warnings / 0 errors。
2. 当前安装游戏的程序集检查：`Action_Consume` 直接启动 `ConsumeDelayed(false)`，后者调用 `OnConsumed`，没有 `totalUses == 1` 门槛；毒性持续时间不包含 delay。
3. Unity 主菜单测试：生产选择逻辑、原生形状合成 UI、全部枚举增加路径、闪烁相位、额外/无限精力、隐藏/释放/重绑定、原有 CanvasGroup 恢复。
4. 只读扫描 194 个已加载物品资源，其中 74 个食物；6 个启用的 OnConsumed 持续毒性组件作为真实资源断言。其读出的效果分别以 Hover/Held 来源传入累计计算与生产增加渲染器，断言出现并清除。
5. 未进入真实关卡、未自动食用物品；以上不是相机命中、真实 HUD 布局、实际食用结果或所有烹饪变体的完整证明。

关键回归资源：Green Crispberry / 绿脆莓，item ID 3。读取结果：Hunger -0.05、Poison 0.025/s × 4s，delay 2s；预计累计 +10 个百分点毒素。不能把资源中的这个估计说成已完成实测。

## 限制与发布阻断项

| 缺口 | 当前处理 | 全覆盖所需工作 |
| --- | --- | --- |
| 荆棘增减、随机孢子爆炸 | 明示效果未完整识别/数量未量化 | 读取物理荆棘/范围/目标，定义随机效果范围与预测语义 |
| 克隆、治疗宝石、变身/死亡/传送等特殊道具 | 部分效果或未知；不能视为完整支持 | 按实际目标、身份转变、事件顺序逐项实现和验证 |
| 火把、遮阳伞、防晒等环境或持续持有作用 | 简单说明/未支持的组件记录 | 定义持有时间窗口、环境条件和实际状态投影 |
| 已有增益与新增益叠加、自然恢复、环境 | 不计入“本次物品累计”估计 | 仿真实际 Stack 规则；与不使用物品的对照时间轴比较 |
| 游戏 2.5% 状态量化、复杂多阶段抵消 | 连续值累计估计，卡片保留各效果 | 使用状态累积器/量化规则；显示中间收益与后续副作用而非只有终点 |
| 蘑菇本局随机映射、FakeItem 的真实实例状态 | 未就绪时明确未知 | 在真实关卡覆盖本局映射、生熟变体、实例与拾取后的一致性 |
| 原生额外条/石化布局及多分辨率 | 合成路由通过，实机待验收 | 原生 HUD 位置/大小/最小宽度、场景切换、多人和其他 HUD Mod |

用户要求的“所有影响精力条的物品，在准星/手持时都预览”是必需门槛，而不是本报告的完成声明。尚不能发布或宣布 ready；也不能用无毒/无孢子替代真正未知的结果。

## 隔离与产物

- 仅更新 `PeakItemInsight-Test`，日常 profile 和旧 Thunderstore 0.1.9 包保持不变。
- 更新前 DLL 和 quicksave 备份：`backups/timed-preview-20260910-002816/`。
- 最终构建/测试 profile DLL SHA256：`E5CAF17BE9FF38E45B25157B0490D17C3F003BABDEAC3607126DB9D94D42636E`。
- quicksave SHA256：`306D8A3C08C8D1E6B7939E2229FE7B68E5D8C6FBE542D0CF2E004BDE88D5518D`，测试前后不变。
- 早期本轮主菜单测试 PID 42064 因旧测试仍期待来源标签而退出 2；修正测试预期后 PID 42464、31816、62476 正常退出 0。最后构建证据见下方追加记录。
- 不提交/推送 GitHub，不上传 Thunderstore，不修改角色状态或调用物品动作。

## English summary

最终构建主菜单运行：PID `45360`，2026-09-10 00:40:16 +08 启动，正常退出 `0`；MVID `baea4360-31d9-402f-af7b-b00a375becd7`。`SMOKE_OPTIMIZATION_PASS`、`SMOKE_ENGLISH_PASS`、`SMOKE_UI_PASS` 及 `consumedPoisonPassed=6` 同一进程通过，无本插件 ERROR。日志/截图保存在本地忽略目录 `backups/timed-021-evidence/`。临时 `steam_appid.txt` 已清除；实际存档与备份哈希再次核对一致。

Development build 0.2.1 fixes direct-consumption poison detection, adds estimated cumulative timed-status pulses, generalizes badge routing and removes redundant card text. 74 offline checks and title-screen synthetic/loaded-asset tests pass. Six real loaded OnConsumed poison actions are checked through both source labels and the production projection/renderer; this does not simulate a live hover raycast or prove real in-level rendering. Full item coverage remains blocked by conditional, random, environmental, special-character and target-dependent effects. Unknown risk remains unknown, never silently safe. No public release or repository push.
