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

    public ItemPreview Build(Item item, bool showDebugId)
    {
        var preview = new ItemPreview
        {
            ItemId = item.itemID,
            Name = string.IsNullOrWhiteSpace(item.UIData?.itemName) ? item.GetItemName() : item.UIData.itemName,
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
            }
        }

        var delta = 0f;
        foreach (var status in preview.Statuses) delta += status.After - status.Before;
        if ((preview.Statuses.Count > 0 || preview.HasExtraStamina) && Math.Abs(delta) < 0.0001f && Math.Abs(preview.ExtraAfter - preview.ExtraBefore) < .0001f)
            preview.Warnings.Add(Labels.NoChange);
        if (preview.HasExtraStamina)
            preview.Resources.Add(new ResourceLine(Labels.ExtraStamina, $"{preview.ExtraBefore * 100:0} → {preview.ExtraAfter * 100:0}"));

        return preview;
    }
}
