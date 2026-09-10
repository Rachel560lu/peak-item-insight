using BepInEx.Configuration;

namespace PeakItemInsight.Core;

internal static class PresentationOptions
{
    internal static ConfigEntry<string>? Language;
    internal static ConfigEntry<bool>? Details;
    internal static ConfigEntry<bool>? Animate;
    internal static ConfigEntry<float>? Opacity;
    internal static ConfigEntry<float>? PulseStrength;
    internal static bool Animation => Animate?.Value ?? true;
    internal static float Strength => PulseStrength?.Value ?? 1f;
}
