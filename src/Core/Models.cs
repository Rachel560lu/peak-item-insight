using System.Collections.Generic;
using UnityEngine;

namespace PeakItemInsight.Core;

internal readonly struct StatusDelta
{
    public StatusDelta(CharacterAfflictions.STATUSTYPE type, float before, float after)
    {
        Type = type;
        Before = before;
        After = after;
    }

    public CharacterAfflictions.STATUSTYPE Type { get; }
    public float Before { get; }
    public float After { get; }
}

internal enum ResourceKind { Other, Uses, Remaining, Fuel, Cooked }

internal readonly struct ResourceLine
{
    public ResourceLine(string label, string value, ResourceKind kind = ResourceKind.Other)
    {
        Label = label;
        Value = value;
        Kind = kind;
    }

    public string Label { get; }
    public string Value { get; }
    public ResourceKind Kind { get; }
}

internal sealed class ItemPreview
{
    public Texture? Icon { get; set; }
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public string CompactUse { get; set; } = "";
    public List<string> CompactNotes { get; } = new List<string>();
    public bool IsFood { get; set; }
    public bool CompleteEffects { get; set; }
    public bool PrefabOnly { get; set; }
    public RiskLevel PoisonRisk { get; set; }
    public RiskLevel SporeRisk { get; set; }
    public List<EffectFact> Effects { get; } = new List<EffectFact>();
    public List<string> Unsupported { get; } = new List<string>();
    public ushort ItemId { get; set; }
    public PreviewSource Source { get; set; }
    public int TargetInstanceId { get; set; }
    public string Name { get; set; } = "Unknown item";
    public string? DebugId { get; set; }
    public bool HasExtraStamina { get; set; }
    public bool InfiniteStamina { get; set; }
    public float ExtraBefore { get; set; }
    public float ExtraAfter { get; set; }
    public List<StatusDelta> Statuses { get; } = new List<StatusDelta>();
    // Numeric snapshot includes unchanged statuses. UI minimum badge widths are
    // decoration, never extra affliction or extra capacity loss.
    public Dictionary<CharacterAfflictions.STATUSTYPE, float> StatusBefore { get; } = new Dictionary<CharacterAfflictions.STATUSTYPE, float>();
    public List<ResourceLine> Resources { get; } = new List<ResourceLine>();
    public List<string> Instructions { get; } = new List<string>();
    public List<string> Warnings { get; } = new List<string>();
    // Developer diagnostics never belong in the player-facing description.
    public List<string> Diagnostics { get; } = new List<string>();
}

internal readonly struct EffectFact
{
    public EffectFact(CharacterAfflictions.STATUSTYPE type, float amount, float duration = 0, float delay = 0, bool clears = false)
    { Type = type; Amount = amount; Duration = duration; Delay = delay; Clears = clears; }
    public CharacterAfflictions.STATUSTYPE Type { get; }
    public float Amount { get; }
    public float Duration { get; }
    public float Delay { get; }
    public bool Clears { get; }
    public bool Timed => Duration > 0 || Delay > 0;
}
