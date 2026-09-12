using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PeakItemInsight.Core;

internal readonly struct MinimalRow
{
    public MinimalRow(string text, CharacterAfflictions.STATUSTYPE? status = null, bool lightning = false, int direction = 0)
    { Text = text; Status = status; Lightning = lightning; Direction = direction; }
    public string Text { get; }
    public CharacterAfflictions.STATUSTYPE? Status { get; }
    public bool Lightning { get; }
    public int Direction { get; }
    public string DisplayText => Direction != 0 && (Text.StartsWith("+") || Text.StartsWith("-")) ? Text.Substring(1) : Text;
}

// Pure presentation of the same snapshot used by the HUD. Never parse card text.
internal static class MinimalRows
{
    private static string Number(float value) => value.ToString("0.#", CultureInfo.InvariantCulture);
    public static List<MinimalRow> Build(ItemPreview preview, bool chinese)
    {
        var rows = new List<MinimalRow>();
        var summary = preview.SummarizeEffects || preview.Effects.Any(e => e.Timed);
        if (summary)
        {
            // Consume the HUD's final snapshot, never recompute or sum raw rates.
            foreach (var status in preview.Statuses)
            {
                var delta = (float)Math.Round((status.After - status.Before) * 100, 1);
                if (delta != 0) rows.Add(new MinimalRow((delta > 0 ? "+" : "") + Number(delta), status.Type, direction: Math.Sign(delta)));
            }
        }
        else
        {
            foreach (var effect in preview.Effects)
            {
                if (!effect.Clears && Math.Abs(effect.Amount) < .00001f) continue;
                var total = effect.Amount * 100;
                var text = effect.Clears ? (chinese ? "清除" : "Clear") : (total > 0 ? "+" : "") + Number(total);
                rows.Add(new MinimalRow(text, effect.Type, direction: effect.Clears ? 0 : Math.Sign(total)));
            }
            if (preview.Effects.Count == 0)
                foreach (var status in preview.Statuses)
                {
                    var delta = (status.After - status.Before) * 100;
                    if (Math.Abs(delta) > .00001f) rows.Add(new MinimalRow((delta > 0 ? "+" : "") + Number(delta), status.Type, direction: Math.Sign(delta)));
                }
        }
        AddRisk(rows, preview, preview.PoisonRisk, CharacterAfflictions.STATUSTYPE.Poison, chinese);
        AddRisk(rows, preview, preview.SporeRisk, CharacterAfflictions.STATUSTYPE.Spores, chinese);
        if (preview.HasExtraStamina && (!summary || preview.ExtraAfter - preview.ExtraBefore >= .0005f))
            rows.Add(new MinimalRow("+" + Number(Math.Max(0, preview.ExtraAfter - preview.ExtraBefore) * 100), lightning: true,
                direction: preview.ExtraAfter > preview.ExtraBefore ? 1 : 0));
        if (preview.CompactInfinity.Length > 0) rows.Add(new MinimalRow(preview.CompactInfinity + " ∞", lightning: true));
        else if (preview.InfiniteStamina) rows.Add(new MinimalRow("∞", lightning: true));
        foreach (var note in preview.CompactNotes.Distinct()) rows.Add(new MinimalRow(note));
        foreach (var resource in preview.Resources)
            if (resource.Kind == ResourceKind.Uses || resource.Kind == ResourceKind.Remaining || resource.Kind == ResourceKind.Fuel)
                rows.Add(new MinimalRow(resource.Value + " " + resource.Label));
        if (preview.CompactUse.Length > 0) rows.Add(new MinimalRow(preview.CompactUse));
        return rows;
    }

    private static void AddRisk(List<MinimalRow> rows, ItemPreview preview, RiskLevel risk, CharacterAfflictions.STATUSTYPE type, bool chinese)
    {
        if (risk == RiskLevel.Absent || !preview.IsFood && risk != RiskLevel.Present) return;
        if (risk == RiskLevel.Present && (preview.SummarizeEffects || preview.Effects.Any(e => e.Timed)
            ? rows.Any(r => r.Status == type && r.Direction > 0)
            : preview.Effects.Any(e => e.Type == type && e.Amount > 0))) return;
        rows.Add(new MinimalRow(risk == RiskLevel.Present ? (chinese ? "有" : "Present") : "?", type));
    }
}
