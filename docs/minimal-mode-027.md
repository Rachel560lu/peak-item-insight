# 极简模式开发记录

日期：2026-09-12；0.2.7 已发布到 GitHub 与 Thunderstore。

## 已实现

- 用户局内验收后要求只保留极简界面：已移除详细卡片、RoundedCard 和 DisplayMode 开关。旧配置中的 Detailed 不再影响运行。
- 极简视图共享 `ItemPreview` 与现有 HUD 数据；原生状态图标、原生字体、无背景，数值和图标分行。
- 手持定位到库存名称上方；准星目标带名称，保留目标优先级。位置读取原生库存 RectTransform 并换算到当前画布，避让操作提示、名称和库存矩形。
- 图标未绑定时显示简短状态名；不会绘制无语义的数字或 emoji 替代图标。
- 恢复、伤害、额外精力、清除、风险问号、持续及延迟条件；危险不随净变化为零消失。
- ResourceKind 识别次数、剩余量和燃料，不从本地化卡片文字解析数值。烹饪字段、调试信息不出现在极简视图。
- 特殊道具保留用途；普通岩钉与锈蚀岩钉准确区分。普通治疗用品已有数值时不重复描述。
- 仅有一个物品信息视图；HUD 保持独立。暂停／阻塞输入、无目标和场景切换统一隐藏。屏幕尺寸改变后重排。
- GitHub 与 Thunderstore README 均已发布中英极简界面设置说明，并标明旧录像中的卡片布局。

## 发布记录

- 用户授权后将源码提交 `71e9617` 推送至 main，创建 GitHub Release `v0.2.7`，附带安装 ZIP。
- Thunderstore 返回 Success，公开页面确认 `Rachel560lu-PeakItemInsight-0.2.7`。保留原分类并加入 AI Generated 分类。
- 公开下载 ZIP 的 SHA256 与候选包逐字节一致：`B0B2AC40EAF25E4195FE072FC0F8D521C35775FF1F16272A2B859DA526AF9F60`。五种无效包回归检查全部通过。
- [GitHub Release](https://github.com/Rachel560lu/peak-item-insight/releases/tag/v0.2.7) · [Thunderstore](https://thunderstore.io/c/peak/p/Rachel560lu/PeakItemInsight/v/0.2.7/)

## 测试证据

- `scripts/verify.ps1`：98 passed，0 failed（已有 88 项 + 9 项极简数据测试 + 旧配置无法恢复旧卡片的回归检查）；Release 构建 0 警告、0 错误。
- 最终 DLL 的 Unity 隔离标题画面自检：PID 27960，版本 0.2.7；退出码 0，临时 steam_appid.txt 已清理。
- 日志 `session-27960-20260912T094107970.log`：`SMOKE_MINIMAL_PASS` 和 `SMOKE_UI_PASS`，无 ERROR。
- 最终构建及隔离配置 DLL 的 SHA256：`93A0206456314F7702D6F77E9CC78E5BBFC6AB130C2D9C2BBD6154FFEF8B921C`。旧隔离 DLL 已保存在本地 `backups/minimal-only-027-*` 目录。
- 极简自检检查中文／英文、手持／准星、0.5/1/2 缩放、饥饿与持续毒性同时显示、TMP 文字几何生成、隐藏后重新绑定以及空内容隐藏。

以上为数值回归和 Unity 合成视图验证，不是局内库存布局的视觉验收。真实原生图标颜色、长名称避让、16:9／16:10／21:9 仍需游戏中确认；README 演示图待验收后录制。

## 使用

启动后即使用极简界面，无需选择模式。旧配置中的 DisplayMode、PanelScale、OffsetX / OffsetY、ShowDetails、BackgroundOpacity 不再绑定，遗留值不影响新界面。

`MinimalScale` 调大小，`MinimalOffsetX` / `MinimalOffsetY` 调相对库存的位置，保留用户已有极简设置。

首轮验收：手持食物 → 准星看另一食物 → 看地板；有毒水果；额外精力食物；绷带／急救箱；岩钉／灵药菇。检查名称、数值与闪烁始终对应同一物品，且文字不遮挡原生名称与操作键。

## 保留边界

- 无限精力显示已确认效果及已有时间条件；未新增参考图的跑步／攀爬秒数推算。
- 石化原生 HUD 错位是独立问题，本次未改动其绘制代码。
- 复杂道具没有专用短句时，保留已核对的用途描述，避免省略关键条件；后续可逐项压缩文案。
