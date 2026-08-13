namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Shared framework-level color identities used by game modules to request
    /// consistent presentation without embedding puzzle-specific meaning.
    /// </summary>
    public enum ColorIdentity
    {
        None = 0,
        Slot0 = 1,
        Slot1 = 2,
        Slot2 = 3,
        Slot3 = 4,
        Slot4 = 5,
        Slot5 = 6,
        Slot6 = 7,
        Slot7 = 8,
        Slot8 = 9,
        Slot9 = 10,

        // Legacy aliases preserve current prototype content compatibility while the
        // shared color system expands to a stable ten-slot model for future editor work.
        Red = Slot0,
        Blue = Slot1,
        Green = Slot2,
        Yellow = Slot3
    }
}
