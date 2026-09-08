using System.Collections.Generic;

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

internal readonly struct ResourceLine
{
    public ResourceLine(string label, string value)
    {
        Label = label;
        Value = value;
    }

    public string Label { get; }
    public string Value { get; }
}

internal sealed class ItemPreview
{
    public ushort ItemId { get; set; }
    public PreviewSource Source { get; set; }
    public int TargetInstanceId { get; set; }
    public string Name { get; set; } = "Unknown item";
    public string? DebugId { get; set; }
    public bool HasExtraStamina { get; set; }
    public float ExtraBefore { get; set; }
    public float ExtraAfter { get; set; }
    public List<StatusDelta> Statuses { get; } = new List<StatusDelta>();
    public List<ResourceLine> Resources { get; } = new List<ResourceLine>();
    public List<string> Instructions { get; } = new List<string>();
    public List<string> Warnings { get; } = new List<string>();
}
