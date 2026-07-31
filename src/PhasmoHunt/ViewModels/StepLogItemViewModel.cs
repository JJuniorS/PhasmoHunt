using CommunityToolkit.Mvvm.ComponentModel;

namespace PhasmoHunt.ViewModels;

public partial class StepLogItemViewModel : ObservableObject
{
    public StepLogItemViewModel(int index, TimeSpan timestamp, double? intervalSeconds)
    {
        Index = index;
        TimestampText = $"{timestamp.TotalSeconds:F2} s";
        IntervalText = intervalSeconds is null
            ? "—"
            : $"{intervalSeconds.Value:F3} s";
    }

    public int Index { get; }
    public string TimestampText { get; }
    public string IntervalText { get; }
    public string DisplayText => $"#{Index}  {TimestampText}  (Δ {IntervalText})";
}
