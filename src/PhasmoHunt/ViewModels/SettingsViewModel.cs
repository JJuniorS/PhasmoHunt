using System.Globalization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhasmoHunt.Models;
using PhasmoHunt.Services;

namespace PhasmoHunt.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly AppSettings _draft;
    private string? _capturingProperty;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        _draft = settingsService.Current.Clone();
        _draft.EnsureHotkeyDefaults();

        Opacity = _draft.Opacity;
        UiScale = _draft.UiScale;
        GhostSpeedPercentText = _draft.GhostSpeedPercent.ToString("0.##", CultureInfo.CurrentCulture);
        RefreshHotkeyLabels();
    }

    public bool Saved { get; private set; }

    [ObservableProperty] private double _opacity;
    [ObservableProperty] private double _uiScale;
    [ObservableProperty] private string _ghostSpeedPercentText = "100";
    [ObservableProperty] private string _stepHotkeyText = "";
    [ObservableProperty] private string _demonHotkeyText = "";
    [ObservableProperty] private string _incenseHotkeyText = "";
    [ObservableProperty] private string _obamboHotkeyText = "";
    [ObservableProperty] private string? _validationMessage;

    partial void OnOpacityChanged(double value) => _draft.Opacity = value;
    partial void OnUiScaleChanged(double value) => _draft.UiScale = value;

    public void BeginCapture(string propertyName) => _capturingProperty = propertyName;

    public bool TryCaptureKey(Key key, ModifierKeys modifiers)
    {
        if (_capturingProperty is null) return false;
        if (key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl
            or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin or Key.System)
        {
            return true;
        }

        var vk = KeyInterop.VirtualKeyFromKey(key);
        var mods = 0;
        if (modifiers.HasFlag(ModifierKeys.Control)) mods |= 0x0002;
        if (modifiers.HasFlag(ModifierKeys.Shift)) mods |= 0x0004;
        if (modifiers.HasFlag(ModifierKeys.Alt)) mods |= 0x0001;
        if (modifiers.HasFlag(ModifierKeys.Windows)) mods |= 0x0008;

        var binding = new HotkeyBinding(vk, mods);
        switch (_capturingProperty)
        {
            case nameof(StepHotkeyText): _draft.StepHotkey = binding; break;
            case nameof(DemonHotkeyText): _draft.DemonCooldownHotkey = binding; break;
            case nameof(IncenseHotkeyText): _draft.IncenseTimerHotkey = binding; break;
            case nameof(ObamboHotkeyText): _draft.ObamboCycleHotkey = binding; break;
        }

        _capturingProperty = null;
        RefreshHotkeyLabels();
        ValidationMessage = null;
        return true;
    }

    [RelayCommand]
    private void Save()
    {
        ValidationMessage = null;

        if (!double.TryParse(GhostSpeedPercentText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
            && !double.TryParse(GhostSpeedPercentText, NumberStyles.Float, CultureInfo.CurrentCulture, out percent))
        {
            ValidationMessage = "Informe uma porcentagem de velocidade válida.";
            return;
        }

        if (percent <= 0)
        {
            ValidationMessage = "A porcentagem de velocidade deve ser maior que 0.";
            return;
        }

        _draft.GhostSpeedPercent = percent;
        _draft.Opacity = Opacity;
        _draft.UiScale = UiScale;

        if (HasHotkeyConflict(out var conflictMsg))
        {
            ValidationMessage = conflictMsg;
            return;
        }

        var live = _settingsService.Current;
        _draft.Left = live.Left;
        _draft.Top = live.Top;
        _draft.Width = live.Width;
        _draft.Height = live.Height;

        _settingsService.SaveImmediate(_draft);
        Saved = true;
    }

    private bool HasHotkeyConflict(out string message)
    {
        var pairs = new (string Name, HotkeyBinding Binding)[]
        {
            ("Passo", _draft.StepHotkey),
            ("Demônio", _draft.DemonCooldownHotkey),
            ("Incenso", _draft.IncenseTimerHotkey),
            ("Obambo", _draft.ObamboCycleHotkey)
        };

        for (var i = 0; i < pairs.Length; i++)
        {
            for (var j = i + 1; j < pairs.Length; j++)
            {
                if (pairs[i].Binding.SameAs(pairs[j].Binding))
                {
                    message = $"Hotkey em conflito: {pairs[i].Name} e {pairs[j].Name}.";
                    return true;
                }
            }
        }

        message = "";
        return false;
    }

    private void RefreshHotkeyLabels()
    {
        StepHotkeyText = HotkeyDisplayHelper.Format(_draft.StepHotkey.VirtualKey, _draft.StepHotkey.Modifiers);
        DemonHotkeyText = HotkeyDisplayHelper.Format(_draft.DemonCooldownHotkey.VirtualKey, _draft.DemonCooldownHotkey.Modifiers);
        IncenseHotkeyText = HotkeyDisplayHelper.Format(_draft.IncenseTimerHotkey.VirtualKey, _draft.IncenseTimerHotkey.Modifiers);
        ObamboHotkeyText = HotkeyDisplayHelper.Format(_draft.ObamboCycleHotkey.VirtualKey, _draft.ObamboCycleHotkey.Modifiers);
    }
}
