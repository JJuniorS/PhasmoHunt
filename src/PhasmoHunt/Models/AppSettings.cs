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

    // Hotkeys de passo são fixas: tecla 1 + botão lateral (XBUTTON1).
    // Campos abaixo mantidos só para compatibilidade com settings.json antigo.
    public HotkeyBinding? StartHotkey { get; set; }
    public HotkeyBinding? StepHotkey { get; set; }
    public HotkeyBinding? FinishHotkey { get; set; }
}
