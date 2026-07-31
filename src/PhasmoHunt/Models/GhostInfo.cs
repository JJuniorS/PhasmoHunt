namespace PhasmoHunt.Models;

public sealed class GhostInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required double BaseSpeedMps { get; init; }
    public required double MinSpeedMps { get; init; }
    public required double MaxSpeedMps { get; init; }
    public required IReadOnlyList<EvidenceType> Evidences { get; init; }
    public string? SpeedNotes { get; init; }

    /// <summary>The Mimic always shows fake Ghost Orbs in addition to its real evidence.</summary>
    public bool HasFakeGhostOrb { get; init; }

    public bool MatchesSpeed(double speedMps, double toleranceMps)
    {
        return speedMps >= MinSpeedMps - toleranceMps
               && speedMps <= MaxSpeedMps + toleranceMps;
    }

    public bool MatchesEvidence(IReadOnlyCollection<EvidenceType> selected)
    {
        if (selected.Count == 0)
        {
            return true;
        }

        var available = HasFakeGhostOrb
            ? Evidences.Append(EvidenceType.GhostOrb).Distinct().ToHashSet()
            : Evidences.ToHashSet();

        return selected.All(available.Contains);
    }

    public string SpeedRangeText =>
        Math.Abs(MinSpeedMps - MaxSpeedMps) < 0.001
            ? $"{BaseSpeedMps:0.##} m/s"
            : $"{MinSpeedMps:0.##}–{MaxSpeedMps:0.##} m/s";
}
