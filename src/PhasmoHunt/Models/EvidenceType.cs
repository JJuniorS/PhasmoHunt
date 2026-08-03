using PhasmoHunt.Services;

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
    public static string ToDisplayName(this EvidenceType evidence) =>
        LocalizationService.Instance.EvidenceDisplayName(evidence);

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
