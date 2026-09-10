using PeakItemInsight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

// Only image copies; never enables/resizes the game's hidden extra-bar objects.
internal sealed class ExtraStaminaOverlay
{
    private RectTransform? _root;
    private Image? _extra, _infinite;
    private StaminaBar? _bar;
    internal bool ExtraVisible => _extra != null && _extra.gameObject.activeInHierarchy;
    internal bool InfiniteVisible => _infinite != null && _infinite.gameObject.activeInHierarchy;
    public void Render(StaminaBar? bar, ItemPreview preview, Image? healthy, float elapsed)
    {
        if (bar == null || bar.fullBar == null || !bar.fullBar.gameObject.activeInHierarchy || healthy == null)
        { Hide(); return; }
        if (_bar != bar || _root == null)
        {
            Dispose(); _bar = bar;
            _root = new GameObject("PeakItemInsight.ExtraPreview", typeof(RectTransform), typeof(LayoutElement)).GetComponent<RectTransform>();
            _root.SetParent(bar.fullBar, false); _root.anchorMin = Vector2.zero; _root.anchorMax = Vector2.one;
            _root.offsetMin = _root.offsetMax = Vector2.zero; _root.GetComponent<LayoutElement>().ignoreLayout = true;
            _extra = Make("ExtraGain", _root); _infinite = Make("InfiniteStamina", _root);
        }
        _root.gameObject.SetActive(true);
        var alpha = (PresentationOptions.Animation ? HungerPulseMath.Alpha(elapsed) : .65f) * PresentationOptions.Strength;
        var gain = Mathf.Max(0, preview.ExtraAfter - preview.ExtraBefore);
        var template = HudGhostOverlay.NativeFill(bar.extraBarStamina, null);
        _extra!.gameObject.SetActive(preview.HasExtraStamina && gain > .00001f && template != null);
        if (_extra.gameObject.activeSelf && template != null)
        {
            Style(template, _extra, alpha);
            var full = bar.fullBar;
            var rect = _extra.rectTransform;
            // Native extra fill can be collapsed to zero height at rest; retain the
            // full bar's height and place the owned projection immediately above it.
            var height = Mathf.Max(full.rect.height, template.rectTransform.rect.height);
            var y = full.rect.yMax + height * .5f + 4;
            if (template.gameObject.activeInHierarchy && template.rectTransform.rect.height > 1)
                y = full.InverseTransformPoint(template.rectTransform.TransformPoint(template.rectTransform.rect.center)).y;
            rect.anchoredPosition = new Vector2(Mathf.Clamp01(preview.ExtraBefore) * full.rect.width, y - full.rect.yMin);
            rect.sizeDelta = new Vector2(Mathf.Min(gain, 1 - Mathf.Clamp01(preview.ExtraBefore)) * full.rect.width, height);
        }
        _infinite!.gameObject.SetActive(preview.InfiniteStamina);
        if (preview.InfiniteStamina)
        {
            Style(healthy, _infinite, alpha);
            _infinite.color = Color.Lerp(_infinite.color, new Color(.55f, .9f, 1, alpha), .6f);
            var full = bar.fullBar;
            var pos = full.InverseTransformPoint(healthy.rectTransform.TransformPoint(new Vector3(healthy.rectTransform.rect.xMin, healthy.rectTransform.rect.center.y)));
            _infinite.rectTransform.anchoredPosition = new Vector2(pos.x - full.rect.xMin, pos.y - full.rect.yMin);
            _infinite.rectTransform.sizeDelta = healthy.rectTransform.rect.size;
        }
    }
    private static Image Make(string name, Transform parent)
    {
        var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent, false); image.raycastTarget = false;
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.zero;
        image.rectTransform.pivot = new Vector2(0, .5f); return image;
    }
    private static void Style(Image source, Image target, float alpha)
    {
        target.sprite = source.overrideSprite; target.type = source.type;
        target.fillMethod = source.fillMethod; target.fillOrigin = source.fillOrigin; target.fillClockwise = source.fillClockwise;
        target.fillAmount = 1; target.fillCenter = source.fillCenter;
        target.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        var colour = source.color; colour.a = alpha; target.color = colour;
    }
    public void Hide() { if (_root != null) _root.gameObject.SetActive(false); }
    public void Dispose() { if (_root != null) Object.Destroy(_root.gameObject); _root = null; _bar = null; _extra = _infinite = null; }
}
