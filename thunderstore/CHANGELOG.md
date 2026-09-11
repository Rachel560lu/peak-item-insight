# Changelog

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
