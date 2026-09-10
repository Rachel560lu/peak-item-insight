using BepInEx;
using BepInEx.Configuration;
using PeakItemInsight.Diagnostics;
using UnityEngine;

namespace PeakItemInsight;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "dev.rachel.peakiteminsight";
    public const string PluginName = "PEAK Item Insight";
    public const string PluginVersion = "0.2.2";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<float> _hoverDelay = null!;
    private ConfigEntry<float> _scale = null!;
    private ConfigEntry<float> _offsetX = null!;
    private ConfigEntry<float> _offsetY = null!;
    private ConfigEntry<bool> _showDebugIds = null!;

    internal static RuntimeController? Runtime { get; private set; }
    private GameObject? _runtimeObject;

    private void Awake()
    {
        SessionTrace.Start();
        _enabled = Config.Bind("General", "Enabled", true, "Enable hover previews.");
        _hoverDelay = Config.Bind("General", "HoverDelaySeconds", 0.12f,
            new ConfigDescription("Delay before a preview appears.", new AcceptableValueRange<float>(0f, 1f)));
        _scale = Config.Bind("UI", "PanelScale", 1f,
            new ConfigDescription("Overall preview scale.", new AcceptableValueRange<float>(0.5f, 2f)));
        _offsetX = Config.Bind("UI", "OffsetX", 32f, "Horizontal offset from screen centre.");
        _offsetY = Config.Bind("UI", "OffsetY", -90f, "Vertical offset from screen centre.");
        _showDebugIds = Config.Bind("Debug", "ShowDebugIds", false, "Show stable item IDs in the tooltip.");
        Core.PresentationOptions.Language = Config.Bind("UI", "Language", "Auto",
            new ConfigDescription("Card language; Auto follows the game.", new AcceptableValueList<string>("Auto", "Chinese", "English")));
        Core.PresentationOptions.Details = Config.Bind("UI", "ShowDetails", false, "Show projected before/after values.");
        Core.PresentationOptions.Opacity = Config.Bind("UI", "BackgroundOpacity", .88f,
            new ConfigDescription("Card background opacity.", new AcceptableValueRange<float>(.4f, 1f)));
        Core.PresentationOptions.Animate = Config.Bind("UI", "AnimateRecovery", true, "Pulse recoverable status regions; false uses a steady tint.");
        Core.PresentationOptions.PulseStrength = Config.Bind("UI", "RecoveryStrength", 1f,
            new ConfigDescription("Recovery overlay opacity.", new AcceptableValueRange<float>(.2f, 1f)));

        Runtime = new RuntimeController();
        Runtime.Initialize(
            _enabled, _hoverDelay, _scale, _offsetX, _offsetY, _showDebugIds, Logger);

        _runtimeObject = new GameObject("PeakItemInsight.Runtime");
        DontDestroyOnLoad(_runtimeObject);
        _runtimeObject.AddComponent<Detection.RuntimeDriver>();
        Logger.LogInfo("Persistent Update driver installed.");

        Logger.LogInfo($"{PluginName} {PluginVersion} loaded for PEAK {Application.version}");
    }

    private void OnDestroy()
    {
        SessionTrace.Write("PLUGIN_DESTROY");
        if (_runtimeObject != null)
            Destroy(_runtimeObject);
        Runtime?.Dispose();
        Runtime = null;
    }

}
