using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PhasmoHunt.Models;
using PhasmoHunt.Services;

namespace PhasmoHunt.ViewModels;

public partial class GhostListItemViewModel : ObservableObject
{
    private static readonly SolidColorBrush EligibleBrush = CreateBrush(0x3D, 0xCF, 0x8E);
    private static readonly SolidColorBrush IneligibleBrush = CreateBrush(0xE3, 0x5D, 0x6A);
    private static readonly SolidColorBrush PendingBrush = CreateBrush(0x9A, 0xA3, 0xB2);

    public GhostListItemViewModel(GhostInfo ghost)
    {
        Ghost = ghost;
        Name = ghost.Name;
        SpeedRangeText = ghost.SpeedRangeText;
        Notes = ghost.SpeedNotes ?? "";
        EvidenceIcons = EvidenceIconService.GetIcons(ghost);
        SetEligibility(null);
    }

    public GhostInfo Ghost { get; }
    public string Name { get; }
    public string Notes { get; }
    public IReadOnlyList<ImageSource> EvidenceIcons { get; }

    [ObservableProperty]
    private string _speedRangeText = "";

    [ObservableProperty]
    private bool? _isEligible;

    [ObservableProperty]
    private string _statusText = "—";

    [ObservableProperty]
    private Brush _statusBrush = PendingBrush;

    public void ApplySpeedFactor(double speedFactor)
    {
        SpeedRangeText = Ghost.FormatSpeedRange(speedFactor);
    }

    public void SetEligibility(bool? eligible)
    {
        IsEligible = eligible;
        if (eligible is null)
        {
            StatusText = "—";
            StatusBrush = PendingBrush;
        }
        else if (eligible.Value)
        {
            StatusText = "Apto";
            StatusBrush = EligibleBrush;
        }
        else
        {
            StatusText = "Fora";
            StatusBrush = IneligibleBrush;
        }
    }

    private static SolidColorBrush CreateBrush(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
