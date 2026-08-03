using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PhasmoHunt.Models;
using PhasmoHunt.Services;

namespace PhasmoHunt.ViewModels;

public partial class EvidenceOptionViewModel : ObservableObject
{
    public EvidenceOptionViewModel(EvidenceType type, Action onChanged)
    {
        Type = type;
        Icon = EvidenceIconService.GetIcon(type);
        _onChanged = onChanged;
        RefreshDisplayName();
    }

    private readonly Action _onChanged;

    public EvidenceType Type { get; }
    public ImageSource? Icon { get; }

    [ObservableProperty]
    private string _displayName = "";

    [ObservableProperty]
    private bool _isSelected;

    public void RefreshDisplayName() =>
        DisplayName = Type.ToDisplayName();

    partial void OnIsSelectedChanged(bool value) => _onChanged();
}
