# 验证结果 — 2026-09-06 / diagnostic 0.1.6

## 结论

已复现并解决本地测试中的主要启动故障；已通过离线逻辑检查和真实 Unity 中的合成 UI 自检。**全部真实物品的端到端预览尚未验收，不宣称所有目标完成。**

## 根因证据

1. 直接启动 PID 39244，独立日志记录 START、DRIVER_AWAKE。
2. Unity 日志明确出现 `RestartAppIfNecessary returned true`，随后该进程记录 DRIVER_DESTROY 和 PLUGIN_DESTROY，尚未执行第一帧。
3. Steam 重启为 PID 42096；旧 BepInEx 日志不能证明新进程仍有 Mod。之前仅凭“没有 tick”判断 Interaction.LateUpdate 不执行，是错误归因。
4. 使用 Steam 官方文档描述的临时 steam_appid.txt 开发启动方式后，PID 54848 在同一进程持续输出 FIRST_FRAME、HEARTBEAT，经过 Pretitle 到 Title；不再于首帧前退出。
5. 后续最终构建 PID 5780，MVID `d7b3f241-9716-4581-839a-87f6f0f888df`，再次成功。标准 Steam/r2modman 启动为何没有注入仍未单独定位，当前验证通过的是受控开发启动脚本。

官方机制：https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown

## 已执行

| 检查 | 结果 | 证据 |
| --- | --- | --- |
| 编译 | PASS，0 warnings / 0 errors | scripts/verify.ps1 |
| 离线检查 | PASS 28/28，含 10,000 个固定种子数学样本 | tools/Verification/Program.cs |
| 状态计算 | PASS：恢复、毒素、上限、满状态、混合抵消、状态总量超出 1 | 运行生产 PreviewMath |
| 悬停状态机 | PASS：延迟、离开、A→B、相同 prefab 的不同世界 ID、重置 | 运行生产 HoverGate |
| 会话证据 | PASS：拒绝错 PID、旧版本、启动-only、已销毁、异常 | 运行生产 SessionEvidence |
| 游戏 API | PASS：FakeItem、OptionableIntItemData、LateUpdate 写 currentHovered | 对本机 Assembly-CSharp 做 Cecil 检查 |
| 状态修改调用扫描 | PASS：编译后 Mod 未调用所列 RunAction/RPC/状态修改 API | 静态调用扫描；不等同全部副作用的形式证明 |
| 同进程帧循环 | PASS：PID 5780 FIRST_FRAME + 多次 HEARTBEAT，进入 Title | session-5780 日志 |
| 实际 UI 显隐 | PASS：正尺寸布局、Show、Hide | SMOKE_UI_PASS |
| 实际幽灵区段 | PASS：增益 125px，减益 100px，零变化隐藏 | SMOKE_GHOST_*_PASS |
| 截图检查 | PASS：中文、数值、状态条、绿色区段可见 | 合成自检截图；不是局内真实 HUD 截图 |

## 已修复的独立缺陷

- 日志带 PID、版本、MVID、进程开始时间，直接刷盘；Unity 日志正确目录为 LocalLow/LandCrab/PEAK。
- A→B 立即清除 A；按世界对象 ID 而非共享 prefab 识别切换。
- UI 成功后才提交缓存；异常清除残留并限频记录。
- 使用 OptionableIntItemData 读取剩余次数，Value 表示剩余值，不能以 totalUses-Value 计算。
- 满状态和净变化抵消提供明确提示，始终显示逐状态前后数值。
- 用真实状态总量计算容量，修正总量超过 1 时的错误恢复预览。
- 子层级激活检查、独立 hunger restoration、extra stamina、毒素减少关联 Spores；复杂未支持效果显式警告。
- 状态行沿用游戏字体；幽灵对象销毁时清除附着在外部 HUD 上的区段。

## 尚未通过，不交由用户反复启动来排查

- 真实 FakeItem → 物品效果 → 当前角色状态 → 原生 HUD 的完整集成测试，含坚果/燕麦棒/蘑菇/气球/岩钉。
- 额外精力新绘制分支、复杂触发时序、毒素随时间变化、随机效果、烹饪和剩余次数的真实物品矩阵。当前 warning 表示不完整，不能把 warning 当作功能覆盖通过。
- 原生 HUD 的遮罩、最小状态段宽度、屏幕缩放、暂停/观战和跨场景重新绑定等视觉回归。
- 工具完整用途描述覆盖：原生按钮提示不等于完整图鉴说明。

自检仅停留主菜单，未进入存档或创建游戏局。备份日志与截图保存在 backups/verification-016。最终用户验收应在上述集成缺口处理完后进行；当前不要求用户再次开游戏。
