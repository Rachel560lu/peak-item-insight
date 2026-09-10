using PeakItemInsight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

// Only our child UI is changed. No cloning a gameplay/BarAffliction component,
// no changes to the native segment's colour, width, activity, or layout.
internal sealed class HungerRecoveryOverlay
{
    private readonly string _name;
    public HungerRecoveryOverlay(string name = "Hunger") { _name = name; }
    private Image? _source;
    private RectTransform? _root;
    private RectTransform? _clip;
    private RectTransform? _fill;
    private Image? _maskImage;
    private Image? _green;
    public bool Visible => _root != null && _root.gameObject.activeInHierarchy;
    public float Width => Visible && _clip != null ? _clip.rect.width : 0;
    public float Opacity => _green != null ? _green.color.a : 0;
    internal RectTransform? OverlayRect => _root;
    internal RectTransform? ClipRect => _clip;

    public void Render(Image source, Image healthy, float before, float after, float elapsed)
    {
        var fraction = HungerPulseMath.Fraction(before, after);
        if (fraction <= 0 || !source.isActiveAndEnabled)
        { Hide(); return; }
        Bind(source);
        CopyStyle(source, _maskImage!);
        _maskImage!.color = Color.white;
        CopyStyle(healthy, _green!);
        var colour = healthy.color;
        colour.a = (PresentationOptions.Animation ? HungerPulseMath.Alpha(elapsed) : .65f) * PresentationOptions.Strength;
        _green!.color = colour;

        // Use the real segment as our coordinate system: follows native layout,
        // HUD scaling, pivots and animation without accumulating pixel offsets.
        var fromLeft = source.rectTransform.InverseTransformPoint(
            healthy.rectTransform.TransformPoint(healthy.rectTransform.rect.center)).x
            <= source.rectTransform.rect.center.x;
        _clip!.anchorMin = new Vector2(fromLeft ? 0 : 1 - fraction, 0);
        _clip.anchorMax = new Vector2(fromLeft ? fraction : 1, 1);
        _clip.offsetMin = _clip.offsetMax = Vector2.zero;
        // Keep the original-sized green texture and crop it, never stretch the
        // texture into a tiny recovery sliver. The outer mask preserves shape.
        _fill!.anchorMin = new Vector2(fromLeft ? 0 : 1, 0);
        _fill.anchorMax = new Vector2(fromLeft ? 0 : 1, 1);
        _fill.pivot = new Vector2(fromLeft ? 0 : 1, .5f);
        _fill.anchoredPosition = Vector2.zero;
        _fill.sizeDelta = new Vector2(source.rectTransform.rect.width, 0);
        _root!.gameObject.SetActive(true);
    }

    private void Bind(Image source)
    {
        if (_source == source && _root != null) return;
        Dispose();
        _source = source;
        _root = Rect("PeakItemInsight." + _name + "Pulse", source.transform);
        _maskImage = _root.gameObject.AddComponent<Image>();
        _maskImage.raycastTarget = false;
        _root.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        _clip = Rect("RecoveryFraction", _root);
        _clip.gameObject.AddComponent<RectMask2D>();
        _fill = Rect("HealthyFill", _clip);
        _green = _fill.gameObject.AddComponent<Image>();
        _green.raycastTarget = false;
        _root.gameObject.SetActive(false);
    }

    private static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
        go.GetComponent<LayoutElement>().ignoreLayout = true;
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static void CopyStyle(Image source, Image destination)
    {
        destination.sprite = source.overrideSprite;
        destination.type = source.type;
        destination.fillMethod = source.fillMethod;
        destination.fillOrigin = source.fillOrigin;
        destination.fillClockwise = source.fillClockwise;
        destination.fillAmount = source.fillAmount;
        destination.fillCenter = source.fillCenter;
        destination.pixelsPerUnitMultiplier = source.pixelsPerUnitMultiplier;
        // Use default UI material so Unity can apply our nested stencil mask;
        // never modify a shared native material.
    }

    public void Hide() { if (_root != null) _root.gameObject.SetActive(false); }
    public void Dispose()
    {
        Hide();
        if (_root != null) Object.Destroy(_root.gameObject);
        _source = null;
        _root = _clip = _fill = null;
        _maskImage = _green = null;
    }
}
