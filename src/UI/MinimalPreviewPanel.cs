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
        public PreviewDeltaArrow Arrow = null!;
    }
    private RectTransform _panel = null!;
    private CanvasGroup _group = null!;
    private readonly List<Row> _rows = new List<Row>();
    private TMP_FontAsset? _font;
    private PreviewSource _source;
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
        _source = preview.Source;
        LastTargetName = preview.Name;
        var lines = MinimalRows.Build(preview, Labels.Chinese);
        if (lines.Count == 0) { Hide(); return; }
        if (preview.Source == PreviewSource.Hover) lines.Add(new MinimalRow(preview.Name));
        // Never display a default-font frame while native resources are loading.
        if (!AdoptFont()) { Hide(); return; }
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
            row.Text.text = (data.DisplayText + fallback).Replace("<", "＜").Replace(">", "＞");
            row.Text.color = color;
            row.Arrow.color = color;
            row.Arrow.gameObject.SetActive(data.Direction != 0);
            row.Arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, data.Direction < 0 ? 180 : 0);
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
        text.transform.SetParent(rect, false); text.raycastTarget = false; text.fontSize = 28;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = new Vector2(.5f, 1);
        text.richText = false;
        text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.Normal;
        var icon = new GameObject("NativeStatusIcon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        icon.transform.SetParent(rect, false); icon.raycastTarget = false; icon.preserveAspect = true;
        icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(.5f, 1);
        icon.rectTransform.sizeDelta = new Vector2(30, 30);
        var arrow = new GameObject("DeltaDirection", typeof(RectTransform), typeof(PreviewDeltaArrow)).GetComponent<PreviewDeltaArrow>();
        arrow.transform.SetParent(rect, false); arrow.raycastTarget = false;
        arrow.rectTransform.anchorMin = arrow.rectTransform.anchorMax = new Vector2(.5f, 1);
        arrow.rectTransform.sizeDelta = new Vector2(20, 18);
        return new Row { Rect = rect, Text = text, Icon = icon, Arrow = arrow };
    }

    private void LayoutRows()
    {
        _screenWidth = Screen.width; _screenHeight = Screen.height;
        var area = ((RectTransform)transform).rect;
        var scale = Mathf.Clamp(PresentationOptions.MinimalScale?.Value ?? 1, .5f, 2);
        _panel.localScale = Vector3.one * scale;
        var maxWidth = Mathf.Min(360, (area.width - 32) / scale);
        float total = 0;
        float width = 0;
        for (var size = 28; size >= 18; size--)
        {
            total = 0; width = 0;
            foreach (var row in _rows.Where(r => r.Rect.gameObject.activeSelf))
            {
                row.Text.fontSize = size;
                var adornments = (row.Icon.gameObject.activeSelf ? 38 : 0) + (row.Arrow.gameObject.activeSelf ? 28 : 0);
                width = Mathf.Max(width, Mathf.Min(maxWidth, row.Text.GetPreferredValues(row.Text.text).x + adornments + 4));
            }
            foreach (var row in _rows.Where(r => r.Rect.gameObject.activeSelf))
            {
                var iconWidth = row.Icon.gameObject.activeSelf ? 38 : 0;
                var arrowWidth = row.Arrow.gameObject.activeSelf ? 28 : 0;
                var textWidth = Mathf.Max(1, width - iconWidth - arrowWidth);
                var preferred = row.Text.GetPreferredValues(row.Text.text, textWidth, 0);
                var height = Mathf.Max(34, preferred.y + 4);
                row.Rect.sizeDelta = new Vector2(width, height);
                row.Rect.anchoredPosition = new Vector2(0, -total);
                row.Text.rectTransform.sizeDelta = new Vector2(textWidth, height);
                row.Text.alignment = iconWidth > 0 ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.Center;
                row.Text.rectTransform.anchoredPosition = new Vector2((arrowWidth - iconWidth) / 2, -height / 2);
                row.Icon.rectTransform.anchoredPosition = new Vector2(width / 2 - 15, -height / 2);
                row.Arrow.rectTransform.anchoredPosition = new Vector2(width / 2 - iconWidth - Mathf.Min(textWidth, preferred.x) - 16, -height / 2);
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

    private bool AdoptFont()
    {
        var fonts = FontFallbackSwapper.instance;
        _font = fonts != null ? fonts.mainBaseFont : GUIManager.instance != null ? GUIManager.instance.interactNameText?.font : null;
        // The native base font delegates Chinese to the game's fallback chain.
        // Query that chain; never replace it or select arbitrary scene UI fonts.
        return _font != null && (!Labels.Chinese || _font.HasCharacter('饥', true, true));
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
        var slot = ResolveAnchor(gui);
        var x = area.xMax - 260; var y = area.yMin + 190;
        if (slot != null)
        {
            var bounds = Bounds(slot.rectTransform != null ? slot.rectTransform : (RectTransform)slot.transform);
            x = bounds.center.x; y = bounds.yMax + 12;
            if (slot.nameText != null && slot.nameText.isActiveAndEnabled && !string.IsNullOrWhiteSpace(slot.nameText.text))
                y = Mathf.Max(y, TextTop(slot.nameText) + 12);
        }
        x += PresentationOptions.MinimalOffsetX?.Value ?? 0; y += PresentationOptions.MinimalOffsetY?.Value ?? 0;
        var width = _panel.rect.width * _panel.localScale.x; var height = _panel.rect.height * _panel.localScale.y;
        // Keep the inventory association stable. Invisible padding on prompt
        // rectangles must not push the effects sideways onto the player's hands.
        _panel.anchoredPosition = ClampToScreen(area, new Vector2(x, y), new Vector2(width, height));
    }

    private InventoryItemUI? ResolveAnchor(GUIManager? gui)
    {
        if (gui == null) return null;
        var items = Character.observedCharacter != null ? Character.observedCharacter.refs?.items : null;
        int? heldSlot = items != null && items.currentSelectedSlot.IsSome ? items.currentSelectedSlot.Value : (int?)null;
        var inventory = Player.localPlayer != null ? Player.localPlayer.itemSlots : null;
        bool Empty(int index) => inventory != null && index < inventory.Length && inventory[index] != null && inventory[index].IsEmpty();
        var temporary = gui.temporaryItem != null && gui.temporaryItem.gameObject.activeInHierarchy;
        var index = PreviewAnchor.Select(_source, heldSlot, Empty(0), Empty(1), Empty(2), temporary);
        if (index == 3) return gui.backpack; // Also anchors correctly when no backpack is equipped.
        if (index == 4) return gui.temporaryItem;
        if (index >= 0 && gui.items != null && index < gui.items.Length) return gui.items[index];
        // Loading/transition fallback for held UI only; hover never falls back to an occupied highlight.
        return _source == PreviewSource.Held && gui.items != null
            ? gui.items.FirstOrDefault(i => i != null && i.nameText != null && i.nameText.isActiveAndEnabled) : null;
    }

    internal static Vector2 ClampToScreen(Rect area, Vector2 anchor, Vector2 size) => new Vector2(
        Mathf.Clamp(anchor.x, area.xMin + size.x / 2 + 16, area.xMax - size.x / 2 - 16),
        Mathf.Clamp(anchor.y, area.yMin + 16, Mathf.Max(area.yMin + 16, area.yMax - size.y - 16)));

    private float TextTop(TextMeshProUGUI text)
    {
        text.ForceMeshUpdate();
        var world = text.transform.TransformPoint(text.textBounds.max);
        var canvas = text.GetComponentInParent<Canvas>();
        var camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        var screen = RectTransformUtility.WorldToScreenPoint(camera, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out var local);
        return local.y;
    }
    public void Hide() { _source = PreviewSource.None; if (_panel != null) _panel.gameObject.SetActive(false); }
}
