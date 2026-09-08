using System.Text;
using System.Linq;
using PeakItemInsight.Core;
using PeakItemInsight.Providers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

internal sealed class PreviewPanel : MonoBehaviour
{
    private RectTransform _panel = null!;
    private TextMeshProUGUI _title = null!;
    private TextMeshProUGUI _body = null!;
    private Transform _barsRoot = null!;
    internal string TitleText => _title.text;
    public bool IsVisible => _panel != null && _panel.gameObject.activeInHierarchy && _panel.rect.width > 0 && _panel.rect.height > 0;

    public static PreviewPanel Create()
    {
        var root = new GameObject("PeakItemInsight.Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(root);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 75;
        root.GetComponent<GraphicRaycaster>().enabled = false;

        var component = root.AddComponent<PreviewPanel>();
        component.Build(root.transform);
        component.Hide();
        return component;
    }

    private void Build(Transform root)
    {
        var panelObject = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panelObject.transform.SetParent(root, false);
        _panel = panelObject.GetComponent<RectTransform>();
        _panel.anchorMin = _panel.anchorMax = new Vector2(0.5f, 0.5f);
        _panel.pivot = new Vector2(0f, 1f);
        _panel.sizeDelta = new Vector2(330f, 0f);
        panelObject.GetComponent<Image>().color = new Color(0.035f, 0.045f, 0.055f, 0.92f);
        var layout = panelObject.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 12, 12);
        layout.spacing = 7f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        panelObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _title = MakeText(_panel, "Title", 21f, FontStyles.Bold, new Color(1f, 0.91f, 0.57f));
        _barsRoot = new GameObject("StatusBars", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).transform;
        _barsRoot.SetParent(_panel, false);
        var barsLayout = _barsRoot.GetComponent<VerticalLayoutGroup>();
        barsLayout.spacing = 5f;
        barsLayout.childControlHeight = true;
        barsLayout.childControlWidth = true;
        barsLayout.childForceExpandHeight = false;
        _barsRoot.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _body = MakeText(_panel, "Body", 14f, FontStyles.Normal, Color.white);
    }

    public void Show(ItemPreview preview, float scale, float offsetX, float offsetY)
    {
        AdoptGameFont();
        _panel.localScale = Vector3.one * scale;
        _panel.anchoredPosition = new Vector2(offsetX, offsetY);
        _title.text = (preview.Source == PreviewSource.None ? "" : $"[{Labels.Source(preview.Source)}] ") + preview.Name + (preview.DebugId == null ? string.Empty : $"  <size=11><color=#9AA4AE>{preview.DebugId}</color></size>");
        RebuildBars(preview);

        var body = new StringBuilder();
        foreach (var resource in preview.Resources)
            body.Append("<color=#AAB5C0>").Append(resource.Label).Append(":</color> ").AppendLine(resource.Value);
        foreach (var instruction in preview.Instructions)
            body.Append("• ").AppendLine(instruction);
        foreach (var warning in preview.Warnings)
            body.Append("<color=#FFB05C>⚠ ").Append(warning).AppendLine("</color>");
        if (preview.Statuses.Count == 0 && preview.Resources.Count == 0 && preview.Instructions.Count == 0)
            body.Append(Labels.NoPreview);
        _body.text = body.ToString().TrimEnd();
        _body.gameObject.SetActive(_body.text.Length > 0);
        _panel.gameObject.SetActive(true);
    }

    private void AdoptGameFont()
    {
        var gameText = GUIManager.instance != null && GUIManager.instance.bar != null
            ? GUIManager.instance.bar.moraleBoostText
            : null;
        if (gameText == null || gameText.font == null)
            gameText = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().FirstOrDefault(t =>
                t != _title && t != _body && t.font != null && t.gameObject.scene.IsValid() && !t.transform.IsChildOf(transform));
        if (gameText == null || gameText.font == null)
            return;

        _title.font = gameText.font;
        _title.fontSharedMaterial = gameText.fontSharedMaterial;
        _body.font = gameText.font;
        _body.fontSharedMaterial = gameText.fontSharedMaterial;
    }

    public void Hide()
    {
        if (_panel != null)
            _panel.gameObject.SetActive(false);
    }

    private void RebuildBars(ItemPreview preview)
    {
        for (var i = _barsRoot.childCount - 1; i >= 0; i--)
            Destroy(_barsRoot.GetChild(i).gameObject);

        foreach (var status in preview.Statuses)
            CreateBar(status);
        _barsRoot.gameObject.SetActive(preview.Statuses.Count > 0);
    }

    private void CreateBar(StatusDelta status)
    {
        var row = new GameObject(Labels.Status(status.Type), typeof(RectTransform), typeof(LayoutElement));
        row.transform.SetParent(_barsRoot, false);
        row.GetComponent<LayoutElement>().preferredHeight = 31f;

        var label = MakeText(row.transform, "Label", 12f, FontStyles.Normal, new Color(0.85f, 0.88f, 0.9f));
        if (_title.font != null) label.font = _title.font;
        var labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.pivot = new Vector2(0.5f, 1f);
        labelRect.offsetMin = new Vector2(0f, -16f);
        labelRect.offsetMax = Vector2.zero;
        label.text = $"{Labels.Status(status.Type)}   {status.Before * 100f:0} → {status.After * 100f:0}";

        var track = MakeImage(row.transform, "Track", new Color(0.15f, 0.17f, 0.19f, 1f));
        var trackRect = track.rectTransform;
        trackRect.anchorMin = new Vector2(0f, 0f);
        trackRect.anchorMax = new Vector2(1f, 0f);
        trackRect.pivot = new Vector2(0f, 0f);
        trackRect.offsetMin = new Vector2(0f, 1f);
        trackRect.offsetMax = new Vector2(0f, 9f);

        var before = MakeImage(track.transform, "Current", new Color(0.72f, 0.72f, 0.72f, 0.62f));
        AnchorFill(before.rectTransform, Mathf.Clamp01(status.Before));
        var afterColor = status.After > status.Before ? new Color(0.95f, 0.36f, 0.24f, 0.9f) : new Color(0.28f, 0.9f, 0.57f, 0.9f);
        var delta = MakeImage(track.transform, "Delta", afterColor);
        AnchorRange(delta.rectTransform, Mathf.Clamp01(Mathf.Min(status.Before, status.After)), Mathf.Clamp01(Mathf.Max(status.Before, status.After)));
    }

    private static void AnchorFill(RectTransform rect, float amount)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(amount, 1f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void AnchorRange(RectTransform rect, float from, float to)
    {
        rect.anchorMin = new Vector2(from, 0f);
        rect.anchorMax = new Vector2(to, 1f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, float size, FontStyles style, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        var text = obj.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        return text;
    }

    private static Image MakeImage(Transform parent, string name, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        var image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }
}
