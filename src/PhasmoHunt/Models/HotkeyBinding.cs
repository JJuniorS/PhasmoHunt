namespace PhasmoHunt.Models;

public sealed class HotkeyBinding
{
    public int VirtualKey { get; set; }
    public int Modifiers { get; set; }

    public HotkeyBinding()
    {
    }

    public HotkeyBinding(int virtualKey, int modifiers = 0)
    {
        VirtualKey = virtualKey;
        Modifiers = modifiers;
    }

    public HotkeyBinding Clone() => new(VirtualKey, Modifiers);

    public bool SameAs(HotkeyBinding? other) =>
        other is not null && VirtualKey == other.VirtualKey && Modifiers == other.Modifiers;
}
