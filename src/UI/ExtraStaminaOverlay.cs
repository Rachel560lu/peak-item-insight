using PeakItemInsight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

// Only image copies; never enables/resizes the game's hidden extra-bar objects.
internal sealed class ExtraStaminaOverlay
{
    private RectTransform? _root;
    private Image? _extra, _infinite;
    private RoundedCard? _track;
    private readonly Vector3[] _corners = new Vector3[4];
    private StaminaBar? _bar;
    internal bool ExtraVisible => _extra != null && _extra.gameObject.activeInHierarchy;
    internal bool InfiniteVisible => _infinite != null && _infinite.gameObject.activeInHierarchy;
    internal Rect ExtraBounds => ExtraVisible ? Bounds(_extra!.rectTransform, _bar!.fullBar) : Rect.zero;
    internal bool TrackVisible => _track != null && _track.gameObject.activeInHierarchy;
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
            _track = new GameObject("ExtraTrack", typeof(RectTransform), typeof(RoundedCard)).GetComponent<RoundedCard>();
            _track.transform.SetParent(_root, false); _track.raycastTarget = false;
            _track.color = new Color(.07f, .12f, .13f, .75f);
            _track.rectTransform.anchorMin = _track.rectTransform.anchorMax = Vector2.zero;
            _track.rectTransform.pivot = new Vector2(0, .5f);
            _extra = Make("ExtraGain", _root); _infinite = Make("InfiniteStamina", _root);
        }
        _root.gameObject.SetActive(true);
        var alpha = (PresentationOptions.Animation ? HungerPulseMath.Alpha(elapsed) : .65f) * PresentationOptions.Strength;
        var before = Mathf.Clamp01(preview.ExtraBefore);
        var after = Mathf.Clamp01(preview.ExtraAfter);
        var gain = Mathf.Max(0, after - before);
        var template = HudGhostOverlay.NativeFill(bar.extraBarStamina, null);
        var full = bar.fullBar;
        var main = Bounds(healthy.rectTransform, full);
        _extra!.gameObject.SetActive(preview.HasExtraStamina && gain > .00001f && main.height > 1 && full.rect.width > 1);
        _track!.gameObject.SetActive(_extra.gameObject.activeSelf);
        if (_extra.gameObject.activeSelf)
        {
            Style(template != null && template.overrideSprite != null ? template : healthy, _extra, alpha);
            // fullBar is a layout container, NOT the visible strip. Its height (and
            // an inactive extraBar's stale height) must never size a preview fill.
            var height = main.height;
            var left = main.xMin;
            var y = main.yMax + height * .5f + 8;
            var nativeRow = false;
            if (template != null && template.gameObject.activeInHierarchy)
            {
                var native = Bounds(template.rectTransform, full);
                if (native.height > 1 && native.height <= main.height * 1.5f)
                { height = native.height; left = native.xMin; y = native.center.y; nativeRow = true; }
            }
            Place(_extra.rectTransform, full, left + before * full.rect.width, y, gain * full.rect.width, height);
            // A stable enclosing track distinguishes bonus capacity from status
            // recovery. Only the added fill pulses; no game-owned UI is modified.
            Place(_track.rectTransform, full, left - 3, y, after * full.rect.width + 6, height + 6);
            _track.color = new Color(.07f, .12f, .13f, nativeRow ? 0 : .75f);
        }
        _infinite!.gameObject.SetActive(preview.InfiniteStamina);
        if (preview.InfiniteStamina)
        {
            Style(healthy, _infinite, alpha);
            _infinite.color = Color.Lerp(_infinite.color, new Color(.55f, .9f, 1, alpha), .6f);
            var pos = full.InverseTransformPoint(healthy.rectTransform.TransformPoint(new Vector3(healthy.rectTransform.rect.xMin, healthy.rectTransform.rect.center.y)));
            _infinite.rectTransform.anchoredPosition = new Vector2(pos.x - full.rect.xMin, pos.y - full.rect.yMin);
            _infinite.rectTransform.sizeDelta = healthy.rectTransform.rect.size;
        }
    }
    private Rect Bounds(RectTransform source, RectTransform relativeTo)
    {
        source.GetWorldCorners(_corners);
        var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        for (var i = 0; i < _corners.Length; i++)
        {
            var p = (Vector2)relativeTo.InverseTransformPoint(_corners[i]);
            min = Vector2.Min(min, p); max = Vector2.Max(max, p);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
    private static void Place(RectTransform target, RectTransform full, float x, float y, float width, float height)
    {
        target.anchoredPosition = new Vector2(x - full.rect.xMin, y - full.rect.yMin);
        target.sizeDelta = new Vector2(width, height);
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
    public void Dispose() { Hide(); if (_root != null) Object.Destroy(_root.gameObject); _root = null; _bar = null; _extra = _infinite = null; _track = null; }
}
