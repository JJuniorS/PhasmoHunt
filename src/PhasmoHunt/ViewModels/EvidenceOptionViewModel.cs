using CommunityToolkit.Mvvm.ComponentModel;
using PhasmoHunt.Models;

namespace PhasmoHunt.ViewModels;

public partial class EvidenceOptionViewModel : ObservableObject
{
    public EvidenceOptionViewModel(EvidenceType type, Action onChanged)
    {
        Type = type;
        DisplayName = type.ToDisplayName();
        _onChanged = onChanged;
    }

    private readonly Action _onChanged;

    public EvidenceType Type { get; }
    public string DisplayName { get; }

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value) => _onChanged();
}
