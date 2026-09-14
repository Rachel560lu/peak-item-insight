using BepInEx.Configuration;

namespace PeakItemInsight.Core;

internal enum DescriptionStyle { Minimal, Detailed }

internal static class PresentationOptions
{
    internal static ConfigEntry<DescriptionStyle>? DescriptionStyle;
    internal static ConfigEntry<PreviewMode>? TextSource, StaminaSource;
    internal static ConfigEntry<bool>? SetupSeen;
    internal static ConfigEntry<float>? MinimalScale, MinimalOffsetX, MinimalOffsetY;
    internal static ConfigEntry<float>? DetailedBackgroundOpacity;
    internal static ConfigEntry<string>? Language;
    internal static ConfigEntry<bool>? Animate;
    internal static ConfigEntry<float>? PulseStrength;
    internal static bool Animation => Animate?.Value ?? true;
    internal static float Strength => PulseStrength?.Value ?? 1f;
}
