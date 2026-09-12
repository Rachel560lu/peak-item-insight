using BepInEx.Configuration;

namespace PeakItemInsight.Core;

internal static class PresentationOptions
{
    internal static ConfigEntry<PreviewMode>? TextSource, StaminaSource;
    internal static ConfigEntry<bool>? SetupSeen;
    internal static ConfigEntry<float>? MinimalScale, MinimalOffsetX, MinimalOffsetY;
    internal static ConfigEntry<string>? Language;
    internal static ConfigEntry<bool>? Animate;
    internal static ConfigEntry<float>? PulseStrength;
    internal static bool Animation => Animate?.Value ?? true;
    internal static float Strength => PulseStrength?.Value ?? 1f;
}
