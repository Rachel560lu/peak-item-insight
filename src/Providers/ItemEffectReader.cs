using System;
using System.Linq;
using Peak.Afflictions;
using PeakItemInsight.Core;

namespace PeakItemInsight.Providers;

// Reads serialized effects only. Never runs actions, creates afflictions on a player,
// consumes an item or initializes ItemInstanceData.
internal static class ItemEffectReader
{
    internal static void Read(Item item, ItemPreview preview)
    {
        var actions = item.GetComponentsInChildren<ItemAction>(true).Where(a => IsActive(a, item)).ToArray();
        var knownUses = ItemDataReader.TryGetInt(item, DataEntryKey.ItemUses, out var uses);
        var direct = actions.Any(a => (a is Action_Consume || a is Action_ConsumeAndSpawn) && (a.OnPressed || a.OnCastFinished));
        var depletes = actions.Any(a => a is Action_ReduceUses reduce && reduce.consumeOnFullyUsed && (a.OnPressed || a.OnCastFinished));
        var consumes = ConsumptionRule.FiresConsumed(direct, depletes, knownUses, uses);
        preview.IsFood = (item.itemTags & (Item.ItemTags.Berry | Item.ItemTags.Mushroom | Item.ItemTags.PackagedFood)) != 0 ||
            string.Equals(item.UIData?.mainInteractPrompt, "eat", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.UIData?.mainInteractPrompt, "drink", StringComparison.OrdinalIgnoreCase);
        preview.Category = (item.itemTags & Item.ItemTags.Mystical) != 0 ? Labels.Text("神秘", "MYSTICAL")
            : preview.IsFood ? Labels.Text("消耗品", "CONSUMABLE") : Labels.Text("道具", "UTILITY");
        preview.CompleteEffects = actions.Length > 0 && !preview.PrefabOnly;
        // Primary input callbacks run before cast completion, then final consumption.
        // An action subscribed to two callbacks must not be silently counted once.
        foreach (var action in actions)
            if (!(action is Action_PlayAnimation) &&
                (action.OnHeld || action.OnReleased || action.OnCancelled || action.OnSecondaryCastFinished || action.OnSecondaryPressed ||
                 action.OnConsumed && !direct && depletes && !knownUses))
                preview.CompleteEffects = false;
        for (var phase = 0; phase < 3; phase++)
        foreach (var action in actions)
        {
            if (!(phase == 0 ? action.OnPressed : phase == 1 ? action.OnCastFinished : action.OnConsumed && consumes)) continue;
            if (action is Action_PlayAnimation || action is Action_ModifyStatus skeletonOnly && skeletonOnly.ifSkeleton) continue;
            if (action is Action_ModifyStatus modify && !modify.ifSkeleton)
                Add(preview, modify.statusType, modify.changeAmount);
            else if (action is Action_ClearAllStatus clear)
            {
                var exclusions = typeof(Action_ClearAllStatus).GetField("defaultExclusions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                    ?.GetValue(clear) as System.Collections.Generic.List<CharacterAfflictions.STATUSTYPE>;
                if (exclusions == null) { preview.CompleteEffects = false; preview.Unsupported.Add("ClearAllStatus exclusions unavailable"); }
                else foreach (var type in StatusTypes.Previewable)
                    if (!exclusions.Contains(type) && !(clear.excludeCurse && type == CharacterAfflictions.STATUSTYPE.Curse) && !clear.otherExclusions.Contains(type))
                        preview.Effects.Add(new EffectFact(type, -5, clears: true));
            }
            else if (action is Action_RestoreHunger hunger)
                Add(preview, CharacterAfflictions.STATUSTYPE.Hunger, -hunger.restorationAmount);
            else if (action is Action_GiveExtraStamina extra)
            { preview.HasExtraStamina = true; preview.ExtraAfter += extra.amount; }
            else if (action is Action_InflictPoison poison)
            {
                if (poison.inflictionTime > 0)
                    preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison,
                        poison.poisonPerSecond, poison.inflictionTime, poison.delay));
            }
            else if (action is Action_RandomMushroomEffect mushroom)
                MushroomEffectReader.Read(mushroom, preview);
            else if (action is Action_ApplyInfiniteStamina infiniteAction)
            {
                preview.InfiniteStamina = true;
                preview.Instructions.Add(Labels.Text($"无限精力 {infiniteAction.buffTime:0.#} 秒。", $"Infinite stamina for {infiniteAction.buffTime:0.#} s."));
            }
            else if (action is Action_ApplyMassAffliction mass && mass.ignoreCaster)
                preview.Instructions.Add(Labels.Text("效果作用于附近队友，不作用于自己。", "Affects nearby teammates, not yourself."));
            else if (action is Action_ApplyAffliction apply)
            {
                ReadAffliction(apply.affliction, preview);
                if (apply.extraAfflictions != null)
                    foreach (var affliction in apply.extraAfflictions) ReadAffliction(affliction, preview);
            }
            else if (!(action is Action_Consume) && !(action is Action_ConsumeAndSpawn) && !(action is Action_ReduceUses))
            { preview.CompleteEffects = false; preview.Unsupported.Add(action.GetType().Name); }
        }
        AssessRisks(preview);
        if (!preview.CompleteEffects) preview.Warnings.Add(Labels.Partial);
        if (preview.PrefabOnly) preview.Warnings.Add(Labels.Text("世界物品：基于预制体，实例状态未确认。",
            "World item: prefab data; instance state unverified."));
        if (preview.Effects.Any(e => e.Timed)) preview.Warnings.Add(Labels.Text("闪烁含持续效果累计预估；不计自然恢复、环境及已有增益。", "Pulse includes estimated timed totals; excludes natural recovery, environment and existing buffs."));
        if (!preview.IsFood && preview.Effects.Any(e => e.Type == CharacterAfflictions.STATUSTYPE.Injury && e.Amount < 0))
            preview.Description = Labels.Text("用于减轻伤势；实际恢复量取决于当前伤势。",
                "Treats injuries; actual recovery depends on your current injury.");
    }

    internal static void AssessRisks(ItemPreview preview)
    {
        preview.PoisonRisk = RiskEvidence.Assess(preview.Effects.Any(e => e.Type == CharacterAfflictions.STATUSTYPE.Poison && e.Amount > 0),
            preview.CompleteEffects, preview.PrefabOnly);
        preview.SporeRisk = RiskEvidence.Assess(preview.Effects.Any(e => e.Type == CharacterAfflictions.STATUSTYPE.Spores && e.Amount > 0),
            preview.CompleteEffects, preview.PrefabOnly);
    }

    private static void ReadAffliction(Affliction? affliction, ItemPreview preview)
    {
        if (affliction == null) return;
        if (affliction is Affliction_AdjustStatus adjust)
            Add(preview, adjust.statusType, adjust.statusAmount);
        else if (affliction is Affliction_PoisonOverTime poison)
            preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Poison,
                poison.statusPerSecond, Math.Max(0, poison.totalTime - poison.delayBeforeEffect), poison.delayBeforeEffect));
        else if (affliction is Affliction_FasterBoi speed)
        {
            preview.Instructions.Add(Labels.Text($"暂时加速，持续 {speed.totalTime:0.#} 秒。",
                $"Temporary speed boost for {speed.totalTime:0.#} s."));
            Add(preview, CharacterAfflictions.STATUSTYPE.Drowsy, -.5f);
            preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, -1, speed.totalTime));
            if (speed.drowsyOnEnd > 0)
                preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, speed.drowsyOnEnd, 0, speed.totalTime));
        }
        else if (affliction is Affliction_AdjustColdOverTime cold)
            preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Cold, cold.statusPerSecond, cold.totalTime));
        else if (affliction is Affliction_AdjustDrowsyOverTime drowsy)
            preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Drowsy, drowsy.statusPerSecond, drowsy.totalTime));
        else if (affliction is Affliction_InfiniteStamina infinite)
        {
            preview.InfiniteStamina = true;
            preview.Instructions.Add(Labels.Text($"无限精力 {infinite.totalTime:0.#} 秒（开始攀爬后计时）。", $"Infinite stamina for {infinite.totalTime:0.#} s after climbing starts."));
            var start = preview.Effects.Count;
            ReadAffliction(infinite.drowsyAffliction, preview);
            for (var i = start; i < preview.Effects.Count; i++)
            {
                var effect = preview.Effects[i];
                preview.Effects[i] = new EffectFact(effect.Type, effect.Amount, effect.Duration, effect.Delay + infinite.totalTime);
            }
        }
        else if (affliction is Affliction_Invincibility)
            preview.Instructions.Add(Labels.Text($"无敌 {affliction.totalTime:0.#} 秒。", $"Invincible for {affliction.totalTime:0.#} s."));
        else if (affliction is Affliction_RadiateInfiniteStam)
        {
            preview.InfiniteStamina = true;
            preview.Instructions.Add(Labels.Text("为范围内角色持续提供无限精力。", "Grants infinite stamina to characters within range."));
        }
        else { preview.CompleteEffects = false; preview.Unsupported.Add(affliction.GetType().Name); }
    }

    private static void Add(ItemPreview preview, CharacterAfflictions.STATUSTYPE type, float amount)
    {
        preview.Effects.Add(new EffectFact(type, amount));
        if (type == CharacterAfflictions.STATUSTYPE.Poison && amount < 0)
            preview.Effects.Add(new EffectFact(CharacterAfflictions.STATUSTYPE.Spores, amount));
    }
    internal static bool IsActive(ItemAction action, Item item)
    {
        if (!action.enabled) return false;
        for (var node = action.transform; node != null && node != item.transform; node = node.parent)
            if (!node.gameObject.activeSelf) return false;
        return true;
    }
}
