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
    public bool CanHandle(Item item) => true;

    public void Populate(Item item, ItemPreview preview)
    {
        ItemEffectReader.Read(item, preview);
        var character = Character.localCharacter;
        if (character == null || character.refs.afflictions == null) return;
        if (character.data.isSkeleton || character.isZombie || character.isScoutmaster)
        { preview.Warnings.Add(Labels.SpecialState); return; }
        if (ItemDataReader.TryGetInt(item, DataEntryKey.ItemUses, out var uses) && uses == 0)
        { preview.Warnings.Add(Labels.EmptyItem); return; }

        var afflictions = character.refs.afflictions;
        StatusProjector.Populate(preview,
            type => type == CharacterAfflictions.STATUSTYPE.Petrify ? character.data.petrifyAmount * .01f : afflictions.GetCurrentStatus(type),
            type => type == CharacterAfflictions.STATUSTYPE.Petrify ? 1f : afflictions.GetStatusCap(type), character.statusesLocked);
        if (preview.HasExtraStamina)
        {
            preview.ExtraBefore = character.data.extraStamina;
            preview.ExtraAfter = character.statusesLocked ? preview.ExtraBefore
                : PreviewMath.Apply(preview.ExtraBefore, preview.ExtraAfter,
                    Mathf.Max(0, 1 - character.data.petrifyAmount * .01f));
        }
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

        if (item.cooking != null && item.cooking.canBeCooked && item.cooking.timesCookedLocal > 0)
            preview.Resources.Add(new ResourceLine(Labels.Cooked, $"x{item.cooking.timesCookedLocal}"));
    }
}

internal sealed class PitonProvider : IItemPreviewProvider
{
    public bool CanHandle(Item item) => item.GetComponentInChildren<ShittyPiton>(true) != null ||
                                        item.name.IndexOf("piton", StringComparison.OrdinalIgnoreCase) >= 0;

    public void Populate(Item item, ItemPreview preview)
    {
        preview.Description = Labels.PitonInstruction;
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
        if (text.Trim().Equals("eat", StringComparison.OrdinalIgnoreCase) ||
            text.Trim().Equals("drink", StringComparison.OrdinalIgnoreCase)) return;
        if (!preview.Instructions.Contains(text) && preview.Description.Length == 0)
            preview.Instructions.Add(text);
    }
}

internal static class Labels
{
    internal static bool Chinese => PresentationOptions.Language?.Value == "Chinese" ||
        PresentationOptions.Language?.Value != "English" &&
        (LocalizedText.CURRENT_LANGUAGE == LocalizedText.Language.SimplifiedChinese ||
         LocalizedText.CURRENT_LANGUAGE == LocalizedText.Language.TraditionalChinese);
    internal static string Text(string zh, string en) => Chinese ? zh : en;
    internal static string ItemName(Item item)
    {
        if (item.UIData == null) return Text("未知物品", "Unknown item");
        if (PresentationOptions.Language?.Value == "Auto" || PresentationOptions.Language == null) return item.GetName();
        var language = Chinese ? LocalizedText.Language.SimplifiedChinese : LocalizedText.Language.English;
        var key = LocalizedText.GetNameIndex(item.UIData.itemName);
        return LocalizedText.GetText(key, language);
    }
    internal static string ItemDescription(Item item)
    {
        if (item.UIData == null) return "";
        var key = LocalizedText.GetDescriptionIndex(item.UIData.itemName);
        var language = PresentationOptions.Language?.Value == "English" ? LocalizedText.Language.English :
            PresentationOptions.Language?.Value == "Chinese" ? LocalizedText.Language.SimplifiedChinese : LocalizedText.CURRENT_LANGUAGE;
        if (LocalizedText.mainTable != null && LocalizedText.mainTable.ContainsKey(key))
            return LocalizedText.GetText(key, language);
        return "";
    }
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
    public static string Partial => Chinese ? "部分效果尚未识别。" : "Some effects are not yet supported.";
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
                case CharacterAfflictions.STATUSTYPE.Spores: return "孢子";
                case CharacterAfflictions.STATUSTYPE.Curse: return "诅咒";
                case CharacterAfflictions.STATUSTYPE.Drowsy: return "困倦";
                case CharacterAfflictions.STATUSTYPE.Weight: return "负重";
                case CharacterAfflictions.STATUSTYPE.Hot: return "炎热";
                case CharacterAfflictions.STATUSTYPE.Petrify: return "石化";
                case CharacterAfflictions.STATUSTYPE.Thorns: return "荆棘";
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
