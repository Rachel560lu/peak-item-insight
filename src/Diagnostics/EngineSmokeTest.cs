using System;
using PeakItemInsight.Core;
using PeakItemInsight.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace PeakItemInsight.Diagnostics;

// Opt-in title-screen probe; creates only our UI and synthetic data, never gameplay objects.
internal sealed class EngineSmokeTest
{
    private readonly bool _enabled = Array.Exists(Environment.GetCommandLineArgs(), a => a == "-insightSmokeTest");
    private int _stage;
    private float _since;
    private PreviewPanel? _panel;
    private HudGhostOverlay? _ghost;
    private HungerRecoveryOverlay? _hunger;
    private StatusRecoveryOverlays? _recoveries;
    private StatusIncreaseOverlay? _optimized;
    private bool _captured;
    private float _capturedAt;
    private bool _englishShown;
    public void Tick()
    {
        if (!_enabled || _stage >= 2 || Time.frameCount < 120 || SceneManager.GetActiveScene().name != "Title") return;
        try
        {
            if (_stage == 0)
            {
                _panel = PreviewPanel.Create();
                var preview = new ItemPreview { Name = "Insight UI self-test / 界面自检" };
                preview.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Hunger, .4f, .15f));
                preview.Statuses.Add(new StatusDelta(CharacterAfflictions.STATUSTYPE.Poison, .1f, .3f));
                preview.Instructions.Add("Synthetic data. No player or save changes.");
                _panel.Show(preview, 1, 32, -90);
                Canvas.ForceUpdateCanvases();
                if (!_panel.IsVisible) throw new InvalidOperationException("Panel has no active layout.");
                SessionTrace.Write("SMOKE_UI_SHOW", "synthetic=true layout=positive");
                var track = new GameObject("SyntheticBar", typeof(RectTransform), typeof(Image));
                track.transform.SetParent(_panel.transform, false);
                var rect = track.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(60, 80);
                rect.sizeDelta = new Vector2(500, 24);
                track.GetComponent<Image>().color = new Color(.2f, .25f, .2f, 1f);
                track.GetComponent<Image>().raycastTarget = false;
                _ghost = HudGhostOverlay.Create();
                _ghost.RenderProbe(rect, 500, .6f, .85f);
                Canvas.ForceUpdateCanvases();
                if (!_ghost.SegmentVisible || Math.Abs(_ghost.SegmentWidth - 125) > .1f)
                    throw new InvalidOperationException("Beneficial ghost is not 125 pixels wide.");
                SessionTrace.Write("SMOKE_GHOST_GAIN_PASS", $"width={_ghost.SegmentWidth}");
                _hunger = HungerPulseSmokeTest.Run(_panel.transform);
                _recoveries = RecoveryRoutingSmokeTest.Run(_panel);
                _optimized = OptimizationSmokeTest.Run(_panel);
                _since = Time.unscaledTime;
                _stage = 1;
            }
            else if (_captured && Time.unscaledTime - _capturedAt >= 2f)
            {
                if (!_englishShown)
                {
                    OptimizationSmokeTest.ShowEnglish(_panel!);
                    _englishShown = true;
                    _capturedAt = Time.unscaledTime;
                    ScreenCapture.CaptureScreenshot(SessionTrace.ScreenshotPath.Replace(".png", "-en.png"));
                    return;
                }
                var rect = _panel!.transform.Find("SyntheticBar").GetComponent<RectTransform>();
                _ghost!.RenderProbe(rect, 500, .6f, .4f);
                if (!_ghost.SegmentVisible || Math.Abs(_ghost.SegmentWidth - 100) > .1f)
                    throw new InvalidOperationException("Harmful ghost is not 100 pixels wide.");
                SessionTrace.Write("SMOKE_GHOST_HARM_PASS");
                _ghost.RenderProbe(rect, 500, .6f, .6f);
                if (_ghost.SegmentVisible) throw new InvalidOperationException("Zero-delta ghost is still visible.");
                SessionTrace.Write("SMOKE_GHOST_ZERO_PASS");
                _panel!.Hide();
                _hunger!.Dispose();
                _recoveries!.Dispose();
                _optimized!.Dispose();
                if (_panel.IsVisible) throw new InvalidOperationException("Panel failed to hide.");
                UnityEngine.Object.Destroy(_panel.gameObject);
                UnityEngine.Object.Destroy(_ghost.gameObject);
                SessionTrace.Write("SMOKE_UI_PASS", "show+hide; visual readability and native HUD still require acceptance");
                _stage = 2;
                if (Array.Exists(Environment.GetCommandLineArgs(), a => a == "-insightSmokeTestExit"))
                    Application.Quit(0);
            }
            else if (!_captured && Time.unscaledTime - _since >= .5f)
            {
                _captured = true;
                _capturedAt = Time.unscaledTime;
                ScreenCapture.CaptureScreenshot(SessionTrace.ScreenshotPath);
                SessionTrace.Write("SMOKE_SCREENSHOT", SessionTrace.ScreenshotPath);
            }
        }
        catch (Exception e)
        {
            SessionTrace.Write("ERROR", $"Smoke test: {e}");
            if (_panel != null) UnityEngine.Object.Destroy(_panel.gameObject);
            if (_ghost != null) UnityEngine.Object.Destroy(_ghost.gameObject);
            _hunger?.Dispose();
            _recoveries?.Dispose();
            _optimized?.Dispose();
            _stage = 2;
            if (Array.Exists(Environment.GetCommandLineArgs(), a => a == "-insightSmokeTestExit"))
                Application.Quit(2);
        }
    }
}
