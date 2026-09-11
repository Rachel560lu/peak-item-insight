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
    public bool CanHandle(Item item) => item.UIData?.itemName == "Piton" || item.GetComponentInChildren<ClimbingSpikeComponent>(true) != null ||
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
        if (preview.Description.Length == 0) preview.Description = Labels.ItemDescription(item);
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
        text = Labels.InteractPrompt(text);
        if (text.Length > 0 && !preview.Instructions.Contains(text) && preview.Description.Length == 0)
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
        // Native assets have action keys but no useful description for these tools.
        if (item.UIData.itemName == "Scout Cannon")
            return Text("放置大炮并调整角度，用它发射角色或物品。", "Place the cannon and adjust its angle to launch scouts or items.");
        if (item.UIData.itemName == "Passport")
            return Text("打开护照。", "Open your passport.");
        if (item.UIData.itemName == "Piton") return PitonInstruction;
        if (item.UIData.itemName == "Remedy Fungus")
            return Text("丢下或投掷，碰撞破裂后释放治疗云，为范围内的自己和队友治疗伤势，并清除毒素与孢子。留在云内可持续获得治疗；不是直接食用。",
                "Drop or throw; it bursts on impact into a healing cloud for you and nearby teammates. It heals injury and removes poison and spores. Stay in the cloud for continued healing; do not eat it.");
        var key = LocalizedText.GetDescriptionIndex(item.UIData.itemName);
        var language = PresentationOptions.Language?.Value == "English" ? LocalizedText.Language.English :
            PresentationOptions.Language?.Value == "Chinese" ? LocalizedText.Language.SimplifiedChinese : LocalizedText.CURRENT_LANGUAGE;
        if (LocalizedText.mainTable != null && LocalizedText.mainTable.ContainsKey(key))
            return LocalizedText.GetText(key, language);
        return "";
    }
    internal static string InteractPrompt(string prompt)
    {
        // UIData stores localization KEYS, not text to display. Resolve without
        // GetText(key, true)'s silent English fallback when Chinese is selected.
        var key = prompt.Trim().ToUpperInvariant();
        var language = PresentationOptions.Language?.Value == "English" ? LocalizedText.Language.English :
            PresentationOptions.Language?.Value == "Chinese" ? LocalizedText.Language.SimplifiedChinese : LocalizedText.CURRENT_LANGUAGE;
        var index = (int)language;
        if (LocalizedText.platformSpecificTable != null && LocalizedText.platformSpecificTable.TryGetValue(key, out var platform) &&
            index >= 0 && index < platform.Count && !string.IsNullOrWhiteSpace(platform[index])) return platform[index];
        if (LocalizedText.mainTable != null && LocalizedText.mainTable.ContainsKey(key))
        {
            var translated = LocalizedText.GetText(key, language);
            if (!string.IsNullOrWhiteSpace(translated)) return translated;
        }
        if (!Chinese) return prompt.Trim();
        switch (key)
        {
            case "OPEN": return "打开";
            case "PLACE": return "放置";
            case "CHANGE ANGLE": return "调整角度";
        }
        // Preserve already localized text, never leak untranslated action keys.
        return prompt.Any(c => c >= '\u3400' && c <= '\u9fff') ? prompt.Trim() : "";
    }
    public static string Source(PreviewSource source) => source == PreviewSource.Held
        ? (Chinese ? "手持" : "Held") : (Chinese ? "准星" : "Hover");
    public static string Uses => Chinese ? "剩余次数" : "Uses";
    public static string Remaining => Chinese ? "剩余" : "Remaining";
    public static string Fuel => Chinese ? "燃料" : "Fuel";
    public static string Cooked => Chinese ? "烹饪" : "Cooked";
    public static string No => Chinese ? "否" : "No";
    public static string PitonInstruction => Chinese
        ? "放置在可攀爬墙面，抓住它休息并恢复精力；同时限1人。普通岩钉可反复休息，没有固定使用次数。注意：地图自带的锈蚀岩钉会断裂。"
        : "Place on a climbable wall and grab it to rest and recover stamina. One scout at a time. A normal piton has no rest-use limit. Beware: rusty pitons found on the map can break.";
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
