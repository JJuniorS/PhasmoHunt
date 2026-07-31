using System.Globalization;
using System.Windows.Input;

namespace PhasmoHunt.Services;

public static class HotkeyDisplayHelper
{
    public static string Format(int virtualKey, int modifiers)
    {
        if (HotkeyService.IsMouseSideButton(virtualKey))
        {
            return virtualKey == HotkeyService.VkXButton1
                ? "Botão lateral (atrás)"
                : "Botão lateral (frente)";
        }

        var parts = new List<string>();
        if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
        if ((modifiers & 0x0004) != 0) parts.Add("Shift");
        if ((modifiers & 0x0001) != 0) parts.Add("Alt");
        if ((modifiers & 0x0008) != 0) parts.Add("Win");
        parts.Add(KeyToDisplay(KeyInterop.KeyFromVirtualKey(virtualKey)));
        return string.Join(" + ", parts);
    }

    private static string KeyToDisplay(Key key) => key switch
    {
        Key.Space => "Espaço",
        Key.Return => "Enter",
        Key.Escape => "Esc",
        _ => key.ToString().ToUpper(CultureInfo.InvariantCulture)
    };
}
