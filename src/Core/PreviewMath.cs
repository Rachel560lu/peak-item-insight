using System;

namespace PeakItemInsight.Core;

// No Unity dependencies: the same functions run in the mod and offline tests.
internal static class PreviewMath
{
    public static float Clamp(float value, float min, float max)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || max < min)
            throw new ArgumentOutOfRangeException(nameof(value));
        return Math.Max(min, Math.Min(max, value));
    }

    public static float Apply(float before, float amount, float cap) => Clamp(before + amount, 0f, cap);
    public static float Capacity(float statusSum) => Clamp(1f - statusSum, 0f, 1f);
    public static float ProjectedCapacity(float statusSum, float delta) => Capacity(statusSum + delta);
}

internal sealed class HoverGate
{
    private int? _target;
    private float _since;
    public bool Changed { get; private set; }

    public bool Advance(int? target, float now, float delay)
    {
        Changed = target != _target;
        if (Changed) { _target = target; _since = now; }
        return target.HasValue && now - _since >= delay;
    }

    public void Reset() { _target = null; _since = 0f; Changed = true; }
}
