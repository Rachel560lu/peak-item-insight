using PeakItemInsight.Core;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

internal sealed class StatusRecoveryOverlays
{
    internal HungerRecoveryOverlay Hunger { get; } = new HungerRecoveryOverlay();
    internal HungerRecoveryOverlay Injury { get; } = new HungerRecoveryOverlay("Injury");
    public void Render(ItemPreview preview, Image? healthy, Image? hunger, Image? injury, float elapsed)
    {
        RenderOne(Hunger, Find(preview, CharacterAfflictions.STATUSTYPE.Hunger), hunger, healthy, elapsed);
        RenderOne(Injury, Find(preview, CharacterAfflictions.STATUSTYPE.Injury), injury, healthy, elapsed);
    }
    internal static StatusDelta? Find(ItemPreview preview, CharacterAfflictions.STATUSTYPE type)
    {
        foreach (var status in preview.Statuses) if (status.Type == type) return status;
        return null;
    }
    private static void RenderOne(HungerRecoveryOverlay overlay, StatusDelta? delta, Image? source, Image? healthy, float elapsed)
    {
        if (delta == null || source == null || healthy == null) { overlay.Hide(); return; }
        overlay.Render(source, healthy, delta.Value.Before, delta.Value.After, elapsed);
    }
    public void Hide() { Hunger.Hide(); Injury.Hide(); }
    public void Dispose() { Hunger.Dispose(); Injury.Dispose(); }
}
