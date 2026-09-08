namespace PeakItemInsight.Detection;

internal sealed class HoverResolver
{
    public string CurrentTargetType { get; private set; } = "null";
    public int? WorldId { get; private set; }

    public Item? Resolve()
    {
        WorldId = null;
        var interaction = Interaction.instance;
        if (interaction == null)
        {
            CurrentTargetType = "no-interaction";
            return null;

        }

        CurrentTargetType = interaction.currentHovered?.GetType().Name ?? "null";

        if (interaction.currentHovered is Item item && item != null)
        {
            WorldId = item.GetInstanceID();
            return item;
        }

        // PEAK represents most uncollected world loot as FakeItem and only
        // instantiates the real Item when it is picked up.
        if (interaction.currentHovered is FakeItem fakeItem && fakeItem != null && !fakeItem.pickedUp)
        {
            WorldId = fakeItem.GetInstanceID();
            return fakeItem.realItemPrefab;
        }

        return null;
    }
}
