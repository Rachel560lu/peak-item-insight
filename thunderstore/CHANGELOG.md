# Changelog

## 0.2.10 — 2026-09-13 / README update

- Separate English and Chinese demos, English first. Add the new English GIF and retain both Chinese demos.
- 演示分为英文和中文，英文在前；新增英文 GIF，保留两张中文演示。
- Documentation only; the tested 0.2.9 Mod DLL is unchanged. 仅更新文档，Mod 功能不变。

## 0.2.9 — 2026-09-12 / Simpler timed effects

- Timed items show one final change per status, using the same snapshot as the stamina preview. Remove raw rates, durations and delay text.
- Keep short labels for speed boosts, temporary infinite stamina and mushroom abilities; preserve poison warnings at the status cap.
- Replace the previous gameplay demo with new energy/sports drink and trail mix recordings.
- 持续类物品按状态合并显示最终预计变化量，去掉速率、持续时长和延迟；保留临时能力短标签与毒性风险提示。
- 更新饮料和混合坚果实机 GIF。精力条闪烁、字体及物品栏定位沿用原有实现。

## 0.2.8 — 2026-09-12 / Native text and inventory positioning

- Fix missing Chinese glyphs and restore the game's rounded font, native status icons and clear up/down effect arrows.
- Anchor aimed-at item information above the first empty inventory slot; use the backpack position when all three slots are full. Holstered slot highlights no longer misplace the preview.
- Keep held-item information above its own slot and preserve stamina-bar preview behavior.
- Replace the old README demos with one new energy-drink gameplay GIF.

- 修复中文方框乱码，恢复原生圆润字体、状态图标与增减三角。
- 准星物品提示显示在第一个空槽上方，满栏时显示在背包栏上方，不再受收起物品后的高亮影响。
- 手持提示仍跟随自身槽位；精力条预览保持不变。
- README 旧演示统一替换为新版能量饮料实机 GIF。

## 0.2.7 — 2026-09-12 / Minimal interface

- Replace the detailed card with a compact, background-free interface using game fonts and status icons. No mode selection is needed.
- Keep stamina-bar previews, signed effects, poison/spore risks, duration, remaining uses and useful item instructions.
- Position information near the inventory; support Chinese/English, adjustable size and offsets.
- Remove legacy card settings. Existing minimal-interface settings remain compatible.

- 仅保留极简界面：原生字体与状态图标、无背景，无需切换模式。
- 保留精力条闪烁、效果数值、毒性／孢子提示、持续时间、剩余次数和必要用途说明。
- 信息定位在物品栏附近，支持中英文、大小和位置调整；旧详情卡配置不再生效。

## 0.2.6 — native stamina previews

- Bonus stamina now uses PEAK's original lightning icon, white outline and textured fill.
- New bonus previews appear above the main bar; existing bonus previews extend the lower bar. Only the gain pulses, with a clear gap between the bars.
- Hunger recovery and poison increases are previewed together, including full and partial hunger recovery.
- Add sports-drink, special-item and Remedy Fungus GIFs. Replace the bandage demo with a first aid kit.

额外精力预览使用原生闪电、白框和填充；没有额外精力时显示在上方，已有时在下方衔接，仅新增部分闪烁。饥饿恢复与毒性增加同步预览。新增运动饮料、特殊道具和灵药菇演示，以急救箱替换绷带演示。

## 0.2.5 — preview fixes and clearer item cards

- Fix the floating flashing block near the stamina bar when previewing bonus stamina.
- Improve Chinese/English descriptions for the Scout Cannon, Passport, Piton and Remedy Fungus.
- Explain piton reuse and Remedy Fungus group healing; remove generic internal diagnostic messages from item cards.
- Refresh gameplay demos: energy drink, poisonous mushroom, poisonous berry, bandage, safe mushroom and item information.

修复精力条外的闪烁方块，完善童军大炮、护照、岩钉和灵药菇的中英文说明，移除无用诊断文案，并更新六组演示。

## 0.2.4 — player-page and cover update

- Replace the package cover with the new Item Insight artwork.
- Streamline the bilingual README around download, gameplay demos, usage and common settings.
- Keep the same gameplay DLL and dependencies as 0.2.3.

## 0.2.3 — demonstration and source-link update

- Add poisonous-food, safe-food and item-card GIF demonstrations to the public page.
- Link the public MIT-licensed GitHub repository and issue tracker.
- Documentation-only package: the DLL is byte-identical to the tested 0.2.2 plugin, whose in-game log version remains 0.2.2. No gameplay changes or additional dependencies.

## 0.2.2 — first public release

- Preview hovered items with held-item fallback.
- Pulse hunger and injury recovery inside the affected native status segment.
- Preview recognized poison/spore/status increases, including poison when the native poison slot starts hidden or empty.
- Include item-only cumulative timed effects with delay and duration in the card; totals remain estimates.
- Add extra/infinite stamina cues and game-style bilingual cards with icons, risk warnings and useful charges.
- Read real runtime poison components rather than hardcoding food names or overwriting cooking-dependent effects.
- Validate six audited poison-food variants and pass 81 offline regression checks.
- Include the MIT License. Player testing supplied poisonous-food, safe-food and card demonstrations; full item/multiplayer compatibility is not claimed.

Earlier version numbers were local development builds, not previous public Thunderstore releases.
