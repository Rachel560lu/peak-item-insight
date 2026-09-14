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
    private readonly List<Button> _styleButtons = new List<Button>();
    private Slider? _transparencySlider;
    private int _scene;
    private readonly List<TextMeshProUGUI> _texts = new List<TextMeshProUGUI>();
    private readonly List<Action> _refresh = new List<Action>();

    private TMP_FontAsset? _settingsFont;
    private string _settingsLanguage = "";
    private Transform? _pausePage;
    private bool _wasPaused;
    private float _nextRefresh;

    private void Update()
    {
        var scene = SceneManager.GetActiveScene();
        if (_scene != scene.handle) { Clear(); _scene = scene.handle; }
        var gui = GUIManager.instance;
        var ready = scene.name != "Title" && Character.localCharacter != null && gui != null && !MainCameraMovement.IsSpectating;
        if (!ready) { if (_window != null && _window.isOpen) _window.Close(); if (_canvas != null) _canvas.SetActive(false); if (_entry != null) _entry.gameObject.SetActive(false); return; }
        var paused = GUIManager.InPauseMenu;
        var setup = scene.name == "Airport" && PresentationOptions.SetupSeen?.Value == false && !gui!.windowBlockingInput;
        if (_canvas != null && _entry == null) Clear();
        // The closed settings UI does no glyph checks, hierarchy scans or label updates.
        if (_canvas != null && !paused && !_window!.isOpen && !setup)
        {
            if (_entry!.gameObject.activeSelf) _entry.gameObject.SetActive(false);
            _canvas.SetActive(false);
            _wasPaused = false;
            return;
        }
        var fonts = FontFallbackSwapper.instance;
        var font = fonts != null ? fonts.mainBaseFont : gui!.interactNameText?.font;
        if (font == null) return;
        var language = Labels.Language;
        var fontChanged = _settingsFont != font || _settingsLanguage != language;
        if (fontChanged)
        {
            const string required = "物品栏上方文字精力条闪烁预览准星手持两者关闭完成设置优先目标没有时显示立即保存";
            foreach (var c in Labels.Chinese ? required : Labels.RequiredGlyphs)
                if (!font.HasCharacter(c, true, true)) return;
            _settingsFont = font; _settingsLanguage = language;
        }
        if (_canvas == null) { Build(); fontChanged = true; }
        var page = gui!.pauseMenuMainPage != null ? gui.pauseMenuMainPage.transform : null;
        if (page != null && (_pausePage != page || paused && !_wasPaused))
        {
            var pauseCanvas = page.GetComponentInParent<Canvas>();
            if (pauseCanvas != null)
            {
                BindPauseEntry(page, pauseCanvas.rootCanvas.GetComponentsInChildren<Canvas>(true));
                _pausePage = page;
            }
        }
        _canvas!.SetActive(paused || _window!.isOpen || setup);
        if (fontChanged) foreach (var text in _texts) text.font = font;
        if (fontChanged || paused && !_wasPaused || _window!.isOpen && Time.unscaledTime >= _nextRefresh || setup)
        {
            foreach (var action in _refresh) action();
            _nextRefresh = Time.unscaledTime + .1f;
        }
        _entry!.gameObject.SetActive(paused && !_window!.isOpen);
        _wasPaused = paused;
        if (setup)
        {
            _window!.Open();
            PresentationOptions.SetupSeen!.Value = true;
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
        var panel = Rect("Panel", root, Vector2.zero, new Vector2(900,600));
        panel.gameObject.AddComponent<Image>().color = new Color(.08f,.14f,.16f,.98f);
        _window = root.gameObject.AddComponent<PreviewSettingsWindow>();
        Label(panel, new Vector2(0,245), new Vector2(820,50), () => Labels.Text("Item Insight 设置", "Item Insight settings"), 32);
        StyleRow(panel, 175);
        TransparencyRow(panel, 95);
        Row(panel, 15, true); Row(panel, -75, false);
        Label(panel, new Vector2(0,-215), new Vector2(840,44), () => Labels.Text("两者：优先准星目标，没有目标时显示手持。设置立即保存。", "Both: aimed item first, otherwise held item. Changes save immediately."), 22);
        var done = Button(panel, new Vector2(0,-265), new Vector2(160,48), () => Labels.Text("完成", "Done"), () => _window.Close());
        _window.First = done;
        _window.Close();
        _entry = Button(_canvas.transform, Vector2.zero, new Vector2(300,60), () => Labels.Text("Item Insight 设置", "Item Insight settings"), () => _window.Open());
        var entryRect = (RectTransform)_entry.transform; entryRect.anchorMin = entryRect.anchorMax = Vector2.one; entryRect.pivot = Vector2.one; entryRect.anchoredPosition = new Vector2(-24,-24);
    }

    private void TransparencyRow(Transform parent, float y)
    {
        Label(parent, new Vector2(-280,y), new Vector2(310,65),
            () => Labels.Text("详细背景透明度", "Detailed background transparency"), 25);
        var root = Rect("TransparencySlider", parent, new Vector2(60,y), new Vector2(320,44));
        var hitArea = root.gameObject.AddComponent<Image>();
        hitArea.color = new Color(0,0,0,0);
        var track = Rect("Track",root,Vector2.zero,new Vector2(320,8));
        track.gameObject.AddComponent<Image>().color = new Color(.28f,.36f,.36f);
        var handleArea = Rect("HandleArea",root,Vector2.zero,new Vector2(300,44));
        var handle = Rect("Handle",handleArea,Vector2.zero,new Vector2(22,32));
        var handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = new Color(.82f,.92f,.72f);
        _transparencySlider = root.gameObject.AddComponent<Slider>();
        _transparencySlider.targetGraphic = handleImage;
        _transparencySlider.handleRect = handle;
        _transparencySlider.minValue = 0; _transparencySlider.maxValue = 100; _transparencySlider.wholeNumbers = true;
        _transparencySlider.direction = Slider.Direction.LeftToRight;
        _transparencySlider.onValueChanged.AddListener(value => {
            if (PresentationOptions.DetailedBackgroundOpacity != null)
                PresentationOptions.DetailedBackgroundOpacity.Value = 1 - value / 100f;
        });
        _refresh.Add(() => _transparencySlider.SetValueWithoutNotify(TransparencyPercent()));
        Label(parent,new Vector2(320,y),new Vector2(150,44),() => TransparencyPercent() + "%",25);
    }

    private static int TransparencyPercent()
        => Mathf.RoundToInt((1-Mathf.Clamp01(PresentationOptions.DetailedBackgroundOpacity?.Value ?? .92f))*100);

    private void StyleRow(Transform parent, float y)
    {
        Label(parent, new Vector2(-280,y), new Vector2(310,65), () => Labels.Text("物品简介样式", "Description style"), 25);
        var styles = new[] { DescriptionStyle.Minimal, DescriptionStyle.Detailed };
        for (var i = 0; i < styles.Length; i++)
        {
            var style = styles[i];
            var button = Button(parent, new Vector2(20+i*260,y), new Vector2(240,54),
                () => style == DescriptionStyle.Minimal ? Labels.Text("极简", "Minimal") : Labels.Text("详细", "Detailed"),
                () => { if (PresentationOptions.DescriptionStyle != null) PresentationOptions.DescriptionStyle.Value = style; });
            _styleButtons.Add(button);
            _refresh.Add(() => button.image.color = (PresentationOptions.DescriptionStyle?.Value ?? DescriptionStyle.Minimal) == style
                ? new Color(.3f,.55f,.2f) : new Color(.2f,.3f,.32f));
        }
    }

    private void Row(Transform parent, float y, bool text)
    {
        Label(parent, new Vector2(-280,y), new Vector2(310,65), () => text ? Labels.Text("物品栏上方文字", "Text above inventory") : Labels.Text("精力条闪烁预览", "Stamina bar preview"), 25);
        var modes = new[] { PreviewMode.Hover, PreviewMode.Held, PreviewMode.Both, PreviewMode.Off };
        for (var i=0;i<modes.Length;i++)
        {
            var mode = modes[i];
            var button = Button(parent, new Vector2(-50+i*135,y), new Vector2(125,54),
                () => Labels.Text(mode == PreviewMode.Hover ? "准星" : mode == PreviewMode.Held ? "手持" : mode == PreviewMode.Both ? "两者" : "关闭", mode.ToString()),
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
        label.fontSize = fontSize; label.enableAutoSizing = true; label.fontSizeMin = 16; label.fontSizeMax = fontSize; label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false; label.richText = false;
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
        _canvas = null; _window = null; _entry = null; _texts.Clear(); _refresh.Clear(); _styleButtons.Clear(); _transparencySlider = null;
        _settingsFont = null; _settingsLanguage = ""; _pausePage = null; _wasPaused = false; _nextRefresh = 0;
    }
    private void OnDestroy() => Clear();

    internal void VerifyPanel()
    {
        var oldLanguage = PresentationOptions.Language!.Value;
        var oldStyle = PresentationOptions.DescriptionStyle!.Value;
        var oldOpacity = PresentationOptions.DetailedBackgroundOpacity!.Value;
        var font = FontFallbackSwapper.instance.mainBaseFont;
        var material = font.material;
        try
        {
            Build();
            foreach (var language in new[] { "Chinese", "English", "Turkish", "Spanish" })
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
                EventSystem.current.SetSelectedGameObject(_transparencySlider!.gameObject);
                _window.Close();
                if (EventSystem.current.currentSelectedGameObject != null) throw new InvalidOperationException("Closed settings kept slider input focus");
                if (_window.isOpen || MenuWindow.AllActiveWindows.Contains(_window)) throw new InvalidOperationException("Native menu input cleanup failed");
                EventSystem.current.SetSelectedGameObject(_entry!.gameObject);
                _window.Close();
                if (EventSystem.current.currentSelectedGameObject != _entry.gameObject) throw new InvalidOperationException("Settings cleared unrelated UI focus");
                EventSystem.current.SetSelectedGameObject(null);
            }
            var oldStamina = PresentationOptions.StaminaSource!.Value;
            _styleButtons[1].onClick.Invoke();
            if (PresentationOptions.DescriptionStyle!.Value != DescriptionStyle.Detailed) throw new InvalidOperationException("Detailed style button failed");
            _styleButtons[0].onClick.Invoke();
            if (PresentationOptions.DescriptionStyle.Value != DescriptionStyle.Minimal || PresentationOptions.StaminaSource.Value != oldStamina)
                throw new InvalidOperationException("Style switch changed stamina source");
            _transparencySlider!.value = 65;
            if (Mathf.Abs(PresentationOptions.DetailedBackgroundOpacity!.Value-.35f) > .0001f)
                throw new InvalidOperationException("Transparency slider did not save opacity");
            _transparencySlider.value = 0;
            if (PresentationOptions.DetailedBackgroundOpacity.Value != 1) throw new InvalidOperationException("Opaque endpoint failed");
            _transparencySlider.value = 100;
            if (PresentationOptions.DetailedBackgroundOpacity.Value != 0) throw new InvalidOperationException("Transparent endpoint failed");
            if (PresentationOptions.StaminaSource!.Value != oldStamina) throw new InvalidOperationException("Transparency changed stamina source");
            if (font.material != material) throw new InvalidOperationException("Shared font material changed");
        }
        finally { PresentationOptions.Language.Value = oldLanguage; PresentationOptions.DescriptionStyle!.Value = oldStyle; PresentationOptions.DetailedBackgroundOpacity!.Value = oldOpacity; Clear(); }
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
    protected override void OnClose()
    {
        // Release only our UI selection; native pause/menu selections belong to the game.
        var events = EventSystem.current;
        var selected = events != null ? events.currentSelectedGameObject : null;
        if (selected != null && selected.transform.IsChildOf(transform))
            events!.SetSelectedGameObject(null);
    }
    public override bool openOnStart => false;
    public override bool closeOnPause => true;
    public override bool closeOnUICancel => true;
    public override Selectable objectToSelectOnOpen => First!;
}
