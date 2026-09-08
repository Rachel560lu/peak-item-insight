using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using PeakItemInsight.Core;
using PeakItemInsight.Detection;
using PeakItemInsight.Providers;
using PeakItemInsight.UI;
using PeakItemInsight.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakItemInsight;

internal sealed class RuntimeController
{
    private ConfigEntry<bool> _enabled = null!;
    private ConfigEntry<float> _hoverDelay = null!;
    private ConfigEntry<float> _scale = null!;
    private ConfigEntry<float> _offsetX = null!;
    private ConfigEntry<float> _offsetY = null!;
    private ConfigEntry<bool> _showDebugIds = null!;
    private ManualLogSource _log = null!;
    private HoverResolver _hoverResolver = null!;
    private PreviewOrchestrator _orchestrator = null!;
    private PreviewPanel _panel = null!;
    private HudGhostOverlay _ghostOverlay = null!;
    private Item? _candidate;
    private Item? _visibleItem;
    private readonly PreviewTargetGate _hoverGate = new PreviewTargetGate();
    private PreviewTargetKey _target;
    private int _scene, _character, _gui;
    private PreviewStateKey _lastState;
    private string _lastTargetType = "";
    private bool _hasTicked;
    private float _nextErrorLog;

    public void Initialize(
        ConfigEntry<bool> enabled,
        ConfigEntry<float> hoverDelay,
        ConfigEntry<float> scale,
        ConfigEntry<float> offsetX,
        ConfigEntry<float> offsetY,
        ConfigEntry<bool> showDebugIds,
        ManualLogSource log)
    {
        _enabled = enabled;
        _hoverDelay = hoverDelay;
        _scale = scale;
        _offsetX = offsetX;
        _offsetY = offsetY;
        _showDebugIds = showDebugIds;
        _log = log;
        _hoverResolver = new HoverResolver();
        _orchestrator = new PreviewOrchestrator(new IItemPreviewProvider[]
        {
            new DirectStatusProvider(),
            new InstanceResourceProvider(),
            new PitonProvider(),
            new PromptProvider()
        }, log);
        _log.LogInfo("Runtime controller initialized.");
    }

    public void Tick()
    {
        if (!_hasTicked)
        {
            _hasTicked = true;
            _log.LogInfo("Persistent Update driver tick received.");
        }

        try
        {
            var scene = SceneManager.GetActiveScene().handle;
            var character = Character.localCharacter != null ? Character.localCharacter.GetInstanceID() : 0;
            var gui = GUIManager.instance != null ? GUIManager.instance.GetInstanceID() : 0;
            if (_scene != scene || _character != character || _gui != gui)
            {
                _scene = scene; _character = character; _gui = gui;
                _hoverGate.Reset(); _candidate = null; _target = default; Hide();
            }
            if (!_enabled.Value || Character.localCharacter == null || MainCameraMovement.IsSpectating || Time.timeScale == 0f)
            {
                _hoverGate.Reset();
                _candidate = null;
                _target = default;
                Hide();
                return;
            }

            var hovered = _hoverResolver.Resolve();
            var held = HeldResolver.Resolve();
            var selected = PreviewTargetSelection.Select(hovered != null ? _hoverResolver.WorldId : null,
                held != null ? held.GetInstanceID() : (int?)null, HeldResolver.IsBusy(held));
            var item = selected.Source == PreviewSource.Hover ? hovered : selected.Source == PreviewSource.Held ? held : null;
            var ready = _hoverGate.Advance(selected, Time.unscaledTime, _hoverDelay.Value);
            if (_hoverGate.Changed || _hoverResolver.CurrentTargetType != _lastTargetType)
            {
                _lastTargetType = _hoverResolver.CurrentTargetType;
                _log.LogInfo($"Hover target: {_lastTargetType}; resolved item: {(hovered != null ? hovered.itemID.ToString() : "none")}");
                SessionTrace.Write("HOVER", $"target={_lastTargetType} world={_hoverResolver.WorldId} item={(hovered != null ? hovered.itemID.ToString() : "none")}");
            }
            if (_hoverGate.Changed || item != _candidate)
            {
                SessionTrace.Write("TARGET", $"from={_target.Source}:{_target.InstanceId} to={selected.Source}:{selected.InstanceId} heldBusy={HeldResolver.IsBusy(held)}");
                _candidate = item;
                _target = selected;
                Hide();
            }

            if (_candidate == null || !ready)
                return;

            var state = PreviewStateKey.Capture(_candidate);
            if (_candidate == _visibleItem && state.Equals(_lastState))
                return;

            var preview = _orchestrator.Build(_candidate, _showDebugIds.Value);
            preview.Source = _target.Source;
            preview.TargetInstanceId = _target.InstanceId;
            EnsureUi();
            _ghostOverlay.Show(preview);
            _panel.Show(preview, _scale.Value, _offsetX.Value, _offsetY.Value);
            // Commit only after both renderers succeed, so failures can retry.
            if (_visibleItem != _candidate)
            {
                SessionTrace.Write("PREVIEW", $"source={preview.Source} target={preview.TargetInstanceId} item={preview.ItemId} statuses={preview.Statuses.Count} instructions={preview.Instructions.Count}");
                WorldPreviewTrace.Preview(_candidate, preview, _target.InstanceId);
            }
            _visibleItem = _candidate;
            _lastState = state;
        }
        catch (Exception exception)
        {
            Hide();
            if (Time.unscaledTime >= _nextErrorLog)
            {
                _nextErrorLog = Time.unscaledTime + 5f;
                _log.LogError($"Hover preview tick failed: {exception}");
                SessionTrace.Write("ERROR", exception.ToString());
            }
        }
    }

    private void EnsureUi()
    {
        if (_panel == null)
            _panel = PreviewPanel.Create();
        if (_ghostOverlay == null)
            _ghostOverlay = HudGhostOverlay.Create();
    }

    private void Hide()
    {
        if (_visibleItem != null) SessionTrace.Write("HIDE");
        _visibleItem = null;
        _lastState = default;
        if (_panel != null)
            _panel.Hide();
        if (_ghostOverlay != null)
            _ghostOverlay.Hide();
    }

    public void Dispose()
    {
        if (_panel != null)
            UnityEngine.Object.Destroy(_panel.gameObject);
        if (_ghostOverlay != null)
            UnityEngine.Object.Destroy(_ghostOverlay.gameObject);
    }
}
