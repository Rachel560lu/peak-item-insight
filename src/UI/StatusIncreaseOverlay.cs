using System;
using System.Collections.Generic;
using System.Linq;
using PeakItemInsight.Core;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PeakItemInsight.UI;

// Owns image-only copies and temporary CanvasGroups; an existing group's alpha
// is scoped to preview visibility, then restored (never destroy an existing group).
// Native badge geometry, colours, activity, components and player data stay intact.
internal sealed class StatusIncreaseOverlay
{
    private sealed class Badge
    {
        internal BarAffliction Native = null!;
        internal RectTransform Current = null!, Added = null!;
        internal Image? Fill, AddedFill;
        internal CanvasGroup Suppress = null!;
        internal CanvasGroup AddedPulse = null!, CurrentPulse = null!;
        internal bool OwnGroup, Hidden;
        internal float OriginalAlpha, LastAlpha;
    }
    private RectTransform? _root;
    private StaminaBar? _bar;
    private Image? _healthy;
    private CanvasGroup? _healthyPulse;
    private readonly List<Badge> _badges = new List<Badge>();
    internal bool Visible => _root != null && _root.gameObject.activeInHierarchy;
    internal int AddedCount { get; private set; }
    internal float HealthyWidth => _healthy != null && _healthy.gameObject.activeInHierarchy ? _healthy.rectTransform.rect.width : 0;
    internal float ProjectedStatusWidth(CharacterAfflictions.STATUSTYPE type) => _badges.Where(b => TypeOf(b.Native) == type)
        .Sum(b => (b.Current.gameObject.activeInHierarchy ? b.Current.rect.width : 0) + (b.Added.gameObject.activeInHierarchy ? b.Added.rect.width : 0));
    internal float AddedOpacity => _badges.Count > 0 ? _badges.Max(b => b.AddedPulse.alpha) : 0;
    internal float VisibleAddedWidth => _badges.Where(b => b.Added.gameObject.activeInHierarchy &&
        b.Added.rect.height > .01f && b.AddedPulse.alpha > .01f &&
        b.AddedFill != null && b.AddedFill.enabled && b.AddedFill.color.a > .01f &&
        b.AddedFill.rectTransform.rect.width > .01f && b.AddedFill.rectTransform.rect.height > .01f &&
        (b.AddedFill.type != Image.Type.Filled || b.AddedFill.fillAmount > .01f))
        .Sum(b => b.Added.rect.width);

    public bool Render(StaminaBar? bar, ItemPreview preview, Image? healthy, float elapsed)
    {
        if (bar == null || bar.fullBar == null || !bar.fullBar.gameObject.activeInHierarchy || bar.afflictions == null ||
            !preview.Statuses.Any(s => Supported(s.Type) && s.After > s.Before + .00001f))
        { Hide(); return false; }
        Bind(bar, healthy);
        if (_root == null || _badges.Count == 0) { Hide(); return false; }
        var widths = new float[_badges.Count];
        var projected = new float[_badges.Count];
        var full = bar.fullBar;
        var right = full.rect.xMax;
        for (var i = 0; i < _badges.Count; i++)
        {
            var native = _badges[i].Native;
            widths[i] = native.gameObject.activeInHierarchy ? Mathf.Max(0, native.rtf.rect.width) : 0;
            projected[i] = widths[i];
            var status = StatusRecoveryOverlays.Find(preview, TypeOf(native));
            if (status.HasValue && Supported(status.Value.Type))
            {
                projected[i] = StatusGhostLayout.ProjectWidth(widths[i], status.Value.Before, status.Value.After,
                    full.sizeDelta.x > 0 ? full.sizeDelta.x : full.rect.width, bar.minAfflictionWidth,
                    native.rtf.rect.width - native.rtf.sizeDelta.x);
            }
            // Respect the actual native row end (including its layout spacing/overflow).
            if (widths[i] > 0 && !native.isPetrify)
            {
                var corners = new Vector3[4]; native.rtf.GetWorldCorners(corners);
                right = Mathf.Max(right, full.InverseTransformPoint(corners[2]).x);
            }
        }
        // Petrify lives in the extra row, not in the main stamina/status row.
        var mainCurrent = (float[])widths.Clone();
        var mainAfter = (float[])projected.Clone();
        for (var i = 0; i < _badges.Count; i++)
            if (_badges[i].Native.isPetrify) mainCurrent[i] = mainAfter[i] = 0;
        var ranges = StatusGhostLayout.BuildProjected(right, mainCurrent, mainAfter);
        var phase = Mathf.Clamp01((PresentationOptions.Animation ? HungerPulseMath.Alpha(elapsed) : .65f) * PresentationOptions.Strength);
        AddedCount = 0;
        _root.gameObject.SetActive(true);
        RenderHealthy(bar, preview, healthy, ranges, phase);
        for (var i = 0; i < _badges.Count; i++)
        {
            var badge = _badges[i];
            var range = ranges[i];
            if (badge.Native.isPetrify)
            {
                var corners = new Vector3[4]; badge.Native.rtf.GetWorldCorners(corners);
                var petrifyRight = full.InverseTransformPoint(corners[2]).x;
                range = StatusGhostLayout.BuildProjected(petrifyRight, new[] { widths[i] }, new[] { projected[i] })[0];
            }
            if (!badge.Hidden) { badge.OriginalAlpha = badge.Suppress.alpha; badge.Hidden = true; }
            // Cross-fade the complete row: at the original phase even existing
            // badges retain their original positions, not permanently shifted ones.
            badge.LastAlpha = badge.OriginalAlpha * (1 - phase);
            badge.Suppress.alpha = badge.LastAlpha;
            badge.CurrentPulse.alpha = badge.OriginalAlpha * phase;
            Position(badge.Current, badge.Native, full, range.CurrentStart, range.CurrentWidth, healthy);
            Position(badge.Added, badge.Native, full, range.AddedStart, range.AddedWidth, healthy);
            badge.AddedPulse.alpha = phase;
            if (badge.Added.gameObject.activeSelf) AddedCount++;
        }
        return true;
    }
    internal static bool Supported(CharacterAfflictions.STATUSTYPE type) => true;
    private static CharacterAfflictions.STATUSTYPE TypeOf(BarAffliction badge) => badge.isPetrify ? CharacterAfflictions.STATUSTYPE.Petrify : badge.afflictionType;

    private void Bind(StaminaBar bar, Image? healthy)
    {
        if (_bar == bar && _root != null && _badges.All(b => b.Native != null && b.Suppress != null)) return;
        Dispose(); _bar = bar;
        _root = new GameObject("PeakItemInsight.ProjectedStatusRow", typeof(RectTransform), typeof(LayoutElement)).GetComponent<RectTransform>();
        _root.SetParent(bar.fullBar, false);
        _root.anchorMin = Vector2.zero; _root.anchorMax = Vector2.one;
        _root.offsetMin = _root.offsetMax = Vector2.zero;
        _root.gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
        // FullBar can be earlier in the hierarchy: an owned override-sorting canvas
        // keeps its copies above the native images without reordering native objects.
        var canvas = _root.gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        var parentCanvas = bar.fullBar.GetComponentInParent<Canvas>();
        canvas.sortingOrder = (parentCanvas != null ? parentCanvas.sortingOrder : 0) + 1;
        if (healthy != null)
        {
            var rect = new GameObject("ProjectedHealthy", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(_root, false);
            _healthy = NativeVisualCopy.CopyImage(healthy, rect);
            _healthyPulse = rect.gameObject.AddComponent<CanvasGroup>();
        }
        foreach (var native in bar.afflictions.Concat(new[] { bar.petrifyAffliction }).Where(b => b != null).Distinct().OrderBy(b => b.rtf.GetSiblingIndex()))
        {
            var badge = new Badge { Native = native };
            badge.Current = CopyImages(native.rtf, _root, true);
            badge.CurrentPulse = badge.Current.gameObject.AddComponent<CanvasGroup>();
            badge.Added = CopyImages(native.rtf, _root, true);
            badge.AddedPulse = badge.Added.gameObject.AddComponent<CanvasGroup>();
            var icon = native.icon == null ? null : FindCopyIcon(native, badge.Current);
            badge.Fill = badge.Current.GetComponentsInChildren<Image>(true).FirstOrDefault(image =>
                image != icon && (icon == null || !image.transform.IsChildOf(icon.transform)));
            var addedIcon = native.icon == null ? null : FindCopyIcon(native, badge.Added);
            badge.AddedFill = badge.Added.GetComponentsInChildren<Image>(true).FirstOrDefault(image =>
                image != addedIcon && (addedIcon == null || !image.transform.IsChildOf(addedIcon.transform)));
            badge.Suppress = native.GetComponent<CanvasGroup>();
            badge.OwnGroup = badge.Suppress == null;
            if (badge.OwnGroup) badge.Suppress = native.gameObject.AddComponent<CanvasGroup>();
            if (badge.Suppress == null) throw new InvalidOperationException("Cannot bind native status visibility.");
            badge.OriginalAlpha = badge.Suppress.alpha;
            _badges.Add(badge);
        }
        _root.gameObject.SetActive(false);
    }
    private static Image? FindCopyIcon(BarAffliction native, RectTransform root)
    {
        var path = new List<string>();
        for (var node = native.icon.transform; node != native.rtf && node != null; node = node.parent) path.Insert(0, node.name);
        return root.Find(string.Join("/", path))?.GetComponent<Image>();
    }
    private static RectTransform CopyImages(RectTransform source, Transform parent, bool ghost)
    {
        var copy = NativeVisualCopy.Create(source, parent);
        if (ghost)
            foreach (var image in copy.GetComponentsInChildren<Image>(true)) image.fillAmount = 1;
        return copy;
    }

    private void RenderHealthy(StaminaBar bar, ItemPreview preview, Image? healthy, GhostRange[] ranges, float phase)
    {
        if (_healthy == null || _healthyPulse == null || healthy == null) return;
        var full = bar.fullBar;
        var corners = new Vector3[4]; healthy.rectTransform.GetWorldCorners(corners);
        var left = full.InverseTransformPoint(corners[0]).x;
        var end = full.rect.xMax;
        for (var i = 0; i < ranges.Length; i++)
            if (!_badges[i].Native.isPetrify && ranges[i].CurrentWidth + ranges[i].AddedWidth > 0)
                end = Mathf.Min(end, ranges[i].AddedStart);
        if (preview.StatusBefore.Count > 0)
        {
            var sum = 0f;
            foreach (var entry in preview.StatusBefore)
            {
                if (entry.Key == CharacterAfflictions.STATUSTYPE.Petrify) continue;
                var change = StatusRecoveryOverlays.Find(preview, entry.Key);
                sum += change.HasValue ? change.Value.After : entry.Value;
            }
            var scale = full.sizeDelta.x > 0 ? full.sizeDelta.x : full.rect.width;
            end = Mathf.Min(end, left + PreviewMath.Capacity(sum) * scale + bar.staminaBarOffset);
        }
        var rect = _healthy.rectTransform;
        rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = new Vector2(0, .5f);
        var centre = full.InverseTransformPoint(healthy.rectTransform.TransformPoint(healthy.rectTransform.rect.center));
        rect.anchoredPosition = new Vector2(left - full.rect.xMin, centre.y - full.rect.yMin);
        rect.sizeDelta = new Vector2(Mathf.Max(0, end - left), Mathf.Abs(full.InverseTransformPoint(corners[1]).y - full.InverseTransformPoint(corners[0]).y));
        _healthy.color = healthy.color; _healthy.fillAmount = 1;
        _healthyPulse.alpha = phase;
        rect.gameObject.SetActive(rect.rect.width > .01f);
    }
    private static void Position(RectTransform copy, BarAffliction native, RectTransform full, float start, float width, Image? healthy)
    {
        copy.anchorMin = copy.anchorMax = Vector2.zero; copy.pivot = new Vector2(0, .5f);
        // A status overflow remains textual, never paints outside the native frame.
        var end = Mathf.Min(full.rect.xMax, start + width);
        start = Mathf.Max(full.rect.xMin, start);
        width = Mathf.Max(0, end - start);
        var height = native.rtf.rect.height;
        var centre = full.InverseTransformPoint(native.rtf.TransformPoint(native.rtf.rect.center));
        // Inactive native slots can have no layout yet. Never inherit zero-height
        // geometry for a brand-new poison preview; use the visible healthy row.
        if (height <= .01f)
        {
            var template = healthy != null ? healthy.rectTransform : full;
            var corners = new Vector3[4]; template.GetWorldCorners(corners);
            height = Mathf.Abs(full.InverseTransformPoint(corners[1]).y - full.InverseTransformPoint(corners[0]).y);
            centre = full.InverseTransformPoint(template.TransformPoint(template.rect.center));
        }
        copy.anchoredPosition = new Vector2(start - full.rect.xMin, centre.y - full.rect.yMin);
        copy.sizeDelta = new Vector2(width, height);
        copy.gameObject.SetActive(width > .01f && height > .01f);
    }
    public void Hide()
    {
        AddedCount = 0;
        if (_root != null) _root.gameObject.SetActive(false);
        foreach (var badge in _badges)
        {
            if (badge.Hidden && badge.Suppress != null && Mathf.Approximately(badge.Suppress.alpha, badge.LastAlpha))
                badge.Suppress.alpha = badge.OriginalAlpha;
            badge.Hidden = false;
        }
    }
    public void Dispose()
    {
        Hide();
        foreach (var badge in _badges)
        { if (badge.OwnGroup && badge.Suppress != null) Object.Destroy(badge.Suppress); }
        _badges.Clear();
        if (_root != null) Object.Destroy(_root.gameObject);
        _root = null; _bar = null; _healthy = null; _healthyPulse = null;
    }
}
