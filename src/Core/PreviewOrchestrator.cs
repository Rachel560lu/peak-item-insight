using System;
using System.Collections.Generic;
using BepInEx.Logging;
using PeakItemInsight.Providers;

namespace PeakItemInsight.Core;

internal sealed class PreviewOrchestrator
{
    private readonly IReadOnlyList<IItemPreviewProvider> _providers;
    private readonly ManualLogSource _log;

    public PreviewOrchestrator(IReadOnlyList<IItemPreviewProvider> providers, ManualLogSource log)
    {
        _providers = providers;
        _log = log;
    }

    public ItemPreview Build(Item item, bool showDebugId, bool prefabOnly = false)
    {
        var preview = new ItemPreview
        {
            ItemId = item.itemID,
            Name = Labels.ItemName(item),
            Icon = item.UIData?.icon,
            PrefabOnly = prefabOnly,
            DebugId = showDebugId ? $"item:{item.itemID}" : null
        };

        foreach (var provider in _providers)
        {
            try
            {
                if (provider.CanHandle(item))
                    provider.Populate(item, preview);
            }
            catch (Exception exception)
            {
                _log.LogWarning($"{provider.GetType().Name} failed for item {item.itemID}: {exception.Message}");
                preview.CompleteEffects = false;
                if (!preview.Diagnostics.Contains(Labels.Partial)) preview.Diagnostics.Add(Labels.Partial);
            }
        }

        ItemEffectReader.AssessRisks(preview);
        var nativeDescription = Labels.ItemDescription(item);
        if (!string.IsNullOrWhiteSpace(nativeDescription)) preview.Description = nativeDescription;
        // Numeric healing effects already explain a bandage/medkit. Preserve
        // short use instructions for utilities and mystical target-dependent tools.
        if (!preview.IsFood && (preview.Effects.Count == 0 || (item.itemTags & Item.ItemTags.Mystical) != 0))
            preview.CompactUse = Labels.CompactUse(item, preview.Description);
        if (preview.HasExtraStamina)
            preview.Resources.Add(new ResourceLine(Labels.ExtraStamina, $"{preview.ExtraBefore * 100:0} → {preview.ExtraAfter * 100:0}"));

        return preview;
    }
}
