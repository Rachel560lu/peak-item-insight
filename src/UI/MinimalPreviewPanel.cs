using System.Collections.Generic;
using System.Linq;
using PeakItemInsight.Core;
using PeakItemInsight.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

internal sealed class MinimalPreviewPanel : MonoBehaviour
{
    private sealed class Row
    {
        public RectTransform Rect = null!;
        public TextMeshProUGUI Text = null!;
        public Image Icon = null!;
    }
    private RectTransform _panel = null!;
    private CanvasGroup _group = null!;
    private readonly List<Row> _rows = new List<Row>();
    private TMP_FontAsset? _font;
    private float _retryFont;
    private int _screenWidth, _screenHeight;
    internal bool IsVisible => _panel != null && _panel.gameObject.activeInHierarchy;
    internal string LastTargetName { get; private set; } = "";
    internal string BodyText => string.Join("\n", _rows.Where(r => r.Rect.gameObject.activeSelf).Select(r => r.Text.text));
    internal int VisibleRows => _rows.Count(r => r.Rect.gameObject.activeSelf);

    public static MinimalPreviewPanel Create()
    {
        var root = new GameObject("PeakItemInsight.MinimalCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        DontDestroyOnLoad(root);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 75;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
        var view = root.AddComponent<MinimalPreviewPanel>();
        view._group = root.GetComponent<CanvasGroup>();
        view._group.blocksRaycasts = false; view._group.interactable = false;
        var panel = new GameObject("MinimalEffects", typeof(RectTransform));
        view._panel = panel.GetComponent<RectTransform>();
        view._panel.SetParent(root.transform, false);
        view._panel.anchorMin = view._panel.anchorMax = new Vector2(.5f, .5f);
        view._panel.pivot = new Vector2(.5f, 0);
        view.Hide(); return view;
    }

    public void Show(ItemPreview preview)
    {
        LastTargetName = preview.Name;
        var lines = MinimalRows.Build(preview, Labels.Chinese);
        if (lines.Count == 0) { Hide(); return; }
        if (preview.Source == PreviewSource.Hover) lines.Add(new MinimalRow(preview.Name));
        AdoptFont();
        for (var i = 0; i < lines.Count; i++)
        {
            if (i == _rows.Count) _rows.Add(CreateRow());
            var row = _rows[i]; var data = lines[i];
            row.Rect.gameObject.SetActive(true);
            var native = NativeIcon(data);
            row.Icon.sprite = native != null ? native.sprite : null;
            row.Icon.color = native != null ? new Color(native.color.r, native.color.g, native.color.b, 1) : Color.white;
            row.Icon.gameObject.SetActive(row.Icon.sprite != null);
            var color = native != null ? row.Icon.color : data.Lightning ? new Color(.65f, 1, .15f) : Color.white;
            // If an icon is unavailable, name the effect rather than leave an
            // unexplained number or a missing glyph. Never reuse fading material.
            var fallback = row.Icon.sprite == null ? data.Status.HasValue ? " " + Labels.Status(data.Status.Value)
                : data.Lightning ? " " + Labels.ExtraStamina : "" : "";
            row.Text.text = (data.Text + fallback).Replace("<", "＜").Replace(">", "＞");
            row.Text.color = color;
            if (_font != null) row.Text.font = _font;
        }
        for (var i = lines.Count; i < _rows.Count; i++) _rows[i].Rect.gameObject.SetActive(false);
        _panel.gameObject.SetActive(true);
        LayoutRows(); Position();
    }

    private Row CreateRow()
    {
        var rect = new GameObject("Effect", typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(_panel, false); rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1);
        var text = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        text.transform.SetParent(rect, false); text.raycastTarget = false; text.fontSize = 22;
        text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.Normal;
        var icon = new GameObject("NativeStatusIcon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        icon.transform.SetParent(rect, false); icon.raycastTarget = false; icon.preserveAspect = true;
        icon.rectTransform.sizeDelta = new Vector2(26, 26);
        return new Row { Rect = rect, Text = text, Icon = icon };
    }

    private void LayoutRows()
    {
        _screenWidth = Screen.width; _screenHeight = Screen.height;
        var area = ((RectTransform)transform).rect;
        var scale = Mathf.Clamp(PresentationOptions.MinimalScale?.Value ?? 1, .5f, 2);
        _panel.localScale = Vector3.one * scale;
        var width = Mathf.Min(380, (area.width - 32) / scale);
        float total = 0;
        for (var size = 22; size >= 14; size--)
        {
            total = 0;
            foreach (var row in _rows.Where(r => r.Rect.gameObject.activeSelf))
            {
                row.Text.fontSize = size;
                var iconWidth = row.Icon.gameObject.activeSelf ? 34 : 0;
                var preferred = row.Text.GetPreferredValues(row.Text.text, width - iconWidth, 0);
                var textWidth = Mathf.Min(width - iconWidth, preferred.x + 2);
                var height = Mathf.Max(28, preferred.y + 4);
                row.Rect.sizeDelta = new Vector2(width, height);
                row.Rect.anchoredPosition = new Vector2(0, -total);
                row.Text.rectTransform.sizeDelta = new Vector2(textWidth, height);
                row.Text.rectTransform.anchoredPosition = new Vector2(-iconWidth / 2, 0);
                row.Icon.rectTransform.anchoredPosition = new Vector2(textWidth / 2 + 4, 0);
                total += height + 3;
            }
            if (total * scale < area.height - 160) break;
        }
        _panel.sizeDelta = new Vector2(width, total);
    }

    private static Image? NativeIcon(MinimalRow data)
    {
        var bar = GUIManager.instance != null ? GUIManager.instance.bar : null;
        if (bar == null) return null;
        if (data.Lightning) return bar.extraStaminaIcon;
        if (!data.Status.HasValue) return null;
        if (data.Status == CharacterAfflictions.STATUSTYPE.Petrify) return bar.petrifyAffliction != null ? bar.petrifyAffliction.icon : null;
        return bar.afflictions?.FirstOrDefault(a => a != null && !a.isPetrify && a.afflictionType == data.Status)?.icon;
    }

    private void AdoptFont()
    {
        if (Time.unscaledTime < _retryFont && _font != null) return;
        _retryFont = Time.unscaledTime + 1;
        var gui = GUIManager.instance;
        var candidate = gui != null ? gui.interactNameText : null;
        if (candidate == null || candidate.font == null || Labels.Chinese && !candidate.font.HasCharacter('饥'))
            candidate = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().FirstOrDefault(t =>
                t != null && t.font != null && t.gameObject.scene.IsValid() && !t.transform.IsChildOf(transform) &&
                (!Labels.Chinese || t.font.HasCharacter('饥')));
        if (candidate != null) _font = candidate.font;
    }

    private Rect Bounds(RectTransform rect)
    {
        var corners = new Vector3[4]; rect.GetWorldCorners(corners);
        var canvas = rect.GetComponentInParent<Canvas>();
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        var min = new Vector2(float.MaxValue, float.MaxValue); var max = new Vector2(float.MinValue, float.MinValue);
        foreach (var corner in corners)
        {
            var screen = RectTransformUtility.WorldToScreenPoint(camera, corner);
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out var local);
            min = Vector2.Min(min, local); max = Vector2.Max(max, local);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private void LateUpdate()
    {
        if (!IsVisible) return;
        if (_screenWidth != Screen.width || _screenHeight != Screen.height) LayoutRows();
        Position();
    }
    private void Position()
    {
        var gui = GUIManager.instance;
        _group.alpha = gui != null && gui.hudCanvasGroup != null ? gui.hudCanvasGroup.alpha : 1;
        var area = ((RectTransform)transform).rect;
        var slots = gui != null && gui.items != null ? gui.items.Where(i => i != null && i.gameObject.activeInHierarchy).ToList() : new List<InventoryItemUI>();
        if (gui != null && gui.temporaryItem != null && gui.temporaryItem.gameObject.activeInHierarchy) slots.Insert(0, gui.temporaryItem);
        var slot = slots.FirstOrDefault(i => i.nameText != null && i.nameText.isActiveAndEnabled && !string.IsNullOrWhiteSpace(i.nameText.text))
            ?? slots.FirstOrDefault(i => i.selectedSlotIcon != null && i.selectedSlotIcon.gameObject.activeInHierarchy && i.selectedSlotIcon.enabled)
            ?? slots.FirstOrDefault();
        var x = area.xMax - 260; var y = area.yMin + 190;
        if (slot != null)
        {
            var bounds = Bounds(slot.rectTransform != null ? slot.rectTransform : (RectTransform)slot.transform);
            x = bounds.center.x; y = bounds.yMax + 12;
            if (slot.nameText != null && slot.nameText.isActiveAndEnabled && !string.IsNullOrWhiteSpace(slot.nameText.text))
                y = Mathf.Max(y, Bounds(slot.nameText.rectTransform).yMax + 12);
        }
        x += PresentationOptions.MinimalOffsetX?.Value ?? 0; y += PresentationOptions.MinimalOffsetY?.Value ?? 0;
        var width = _panel.rect.width * _panel.localScale.x; var height = _panel.rect.height * _panel.localScale.y;
        x = Mathf.Clamp(x, area.xMin + width / 2 + 16, area.xMax - width / 2 - 16);
        y = Mathf.Clamp(y, area.yMin + 16, Mathf.Max(area.yMin + 16, area.yMax - height - 16));
        var obstacles = new List<Rect>();
        foreach (var item in slots)
        {
            obstacles.Add(Bounds(item.rectTransform != null ? item.rectTransform : (RectTransform)item.transform));
            if (item.nameText != null && item.nameText.isActiveAndEnabled && !string.IsNullOrWhiteSpace(item.nameText.text))
                obstacles.Add(Bounds(item.nameText.rectTransform));
        }
        if (gui != null)
            foreach (var prompt in new[] { gui.itemPromptMain, gui.itemPromptSecondary, gui.itemPromptDrop, gui.itemPromptThrow, gui.itemPromptScroll, gui.interactNameText, gui.interactPromptText })
            {
                if (prompt != null && prompt.isActiveAndEnabled && !string.IsNullOrWhiteSpace(prompt.text)) obstacles.Add(Bounds(prompt.rectTransform));
            }
        var candidates = new List<Vector2> { new Vector2(x, y) };
        foreach (var obstacle in obstacles)
        {
            candidates.Add(new Vector2(obstacle.xMin - 12 - width / 2, y));
            candidates.Add(new Vector2(obstacle.xMax + 12 + width / 2, y));
            candidates.Add(new Vector2(x, obstacle.yMax + 12));
        }
        foreach (var candidate in candidates)
        {
            var proposed = new Rect(candidate.x - width / 2, candidate.y, width, height);
            if (proposed.xMin < area.xMin + 16 || proposed.xMax > area.xMax - 16 || proposed.yMin < area.yMin + 16 || proposed.yMax > area.yMax - 16) continue;
            if (obstacles.Any(o => proposed.Overlaps(o))) continue;
            x = candidate.x; y = candidate.y; break;
        }
        _panel.anchoredPosition = new Vector2(x, y);
    }
    public void Hide() { if (_panel != null) _panel.gameObject.SetActive(false); }
}
