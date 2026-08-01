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
        DisplayName = type.ToDisplayName();
        Icon = EvidenceIconService.GetIcon(type);
        _onChanged = onChanged;
    }

    private readonly Action _onChanged;

    public EvidenceType Type { get; }
    public string DisplayName { get; }
    public ImageSource? Icon { get; }

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value) => _onChanged();
}
