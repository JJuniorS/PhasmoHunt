namespace PhasmoHunt.Models;

public enum SpeedPattern
{
    Stable,
    Accelerating,
    Decelerating,
    Irregular
}

public sealed class SpeedMeasurement
{
    public required DateTimeOffset RecordedAt { get; init; }
    public required IReadOnlyList<TimeSpan> StepTimestamps { get; init; }
    public required TimeSpan TotalTime { get; init; }
    public required double AverageIntervalSeconds { get; init; }
    public required double StepsPerSecond { get; init; }
    public required double EstimatedSpeedMps { get; init; }
    public required double ConfidencePercent { get; init; }
    public required IReadOnlyList<SpeedSegment> Segments { get; init; }
    public required SpeedPattern Pattern { get; init; }
    public required string PatternText { get; init; }
    public required IReadOnlyList<GhostInfo> CompatibleGhosts { get; init; }
}
