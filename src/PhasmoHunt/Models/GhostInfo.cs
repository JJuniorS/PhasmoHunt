using System.Globalization;

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

    public bool MatchesSpeed(double speedMps, double toleranceMps, double speedFactor = 1.0)
    {
        if (speedFactor <= 0)
        {
            speedFactor = 1.0;
        }

        return speedMps >= MinSpeedMps * speedFactor - toleranceMps
               && speedMps <= MaxSpeedMps * speedFactor + toleranceMps;
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

    public string SpeedRangeText => FormatSpeedRange(1.0);

    public string FormatSpeedRange(double speedFactor = 1.0)
    {
        if (speedFactor <= 0)
        {
            speedFactor = 1.0;
        }

        var min = MinSpeedMps * speedFactor;
        var max = MaxSpeedMps * speedFactor;
        var baseSpeed = BaseSpeedMps * speedFactor;
        return Math.Abs(min - max) < 0.001
            ? $"{baseSpeed.ToString("0.##", CultureInfo.InvariantCulture)} m/s"
            : $"{min.ToString("0.##", CultureInfo.InvariantCulture)}–{max.ToString("0.##", CultureInfo.InvariantCulture)} m/s";
    }
}
