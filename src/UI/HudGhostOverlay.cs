using PeakItemInsight.Core;
using UnityEngine;
using UnityEngine.UI;
using PeakItemInsight.Diagnostics;

namespace PeakItemInsight.UI;

internal sealed class HudGhostOverlay : MonoBehaviour
{
    private RectTransform? _source;
    private RectTransform? _segment;
    private Image? _image;
    private ItemPreview? _preview;
    private bool _probe;
    private readonly StatusRecoveryOverlays _recovery = new StatusRecoveryOverlays();
    private readonly StatusIncreaseOverlay _increases = new StatusIncreaseOverlay();
    private readonly ExtraStaminaOverlay _extra = new ExtraStaminaOverlay();
    private readonly System.Collections.Generic.Dictionary<CharacterAfflictions.STATUSTYPE, HungerRecoveryOverlay> _otherRecoveries =
        new System.Collections.Generic.Dictionary<CharacterAfflictions.STATUSTYPE, HungerRecoveryOverlay>();
    private float _pulseStart;
    private float _nextTrace;
    public bool SegmentVisible => _segment != null && _segment.gameObject.activeInHierarchy;
    public float SegmentWidth => SegmentVisible ? _segment!.rect.width : 0f;

    public void RenderProbe(RectTransform source, float fullWidth, float before, float after)
    {
        _probe = true;
        Bind(source);
        RenderSegment(fullWidth * before, fullWidth * after);
    }

    public static HudGhostOverlay Create()
    {
        var root = new GameObject("PeakItemInsight.HudGhostOverlay");
        DontDestroyOnLoad(root);
        return root.AddComponent<HudGhostOverlay>();
    }

    public void Show(ItemPreview preview)
    {
        if (_preview == null) _pulseStart = Time.unscaledTime;
        _preview = preview.Statuses.Count > 0 || preview.HasExtraStamina || preview.InfiniteStamina ? preview : null;
        if (_preview == null)
            HideSegment();
    }

    public void Hide()
    {
        _preview = null;
        HideSegment();
    }

    private void LateUpdate()
    {
        if (_probe) return;
        if (_preview == null)
        {
            HideSegment();
            return;
        }

        try
        {
            RenderRecoveries();
        }
        catch (System.Exception error)
        {
            HideSegment();
            if (Time.unscaledTime >= _nextTrace)
            {
                _nextTrace = Time.unscaledTime + 2;
                SessionTrace.Write("ERROR", $"Recovery pulse: {error}");
            }
        }
    }

    private void RenderRecoveries()
    {
        var bar = GUIManager.instance != null ? GUIManager.instance.bar : null;
        var healthy = bar != null ? NativeFill(bar.staminaBar, null) : null;
        var hunger = FindNativeStatus(bar, CharacterAfflictions.STATUSTYPE.Hunger);
        var injury = FindNativeStatus(bar, CharacterAfflictions.STATUSTYPE.Injury);
        var projected = _increases.Render(bar, _preview!, healthy, Time.unscaledTime - _pulseStart);
        if (projected) _recovery.Hide();
        else _recovery.Render(_preview!, healthy, hunger, injury, Time.unscaledTime - _pulseStart);
        foreach (var type in StatusTypes.Previewable)
        {
            if (type == CharacterAfflictions.STATUSTYPE.Hunger || type == CharacterAfflictions.STATUSTYPE.Injury) continue;
            if (!_otherRecoveries.TryGetValue(type, out var overlay))
                _otherRecoveries[type] = overlay = new HungerRecoveryOverlay(type.ToString());
            if (projected) overlay.Hide();
            else RenderRecovery(overlay, bar, healthy, type);
        }
        _extra.Render(bar, _preview!, healthy, Time.unscaledTime - _pulseStart);
        if (Time.unscaledTime >= _nextTrace)
        {
            _nextTrace = Time.unscaledTime + 2;
            if (projected)
                SessionTrace.Write("HUD_PROJECTED_ROW", $"item={_preview!.ItemId} source={_preview.Source} hungerWidth={_increases.ProjectedStatusWidth(CharacterAfflictions.STATUSTYPE.Hunger):0.##} poisonWidth={_increases.ProjectedStatusWidth(CharacterAfflictions.STATUSTYPE.Poison):0.##} healthyWidth={_increases.HealthyWidth:0.##} phase={_increases.AddedOpacity:0.##}");
            TraceRecovery(CharacterAfflictions.STATUSTYPE.Hunger, "HUNGER", hunger, healthy, _recovery.Hunger);
            TraceRecovery(CharacterAfflictions.STATUSTYPE.Injury, "INJURY", injury, healthy, _recovery.Injury);
            var poison = StatusRecoveryOverlays.Find(_preview!, CharacterAfflictions.STATUSTYPE.Poison);
            if (poison.HasValue)
                SessionTrace.Write("POISON_PULSE", $"item={_preview!.ItemId} source={_preview.Source} before={poison.Value.Before:0.####} after={poison.Value.After:0.####} visible={_increases.Visible} segments={_increases.AddedCount} visibleWidth={_increases.VisibleAddedWidth:0.##} opacity={_increases.AddedOpacity:0.##}");
        }
    }

    private void RenderRecovery(HungerRecoveryOverlay overlay, StaminaBar? bar, Image? healthy, CharacterAfflictions.STATUSTYPE type)
    {
        var delta = StatusRecoveryOverlays.Find(_preview!, type);
        var source = FindNativeStatus(bar, type);
        if (delta.HasValue && source != null && healthy != null)
            overlay.Render(source, healthy, delta.Value.Before, delta.Value.After, Time.unscaledTime - _pulseStart);
        else overlay.Hide();
    }

    private static Image? FindNativeStatus(StaminaBar? bar, CharacterAfflictions.STATUSTYPE type)
    {
        if (type == CharacterAfflictions.STATUSTYPE.Petrify && bar != null && bar.petrifyAffliction != null)
            return NativeFill(bar.petrifyAffliction.rtf, bar.petrifyAffliction.icon);
        if (bar != null && bar.afflictions != null)
            foreach (var badge in bar.afflictions)
                if (badge != null && !badge.isPetrify && badge.afflictionType == type)
                    return NativeFill(badge.rtf, badge.icon);
        return null;
    }

    private void TraceRecovery(CharacterAfflictions.STATUSTYPE type, string label, Image? source, Image? healthy, HungerRecoveryOverlay overlay)
    {
        var delta = StatusRecoveryOverlays.Find(_preview!, type);
        if (delta == null || HungerPulseMath.Fraction(delta.Value.Before, delta.Value.After) <= 0) return;
        if (source == null || healthy == null)
        {
            SessionTrace.Write(label + "_PULSE_UNBOUND", $"statusImage={source != null} healthyImage={healthy != null}");
            return;
        }
        SessionTrace.Write(label + "_PULSE", $"source={_preview!.Source} target={_preview.TargetInstanceId} item={_preview.ItemId} before={delta.Value.Before} after={delta.Value.After} fraction={HungerPulseMath.Fraction(delta.Value.Before, delta.Value.After)} nativeWidth={source.rectTransform.rect.width} overlayWidth={overlay.Width} visible={overlay.Visible} alpha={overlay.Opacity}");
    }

    internal static Image? NativeFill(RectTransform? rect, Image? icon)
    {
        if (rect == null) return null;
        var direct = rect.GetComponent<Image>();
        if (direct != null && direct != icon) return direct;
        foreach (var candidate in rect.GetComponentsInChildren<Image>(true))
            if (candidate != icon && (icon == null || !candidate.transform.IsChildOf(icon.transform))
                && !IsPreviewChild(candidate.transform))
                return candidate;
        return null;
    }

    private static bool IsPreviewChild(Transform node)
    {
        for (var parent = node; parent != null; parent = parent.parent)
            if (parent.name.StartsWith("PeakItemInsight")) return true;
        return false;
    }

    private void RenderSegment(float currentWidth, float projectedWidth)
    {
        var deltaWidth = Mathf.Abs(projectedWidth - currentWidth);
        if (deltaWidth < 0.5f)
        {
            HideSegment();
            return;
        }

        var beginsAt = Mathf.Min(currentWidth, projectedWidth);
        _segment!.anchorMin = _segment.anchorMax = _source!.anchorMin;
        _segment.pivot = new Vector2(0f, _source.pivot.y);
        _segment.anchoredPosition = new Vector2(_source.anchoredPosition.x - _source.rect.width * _source.pivot.x + beginsAt, _source.anchoredPosition.y);
        _segment.sizeDelta = new Vector2(deltaWidth, _source.rect.height);
        _image!.color = projectedWidth > currentWidth
            ? new Color(0.35f, 1f, 0.55f, 0.42f)
            : new Color(1f, 0.34f, 0.25f, 0.48f);
        _segment.gameObject.SetActive(true);
    }

    private void Bind(RectTransform source)
    {
        if (_source == source && _segment != null)
            return;

        if (_segment != null)
            Destroy(_segment.gameObject);

        _source = source;
        var go = new GameObject("PeakItemInsight.GhostStamina", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(source.parent, false);
        _segment = go.GetComponent<RectTransform>();
        _image = go.GetComponent<Image>();
        _image.raycastTarget = false;
        go.transform.SetSiblingIndex(source.GetSiblingIndex() + 1);
        go.SetActive(false);
    }

    private void HideSegment()
    {
        if (_segment != null)
            _segment.gameObject.SetActive(false);
        _recovery.Hide();
        _increases.Hide();
        _extra.Hide();
        foreach (var overlay in _otherRecoveries.Values) overlay.Hide();
    }

    private void OnDestroy()
    {
        if (_segment != null) Destroy(_segment.gameObject);
        _recovery.Dispose();
        _increases.Dispose();
        _extra.Dispose();
        foreach (var overlay in _otherRecoveries.Values) overlay.Dispose();
    }
}
