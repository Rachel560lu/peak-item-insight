using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PeakItemInsight.Core;

internal readonly struct DetailedRow
{
    public DetailedRow(string text, string timing = "", CharacterAfflictions.STATUSTYPE? status = null,
        bool lightning = false, BuffKind? buff = null, bool title = false, bool namesStatus = false)
    { Text = text; Timing = timing; Status = status; Lightning = lightning; Buff = buff; Title = title; NamesStatus = namesStatus; }
    public string Text { get; }
    public string Timing { get; }
    public CharacterAfflictions.STATUSTYPE? Status { get; }
    public bool Lightning { get; }
    public BuffKind? Buff { get; }
    public bool Title { get; }
    public bool NamesStatus { get; }
}

// Describe structured facts; never execute actions or change HUD projections.
internal static class DetailedRows
{
    private static string Number(float value) => value.ToString("0.#", CultureInfo.InvariantCulture);
    private static string Text(string language, string zh, string en) => LanguageCatalog.Text(language, en, zh);
    private static string Format(string language, string zh, string en, params object[] values)
        => string.Format(CultureInfo.InvariantCulture, Text(language, zh, en), values);

    private static string StatusName(string language, CharacterAfflictions.STATUSTYPE type)
    {
        string name;
        switch (type)
        {
            case CharacterAfflictions.STATUSTYPE.Injury: name = Text(language, "伤势", "Injury"); break;
            case CharacterAfflictions.STATUSTYPE.Hunger: name = Text(language, "饥饿", "Hunger"); break;
            case CharacterAfflictions.STATUSTYPE.Cold: name = Text(language, "寒冷", "Cold"); break;
            case CharacterAfflictions.STATUSTYPE.Poison: name = Text(language, "毒素", "Poison"); break;
            case CharacterAfflictions.STATUSTYPE.Spores: name = Text(language, "孢子", "Spores"); break;
            case CharacterAfflictions.STATUSTYPE.Curse: name = Text(language, "诅咒", "Curse"); break;
            case CharacterAfflictions.STATUSTYPE.Drowsy: name = Text(language, "困倦", "Drowsy"); break;
            case CharacterAfflictions.STATUSTYPE.Weight: name = Text(language, "负重", "Weight"); break;
            case CharacterAfflictions.STATUSTYPE.Hot: name = Text(language, "炎热", "Heat"); break;
            case CharacterAfflictions.STATUSTYPE.Petrify: name = Text(language, "石化", "Petrify"); break;
            case CharacterAfflictions.STATUSTYPE.Thorns: name = Text(language, "荆棘", "Thorns"); break;
            default: name = type.ToString(); break;
        }
        return language == "English" ? type == CharacterAfflictions.STATUSTYPE.Drowsy ? "drowsiness" : name.ToLowerInvariant() : name;
    }

    // Drowsiness has a native full scale of 1. Other statuses can have larger caps;
    // explicit Clears remain authoritative. A timed total alone is never a clear.
    private static bool Clears(EffectFact effect)
        => effect.Clears || effect.Type == CharacterAfflictions.STATUSTYPE.Drowsy && effect.Duration <= 0 && effect.Amount <= -1;

    private static string Start(string language, float delay, EffectTrigger trigger, float endDelay)
    {
        if (trigger == EffectTrigger.EffectEnd)
            return endDelay > 0 ? Format(language, "效果结束后 {0:0.#} 秒，", "{0:0.#} s after the effect ends, ", endDelay)
                : Text(language, "效果结束后，", "After the effect ends, ");
        if (delay > 0) return Format(language, "{0:0.#} 秒后，", "After {0:0.#} s, ", delay);
        if (trigger == EffectTrigger.Climbing) return Text(language, "开始攀爬后，", "After climbing starts, ");
        return "";
    }

    private static string Duration(string language, float duration)
        => duration > 0 ? Format(language, "持续 {0:0.#} 秒", "For {0:0.#} s", duration) : "";

    private static string EffectText(string language, EffectFact effect)
    {
        var name = StatusName(language, effect.Type);
        var amount = Number(Math.Abs(effect.Amount) * 100);
        var prefix = Start(language, effect.Delay, effect.Trigger, effect.EndDelay);
        if (Clears(effect))
            return prefix.Length == 0 ? Format(language, "立即清除{0}", "Immediately clear {0}", name)
                : prefix + Format(language, "清除{0}", "clear {0}", name);
        if (effect.Duration > 0)
            return prefix + (effect.Amount < 0
                ? Format(language, "每秒减少 {1} {0}", "reduce {0} by {1} each second", name, amount)
                : Format(language, "每秒增加 {1} {0}", "increase {0} by {1} each second", name, amount));
        return prefix.Length == 0
            ? effect.Amount < 0 ? Format(language, "立即减少 {1} {0}", "Immediately reduce {0} by {1}", name, amount)
                : Format(language, "立即增加 {1} {0}", "Immediately increase {0} by {1}", name, amount)
            : prefix + (effect.Amount < 0 ? Format(language, "减少 {1} {0}", "reduce {0} by {1}", name, amount)
                : Format(language, "增加 {1} {0}", "increase {0} by {1}", name, amount));
    }

    public static List<DetailedRow> Build(ItemPreview preview, string language)
    {
        var rows = new List<DetailedRow>();
        // Merge immediate recoveries with identical trigger timing. Opposing signs,
        // delayed effects and rates retain their separate clamping/time semantics.
        var effects = preview.Effects.Where(e => e.Clears || Math.Abs(e.Amount) >= .00001f)
            .GroupBy(e => new { e.Type, e.Duration, e.Delay, e.Trigger, e.EndDelay,
                Merge = e.Duration <= 0 && e.Delay == 0 && e.Trigger == EffectTrigger.Use && e.Amount < 0, Sign = e.Amount < 0, e.Clears })
            .SelectMany(g => g.Key.Merge
                ? new[] { new EffectFact(g.Key.Type, g.Sum(e => e.Amount), g.Key.Duration, g.Key.Delay,
                    g.Key.Clears, g.Key.Trigger, g.Key.EndDelay) } : g.AsEnumerable())
            .OrderBy(e => e.Trigger == EffectTrigger.EffectEnd ? 2 : e.Amount > 0 ? 1 : 0)
            .ThenBy(e => e.Duration > 0 ? 1 : 0).ToList();
        var combined = new HashSet<int>();
        for (var i = 0; i < effects.Count; i++)
        {
            if (combined.Contains(i)) continue;
            var effect = effects[i];
            var value = EffectText(language, effect);
            var duration = effect.Duration;
            if (Clears(effect) && effect.Type == CharacterAfflictions.STATUSTYPE.Drowsy &&
                effect.Duration <= 0 && effect.Delay == 0 && effect.Trigger == EffectTrigger.Use)
            {
                var steady = effects.FindIndex(e => e.Type == effect.Type && !e.Clears && e.Amount <= -1 &&
                    e.Duration > 0 && e.Delay == 0 && e.Trigger == EffectTrigger.Use);
                if (steady >= 0 && !combined.Contains(steady))
                {
                    combined.Add(steady);
                    duration = effects[steady].Duration;
                    value = Format(language, "立即清除{0}，随后持续消除{0}",
                        "Immediately clear {0}, then keep clearing it", StatusName(language, effect.Type));
                }
            }
            rows.Add(new DetailedRow(value, Duration(language, duration), effect.Type, namesStatus: true));
        }
        foreach (var buff in preview.Buffs.AsEnumerable().Reverse())
        {
            var value = buff.Kind == BuffKind.InfiniteStamina ? Text(language, "获得无限精力", "Gain infinite stamina")
                : buff.Kind == BuffKind.Speed ? Text(language, "获得加速", "Gain a speed boost")
                : Text(language, "获得无敌效果", "Gain invincibility");
            // The climbing timer condition is already supplied by DetailConditions.
            rows.Insert(0, new DetailedRow(Start(language, buff.Delay,
                buff.Trigger == EffectTrigger.Climbing ? EffectTrigger.Use : buff.Trigger, 0) + value,
                Duration(language, buff.Duration), lightning: buff.Kind == BuffKind.InfiniteStamina,
                buff: buff.Kind, namesStatus: true));
        }
        if (preview.HasExtraStamina && preview.ExtraAfter - preview.ExtraBefore >= .0005f)
            rows.Add(new DetailedRow(Format(language, "增加 {0} 额外精力", "Gain {0} extra stamina",
                Number((preview.ExtraAfter - preview.ExtraBefore) * 100)), lightning: true, namesStatus: true));
        if ((preview.InfiniteStamina || preview.CompactInfinity.Length > 0) &&
            !preview.Buffs.Any(b => b.Kind == BuffKind.InfiniteStamina))
            rows.Add(new DetailedRow(Text(language, "获得无限精力", "Gain infinite stamina"), lightning: true, namesStatus: true));

        AddRisk(rows, preview, preview.PoisonRisk, CharacterAfflictions.STATUSTYPE.Poison, language);
        AddRisk(rows, preview, preview.SporeRisk, CharacterAfflictions.STATUSTYPE.Spores, language);
        foreach (var note in preview.DetailNotes.Distinct())
            if (!string.IsNullOrWhiteSpace(note)) rows.Add(new DetailedRow(note));
        foreach (var resource in preview.Resources)
            if ((resource.Kind == ResourceKind.Uses || resource.Kind == ResourceKind.Remaining || resource.Kind == ResourceKind.Fuel || resource.Kind == ResourceKind.Cooked)
                && !string.IsNullOrWhiteSpace(resource.Value))
                rows.Add(new DetailedRow(resource.Kind == ResourceKind.Cooked ? resource.Label + ": " + resource.Value : resource.Value + " " + resource.Label));
        var conditions = new List<string>(preview.DetailConditions);
        if (preview.Buffs.Any(b => b.Trigger == EffectTrigger.Climbing))
            conditions.Add(Text(language, "开始攀爬后计时。", "Timer starts when climbing."));
        if (!string.IsNullOrWhiteSpace(preview.CompactUse)) conditions.Add(preview.CompactUse);
        else if (!preview.IsFood && !string.IsNullOrWhiteSpace(preview.Description)) conditions.Add(preview.Description);
        var condition = string.Join(" ", conditions.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct());
        if (condition.Length > 0) rows.Add(new DetailedRow(condition));
        if (rows.Count > 0 && !string.IsNullOrWhiteSpace(preview.Name))
            rows.Insert(0, new DetailedRow(preview.Name, title: true));
        return rows;
    }

    private static void AddRisk(List<DetailedRow> rows, ItemPreview preview, RiskLevel risk,
        CharacterAfflictions.STATUSTYPE type, string language)
    {
        if (risk == RiskLevel.Absent || !preview.IsFood && risk != RiskLevel.Present) return;
        if (risk == RiskLevel.Present && preview.Effects.Any(e => e.Type == type && e.Amount > 0)) return;
        var name = StatusName(language, type);
        rows.Add(new DetailedRow(risk == RiskLevel.Present
            ? Format(language, "含有{0}", "Contains {0}", name)
            : Format(language, "{0}效果未知", "Unknown {0} effects", name), status: type, namesStatus: true));
    }
}
