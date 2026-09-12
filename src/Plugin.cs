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
    public const string PluginVersion = "0.2.11";

    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<float> _hoverDelay = null!;
    private ConfigEntry<bool> _showDebugIds = null!;

    internal static RuntimeController? Runtime { get; private set; }
    internal static RuntimeController? StaminaRuntime { get; private set; }
    private GameObject? _runtimeObject;

    private void Awake()
    {
        SessionTrace.Start();
        _enabled = Config.Bind("General", "Enabled", true, "Enable hover previews.");
        _hoverDelay = Config.Bind("General", "HoverDelaySeconds", 0.12f,
            new ConfigDescription("Delay before a preview appears.", new AcceptableValueRange<float>(0f, 1f)));
        _showDebugIds = Config.Bind("Debug", "ShowDebugIds", false, "Show stable item IDs in the tooltip.");
        Core.PresentationOptions.Language = Config.Bind("UI", "Language", "Auto",
            new ConfigDescription("Display language; Auto follows the game.", new AcceptableValueList<string>("Auto", "Chinese", "English")));
        Core.PresentationOptions.MinimalScale = Config.Bind("UI", "MinimalScale", 1f,
            new ConfigDescription("Minimal display scale.", new AcceptableValueRange<float>(.5f, 2f)));
        Core.PresentationOptions.MinimalOffsetX = Config.Bind("UI", "MinimalOffsetX", 0f, "Minimal horizontal offset from inventory.");
        Core.PresentationOptions.MinimalOffsetY = Config.Bind("UI", "MinimalOffsetY", 0f, "Minimal vertical offset above inventory.");
        Core.PresentationOptions.Animate = Config.Bind("UI", "AnimateRecovery", true, "Pulse recoverable status regions; false uses a steady tint.");
        Core.PresentationOptions.PulseStrength = Config.Bind("UI", "RecoveryStrength", 1f,
            new ConfigDescription("Recovery overlay opacity.", new AcceptableValueRange<float>(.2f, 1f)));

        Core.PresentationOptions.TextSource = Config.Bind("Preview", "InventoryTextSource", Core.PreviewMode.Both, "Inventory text: Hover, Held, Both or Off.");
        Core.PresentationOptions.StaminaSource = Config.Bind("Preview", "StaminaPreviewSource", Core.PreviewMode.Both, "Stamina preview: Hover, Held, Both or Off.");
        Core.PresentationOptions.SetupSeen = Config.Bind("UI", "AirportSetupSeen", false, "Airport setup has been shown.");
        Runtime = new RuntimeController();
        Runtime.Initialize(
            _enabled, _hoverDelay, _showDebugIds, Logger);
        StaminaRuntime = new RuntimeController(false);
        StaminaRuntime.Initialize(_enabled, _hoverDelay, _showDebugIds, Logger);

        _runtimeObject = new GameObject("PeakItemInsight.Runtime");
        DontDestroyOnLoad(_runtimeObject);
        _runtimeObject.AddComponent<Detection.RuntimeDriver>();
        _runtimeObject.AddComponent<UI.PreviewSettingsDriver>();
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
        StaminaRuntime?.Dispose();
        StaminaRuntime = null;
    }

}
