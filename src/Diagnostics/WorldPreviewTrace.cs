using System;
using System.Linq;
using PeakItemInsight.Core;
using UnityEngine;

namespace PeakItemInsight.Diagnostics;

// Only observes state. Never invokes item actions or changes a character.
internal static class WorldPreviewTrace
{
    private static readonly bool Enabled = Array.Exists(Environment.GetCommandLineArgs(), a => a == "-insightWorldTest");
    private static float _nextSample;
    private static float? _hunger;
    private static float _poison;
    private static float _extra;
    private static string _lastHeld = "none";

    public static void Preview(Item item, ItemPreview preview, int? worldId)
    {
        if (!Enabled) return;
        var character = Character.localCharacter;
        SessionTrace.Write("WORLD_PREDICTION", $"frame={Time.frameCount} world={worldId} item={item.itemID} name={item.name} total={character.refs.afflictions.statusSum:R} max={character.GetMaxStamina():R} locked={character.statusesLocked} statuses=[{string.Join(";", preview.Statuses.Select(s => $"{s.Type}:{s.Before:R}->{s.After:R}"))}] extra={preview.ExtraBefore:R}->{preview.ExtraAfter:R} warnings=[{string.Join(";", preview.Warnings)}]");
        foreach (var a in item.GetComponentsInChildren<ItemAction>(true))
        {
            var amount = a is Action_ModifyStatus modify ? $"{modify.statusType}:{modify.changeAmount:R}" : a is Action_RestoreHunger hunger ? $"Hunger:-{hunger.restorationAmount:R}" : a is Action_GiveExtraStamina extra ? $"Extra:{extra.amount:R}" : "custom";
            SessionTrace.Write("WORLD_ACTION", $"item={item.itemID} type={a.GetType().Name} enabled={a.enabled} activeSelf={a.gameObject.activeSelf} pressed={a.OnPressed} held={a.OnHeld} cast={a.OnCastFinished} consumed={a.OnConsumed} amount={amount}");
        }
    }

    public static void Tick()
    {
        if (!Enabled || Time.unscaledTime < _nextSample) return;
        _nextSample = Time.unscaledTime + .1f;
        var c = Character.localCharacter;
        if (c == null) { _hunger = null; return; }
        var h = c.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Hunger);
        var p = c.refs.afflictions.GetCurrentStatus(CharacterAfflictions.STATUSTYPE.Poison);
        var held = c.data.currentItem != null ? $"{c.data.currentItem.itemID}:{c.data.currentItem.name}:consuming={c.data.currentItem.consuming}" : "none";
        if (!_hunger.HasValue || Math.Abs(h - _hunger.Value) > .005f || Math.Abs(p - _poison) > .005f || Math.Abs(c.data.extraStamina - _extra) > .005f || held != _lastHeld)
            SessionTrace.Write("WORLD_OBSERVED", $"frame={Time.frameCount} hunger={h:R} poison={p:R} extra={c.data.extraStamina:R} held={held} previousHeld={_lastHeld}");
        _hunger = h; _poison = p; _extra = c.data.extraStamina; _lastHeld = held;
    }

    public static void Hud(ItemPreview preview, float current, float projected, float drawn, bool visible)
    {
        if (!Enabled) return;
        var bar = GUIManager.instance.bar;
        SessionTrace.Write("WORLD_HUD", $"frame={Time.frameCount} item={preview.ItemId} full={bar.fullBar.sizeDelta.x:R} offset={bar.staminaBarOffset:R} nativeWidth={bar.maxStaminaBar.sizeDelta.x:R} expectedCurrent={current:R} projected={projected:R} drawnWidth={drawn:R} visible={visible} sourceAnchor={bar.maxStaminaBar.anchorMin} sourcePivot={bar.maxStaminaBar.pivot}");
    }
}
