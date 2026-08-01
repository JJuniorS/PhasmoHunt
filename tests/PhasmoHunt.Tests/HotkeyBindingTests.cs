using PhasmoHunt.Models;

namespace PhasmoHunt.Tests;

public class HotkeyBindingTests
{
    [Fact]
    public void SameAs_DetectsConflict()
    {
        var a = new HotkeyBinding(0x31, 0x0004);
        var b = new HotkeyBinding(0x31, 0x0004);
        var c = new HotkeyBinding(0x31, 0);
        Assert.True(a.SameAs(b));
        Assert.False(a.SameAs(c));
    }
}
