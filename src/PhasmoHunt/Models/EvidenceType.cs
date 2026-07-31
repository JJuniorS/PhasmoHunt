namespace PhasmoHunt.Models;

public enum EvidenceType
{
    Emf5,
    SpiritBox,
    Ultraviolet,
    GhostOrb,
    GhostWriting,
    Freezing,
    Dots
}

public static class EvidenceTypeExtensions
{
    public static string ToDisplayName(this EvidenceType evidence) => evidence switch
    {
        EvidenceType.Emf5 => "EMF 5",
        EvidenceType.SpiritBox => "Spirit Box",
        EvidenceType.Ultraviolet => "UV",
        EvidenceType.GhostOrb => "Orb",
        EvidenceType.GhostWriting => "Writing",
        EvidenceType.Freezing => "Freezing",
        EvidenceType.Dots => "D.O.T.S.",
        _ => evidence.ToString()
    };

    public static IReadOnlyList<EvidenceType> All { get; } =
    [
        EvidenceType.Emf5,
        EvidenceType.SpiritBox,
        EvidenceType.Ultraviolet,
        EvidenceType.GhostOrb,
        EvidenceType.GhostWriting,
        EvidenceType.Freezing,
        EvidenceType.Dots
    ];
}
