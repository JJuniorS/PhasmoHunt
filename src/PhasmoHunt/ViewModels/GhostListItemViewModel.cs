using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhasmoHunt.Models;
using PhasmoHunt.Services;

namespace PhasmoHunt.ViewModels;

public partial class GhostListItemViewModel : ObservableObject
{
    private static readonly SolidColorBrush EligibleBrush = CreateBrush(0x3D, 0xCF, 0x8E);
    private static readonly SolidColorBrush IneligibleBrush = CreateBrush(0xE3, 0x5D, 0x6A);
    private static readonly SolidColorBrush PendingBrush = CreateBrush(0x9A, 0xA3, 0xB2);

    private readonly Action? _onEligibilityChanged;
    private bool? _autoEligible;
    private bool? _manualOverride;

    public GhostListItemViewModel(GhostInfo ghost, Action? onEligibilityChanged = null)
    {
        Ghost = ghost;
        Name = ghost.Name;
        SpeedRangeText = ghost.SpeedRangeText;
        Notes = ghost.SpeedNotes ?? "";
        EvidenceIcons = EvidenceIconService.GetIcons(ghost);
        _onEligibilityChanged = onEligibilityChanged;
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

    /// <summary>
    /// System-driven eligibility (filters). Manual overrides still win for display.
    /// </summary>
    public void SetEligibility(bool? eligible)
    {
        _autoEligible = eligible;
        ApplyDisplay();
    }

    public void ClearManualOverride()
    {
        _manualOverride = null;
        ApplyDisplay();
    }

    [RelayCommand]
    private void ToggleEligibility()
    {
        // Alterna só entre apto (verde) e fora (vermelho).
        var currentlyApto = (_manualOverride ?? _autoEligible) == true;
        _manualOverride = !currentlyApto;
        ApplyDisplay();
        _onEligibilityChanged?.Invoke();
    }

    private void ApplyDisplay()
    {
        var effective = _manualOverride ?? _autoEligible;
        IsEligible = effective;
        if (effective is null)
        {
            StatusText = "—";
            StatusBrush = PendingBrush;
        }
        else if (effective.Value)
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
