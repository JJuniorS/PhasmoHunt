namespace PhasmoHunt.Models;

public sealed class AppSettings
{
    public double Left { get; set; } = 120;
    public double Top { get; set; } = 120;
    public double Width { get; set; } = 420;
    public double Height { get; set; } = 640;
    public double Opacity { get; set; } = 0.92;
    public double UiScale { get; set; } = 1.0;
    public AppTheme Theme { get; set; } = AppTheme.Dark;

    // VK_F8 = 0x77, VK_XBUTTON1 = 0x05 (botão lateral atrás), VK_RETURN = 0x0D
    // Passo: botão lateral + tecla 1 (registrada sempre em HotkeyService).
    public HotkeyBinding StartHotkey { get; set; } = new(0x77);
    public HotkeyBinding StepHotkey { get; set; } = new(0x05);
    public HotkeyBinding FinishHotkey { get; set; } = new(0x0D);
}
