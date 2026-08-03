using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using PhasmoHunt.Models;

namespace PhasmoHunt.Services;

public enum HotkeyAction
{
    Step = 1,
    DemonCooldown = 2,
    ObamboCycle = 3,
    IncenseTimer = 4,
    /// <summary>Fixed Shift+L — not user-configurable.</summary>
    Clear = 5
}

/// <summary>
/// Global hotkeys registered from user settings.
/// Does not interact with any game process.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    public const int VkXButton1 = 0x05;
    public const int VkXButton2 = 0x06;
    /// <summary>VK_1–3 — teclado principal (não numpad).</summary>
    public const int VkDigit1 = 0x31;
    public const int VkDigit2 = 0x32;
    public const int VkDigit3 = 0x33;
    /// <summary>VK_L — fixed Clear hotkey with Shift.</summary>
    public const int VkL = 0x4C;
    public const int ModShift = 0x0004;

    public static HotkeyBinding FixedClearHotkey { get; } = new(VkL, ModShift);

    private const int WmHotkey = 0x0312;
    private const int WhMouseLl = 14;
    private const int WmXButtonDown = 0x020B;
    private const int XButton1 = 0x0001;
    private const int XButton2 = 0x0002;
    private const uint ModNoRepeat = 0x4000;

    private readonly Dictionary<int, HotkeyAction> _idToAction = new();
    private readonly Dictionary<int, HotkeyAction> _mouseVkToAction = new();

    private HwndSource? _source;
    private IntPtr _hwnd = IntPtr.Zero;
    private IntPtr _mouseHook = IntPtr.Zero;
    private LowLevelMouseProc? _mouseProc;
    private int _nextHotkeyId = 100;
    private bool _disposed;

    public event Action<HotkeyAction>? HotkeyPressed;

    public void Attach(Window window)
    {
        var helper = new WindowInteropHelper(window);
        helper.EnsureHandle();
        _hwnd = helper.Handle;
        _source = HwndSource.FromHwnd(_hwnd);
        _source?.AddHook(WndProc);
    }

    public IReadOnlyList<string> RegisterFromSettings(AppSettings settings)
    {
        UnregisterAll();
        settings.EnsureHotkeyDefaults();
        var loc = LocalizationService.Instance;
        var failed = new List<string>();

        // Fixed Clear first so user bindings lose on collision.
        if (!RegisterBinding(HotkeyAction.Clear, FixedClearHotkey.VirtualKey, FixedClearHotkey.Modifiers))
            failed.Add(loc.Clear + " (Shift + L)");

        if (!RegisterBinding(HotkeyAction.Step, settings.StepHotkey.VirtualKey, settings.StepHotkey.Modifiers))
            failed.Add(loc.Step);
        if (!RegisterBinding(HotkeyAction.DemonCooldown, settings.DemonCooldownHotkey.VirtualKey, settings.DemonCooldownHotkey.Modifiers))
            failed.Add(loc.Demon);
        if (!RegisterBinding(HotkeyAction.IncenseTimer, settings.IncenseTimerHotkey.VirtualKey, settings.IncenseTimerHotkey.Modifiers))
            failed.Add(loc.Incense);
        if (!RegisterBinding(HotkeyAction.ObamboCycle, settings.ObamboCycleHotkey.VirtualKey, settings.ObamboCycleHotkey.Modifiers))
            failed.Add(loc.Obambo);
        EnsureMouseHook();
        return failed;
    }

    public void UnregisterAll()
    {
        if (_hwnd != IntPtr.Zero)
        {
            foreach (var id in _idToAction.Keys.ToArray())
            {
                UnregisterHotKey(_hwnd, id);
            }
        }

        _idToAction.Clear();
        _mouseVkToAction.Clear();
        _nextHotkeyId = 100;
        RemoveMouseHook();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        UnregisterAll();
        _source?.RemoveHook(WndProc);
        _source = null;
    }

    private bool RegisterBinding(HotkeyAction action, int virtualKey, int modifiers)
    {
        if (IsMouseSideButton(virtualKey))
        {
            _mouseVkToAction[virtualKey] = action;
            return true;
        }

        if (_hwnd == IntPtr.Zero)
            return false;

        var id = _nextHotkeyId++;
        var mods = (uint)modifiers | ModNoRepeat;
        if (!RegisterHotKey(_hwnd, id, mods, (uint)virtualKey))
            return false;

        _idToAction[id] = action;
        return true;
    }

    private void EnsureMouseHook()
    {
        if (_mouseVkToAction.Count == 0 || _mouseHook != IntPtr.Zero)
        {
            return;
        }

        _mouseProc = MouseHookCallback;
        using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule!;
        _mouseHook = SetWindowsHookEx(WhMouseLl, _mouseProc, GetModuleHandle(curModule.ModuleName), 0);
    }

    private void RemoveMouseHook()
    {
        if (_mouseHook == IntPtr.Zero)
        {
            return;
        }

        UnhookWindowsHookEx(_mouseHook);
        _mouseHook = IntPtr.Zero;
        _mouseProc = null;
    }

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WmXButtonDown)
        {
            var info = Marshal.PtrToStructure<MsllHookStruct>(lParam);
            var xButton = (int)((info.MouseData >> 16) & 0xFFFF);
            var vk = xButton == XButton1 ? VkXButton1 : xButton == XButton2 ? VkXButton2 : 0;
            if (vk != 0 && _mouseVkToAction.TryGetValue(vk, out var action))
            {
                HotkeyPressed?.Invoke(action);
            }
        }

        return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey)
        {
            var id = wParam.ToInt32();
            if (_idToAction.TryGetValue(id, out var action))
            {
                HotkeyPressed?.Invoke(action);
                handled = true;
            }
        }

        return IntPtr.Zero;
    }

    public static bool IsMouseSideButton(int virtualKey) =>
        virtualKey is VkXButton1 or VkXButton2;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MsllHookStruct
    {
        public Point Point;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}
