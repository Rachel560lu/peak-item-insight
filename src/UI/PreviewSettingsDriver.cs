using System;
using System.Collections.Generic;
using PeakItemInsight.Core;
using PeakItemInsight.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace PeakItemInsight.UI;

// Independent canvas: never mutate preview fonts, shared materials or HUD objects.
internal sealed class PreviewSettingsDriver : MonoBehaviour
{
    private GameObject? _canvas;
    private PreviewSettingsWindow? _window;
    private Button? _entry;
    private int _scene;
    private readonly List<TextMeshProUGUI> _texts = new List<TextMeshProUGUI>();
    private readonly List<Action> _refresh = new List<Action>();

    private void Update()
    {
        var scene = SceneManager.GetActiveScene();
        if (_scene != scene.handle) { Clear(); _scene = scene.handle; }
        var gui = GUIManager.instance;
        var ready = scene.name != "Title" && Character.localCharacter != null && gui != null && !MainCameraMovement.IsSpectating;
        if (!ready) { if (_window != null && _window.isOpen) _window.Close(); if (_canvas != null) _canvas.SetActive(false); if (_entry != null) _entry.gameObject.SetActive(false); return; }
        var fonts = FontFallbackSwapper.instance;
        var font = fonts != null ? fonts.mainBaseFont : gui!.interactNameText?.font;
        // No default-font frame, including for all Chinese setup labels.
        var required = "物品栏上方文字精力条闪烁预览准星手持两者关闭完成设置优先目标没有时显示立即保存";
        if (font == null) return;
        if (Labels.Chinese) foreach (var c in required) if (!font.HasCharacter(c, true, true)) return;
        if (_canvas != null && _entry == null) Clear();
        if (_canvas == null) Build();
        if (gui!.pauseMenuMainPage != null)
        {
            var pauseCanvas = gui.pauseMenuMainPage.GetComponentInParent<Canvas>();
            if (pauseCanvas != null) BindPauseEntry(gui.pauseMenuMainPage.transform, pauseCanvas.rootCanvas.GetComponentsInChildren<Canvas>(true));
        }
        _canvas!.SetActive(true);
        foreach (var text in _texts) text.font = font;
        foreach (var action in _refresh) action();
        _entry!.gameObject.SetActive(GUIManager.InPauseMenu && !_window!.isOpen);
        if (scene.name == "Airport" && PresentationOptions.SetupSeen?.Value == false && !gui!.windowBlockingInput)
        {
            _window!.Open();
            PresentationOptions.SetupSeen.Value = true;
        }
    }

    private void BindPauseEntry(Transform page, Canvas[] pauseCanvases)
    {
        if (_entry!.transform.parent != page) _entry.transform.SetParent(page, false);
        _entry.transform.SetAsLastSibling();
        var native = page.GetComponentInParent<Canvas>();
        if (native == null) return;
        var top = native;
        foreach (var candidate in pauseCanvases)
        {
            if (candidate.gameObject == _entry.gameObject) continue;
            var layer = SortingLayer.GetLayerValueFromID(candidate.sortingLayerID);
            var current = SortingLayer.GetLayerValueFromID(top.sortingLayerID);
            if (layer > current || layer == current && candidate.sortingOrder > top.sortingOrder) top = candidate;
        }
        // Inherit the native pause hierarchy, but sort above its full-screen blocker.
        // Exclude our own entry canvas when calculating native order on later frames.
        var entryCanvas = _entry.GetComponent<Canvas>();
        if (entryCanvas == null) { entryCanvas = _entry.gameObject.AddComponent<Canvas>(); _entry.gameObject.AddComponent<GraphicRaycaster>(); }
        entryCanvas.overrideSorting = true;
        entryCanvas.sortingLayerID = top.sortingLayerID;
        entryCanvas.sortingOrder = top.sortingOrder + 1;
        var panelCanvas = _canvas!.GetComponent<Canvas>();
        panelCanvas.sortingLayerID = top.sortingLayerID;
        panelCanvas.sortingOrder = top.sortingOrder + 2;
    }

    private void Build()
    {
        _canvas = new GameObject("PeakItemInsight.Settings", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = _canvas.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200;
        var scaler = _canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
        var root = Rect("Window", _canvas.transform, Vector2.zero, new Vector2(1920,1080));
        root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.sizeDelta = Vector2.zero;
        root.gameObject.AddComponent<Image>().color = new Color(0,0,0,.5f);
        var panel = Rect("Panel", root, Vector2.zero, new Vector2(900,400));
        panel.gameObject.AddComponent<Image>().color = new Color(.08f,.14f,.16f,.98f);
        _window = root.gameObject.AddComponent<PreviewSettingsWindow>();
        Label(panel, new Vector2(0,145), new Vector2(820,50), () => Labels.Chinese ? "Item Insight 设置" : "Item Insight settings", 32);
        Row(panel, 55, true); Row(panel, -40, false);
        Label(panel, new Vector2(0,-110), new Vector2(840,44), () => Labels.Chinese ? "两者：优先准星目标，没有目标时显示手持。设置立即保存。" : "Both: aimed item first, otherwise held item. Changes save immediately.", 22);
        var done = Button(panel, new Vector2(0,-165), new Vector2(160,48), () => Labels.Chinese ? "完成" : "Done", () => _window.Close());
        _window.First = done;
        _window.Close();
        _entry = Button(_canvas.transform, Vector2.zero, new Vector2(300,60), () => Labels.Chinese ? "Item Insight 设置" : "Item Insight settings", () => _window.Open());
        var entryRect = (RectTransform)_entry.transform; entryRect.anchorMin = entryRect.anchorMax = Vector2.one; entryRect.pivot = Vector2.one; entryRect.anchoredPosition = new Vector2(-24,-24);
    }

    private void Row(Transform parent, float y, bool text)
    {
        Label(parent, new Vector2(-280,y), new Vector2(310,65), () => Labels.Chinese ? (text ? "物品栏上方文字" : "精力条闪烁预览") : (text ? "Text above inventory" : "Stamina bar preview"), 25);
        var modes = new[] { PreviewMode.Hover, PreviewMode.Held, PreviewMode.Both, PreviewMode.Off };
        for (var i=0;i<modes.Length;i++)
        {
            var mode = modes[i];
            var button = Button(parent, new Vector2(-50+i*135,y), new Vector2(125,54),
                () => Labels.Chinese ? mode == PreviewMode.Hover ? "准星" : mode == PreviewMode.Held ? "手持" : mode == PreviewMode.Both ? "两者" : "关闭" : mode.ToString(),
                () => { var option = text ? PresentationOptions.TextSource : PresentationOptions.StaminaSource; if (option != null) option.Value = mode; });
            _refresh.Add(() => button.image.color = (text ? PresentationOptions.TextSource : PresentationOptions.StaminaSource)?.Value == mode ? new Color(.3f,.55f,.2f) : new Color(.2f,.3f,.32f));
        }
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent,false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f,.5f); rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
    }
    private TextMeshProUGUI Label(Transform parent, Vector2 position, Vector2 size, Func<string> value, float fontSize)
    {
        var label = Rect("Label", parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        label.fontSize = fontSize; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; label.richText = false;
        _texts.Add(label); _refresh.Add(() => label.text = value()); return label;
    }
    private Button Button(Transform parent, Vector2 position, Vector2 size, Func<string> label, Action clicked)
    {
        var rect = Rect("Button", parent, position, size); var image = rect.gameObject.AddComponent<Image>(); image.color = new Color(.2f,.3f,.32f);
        var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(() => clicked());
        Label(rect, Vector2.zero, size - new Vector2(8,4), label, 25); return button;
    }
    private void Clear()
    {
        if (_window != null && _window.isOpen) _window.Close();
        if (_entry != null) { _entry.gameObject.SetActive(false); Destroy(_entry.gameObject); }
        if (_canvas != null) Destroy(_canvas);
        if (_probePause != null) Destroy(_probePause);
        _canvas = null; _window = null; _entry = null; _texts.Clear(); _refresh.Clear();
    }
    private void OnDestroy() => Clear();

    internal void VerifyPanel()
    {
        var oldLanguage = PresentationOptions.Language!.Value;
        var font = FontFallbackSwapper.instance.mainBaseFont;
        var material = font.material;
        try
        {
            Build();
            foreach (var language in new[] { "Chinese", "English" })
            {
                PresentationOptions.Language.Value = language;
                foreach (var action in _refresh) action();
                foreach (var label in _texts)
                {
                    label.font = font;
                    foreach (var c in label.text) if (!char.IsWhiteSpace(c) && !font.HasCharacter(c, true, true)) throw new InvalidOperationException("Missing settings glyph: " + c);
                }
                _window!.Open();
                Canvas.ForceUpdateCanvases();
                if (!_window.isOpen || !MenuWindow.AllActiveWindows.Contains(_window) || !_window.blocksPlayerInput || !_window.showCursorWhileOpen)
                    throw new InvalidOperationException("Native menu input registration failed");
                _window.Close();
                if (_window.isOpen || MenuWindow.AllActiveWindows.Contains(_window)) throw new InvalidOperationException("Native menu input cleanup failed");
            }
            if (font.material != material) throw new InvalidOperationException("Shared font material changed");
        }
        finally { PresentationOptions.Language.Value = oldLanguage; Clear(); }
    }

    private GameObject? _probePause;
    private int _probeStep;
    internal bool VerifyPauseClick()
    {
        if (_probeStep == 0)
        {
            Build();
            _probePause = new GameObject("SyntheticPause", typeof(Canvas), typeof(GraphicRaycaster));
            var canvas = _probePause.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 30000;
            var blocker = Rect("PauseBlocker", _probePause.transform, Vector2.zero, Vector2.zero);
            blocker.anchorMin = Vector2.zero; blocker.anchorMax = Vector2.one;
            blocker.gameObject.AddComponent<Image>();
            var page = Rect("MainPage", _probePause.transform, Vector2.zero, Vector2.zero);
            page.anchorMin = Vector2.zero; page.anchorMax = Vector2.one;
            BindPauseEntry(page, new[] { canvas });
            _entry!.gameObject.SetActive(true);
            _probeStep = 1;
            return false; // UI graphics acquire raycast depth after a rendered frame.
        }
        if (_probeStep == 1)
        {
            ClickThroughRaycast(_entry!);
            if (!_window!.isOpen) throw new InvalidOperationException("Pause entry pointer click did not open settings");
            _entry!.gameObject.SetActive(false);
            _probeStep = 2;
            return false;
        }
            ClickThroughRaycast((Button)_window!.First!);
            if (_window.isOpen || !_probePause!.activeSelf) throw new InvalidOperationException("Done did not return to pause menu");
            PeakItemInsight.Diagnostics.SessionTrace.Write("SMOKE_SETTINGS_CLICK_PASS", "pause blocker at order30000; entry raycast+pointer click opens; Done raycast+click closes without hiding pause");
        Clear(); _probePause = null;
        return true;
    }

    private static void ClickThroughRaycast(Button button)
    {
        Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)button.transform;
        var canvas = button.GetComponentInParent<Canvas>();
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        var pointer = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(camera, rect.TransformPoint(rect.rect.center)), button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        if (hits.Count == 0 || hits[0].gameObject != button.gameObject) throw new InvalidOperationException("Settings click blocked by " + (hits.Count == 0 ? "no hit" : hits[0].gameObject.name));
        ExecuteEvents.Execute(hits[0].gameObject, pointer, ExecuteEvents.pointerClickHandler);
    }
}

internal sealed class PreviewSettingsWindow : MenuWindow
{
    private static readonly System.Reflection.MethodInfo OpenNative = HarmonyLib.AccessTools.Method(typeof(MenuWindow), "Open");
    private static readonly System.Reflection.MethodInfo CloseNative = HarmonyLib.AccessTools.Method(typeof(MenuWindow), "Close");
    public void Open() => OpenNative.Invoke(this, null);
    public void Close() => CloseNative.Invoke(this, null);
    internal Selectable? First;
    public override bool openOnStart => false;
    public override bool closeOnPause => true;
    public override bool closeOnUICancel => true;
    public override Selectable objectToSelectOnOpen => First!;
}
