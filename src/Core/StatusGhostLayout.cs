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
