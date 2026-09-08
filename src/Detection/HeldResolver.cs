namespace PeakItemInsight.Detection;

internal static class HeldResolver
{
    public static Item? Resolve()
    {
        var character = Character.localCharacter;
        return character != null && character.data.currentItem != null ? character.data.currentItem : null;
    }
    public static bool IsBusy(Item? item) => item != null &&
        (item.consuming || item.isUsingPrimary || item.isUsingSecondary || item.castProgress > 0);
}
