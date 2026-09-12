using PeakItemInsight.Core;
using UnityEngine;

namespace PeakItemInsight.Providers;

internal static class MushroomEffectReader
{
    // Audited PEAK 2.4.b Action_RandomMushroomEffect.RunAction / coroutine.
    // Never roll RNG or infer effects from colours: read the current level's mapping.
    internal static void Read(Action_RandomMushroomEffect action, ItemPreview preview)
    {
        if (Application.version != "2.4.b")
        { Unknown(preview); return; }
        int effect;
        if (action.useDebugEffect) effect = action.debugEffect;
        else
        {
            var manager = MushroomManager.instance;
            if (manager == null || manager.mushroomEffects == null || manager.mushroomEffects.Length == 0 ||
                action.mushroomTypeIndex < 0) { Unknown(preview); return; }
            var index = action.mushroomTypeIndex % manager.mushroomEffects.Length;
            if (manager.mushroomStamAmt == null || index >= manager.mushroomStamAmt.Length)
            { Unknown(preview); return; }
            effect = manager.mushroomEffects[index];
            preview.HasExtraStamina = true;
            preview.ExtraAfter += manager.mushroomStamAmt[index] * .05f;
        }
        ApplyKnown(effect, preview);
    }

    internal static void ApplyKnown(int effect, ItemPreview preview)
    {
        switch (effect)
        {
            case 4:
                preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Hunger, -.15f));
                preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Injury, -.15f));
                preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison, -.15f));
                preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Spores, -.15f));
                preview.Instructions.Add(Labels.Text("并清除持续中毒效果。", "Also clears ongoing poison effects."));
                preview.CompactNotes.Add(Labels.Text("解除持续中毒", "Stops ongoing poisoning"));
                break;
            case 8:
                preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Spores, .25f));
                break;
            case 0:
                preview.SummarizeEffects = true;
                preview.CompactInfinity = Labels.Text("临时", "Temporary");
                preview.Instructions.Add(Labels.Text("3 秒后获得 4 秒无限精力。", "Infinite stamina for 4 s, after 3 s."));
                break;
            case 1: Describe(preview, "3 秒后获得 5 秒加速效果。", "Speed boost for 5 s, after 3 s.", "加速", "Speed boost"); break;
            case 2: Describe(preview, "3 秒后获得 15 秒低重力效果。", "Low gravity for 15 s, after 3 s.", "低重力", "Low gravity"); break;
            case 3: Describe(preview, "获得 10 秒无敌效果。", "Invincibility for 10 s.", "暂时无敌", "Temporary invincibility"); break;
            case 5:
                Describe(preview, "3 秒后触发孢子爆炸并被向上弹起；范围伤害未量化。",
                    "Spore explosion and upward launch after 3 s; area damage not quantified.", "孢子爆炸／弹起", "Spore blast / launch");
                preview.CompleteEffects = false;
                break;
            case 6: Describe(preview, "3 秒后失明 60 秒。", "Blindness for 60 s, after 3 s.", "暂时失明", "Temporary blindness"); break;
            case 7: Describe(preview, "3 秒后跌倒，持续约 8 秒。", "Fall for about 8 s, after 3 s.", "跌倒", "Fall"); break;
            case 9: Describe(preview, "3 秒后麻木 60 秒。", "Numbness for 60 s, after 3 s.", "暂时麻木", "Temporary numbness"); break;
            default: Unknown(preview); break;
        }
    }
    private static void Describe(ItemPreview p, string zh, string en, string compactZh, string compactEn)
    {
        p.Instructions.Add(Labels.Text(zh, en));
        p.SummarizeEffects = true;
        p.CompactNotes.Add(Labels.Text(compactZh, compactEn));
    }
    private static void Unknown(ItemPreview preview)
    {
        preview.CompleteEffects = false;
        preview.Warnings.Add(Labels.Text("本局蘑菇效果尚未确认。", "This level's mushroom effect is unconfirmed."));
    }
}
