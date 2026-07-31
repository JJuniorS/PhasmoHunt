using PhasmoHunt.Models;

namespace PhasmoHunt.Services;

public sealed class SpeedCalculatorService
{
    public const int SegmentCount = 3;
    public const int MinStepsPerSegment = 2;
    public const int MinTotalSteps = SegmentCount * MinStepsPerSegment;

    public const double ReferenceSpeedMps = 1.7;
    public const double ReferenceIntervalSeconds = 0.64;
    public const double CvSoftCap = 0.25;
    public const double SpeedMatchToleranceMps = 0.15;
    public const double StableSpreadMps = 0.20;
    public const double TrendDeltaMps = 0.12;

    public static int StepsPerSegment(int totalClicks) => totalClicks / SegmentCount;

    public static int UsableStepCount(int totalClicks)
    {
        var per = StepsPerSegment(totalClicks);
        return per * SegmentCount;
    }

    public static bool CanCalculate(int totalClicks) =>
        StepsPerSegment(totalClicks) >= MinStepsPerSegment;

    public SpeedMeasurement Calculate(
        IReadOnlyList<TimeSpan> stepTimestamps,
        IReadOnlyList<GhostInfo> ghostCatalog,
        DateTimeOffset? recordedAt = null)
    {
        if (!TryPrepare(stepTimestamps, out var ordered, out var stepsPerSegment))
        {
            throw new ArgumentException(
                $"Need at least {MinTotalSteps} clicks (floor(n/3) >= {MinStepsPerSegment} per part).",
                nameof(stepTimestamps));
        }

        var intervals = BuildIntervals(ordered);
        var totalTime = ordered[^1] - ordered[0];
        var averageInterval = intervals.Average();
        var stepsPerSecond = 1.0 / averageInterval;
        var estimatedSpeed = IntervalToSpeed(averageInterval);
        var confidence = ComputeConfidence(intervals, averageInterval);
        var segments = BuildSegments(ordered, stepsPerSegment);
        var pattern = DetectPattern(segments);
        var compatible = MatchGhosts(estimatedSpeed, segments, ghostCatalog);

        return new SpeedMeasurement
        {
            RecordedAt = recordedAt ?? DateTimeOffset.Now,
            StepTimestamps = ordered,
            TotalTime = totalTime,
            AverageIntervalSeconds = averageInterval,
            StepsPerSecond = stepsPerSecond,
            EstimatedSpeedMps = estimatedSpeed,
            ConfidencePercent = confidence,
            Segments = segments,
            Pattern = pattern,
            PatternText = PatternToText(pattern, segments),
            CompatibleGhosts = compatible
        };
    }

    public static double IntervalToSpeed(double averageIntervalSeconds) =>
        ReferenceSpeedMps * (ReferenceIntervalSeconds / averageIntervalSeconds);

    private static bool TryPrepare(
        IReadOnlyList<TimeSpan> stepTimestamps,
        out TimeSpan[] usable,
        out int stepsPerSegment)
    {
        stepsPerSegment = StepsPerSegment(stepTimestamps.Count);
        if (stepsPerSegment < MinStepsPerSegment)
        {
            usable = [];
            return false;
        }

        var usableCount = stepsPerSegment * SegmentCount;
        usable = stepTimestamps.OrderBy(t => t).Take(usableCount).ToArray();
        return true;
    }

    private static double[] BuildIntervals(IReadOnlyList<TimeSpan> ordered)
    {
        var intervals = new double[ordered.Count - 1];
        for (var i = 0; i < intervals.Length; i++)
        {
            intervals[i] = (ordered[i + 1] - ordered[i]).TotalSeconds;
            if (intervals[i] <= 0)
            {
                throw new ArgumentException("Step timestamps must be strictly increasing.");
            }
        }

        return intervals;
    }

    private static IReadOnlyList<SpeedSegment> BuildSegments(IReadOnlyList<TimeSpan> ordered, int stepsPerSegment)
    {
        var segments = new SpeedSegment[SegmentCount];
        for (var s = 0; s < SegmentCount; s++)
        {
            var start = s * stepsPerSegment;
            var end = start + stepsPerSegment - 1;
            var duration = ordered[end] - ordered[start];
            var avgInterval = duration.TotalSeconds / (stepsPerSegment - 1);
            segments[s] = new SpeedSegment
            {
                Index = s,
                Label = $"P{s + 1}",
                StartStep = start + 1,
                EndStep = end + 1,
                AverageIntervalSeconds = avgInterval,
                EstimatedSpeedMps = IntervalToSpeed(avgInterval),
                Duration = duration
            };
        }

        return segments;
    }

    private static SpeedPattern DetectPattern(IReadOnlyList<SpeedSegment> segments)
    {
        var speeds = segments.Select(s => s.EstimatedSpeedMps).ToArray();
        var spread = speeds.Max() - speeds.Min();
        if (spread <= StableSpreadMps)
        {
            return SpeedPattern.Stable;
        }

        var up = 0;
        var down = 0;
        for (var i = 1; i < speeds.Length; i++)
        {
            var delta = speeds[i] - speeds[i - 1];
            if (delta >= TrendDeltaMps) up++;
            else if (delta <= -TrendDeltaMps) down++;
        }

        if (up == speeds.Length - 1 && down == 0)
        {
            return SpeedPattern.Accelerating;
        }

        if (down == speeds.Length - 1 && up == 0)
        {
            return SpeedPattern.Decelerating;
        }

        return SpeedPattern.Irregular;
    }

    private static string PatternToText(SpeedPattern pattern, IReadOnlyList<SpeedSegment> segments)
    {
        var parts = string.Join(" → ", segments.Select(s => $"{s.EstimatedSpeedMps:F2}"));
        return pattern switch
        {
            SpeedPattern.Stable => $"Estável ({parts})",
            SpeedPattern.Accelerating => $"Acelerando ({parts})",
            SpeedPattern.Decelerating => $"Desacelerando ({parts})",
            _ => $"Irregular ({parts})"
        };
    }

    private static IReadOnlyList<GhostInfo> MatchGhosts(
        double overallSpeed,
        IReadOnlyList<SpeedSegment> segments,
        IReadOnlyList<GhostInfo> catalog)
    {
        var segmentSpeeds = segments.Select(s => s.EstimatedSpeedMps).ToArray();

        return catalog
            .Where(g =>
            {
                if (g.MatchesSpeed(overallSpeed, SpeedMatchToleranceMps))
                {
                    return true;
                }

                var hits = segmentSpeeds.Count(speed => g.MatchesSpeed(speed, SpeedMatchToleranceMps));
                return hits >= 2;
            })
            .OrderBy(g => segmentSpeeds.Min(speed => DistanceToRange(speed, g)))
            .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static double DistanceToRange(double speed, GhostInfo ghost)
    {
        if (speed < ghost.MinSpeedMps)
        {
            return ghost.MinSpeedMps - speed;
        }

        if (speed > ghost.MaxSpeedMps)
        {
            return speed - ghost.MaxSpeedMps;
        }

        return 0;
    }

    private static double ComputeConfidence(IReadOnlyList<double> intervals, double mean)
    {
        if (intervals.Count < 2 || mean <= 0)
        {
            return 0;
        }

        var variance = intervals.Sum(x => (x - mean) * (x - mean)) / intervals.Count;
        var stdDev = Math.Sqrt(variance);
        var cv = stdDev / mean;
        return Math.Clamp(100.0 * (1.0 - cv / CvSoftCap), 0, 100);
    }
}
