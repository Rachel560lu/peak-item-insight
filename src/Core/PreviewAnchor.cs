namespace PeakItemInsight.Core;

// Indices 0..2 are ordinary inventory slots; 3 is the backpack, 4 temporary hands.
// A highlighted slot is not evidence that a hovered world item occupies it.
internal static class PreviewAnchor
{
    internal static int Select(PreviewSource source, int? heldSlot, bool firstEmpty, bool secondEmpty,
        bool thirdEmpty, bool temporaryHeld)
    {
        if (source == PreviewSource.Hover) return firstEmpty ? 0 : secondEmpty ? 1 : thirdEmpty ? 2 : 3;
        if (source == PreviewSource.Held) return temporaryHeld ? 4 : heldSlot ?? -1;
        return -1;
    }
}
