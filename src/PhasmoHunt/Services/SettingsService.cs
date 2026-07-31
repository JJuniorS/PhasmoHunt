using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PhasmoHunt.Models;

namespace PhasmoHunt.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _settingsPath;
    private readonly object _sync = new();
    private CancellationTokenSource? _debounceCts;

    public SettingsService()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PhasmoHunt");
        Directory.CreateDirectory(folder);
        _settingsPath = Path.Combine(folder, "settings.json");
    }

    public AppSettings Current { get; private set; } = new();

    public AppSettings Load()
    {
        lock (_sync)
        {
            if (!File.Exists(_settingsPath))
            {
                Current = new AppSettings();
                SaveImmediate(Current);
                return Current;
            }

            try
            {
                var json = File.ReadAllText(_settingsPath);
                Current = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch
            {
                Current = new AppSettings();
            }

            var previousStepVk = Current.StepHotkey?.VirtualKey;
            Normalize(Current);
            if (previousStepVk == 0x20 && Current.StepHotkey is { VirtualKey: HotkeyService.VkXButton1 })
            {
                SaveImmediate(Current);
            }

            return Current;
        }
    }

    public void SaveImmediate(AppSettings settings)
    {
        lock (_sync)
        {
            Normalize(settings);
            Current = settings;
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsPath, json);
        }
    }

    public void SaveDebounced(AppSettings settings, int delayMs = 400)
    {
        lock (_sync)
        {
            Current = settings;
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delayMs, token);
                    SaveImmediate(settings);
                }
                catch (OperationCanceledException)
                {
                }
            }, token);
        }
    }

    private static void Normalize(AppSettings settings)
    {
        settings.Opacity = Math.Clamp(settings.Opacity, 0.3, 1.0);
        settings.UiScale = Math.Clamp(settings.UiScale, 0.8, 1.5);
        settings.Width = Math.Max(settings.Width, 320);
        settings.Height = Math.Max(settings.Height, 420);
        settings.StartHotkey ??= new HotkeyBinding(0x77);
        settings.StepHotkey ??= new HotkeyBinding(HotkeyService.VkXButton1);
        settings.FinishHotkey ??= new HotkeyBinding(0x0D);

        // Migra default antigo (Espaço) para o botão lateral do mouse.
        if (settings.StepHotkey.VirtualKey == 0x20 && settings.StepHotkey.Modifiers == 0)
        {
            settings.StepHotkey = new HotkeyBinding(HotkeyService.VkXButton1);
        }
    }
}
