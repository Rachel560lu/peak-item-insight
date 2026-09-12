using System;

namespace PeakItemInsight.Core;

internal enum PreviewSource { None, Hover, Held }
internal enum PreviewMode { Both, Hover, Held, Off }

internal readonly struct PreviewTargetKey : IEquatable<PreviewTargetKey>
{
    public PreviewTargetKey(PreviewSource source, int instanceId) { Source = source; InstanceId = instanceId; }
    public PreviewSource Source { get; }
    public int InstanceId { get; }
    public bool IsValid => Source != PreviewSource.None;
    public bool Equals(PreviewTargetKey other) => Source == other.Source && InstanceId == other.InstanceId;
    public override bool Equals(object? other) => other is PreviewTargetKey key && Equals(key);
    public override int GetHashCode() => unchecked((int)Source * 397 ^ InstanceId);
}

internal static class PreviewTargetSelection
{
    // Deliberately knows nothing about effects: a hovered tool/unknown item
    // must never show the held food's recovery instead.
    public static PreviewTargetKey Select(int? hoveredId, int? heldId, bool heldBusy, PreviewMode mode = PreviewMode.Both)
    {
        if (mode == PreviewMode.Off) return default;
        var hoveredBusyItem = heldBusy && hoveredId.HasValue && hoveredId == heldId;
        if (mode == PreviewMode.Held) hoveredId = null;
        if (mode == PreviewMode.Hover) heldId = null;
        if (hoveredId.HasValue)
            return hoveredBusyItem ? default : new PreviewTargetKey(PreviewSource.Hover, hoveredId.Value);
        return heldId.HasValue && !heldBusy ? new PreviewTargetKey(PreviewSource.Held, heldId.Value) : default;
    }
}

internal sealed class PreviewTargetGate
{
    private PreviewTargetKey _target;
    private float _since;
    public bool Changed { get; private set; }
    public bool Advance(PreviewTargetKey target, float now, float delay)
    {
        Changed = !_target.Equals(target);
        if (Changed) { _target = target; _since = now; }
        return target.IsValid && now - _since >= delay;
    }
    public void Reset() { _target = default; _since = 0; Changed = true; }
}
