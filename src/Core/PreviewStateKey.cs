using System;

namespace PeakItemInsight.Core;

internal readonly struct PreviewStateKey : IEquatable<PreviewStateKey>
{
    private PreviewStateKey(int instanceId, int uses, int cooked, int statusHash)
    {
        InstanceId = instanceId;
        Uses = uses;
        Cooked = cooked;
        StatusHash = statusHash;
    }

    private int InstanceId { get; }
    private int Uses { get; }
    private int Cooked { get; }
    private int StatusHash { get; }

    public static PreviewStateKey Capture(Item item)
    {
        var uses = ItemDataReader.TryGetInt(item, DataEntryKey.ItemUses, out var itemUses) ? itemUses : -1;
        var cooked = ItemDataReader.TryGetInt(item, DataEntryKey.CookedAmount, out var cookedAmount) ? cookedAmount : -1;
        var hash = 17;
        var character = Character.localCharacter;
        var afflictions = character != null ? character.refs.afflictions : null;
        if (afflictions != null)
        {
            foreach (var type in StatusTypes.Previewable)
                hash = unchecked(hash * 31 + afflictions.GetCurrentStatus(type).GetHashCode());
        }
        if (character != null)
        {
            hash = unchecked(hash * 31 + character.data.extraStamina.GetHashCode());
            hash = unchecked(hash * 31 + character.data.petrifyAmount);
            hash = unchecked(hash * 31 + character.data.isSkeleton.GetHashCode());
            hash = unchecked(hash * 31 + character.statusesLocked.GetHashCode());
            hash = unchecked(hash * 31 + character.isZombie.GetHashCode());
            hash = unchecked(hash * 31 + character.isScoutmaster.GetHashCode());
        }
        if (ItemDataReader.TryGetFloat(item, DataEntryKey.Fuel, out var fuel)) hash = unchecked(hash * 31 + fuel.GetHashCode());
        if (item.cooking != null) hash = unchecked(hash * 31 + item.cooking.timesCookedLocal);
        hash = unchecked(hash * 31 + item.consuming.GetHashCode());
        hash = unchecked(hash * 31 + item.isUsingPrimary.GetHashCode());
        hash = unchecked(hash * 31 + item.isUsingSecondary.GetHashCode());

        return new PreviewStateKey(item.GetInstanceID(), uses, cooked, hash);
    }

    public bool Equals(PreviewStateKey other) => InstanceId == other.InstanceId && Uses == other.Uses && Cooked == other.Cooked && StatusHash == other.StatusHash;
    public override bool Equals(object? obj) => obj is PreviewStateKey other && Equals(other);
    public override int GetHashCode() => unchecked((((InstanceId * 397) ^ Uses) * 397 ^ Cooked) * 397 ^ StatusHash);
}

internal static class ItemDataReader
{
    public static bool TryGetInt(Item item, DataEntryKey key, out int value)
    {
        value = 0;
        if (key == DataEntryKey.ItemUses)
        {
            if (item.data == null || !item.data.TryGetDataEntry<OptionableIntItemData>(key, out var uses) || uses == null || !uses.HasData)
                return false;
            value = uses.Value;
            return true;
        }
        if (item.data == null || !item.data.TryGetDataEntry<IntItemData>(key, out var entry) || entry == null)
            return false;
        value = entry.Value;
        return true;
    }

    public static bool TryGetFloat(Item item, DataEntryKey key, out float value)
    {
        value = 0f;
        if (item.data == null || !item.data.TryGetDataEntry<FloatItemData>(key, out var entry) || entry == null)
            return false;
        value = entry.Value;
        return true;
    }
}

internal static class StatusTypes
{
    public static readonly CharacterAfflictions.STATUSTYPE[] Previewable =
        (CharacterAfflictions.STATUSTYPE[])Enum.GetValues(typeof(CharacterAfflictions.STATUSTYPE));
}
