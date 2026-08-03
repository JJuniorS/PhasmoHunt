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
        SyncLanguageFlags();
        RefreshHotkeyLabels();
        LocalizationService.Instance.LanguageChanged += OnLanguageChanged;
    }

    public bool Saved { get; private set; }

    public LocalizationService Loc => LocalizationService.Instance;

    [ObservableProperty] private double _opacity;
    [ObservableProperty] private double _uiScale;
    [ObservableProperty] private string _ghostSpeedPercentText = "100";
    [ObservableProperty] private string _stepHotkeyText = "";
    [ObservableProperty] private string _demonHotkeyText = "";
    [ObservableProperty] private string _incenseHotkeyText = "";
    [ObservableProperty] private string _obamboHotkeyText = "";
    [ObservableProperty] private string? _validationMessage;
    [ObservableProperty] private bool _isLanguagePtBr;
    [ObservableProperty] private bool _isLanguageEn;

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
        if (binding.SameAs(HotkeyService.FixedClearHotkey))
        {
            ValidationMessage = Loc.T("hotkey_reserved_clear");
            _capturingProperty = null;
            return true;
        }

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
    private void SelectLanguage(string? code)
    {
        var language = LocalizationService.Parse(code);
        _draft.Language = LocalizationService.ToCode(language);
        LocalizationService.Instance.SetLanguage(language);
        SyncLanguageFlags();
        RefreshHotkeyLabels();
    }

    [RelayCommand]
    private void Save()
    {
        ValidationMessage = null;
        var loc = LocalizationService.Instance;

        if (!double.TryParse(GhostSpeedPercentText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
            && !double.TryParse(GhostSpeedPercentText, NumberStyles.Float, CultureInfo.CurrentCulture, out percent))
        {
            ValidationMessage = loc.T("invalid_speed_percent");
            return;
        }

        if (percent <= 0)
        {
            ValidationMessage = loc.T("speed_percent_gt_zero");
            return;
        }

        _draft.GhostSpeedPercent = percent;
        _draft.Opacity = Opacity;
        _draft.UiScale = UiScale;
        _draft.Language = LocalizationService.ToCode(LocalizationService.Instance.Language);

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

    public void Detach() =>
        LocalizationService.Instance.LanguageChanged -= OnLanguageChanged;

    private void OnLanguageChanged()
    {
        SyncLanguageFlags();
        RefreshHotkeyLabels();
        OnPropertyChanged(nameof(Loc));
    }

    private void SyncLanguageFlags()
    {
        var lang = LocalizationService.Parse(_draft.Language);
        // Prefer live service (immediate flag switch) when already applied.
        lang = LocalizationService.Instance.Language;
        IsLanguagePtBr = lang == AppLanguage.PtBr;
        IsLanguageEn = lang == AppLanguage.En;
        _draft.Language = LocalizationService.ToCode(lang);
    }

    private bool HasHotkeyConflict(out string message)
    {
        var loc = LocalizationService.Instance;
        var pairs = new (string Name, HotkeyBinding Binding)[]
        {
            (loc.Step, _draft.StepHotkey),
            (loc.Demon, _draft.DemonCooldownHotkey),
            (loc.Incense, _draft.IncenseTimerHotkey),
            (loc.Obambo, _draft.ObamboCycleHotkey)
        };

        var clear = HotkeyService.FixedClearHotkey;
        foreach (var pair in pairs)
        {
            if (pair.Binding.SameAs(clear))
            {
                message = loc.T("hotkey_reserved_clear");
                return true;
            }
        }

        for (var i = 0; i < pairs.Length; i++)
        {
            for (var j = i + 1; j < pairs.Length; j++)
            {
                if (pairs[i].Binding.SameAs(pairs[j].Binding))
                {
                    message = loc.Format("hotkey_conflict", pairs[i].Name, pairs[j].Name);
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
