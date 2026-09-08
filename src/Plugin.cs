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
    public const string PluginVersion = "0.1.9";

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
