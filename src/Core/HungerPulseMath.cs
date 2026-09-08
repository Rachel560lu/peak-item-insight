using System;

namespace PeakItemInsight.Core;

// Pure UI math: never writes player state. Fraction is relative to the visible
// native hunger badge (which may have a minimum width), not the full HUD width.
internal static class HungerPulseMath
{
    public const float Period = 1.4f;

    public static float Fraction(float before, float after)
    {
        if (!Finite(before) || !Finite(after) || before <= 0 || after < 0) return 0;
        return Math.Max(0, Math.Min(1, (before - after) / before));
    }

    public static float Alpha(float elapsed)
    {
        if (!Finite(elapsed)) return 0;
        // Starts on the original yellow, reaches fully green, then returns.
        return (float)(.5 - .5 * Math.Cos(2 * Math.PI * Math.Max(0, elapsed) / Period));
    }

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
