using System;

namespace PeakItemInsight.Core;

internal readonly struct GhostRange
{
    internal GhostRange(float currentStart, float currentWidth, float addedStart, float addedWidth)
    { CurrentStart = currentStart; CurrentWidth = currentWidth; AddedStart = addedStart; AddedWidth = addedWidth; }
    internal float CurrentStart { get; }
    internal float CurrentWidth { get; }
    internal float AddedStart { get; }
    internal float AddedWidth { get; }
}

internal static class StatusGhostLayout
{
    // Mirrors BarAffliction.ChangeAffliction: values <=1% have no badge;
    // larger values use the native minimum size. Stretch-anchor padding is
    // measured separately from the status-sized sizeDelta.
    internal static float BadgeWidth(float value, float scale, float minimum, float padding = 0)
    {
        if (float.IsNaN(value) || float.IsInfinity(value) || value < 0 || scale <= 0 || minimum < 0)
            throw new ArgumentOutOfRangeException(nameof(value));
        return value <= .01f ? 0 : Math.Max(0, Math.Max(value * scale, minimum) + padding);
    }

    internal static float ProjectWidth(float nativeWidth, float before, float after, float scale, float minimum, float padding = 0)
        => Math.Abs(after - before) <= .00001f ? nativeWidth : BadgeWidth(after, scale, minimum, padding);

    internal static GhostRange[] BuildProjected(float right, float[] current, float[] projected)
    {
        if (current.Length != projected.Length) throw new ArgumentException("Mismatched rows.");
        var retained = new float[current.Length];
        var added = new float[current.Length];
        for (var i = 0; i < current.Length; i++)
        {
            if (current[i] < 0 || projected[i] < 0 || float.IsNaN(current[i]) || float.IsNaN(projected[i]) ||
                float.IsInfinity(current[i]) || float.IsInfinity(projected[i]))
                throw new ArgumentOutOfRangeException(nameof(projected));
            retained[i] = Math.Min(current[i], projected[i]);
            added[i] = Math.Max(0, projected[i] - retained[i]);
        }
        return Build(right, retained, added);
    }

    // Native order, right edge anchored; additions consume room to the left.
    // No percentage normalization: a native minimum-width badge is not extra status.
    internal static GhostRange[] Build(float right, float[] current, float[] additions)
    {
        if (current.Length != additions.Length) throw new ArgumentException("Mismatched rows.");
        var result = new GhostRange[current.Length];
        for (var i = current.Length - 1; i >= 0; i--)
        {
            if (current[i] < 0 || additions[i] < 0 || float.IsNaN(current[i]) || float.IsNaN(additions[i]))
                throw new ArgumentOutOfRangeException(nameof(current));
            right -= current[i];
            var start = right;
            right -= additions[i];
            result[i] = new GhostRange(start, current[i], right, additions[i]);
        }
        return result;
    }
}
