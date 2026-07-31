using PhasmoHunt.Models;

namespace PhasmoHunt.Services;

public sealed class SessionComparisonResult
{
    public required string SummaryText { get; init; }
    public required string SpeedsText { get; init; }
    public required string CommonGhostsText { get; init; }
    public required IReadOnlyList<GhostInfo> CommonGhosts { get; init; }
}

public sealed class SessionComparisonService
{
    public SessionComparisonResult Compare(IReadOnlyList<SpeedMeasurement> readings)
    {
        if (readings.Count == 0)
        {
            return new SessionComparisonResult
            {
                SummaryText = "Nenhuma leitura na sessão.",
                SpeedsText = "—",
                CommonGhostsText = "—",
                CommonGhosts = []
            };
        }

        if (readings.Count == 1)
        {
            var only = readings[0];
            return new SessionComparisonResult
            {
                SummaryText = $"1 leitura · {only.PatternText}",
                SpeedsText = FormatReadingSpeeds(only),
                CommonGhostsText = FormatGhosts(only.CompatibleGhosts),
                CommonGhosts = only.CompatibleGhosts
            };
        }

        var overallSpeeds = readings.Select(r => r.EstimatedSpeedMps).ToArray();
        var allSegmentSpeeds = readings.SelectMany(r => r.Segments.Select(s => s.EstimatedSpeedMps)).ToArray();
        var min = overallSpeeds.Min();
        var max = overallSpeeds.Max();
        var avg = overallSpeeds.Average();
        var segMin = allSegmentSpeeds.Min();
        var segMax = allSegmentSpeeds.Max();

        var common = readings
            .Select(r => r.CompatibleGhosts.Select(g => g.Id).ToHashSet(StringComparer.OrdinalIgnoreCase))
            .Aggregate((a, b) =>
            {
                a.IntersectWith(b);
                return a;
            });

        var commonGhosts = readings[0].CompatibleGhosts
            .Where(g => common.Contains(g.Id))
            .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // Se a interseção for vazia, mostra fantasmas que aparecem em pelo menos metade das leituras.
        if (commonGhosts.Length == 0)
        {
            var threshold = Math.Max(2, (readings.Count + 1) / 2);
            var counts = readings
                .SelectMany(r => r.CompatibleGhosts)
                .GroupBy(g => g.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => (Ghost: g.First(), Count: g.Count()))
                .Where(x => x.Count >= threshold)
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Ghost.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.Ghost)
                .ToArray();
            commonGhosts = counts;
        }

        var spreadNote = segMax - segMin >= 0.8
            ? "Partes da sessão cobrem faixa larga (possível fantasma variável)."
            : max - min <= 0.25
                ? "Leituras consistentes entre si."
                : "Leituras divergem — compare as 3 partes de cada uma.";

        return new SessionComparisonResult
        {
            SummaryText = $"{readings.Count} leituras · média {avg:F2} m/s ({min:F2}–{max:F2}) · {spreadNote}",
            SpeedsText = string.Join("  |  ", readings.Select((r, i) =>
                $"#{readings.Count - i} {FormatReadingSpeeds(r)}")),
            CommonGhostsText = FormatGhosts(commonGhosts),
            CommonGhosts = commonGhosts
        };
    }

    private static string FormatReadingSpeeds(SpeedMeasurement reading) =>
        string.Join("/", reading.Segments.Select(s => s.EstimatedSpeedMps.ToString("F2")));

    private static string FormatGhosts(IReadOnlyList<GhostInfo> ghosts) =>
        ghosts.Count == 0
            ? "Sem consenso ainda"
            : string.Join(", ", ghosts.Select(g => g.Name));
}
