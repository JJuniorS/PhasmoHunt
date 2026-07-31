namespace PhasmoHunt.Models;

public sealed class SpeedSegment
{
    public required int Index { get; init; }
    public required string Label { get; init; }
    public required int StartStep { get; init; }
    public required int EndStep { get; init; }
    public required double AverageIntervalSeconds { get; init; }
    public required double EstimatedSpeedMps { get; init; }
    public required TimeSpan Duration { get; init; }
}
