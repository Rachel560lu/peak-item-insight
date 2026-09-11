using System;
using System.Linq;
using System.Text;
using PeakItemInsight.Core;
using PeakItemInsight.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

internal sealed class PreviewPanel : MonoBehaviour
{
    private RectTransform _panel = null!;
    private TextMeshProUGUI _title = null!, _body = null!, _context = null!;
    private RawImage _icon = null!;
    private RoundedCard _background = null!;
    private float _fontRetry;
    internal string TitleText => _title.text + " " + _context.text;
    internal string BodyText => _body.text;
    public bool IsVisible => _panel != null && _panel.gameObject.activeInHierarchy && _panel.rect.width > 0 && _panel.rect.height > 0;

    public static PreviewPanel Create()
    {
        var root = new GameObject("PeakItemInsight.Canvas", typeof(Canvas), typeof(CanvasScaler));
        DontDestroyOnLoad(root);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 75;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = .5f;
        var component = root.AddComponent<PreviewPanel>();
        component.Build(root.transform);
        component.Hide();
        return component;
    }

    private void Build(Transform root)
    {
        var go = new GameObject("Panel", typeof(RectTransform), typeof(RoundedCard),
            typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(root, false);
        _panel = go.GetComponent<RectTransform>();
        _panel.anchorMin = _panel.anchorMax = new Vector2(.5f, .5f);
        _panel.pivot = new Vector2(0, 1);
        _panel.sizeDelta = new Vector2(365, 0);
        _background = go.GetComponent<RoundedCard>();
        _background.raycastTarget = false;
        var layout = go.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 16, 16);
        layout.spacing = 10;
        layout.childControlHeight = layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(_panel, false);
        var row = header.GetComponent<HorizontalLayoutGroup>();
        row.spacing = 12; row.childForceExpandWidth = false; row.childControlHeight = true; row.childForceExpandHeight = false;
        row.childAlignment = TextAnchor.MiddleLeft;
        var iconObject = new GameObject("ItemIcon", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
        iconObject.transform.SetParent(header.transform, false);
        _icon = iconObject.GetComponent<RawImage>(); _icon.raycastTarget = false;
        var iconLayout = iconObject.GetComponent<LayoutElement>();
        iconLayout.preferredWidth = iconLayout.minWidth = 42; iconLayout.preferredHeight = 42;
        _title = MakeText(header.transform, "Title", 24, FontStyles.Bold, new Color(.98f,.97f,.88f));
        _title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        _context = MakeText(_panel, "Category", 16, FontStyles.Bold, Color.white);
        _body = MakeText(_panel, "Content", 18, FontStyles.Normal, new Color(.96f,.96f,.91f));
    }

    public void Show(ItemPreview preview, float scale, float offsetX, float offsetY)
    {
        _title.text = Escape(preview.Name);
        _context.text = "<color=#91D45E>" + Escape(preview.Category) + "</color>";
        _icon.texture = preview.Icon;
        _icon.gameObject.SetActive(preview.Icon != null);
        _body.text = FormatBody(preview);
        AdoptGameFont();
        _background.color = new Color(.13f, .16f, .16f, PresentationOptions.Opacity?.Value ?? .88f);
        _panel.localScale = Vector3.one * Mathf.Clamp(scale, .5f, 2);
        // Reserve the centre's native pickup label; retain user configurable offsets.
        _panel.anchoredPosition = new Vector2(offsetX + 150, offsetY + 120);
        _panel.gameObject.SetActive(true);
        _body.fontSize = 18;
        LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
        var available = ((RectTransform)transform).rect.height - 32;
        while (_panel.rect.height * _panel.localScale.y > available && _body.fontSize > 14)
        { _body.fontSize -= 1; LayoutRebuilder.ForceRebuildLayoutImmediate(_panel); }
        ClampToScreen();
    }

    internal static string FormatBody(ItemPreview preview)
    {
        var body = new StringBuilder();
        if (preview.IsFood || preview.PoisonRisk == RiskLevel.Present || preview.SporeRisk == RiskLevel.Present)
            body.AppendLine(Risk(preview.PoisonRisk, false) + " · " + Risk(preview.SporeRisk, true));
        if (preview.Description.Length > 0) body.AppendLine(Escape(preview.Description));
        foreach (var instruction in preview.Instructions) body.AppendLine(Escape(instruction));
        if (preview.Effects.Count > 0)
        {
            foreach (var effect in preview.Effects)
            {
                if (Math.Abs(effect.Amount) < .00001f) continue;
                if (effect.Clears) { body.AppendLine("<color=#9CDC6A>" + Labels.Text("清除 ", "Clear ") + Labels.Status(effect.Type) + "</color>"); continue; }
                var total = effect.Amount * (effect.Duration > 0 ? effect.Duration : 1);
                var signed = (total > 0 ? "+" : "") + (total * 100).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
                var suffix = effect.Duration > 0 ? Labels.Text($"（累计，{effect.Duration:0.#} 秒）", $" (total over {effect.Duration:0.#} s)") : "";
                if (effect.Delay > 0) suffix += Labels.Text($"（{effect.Delay:0.#} 秒后）", $" (after {effect.Delay:0.#} s)");
                body.Append("<color=").Append(effect.Amount > 0 ? "#EBA078" : "#9CDC6A").Append(">")
                    .Append(signed).Append(" ").Append(Labels.Status(effect.Type)).Append(Escape(suffix)).AppendLine("</color>");
            }
        }
        else
            foreach (var status in preview.Statuses)
                body.AppendLine(Labels.Status(status.Type) + $" {status.Before * 100:0.#} → {status.After * 100:0.#}");
        if (PresentationOptions.Details?.Value == true && preview.Effects.Count > 0)
            foreach (var status in preview.Statuses)
                body.AppendLine("<size=15>" + Labels.Status(status.Type) + $" {status.Before * 100:0.#} → {status.After * 100:0.#}</size>");
        foreach (var resource in preview.Resources)
            body.Append("<color=#D1D5D2>").Append(Escape(resource.Label)).Append(": ").Append(Escape(resource.Value)).AppendLine("</color>");
        foreach (var warning in preview.Warnings.Distinct())
            body.Append("<size=15><color=#E7BA81>").Append(Escape(warning)).AppendLine("</color></size>");
        if (preview.DebugId != null) body.AppendLine("<size=13>" + Escape(preview.DebugId) + "</size>");
        return body.ToString().TrimEnd();
    }

    private static string Risk(RiskLevel risk, bool spores)
    {
        var text = spores ? (risk == RiskLevel.Present ? Labels.Text("含孢子", "Contains spores") :
            risk == RiskLevel.Absent ? Labels.Text("无孢子", "No spores") : Labels.Text("孢子未知", "Spores unknown")) :
            (risk == RiskLevel.Present ? Labels.Text("有毒", "Poisonous") :
            risk == RiskLevel.Absent ? Labels.Text("无毒", "No poison") : Labels.Text("毒性未知", "Poison unknown"));
        return "<color=" + (risk == RiskLevel.Present ? "#E9A1D6" : risk == RiskLevel.Absent ? "#9CDC6A" : "#D8C591") + ">" + text + "</color>";
    }
    internal static string Escape(string value) => value.Replace("<", "＜").Replace(">", "＞");

    private void AdoptGameFont()
    {
        if (Time.unscaledTime < _fontRetry) return;
        _fontRetry = Time.unscaledTime + 1;
        var texts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        var gameText = texts.FirstOrDefault(t => t.font != null && t.gameObject.scene.IsValid() &&
            !t.transform.IsChildOf(transform) && (!Labels.Chinese || t.font.HasCharacter('饥')));
        if (gameText == null) gameText = texts.FirstOrDefault(t => t.font != null && !t.transform.IsChildOf(transform));
        if (gameText == null) return;
        foreach (var text in new[] { _title, _context, _body })
        {
            text.font = gameText.font;
            // Do not reuse a game's fading/glowing shared material.
        }
    }
    private void ClampToScreen()
    {
        var canvasRect = (RectTransform)transform;
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, _panel);
        var area = canvasRect.rect;
        var delta = Vector2.zero;
        if (bounds.max.x > area.xMax - 16) delta.x = area.xMax - 16 - bounds.max.x;
        if (bounds.min.x + delta.x < area.xMin + 16) delta.x += area.xMin + 16 - (bounds.min.x + delta.x);
        if (bounds.min.y < area.yMin + 16) delta.y = area.yMin + 16 - bounds.min.y;
        if (bounds.max.y + delta.y > area.yMax - 16) delta.y += area.yMax - 16 - (bounds.max.y + delta.y);
        _panel.anchoredPosition += delta;
    }
    public void Hide() { if (_panel != null) _panel.gameObject.SetActive(false); }
    private static TextMeshProUGUI MakeText(Transform parent, string name, float size, FontStyles style, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        var text = obj.GetComponent<TextMeshProUGUI>();
        text.fontSize = size; text.fontStyle = style; text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal; text.raycastTarget = false;
        text.lineSpacing = 5;
        return text;
    }
}
