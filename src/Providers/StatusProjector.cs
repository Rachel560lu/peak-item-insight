using System;
using System.Linq;
using PeakItemInsight.Core;

namespace PeakItemInsight.Providers;

// Shared by real hover/held previews and loaded-asset regression tests. Delay is
// relative to a hypothetical use, never a countdown while hovering over food.
internal static class StatusProjector
{
    internal static void Populate(ItemPreview preview,
        Func<CharacterAfflictions.STATUSTYPE, float> current,
        Func<CharacterAfflictions.STATUSTYPE, float> cap, bool locked)
    {
        preview.Statuses.Clear();
        foreach (var effects in preview.Effects.GroupBy(e => e.Type))
        {
            var before = current(effects.Key);
            var limit = cap(effects.Key);
            var after = EffectProjection.Project(before, limit,
                effects.Select(e => new ProjectedEffect(e.Amount, e.Duration, e.Delay)), locked);
            preview.Statuses.Add(new StatusDelta(effects.Key, before, after));
            if (after > before && after >= limit - .001f)
                preview.Warnings.Add(Labels.ReachesLimit(effects.Key));
        }
    }
}
