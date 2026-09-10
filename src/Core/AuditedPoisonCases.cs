namespace PeakItemInsight.Core;

// Regression expectations only, NOT runtime overrides. PEAK 2.4.b loaded assets
// cross-checked against Food wiki; cooked/disabled actions must never use these
// numbers as a fallback. See docs/food-toxicity-audit.md.
internal readonly struct AuditedPoisonCase
{
    internal AuditedPoisonCase(ushort id, string name, float duration, float delay, float total)
    { Id = id; Name = name; Duration = duration; Delay = delay; Total = total; }
    internal ushort Id { get; }
    internal string Name { get; }
    internal float Rate => .025f;
    internal float Duration { get; }
    internal float Delay { get; }
    internal float Total { get; }
}

internal static class AuditedPoisonCases
{
    internal static readonly AuditedPoisonCase[] All = {
        new AuditedPoisonCase(3, "Green Crispberry", 4, 2, .10f),
        new AuditedPoisonCase(41, "Yellow Kingberry", 10, 3, .25f),
        new AuditedPoisonCase(11, "Pink Berrynana", 6, 2, .15f),
        new AuditedPoisonCase(89, "Bugle Shroom", 8, 10, .20f),
        new AuditedPoisonCase(97, "Button Shroom", 12, 10, .30f),
        new AuditedPoisonCase(84, "Cluster Shroom", 16, 10, .40f)
    };
}
