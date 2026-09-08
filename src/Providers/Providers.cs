using System;
using System.Linq;
using PeakItemInsight.Core;
using UnityEngine;

namespace PeakItemInsight.Providers;

internal interface IItemPreviewProvider
{
    bool CanHandle(Item item);
    void Populate(Item item, ItemPreview preview);
}

internal sealed class DirectStatusProvider : IItemPreviewProvider
{
    public bool CanHandle(Item item) => item.GetComponentsInChildren<ItemAction>(true).Length > 0;

    public void Populate(Item item, ItemPreview preview)
    {
        var character = Character.localCharacter;
        var afflictions = character != null ? character.refs.afflictions : null;
        if (character == null || afflictions == null)
            return;

        if (character.data.isSkeleton || character.isZombie || character.isScoutmaster)
        {
            preview.Warnings.Add(Labels.SpecialState);
            return;
        }
        var usesKnown = ItemDataReader.TryGetInt(item, DataEntryKey.ItemUses, out var uses);
        if (usesKnown && uses == 0)
        {
            preview.Warnings.Add(Labels.EmptyItem);
            return;
        }

        var active = item.GetComponentsInChildren<ItemAction>(true).Where(a => IsActive(a, item)).ToArray();
        var consumes = (usesKnown ? uses == 1 : item.totalUses <= 1) && active.Any(a =>
            (a is Action_Consume || a is Action_ConsumeAndSpawn) && (a.OnPressed || a.OnCastFinished));
        var relevant = active.Where(a => a.OnPressed || a.OnCastFinished || (a.OnConsumed && consumes)).ToArray();
        if (active.Any(a => a.OnHeld || a.OnCancelled || a.OnConsumed && !consumes && !a.OnPressed && !a.OnCastFinished))
            preview.Warnings.Add(Labels.Conditional);

        if (relevant.Any(a => !(a is Action_ModifyStatus) && !(a is Action_RestoreHunger) && !(a is Action_GiveExtraStamina) &&
                             !(a is Action_Consume) && !(a is Action_ConsumeAndSpawn)))
            preview.Warnings.Add(Labels.Partial);

        var amounts = new System.Collections.Generic.Dictionary<CharacterAfflictions.STATUSTYPE, float>();
        foreach (var action in relevant)
        {
            if (action is Action_ModifyStatus modify && !modify.ifSkeleton && modify.statusType != CharacterAfflictions.STATUSTYPE.Petrify)
            {
                AddAmount(modify.statusType, modify.changeAmount);
                if (modify.statusType == CharacterAfflictions.STATUSTYPE.Poison && modify.changeAmount < 0)
                    AddAmount(CharacterAfflictions.STATUSTYPE.Spores, modify.changeAmount);
            }
            else if (action is Action_RestoreHunger hunger)
                AddAmount(CharacterAfflictions.STATUSTYPE.Hunger, -hunger.restorationAmount);
            else if (action is Action_GiveExtraStamina extra)
            {
                if (!preview.HasExtraStamina) preview.ExtraBefore = preview.ExtraAfter = character.data.extraStamina;
                preview.HasExtraStamina = true;
                preview.ExtraAfter = PreviewMath.Apply(preview.ExtraAfter, extra.amount, Mathf.Max(0, 1 - character.data.petrifyAmount * .01f));
            }
        }

        void AddAmount(CharacterAfflictions.STATUSTYPE type, float amount)
        {
            amounts.TryGetValue(type, out var existing);
            amounts[type] = existing + amount;
        }

        foreach (var effect in amounts)
        {
            var before = afflictions.GetCurrentStatus(effect.Key);
            var cap = afflictions.GetStatusCap(effect.Key);
            var after = character.statusesLocked ? before : PreviewMath.Apply(before, effect.Value, cap);
            preview.Statuses.Add(new StatusDelta(effect.Key, before, after));
            if (after >= cap - 0.001f)
                preview.Warnings.Add(Labels.ReachesLimit(effect.Key));
        }
    }

    private static bool IsActive(ItemAction action, Item item)
    {
        if (!action.enabled) return false;
        // A world FakeItem references an inactive prefab root. Respect all child
        // toggles, but do not require the asset root to be active in a scene.
        for (var node = action.transform; node != null && node != item.transform; node = node.parent)
            if (!node.gameObject.activeSelf) return false;
        return true;
    }
}

internal sealed class InstanceResourceProvider : IItemPreviewProvider
{
    public bool CanHandle(Item item) => item.totalUses > 1 || item.cooking != null || item.data != null;

    public void Populate(Item item, ItemPreview preview)
    {
        if (item.totalUses > 1 && ItemDataReader.TryGetInt(item, DataEntryKey.ItemUses, out var remainingUses))
            preview.Resources.Add(new ResourceLine(Labels.Uses, remainingUses < 0 ? "∞" : $"{Math.Max(0, remainingUses)} / {item.totalUses}"));

        if (ItemDataReader.TryGetFloat(item, DataEntryKey.UseRemainingPercentage, out var remaining))
            preview.Resources.Add(new ResourceLine(Labels.Remaining, $"{Mathf.RoundToInt(Mathf.Clamp01(remaining) * 100f)}%"));

        if (ItemDataReader.TryGetFloat(item, DataEntryKey.Fuel, out var fuel))
            preview.Resources.Add(new ResourceLine(Labels.Fuel, $"{Mathf.Max(0f, fuel):0.#}"));

        if (item.cooking != null && item.cooking.canBeCooked)
            preview.Resources.Add(new ResourceLine(Labels.Cooked, item.cooking.timesCookedLocal > 0 ? $"x{item.cooking.timesCookedLocal}" : Labels.No));
    }
}

internal sealed class PitonProvider : IItemPreviewProvider
{
    public bool CanHandle(Item item) => item.GetComponentInChildren<ShittyPiton>(true) != null ||
                                        item.name.IndexOf("piton", StringComparison.OrdinalIgnoreCase) >= 0;

    public void Populate(Item item, ItemPreview preview)
    {
        preview.Instructions.Add(Labels.PitonInstruction);
    }
}

internal sealed class PromptProvider : IItemPreviewProvider
{
    public bool CanHandle(Item item) => item.UIData != null;

    public void Populate(Item item, ItemPreview preview)
    {
        var data = item.UIData;
        if (data.hasMainInteract && !string.IsNullOrWhiteSpace(data.mainInteractPrompt))
            AddUnique(preview, data.mainInteractPrompt);
        if (data.hasSecondInteract && !data.hideSecondInteract && !string.IsNullOrWhiteSpace(data.secondaryInteractPrompt))
            AddUnique(preview, data.secondaryInteractPrompt);
        if (data.hasScrollingInteract && !string.IsNullOrWhiteSpace(data.scrollInteractPrompt))
            AddUnique(preview, data.scrollInteractPrompt);
    }

    private static void AddUnique(ItemPreview preview, string text)
    {
        if (!preview.Instructions.Contains(text))
            preview.Instructions.Add(text);
    }
}

internal static class Labels
{
    private static bool Chinese => Application.systemLanguage == SystemLanguage.ChineseSimplified;
    public static string Source(PreviewSource source) => source == PreviewSource.Held
        ? (Chinese ? "手持" : "Held") : (Chinese ? "准星" : "Hover");
    public static string Uses => Chinese ? "剩余次数" : "Uses";
    public static string Remaining => Chinese ? "剩余" : "Remaining";
    public static string Fuel => Chinese ? "燃料" : "Fuel";
    public static string Cooked => Chinese ? "烹饪" : "Cooked";
    public static string No => Chinese ? "否" : "No";
    public static string PitonInstruction => Chinese
        ? "放置在可攀爬墙面；抓住它休息并恢复体力。"
        : "Place on a climbable wall. Hold it to rest and recover stamina.";
    public static string NoPreview => Chinese ? "未检测到可预览的效果。" : "No previewable effect detected.";
    public static string NoChange => Chinese ? "当前状态下无净精力变化；各状态变化见上方。" : "No net stamina change at current state; see individual effects above.";
    public static string EmptyItem => Chinese ? "已无剩余使用次数。" : "No uses remaining.";
    public static string SpecialState => Chinese ? "特殊角色状态：无法可靠预测。" : "Special character state: exact preview unavailable.";
    public static string Conditional => Chinese ? "持续/取消/消耗触发的条件效果未完整计入。" : "Conditional held/cancel/consume effects are not fully included.";
    public static string Partial => Chinese ? "仅预览已识别的即时效果；其他效果未完整计入，不能据此判断无毒。" : "Known immediate effects only; other effects may apply. Not a safety guarantee.";
    public static string ExtraStamina => Chinese ? "额外精力" : "Extra stamina";

    public static string ReachesLimit(CharacterAfflictions.STATUSTYPE type) => Chinese
        ? $"{Status(type)}将达到上限"
        : $"{Status(type)} reaches its limit";

    public static string Status(CharacterAfflictions.STATUSTYPE type)
    {
        if (Chinese)
        {
            switch (type)
            {
                case CharacterAfflictions.STATUSTYPE.Injury: return "伤势";
                case CharacterAfflictions.STATUSTYPE.Hunger: return "饥饿";
                case CharacterAfflictions.STATUSTYPE.Cold: return "寒冷";
                case CharacterAfflictions.STATUSTYPE.Poison: return "毒素";
                case CharacterAfflictions.STATUSTYPE.Curse: return "诅咒";
                case CharacterAfflictions.STATUSTYPE.Drowsy: return "困倦";
                case CharacterAfflictions.STATUSTYPE.Weight: return "负重";
                case CharacterAfflictions.STATUSTYPE.Hot: return "炎热";
            }
        }

        switch (type)
        {
            case CharacterAfflictions.STATUSTYPE.Injury: return "Injury";
            case CharacterAfflictions.STATUSTYPE.Hunger: return "Hunger";
            case CharacterAfflictions.STATUSTYPE.Cold: return "Cold";
            case CharacterAfflictions.STATUSTYPE.Poison: return "Poison";
            case CharacterAfflictions.STATUSTYPE.Curse: return "Curse";
            case CharacterAfflictions.STATUSTYPE.Drowsy: return "Drowsy";
            case CharacterAfflictions.STATUSTYPE.Weight: return "Weight";
            case CharacterAfflictions.STATUSTYPE.Hot: return "Heat";
            default: return type.ToString();
        }
    }
}
