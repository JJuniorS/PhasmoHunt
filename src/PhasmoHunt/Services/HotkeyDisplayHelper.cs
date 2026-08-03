using System.Globalization;
using System.Windows.Input;

namespace PhasmoHunt.Services;

public static class HotkeyDisplayHelper
{
    public static string Format(int virtualKey, int modifiers)
    {
        var loc = LocalizationService.Instance;
        if (HotkeyService.IsMouseSideButton(virtualKey))
        {
            return virtualKey == HotkeyService.VkXButton1
                ? loc.MouseSideBack
                : loc.MouseSideFront;
        }

        var parts = new List<string>();
        if ((modifiers & 0x0002) != 0) parts.Add("Ctrl");
        if ((modifiers & 0x0004) != 0) parts.Add("Shift");
        if ((modifiers & 0x0001) != 0) parts.Add("Alt");
        if ((modifiers & 0x0008) != 0) parts.Add("Win");
        parts.Add(KeyToDisplay(KeyInterop.KeyFromVirtualKey(virtualKey), loc));
        return string.Join(" + ", parts);
    }

    private static string KeyToDisplay(Key key, LocalizationService loc) => key switch
    {
        Key.Space => loc.KeySpace,
        Key.Return => "Enter",
        Key.Escape => "Esc",
        _ => key.ToString().ToUpper(CultureInfo.InvariantCulture)
    };
}
