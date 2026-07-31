using CommunityToolkit.Mvvm.ComponentModel;
using PhasmoHunt.Models;

namespace PhasmoHunt.ViewModels;

public partial class HistoryItemViewModel : ObservableObject
{
    public HistoryItemViewModel(SpeedMeasurement measurement, int readingNumber)
    {
        Measurement = measurement;
        ReadingNumber = readingNumber;
        SpeedText = $"{measurement.EstimatedSpeedMps:F2} m/s";
        ConfidenceText = $"{measurement.ConfidencePercent:F0}%";
        TimeText = measurement.TotalTime.TotalSeconds.ToString("F2") + " s";
        PartsText = string.Join(" / ", measurement.Segments.Select(s => $"{s.EstimatedSpeedMps:F2}"));
        PatternText = measurement.PatternText;
        CompatibleText = measurement.CompatibleGhosts.Count == 0
            ? "—"
            : string.Join(", ", measurement.CompatibleGhosts.Select(g => g.Name));
        TimestampText = measurement.RecordedAt.ToLocalTime().ToString("HH:mm:ss");
        HeaderText = $"#{readingNumber}  {TimestampText}  ·  {SpeedText}";
    }

    public SpeedMeasurement Measurement { get; }
    public int ReadingNumber { get; }
    public string HeaderText { get; }
    public string SpeedText { get; }
    public string ConfidenceText { get; }
    public string TimeText { get; }
    public string PartsText { get; }
    public string PatternText { get; }
    public string CompatibleText { get; }
    public string TimestampText { get; }
}
