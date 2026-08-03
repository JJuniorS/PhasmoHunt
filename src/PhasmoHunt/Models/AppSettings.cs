namespace PhasmoHunt.Models;

public sealed class AppSettings
{
    public const int DefaultStepVk = 0x31;      // VK_1
    public const int DefaultDemonVk = 0x31;     // VK_1
    public const int DefaultIncenseVk = 0x32;   // VK_2
    public const int DefaultObamboVk = 0x33;    // VK_3
    public const int ModShift = 0x0004;

    public double Left { get; set; } = 120;
    public double Top { get; set; } = 120;
    public double Width { get; set; } = 420;
    public double Height { get; set; } = 640;
    public double Opacity { get; set; } = 0.92;
    public double UiScale { get; set; } = 1.0;
    public AppTheme Theme { get; set; } = AppTheme.Dark;

    public double GhostSpeedPercent { get; set; } = 100;

    /// <summary>UI language code: "pt-BR" (default) or "en".</summary>
    public string Language { get; set; } = "pt-BR";

    public HotkeyBinding StepHotkey { get; set; } = new(DefaultStepVk);
    public HotkeyBinding DemonCooldownHotkey { get; set; } = new(DefaultDemonVk, ModShift);
    public HotkeyBinding IncenseTimerHotkey { get; set; } = new(DefaultIncenseVk, ModShift);
    public HotkeyBinding ObamboCycleHotkey { get; set; } = new(DefaultObamboVk, ModShift);

    // Legacy — ignored by UI/registration
    public HotkeyBinding? StartHotkey { get; set; }
    public HotkeyBinding? FinishHotkey { get; set; }

    public AppSettings Clone()
    {
        EnsureHotkeyDefaults();
        return new()
        {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            Opacity = Opacity,
            UiScale = UiScale,
            Theme = Theme,
            GhostSpeedPercent = GhostSpeedPercent,
            Language = Language,
            StepHotkey = StepHotkey.Clone(),
            DemonCooldownHotkey = DemonCooldownHotkey.Clone(),
            IncenseTimerHotkey = IncenseTimerHotkey.Clone(),
            ObamboCycleHotkey = ObamboCycleHotkey.Clone(),
            StartHotkey = StartHotkey?.Clone(),
            FinishHotkey = FinishHotkey?.Clone()
        };
    }

    public void EnsureHotkeyDefaults()
    {
        StepHotkey = Coalesce(StepHotkey, DefaultStepVk, 0);
        DemonCooldownHotkey = Coalesce(DemonCooldownHotkey, DefaultDemonVk, ModShift);
        IncenseTimerHotkey = Coalesce(IncenseTimerHotkey, DefaultIncenseVk, ModShift);
        ObamboCycleHotkey = Coalesce(ObamboCycleHotkey, DefaultObamboVk, ModShift);
    }

    private static HotkeyBinding Coalesce(HotkeyBinding? binding, int vk, int mods) =>
        binding is null || binding.VirtualKey == 0 ? new HotkeyBinding(vk, mods) : binding;
}
