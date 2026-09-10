using System;
using System.Collections.Generic;
using System.Linq;

namespace PeakItemInsight.Core;

// Item-only timeline: excludes natural recovery, environment and existing buffs.
internal readonly struct ProjectedEffect
{
    public ProjectedEffect(float amount, float duration = 0, float delay = 0)
    { Amount = amount; Duration = Math.Max(0, duration); Delay = Math.Max(0, delay); }
    public float Amount { get; }
    public float Duration { get; }
    public float Delay { get; }
}

internal static class EffectProjection
{
    public static float Project(float before, float cap, IEnumerable<ProjectedEffect> input, bool locked = false)
    {
        if (locked) return before;
        var effects = input.ToArray();
        var points = effects.SelectMany(e => new[] { e.Delay, e.Delay + e.Duration }).Append(0f).Distinct().OrderBy(t => t).ToArray();
        var value = before;
        for (var i = 0; i < points.Length; i++)
        {
            var now = points[i];
            foreach (var effect in effects.Where(e => e.Duration == 0 && e.Delay == now))
                value = PreviewMath.Apply(value, effect.Amount, cap);
            if (i + 1 == points.Length) break;
            var rate = effects.Where(e => e.Duration > 0 && e.Delay <= now && e.Delay + e.Duration > now).Sum(e => e.Amount);
            value = PreviewMath.Apply(value, rate * (points[i + 1] - now), cap);
        }
        return value;
    }
}

internal static class ConsumptionRule
{
    // Direct consumption is NOT charge depletion; totalUses is irrelevant.
    public static bool FiresConsumed(bool direct, bool depletes, bool knownUses, int uses) =>
        (!knownUses || uses != 0) && (direct || depletes && knownUses && uses == 1);
}
