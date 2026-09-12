using System.Collections.Generic;
using PeakItemInsight.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PeakItemInsight.UI;

// Independent native visuals: never run HUD scripts or change player state.
internal sealed class ExtraStaminaOverlay
{
    private RectTransform? _root, _fill, _gain, _outline, _icon;
    private Image? _infinite;
    private CanvasGroup? _gainPulse;
    private StaminaBar? _bar;
    private readonly Dictionary<Graphic, bool> _suppressed = new Dictionary<Graphic, bool>();
    private readonly Vector3[] _corners = new Vector3[4];
    internal bool ExtraVisible => _gain != null && _gain.gameObject.activeInHierarchy;
    internal bool InfiniteVisible => _infinite != null && _infinite.gameObject.activeInHierarchy;
    internal bool TrackVisible => _outline != null && _outline.gameObject.activeInHierarchy;
    internal bool IconVisible => _icon != null && _icon.gameObject.activeInHierarchy;
    internal Rect ExtraBounds => ExtraVisible ? Bounds(_gain!, _bar!.fullBar) : Rect.zero;
    internal Rect RowBounds => ExtraVisible ? RowBoundsIn(_bar!.fullBar) : Rect.zero;
    internal Image? OutlineImage => _outline != null ? _outline.GetComponentInChildren<Image>(true) : null;
    internal Image? IconImage => _icon != null ? _icon.GetComponent<Image>() : null;

    public void Render(StaminaBar? bar, ItemPreview preview, Image? healthy, float elapsed)
    {
        if (bar == null || bar.fullBar == null || !bar.fullBar.gameObject.activeInHierarchy || healthy == null)
        { Hide(); return; }
        if (_bar != bar) { Dispose(); _bar = bar; }
        var alpha = Mathf.Clamp01((PresentationOptions.Animation ? HungerPulseMath.Alpha(elapsed) : .65f) * PresentationOptions.Strength);
        var before = Mathf.Max(0, preview.ExtraBefore);
        var after = Mathf.Max(0, preview.ExtraAfter);
        var gain = Mathf.Max(0, after - before);
        if (preview.HasExtraStamina && gain > .00001f && Bind(bar))
        {
            // Native Update opens this wrapper to 45x45, then collapses it on
            // expiry. Set the settled dimensions on our independent copy only.
            _root!.sizeDelta = new Vector2(45, 45);
            _root.anchoredPosition = bar.extraBar.anchoredPosition;
            _root.gameObject.SetActive(true);
            Reveal(_outline!); Reveal(_icon!); Reveal(_fill!); Reveal(_gain!);
            var scale = bar.fullBar.sizeDelta.x;
            if (scale <= 0) scale = bar.fullBar.rect.width;
            _fill!.anchorMin = _gain!.anchorMin = bar.extraBarStamina.anchorMin;
            _fill.anchorMax = _gain.anchorMax = bar.extraBarStamina.anchorMax;
            _fill.pivot = _gain.pivot = bar.extraBarStamina.pivot;
            _fill.anchoredPosition = bar.extraBarStamina.anchoredPosition;
            _outline!.sizeDelta = new Vector2(Mathf.Max(20, (bar.petrifyAffliction != null &&
                bar.petrifyAffliction.gameObject.activeInHierarchy ? scale : after * scale) + 12), _outline.sizeDelta.y);
            _fill!.sizeDelta = new Vector2(before * scale, bar.extraBarStamina.sizeDelta.y);
            _fill.gameObject.SetActive(before * scale > 6.1f);
            _gain!.sizeDelta = new Vector2(gain * scale, bar.extraBarStamina.sizeDelta.y);
            _gain.anchoredPosition = _fill.anchoredPosition + new Vector2(before * scale +
                (gain * scale - before * scale) * _fill.pivot.x, 0);
            _gainPulse!.alpha = alpha;
            foreach (var image in _gain.GetComponentsInChildren<Image>(true)) image.fillAmount = 1;
            foreach (var image in _fill.GetComponentsInChildren<Image>(true)) image.fillAmount = 1;
            PositionRow(bar, healthy, before > 0);
            if (before > 0) Suppress(bar);
            else RestoreNative();
        }
        else HideExtra();

        if (_infinite == null)
        {
            var rect = new GameObject("PeakItemInsight.InfinitePreview", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(bar.fullBar, false);
            rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = new Vector2(0, .5f);
            _infinite = NativeVisualCopy.CopyImage(healthy, rect);
        }
        _infinite.gameObject.SetActive(preview.InfiniteStamina);
        if (preview.InfiniteStamina)
        {
            var main = Bounds(healthy.rectTransform, bar.fullBar);
            _infinite.rectTransform.anchoredPosition = new Vector2(main.xMin - bar.fullBar.rect.xMin, main.center.y - bar.fullBar.rect.yMin);
            _infinite.rectTransform.sizeDelta = main.size;
            _infinite.color = Color.Lerp(healthy.color, new Color(.55f, .9f, 1, 1), .6f) * new Color(1, 1, 1, alpha);
        }
    }

    private bool Bind(StaminaBar bar)
    {
        if (_root != null) return true;
        if (bar.extraBar == null || bar.extraBarStamina == null || bar.extraBarOutline == null || bar.extraStaminaIcon == null) return false;
        _root = NativeVisualCopy.Create(bar.extraBar, bar.extraBar.parent, bar.petrifyAffliction != null ? bar.petrifyAffliction.rtf : null);
        _root.name = "PeakItemInsight.NativeExtraPreview";
        _root.gameObject.SetActive(false);
        _fill = NativeVisualCopy.Find(bar.extraBar, _root, bar.extraBarStamina);
        _outline = NativeVisualCopy.Find(bar.extraBar, _root, bar.extraBarOutline);
        _icon = NativeVisualCopy.Find(bar.extraBar, _root, bar.extraStaminaIcon.rectTransform);
        if (_fill == null || _outline == null || _icon == null)
        { Object.Destroy(_root.gameObject); _root = null; return false; }
        _gain = NativeVisualCopy.Create(bar.extraBarStamina, _fill.parent);
        _gain.name = "ExtraGain";
        _gainPulse = _gain.gameObject.AddComponent<CanvasGroup>();
        _root.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        return true;
    }

    private void Suppress(StaminaBar bar)
    {
        foreach (var graphic in bar.extraBar.GetComponentsInChildren<Graphic>(true))
        {
            if (bar.petrifyAffliction != null && graphic.transform.IsChildOf(bar.petrifyAffliction.transform)) continue;
            if (!_suppressed.ContainsKey(graphic)) _suppressed.Add(graphic, graphic.enabled);
            graphic.enabled = false;
        }
    }
    private void HideExtra()
    {
        if (_root != null) _root.gameObject.SetActive(false);
        RestoreNative();
    }
    private void RestoreNative()
    {
        foreach (var pair in _suppressed)
            if (pair.Key != null && !pair.Key.enabled) pair.Key.enabled = pair.Value;
        _suppressed.Clear();
    }
    private void PositionRow(StaminaBar bar, Image healthy, bool hasExtra)
    {
        // Measure visible frames in one coordinate space. The native wrapper
        // can still be collapsed/tweening even when the real value has changed.
        var relativeTo = (RectTransform)_root!.parent;
        var main = Bounds(bar.staminaBarOutline != null ? bar.staminaBarOutline : healthy.rectTransform, relativeTo);
        var row = RowBoundsIn(relativeTo);
        var gap = main.height * .2f;
        var offset = hasExtra
            ? Mathf.Min(0, main.yMin - gap - row.yMax)
            : main.yMax + gap - row.yMin;
        _root.position += relativeTo.TransformVector(new Vector3(0, offset, 0));
    }
    private Rect RowBoundsIn(RectTransform relativeTo)
    {
        var frame = Bounds(_outline!, relativeTo);
        var icon = Bounds(_icon!, relativeTo);
        var fill = Bounds(_gain!, relativeTo);
        return Rect.MinMaxRect(Mathf.Min(frame.xMin, icon.xMin, fill.xMin),
            Mathf.Min(frame.yMin, icon.yMin, fill.yMin),
            Mathf.Max(frame.xMax, icon.xMax, fill.xMax),
            Mathf.Max(frame.yMax, icon.yMax, fill.yMax));
    }
    private void Reveal(RectTransform node)
    {
        for (var current = (Transform)node; current != null && current != _root; current = current.parent)
            current.gameObject.SetActive(true);
    }
    private Rect Bounds(RectTransform source, RectTransform relativeTo)
    {
        source.GetWorldCorners(_corners);
        var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (var corner in _corners)
        { var p = (Vector2)relativeTo.InverseTransformPoint(corner); min = Vector2.Min(min, p); max = Vector2.Max(max, p); }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
    public void Hide() { HideExtra(); if (_infinite != null) _infinite.gameObject.SetActive(false); }
    public void Dispose()
    {
        Hide();
        if (_root != null) Object.Destroy(_root.gameObject);
        if (_infinite != null) Object.Destroy(_infinite.gameObject);
        _root = _fill = _gain = _outline = _icon = null; _infinite = null; _gainPulse = null; _bar = null;
    }
}
