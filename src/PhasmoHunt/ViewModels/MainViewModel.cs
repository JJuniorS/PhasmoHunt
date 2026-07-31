using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhasmoHunt.Models;
using PhasmoHunt.Services;

namespace PhasmoHunt.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private const int MaxHistoryItems = 50;
    private static readonly TimeSpan IdleFinalizeDelay = TimeSpan.FromSeconds(3);

    private readonly SpeedCalculatorService _calculator = new();
    private readonly GhostCatalogService _ghostCatalog = new();
    private readonly SessionComparisonService _sessionComparison = new();
    private readonly SettingsService _settingsService;
    private readonly HotkeyService _hotkeyService;
    private readonly List<TimeSpan> _stepTimestamps = new();
    private readonly List<SpeedMeasurement> _sessionReadings = new();
    private readonly Stopwatch _sessionWatch = new();
    private readonly DispatcherTimer _uiTimer;
    private readonly DispatcherTimer _idleTimer;

    private bool _disposed;

    public MainViewModel(SettingsService settingsService, HotkeyService hotkeyService)
    {
        _settingsService = settingsService;
        _hotkeyService = hotkeyService;

        var settings = _settingsService.Load();
        Opacity = settings.Opacity;
        UiScale = settings.UiScale;

        _uiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _uiTimer.Tick += OnUiTimerTick;

        _idleTimer = new DispatcherTimer { Interval = IdleFinalizeDelay };
        _idleTimer.Tick += OnIdleTimerTick;

        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        RefreshHotkeyLabels();

        foreach (var ghost in _ghostCatalog.GetAll().OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase))
        {
            Ghosts.Add(new GhostListItemViewModel(ghost));
        }

        CatalogCountText = $"{Ghosts.Count} fantasmas no catálogo";
        foreach (var evidence in EvidenceTypeExtensions.All)
        {
            EvidenceOptions.Add(new EvidenceOptionViewModel(evidence, OnEvidenceSelectionChanged));
        }

        ResetDisplay();
        RefreshSessionComparison();
        RefreshGhostEligibility();
    }

    public ObservableCollection<HistoryItemViewModel> History { get; } = new();
    public ObservableCollection<GhostListItemViewModel> Ghosts { get; } = new();
    public ObservableCollection<EvidenceOptionViewModel> EvidenceOptions { get; } = new();

    [ObservableProperty] private MeasurementSessionState _sessionState = MeasurementSessionState.Idle;
    [ObservableProperty] private string _statusText = "Pronto — clique para contar; 3s sem clique finaliza";
    [ObservableProperty] private string _elapsedText = "0.00 s";
    [ObservableProperty] private string _totalTimeText = "—";
    [ObservableProperty] private string _stepCountText = "0 passos";
    [ObservableProperty] private int _stepCount;
    [ObservableProperty] private string _speedText = "—";
    [ObservableProperty] private string _confidenceText = "—";
    [ObservableProperty] private string _stepsPerSecondText = "—";
    [ObservableProperty] private string _averageIntervalText = "—";
    [ObservableProperty] private string _part1Text = "—";
    [ObservableProperty] private string _part2Text = "—";
    [ObservableProperty] private string _part3Text = "—";
    [ObservableProperty] private string _patternText = "—";
    [ObservableProperty] private string _compatibleGhostsText = "—";
    [ObservableProperty] private string _sessionSummaryText = "Nenhuma leitura na sessão.";
    [ObservableProperty] private string _sessionSpeedsText = "—";
    [ObservableProperty] private string _sessionGhostsText = "—";
    [ObservableProperty] private string _catalogCountText = "";
    [ObservableProperty] private double _opacity = 0.92;
    [ObservableProperty] private double _uiScale = 1.0;
    [ObservableProperty] private string _startHotkeyLabel = "F8";
    [ObservableProperty] private string _stepHotkeyLabel = "Botão lateral / 1";
    [ObservableProperty] private string _finishHotkeyLabel = "Enter";
    [ObservableProperty] private bool _isSettingsExpanded;
    [ObservableProperty] private bool _isEvidencePanelOpen;
    [ObservableProperty] private string _selectedEvidenceSummary = "Nenhuma evidência marcada";

    public bool CanFinish =>
        SessionState == MeasurementSessionState.Running
        && SpeedCalculatorService.CanCalculate(StepCount);

    partial void OnOpacityChanged(double value)
    {
        var settings = _settingsService.Current;
        settings.Opacity = value;
        _settingsService.SaveDebounced(settings);
    }

    partial void OnUiScaleChanged(double value)
    {
        var settings = _settingsService.Current;
        settings.UiScale = value;
        _settingsService.SaveDebounced(settings);
    }

    partial void OnSessionStateChanged(MeasurementSessionState value)
    {
        OnPropertyChanged(nameof(CanFinish));
        StartCommand.NotifyCanExecuteChanged();
        RecordStepCommand.NotifyCanExecuteChanged();
        FinishCommand.NotifyCanExecuteChanged();
    }

    partial void OnStepCountChanged(int value)
    {
        var perPart = SpeedCalculatorService.StepsPerSegment(value);
        var usable = SpeedCalculatorService.UsableStepCount(value);
        StepCountText = perPart >= SpeedCalculatorService.MinStepsPerSegment
            ? $"{value} passos · {perPart}/parte ({usable} usados)"
            : $"{value} passos · mín. {SpeedCalculatorService.MinTotalSteps}";
        OnPropertyChanged(nameof(CanFinish));
        FinishCommand.NotifyCanExecuteChanged();
    }

    public void AttachHotkeys(Window window)
    {
        _hotkeyService.Attach(window);
        ApplyHotkeysFromSettings();
    }

    public void PersistWindowBounds(double left, double top, double width, double height)
    {
        var settings = _settingsService.Current;
        settings.Left = left;
        settings.Top = top;
        settings.Width = width;
        settings.Height = height;
        _settingsService.SaveDebounced(settings);
    }

    public AppSettings GetSettingsSnapshot() => _settingsService.Current;

    [RelayCommand]
    private void ToggleSettings() => IsSettingsExpanded = !IsSettingsExpanded;

    [RelayCommand]
    private void ToggleEvidencePanel() => IsEvidencePanelOpen = !IsEvidencePanelOpen;

    [RelayCommand]
    private void ClearEvidence()
    {
        foreach (var option in EvidenceOptions)
        {
            option.IsSelected = false;
        }

        RefreshEvidenceSummary();
        RefreshGhostEligibility();
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start() => BeginRunningSession(recordFirstStep: false);

    private bool CanStart() => SessionState != MeasurementSessionState.Running;

    [RelayCommand]
    private void RecordStep()
    {
        if (SessionState != MeasurementSessionState.Running)
        {
            BeginRunningSession(recordFirstStep: true);
            return;
        }

        AddStepTimestamp();
        ResetIdleTimer();
    }

    [RelayCommand(CanExecute = nameof(CanFinishExecute))]
    private void Finish() => TryFinalizeReading();

    private bool CanFinishExecute() => CanFinish;

    [RelayCommand]
    private void Clear()
    {
        StopTimers();
        _sessionWatch.Reset();
        _stepTimestamps.Clear();
        StepCount = 0;
        SessionState = MeasurementSessionState.Idle;
        StatusText = "Pronto — clique para contar; 3s sem clique finaliza";
        ElapsedText = "0.00 s";
        ResetResultDisplay();
    }

    [RelayCommand]
    private void ClearHistory()
    {
        _sessionReadings.Clear();
        History.Clear();
        RefreshSessionComparison();
        RefreshGhostEligibility();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopTimers();
        _uiTimer.Tick -= OnUiTimerTick;
        _idleTimer.Tick -= OnIdleTimerTick;
        _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
        _hotkeyService.Dispose();
    }

    private void BeginRunningSession(bool recordFirstStep)
    {
        _stepTimestamps.Clear();
        StepCount = 0;
        ResetResultDisplay();
        _sessionWatch.Restart();
        SessionState = MeasurementSessionState.Running;
        StatusText = "Contando… pare 3s para finalizar";
        ElapsedText = "0.00 s";
        _uiTimer.Start();

        if (recordFirstStep)
        {
            AddStepTimestamp();
            ResetIdleTimer();
        }
        else
        {
            _idleTimer.Stop();
        }
    }

    private void AddStepTimestamp()
    {
        _stepTimestamps.Add(_sessionWatch.Elapsed);
        StepCount = _stepTimestamps.Count;

        var perPart = SpeedCalculatorService.StepsPerSegment(StepCount);
        StatusText = perPart >= SpeedCalculatorService.MinStepsPerSegment
            ? $"Passo {StepCount} · {perPart}/parte — aguarde 3s p/ fechar"
            : $"Passo {StepCount} · precisa ≥{SpeedCalculatorService.MinTotalSteps} — aguarde 3s p/ fechar";
    }

    private void ResetIdleTimer()
    {
        _idleTimer.Stop();
        _idleTimer.Interval = IdleFinalizeDelay;
        _idleTimer.Start();
    }

    private void OnIdleTimerTick(object? sender, EventArgs e)
    {
        _idleTimer.Stop();
        if (SessionState == MeasurementSessionState.Running)
        {
            TryFinalizeReading();
        }
    }

    private void TryFinalizeReading()
    {
        if (SessionState != MeasurementSessionState.Running)
        {
            return;
        }

        StopTimers();
        _sessionWatch.Stop();

        if (!SpeedCalculatorService.CanCalculate(_stepTimestamps.Count))
        {
            SessionState = MeasurementSessionState.Idle;
            StatusText =
                $"Poucos passos ({_stepTimestamps.Count}). Mínimo {SpeedCalculatorService.MinTotalSteps} (floor(n/3)≥2).";
            StepCount = 0;
            _stepTimestamps.Clear();
            ElapsedText = "0.00 s";
            return;
        }

        var discarded = _stepTimestamps.Count - SpeedCalculatorService.UsableStepCount(_stepTimestamps.Count);
        var measurement = _calculator.Calculate(_stepTimestamps, _ghostCatalog.GetAll());
        ApplyMeasurement(measurement);

        _sessionReadings.Insert(0, measurement);
        while (_sessionReadings.Count > MaxHistoryItems)
        {
            _sessionReadings.RemoveAt(_sessionReadings.Count - 1);
        }

        RebuildHistory();
        RefreshSessionComparison();
        RefreshGhostEligibility();

        SessionState = MeasurementSessionState.Completed;
        var per = measurement.Segments[0].EndStep - measurement.Segments[0].StartStep + 1;
        StatusText = discarded > 0
            ? $"Concluído · {per}/parte (descartou {discarded}) — clique p/ nova"
            : $"Concluído · {per} passos/parte — clique p/ nova";
    }

    private void StopTimers()
    {
        _uiTimer.Stop();
        _idleTimer.Stop();
    }

    private void OnUiTimerTick(object? sender, EventArgs e)
    {
        ElapsedText = $"{_sessionWatch.Elapsed.TotalSeconds:F2} s";
    }

    private void OnHotkeyPressed(HotkeyAction action)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            switch (action)
            {
                case HotkeyAction.Start when StartCommand.CanExecute(null):
                    StartCommand.Execute(null);
                    break;
                case HotkeyAction.Step:
                    RecordStepCommand.Execute(null);
                    break;
                case HotkeyAction.Finish when FinishCommand.CanExecute(null):
                    FinishCommand.Execute(null);
                    break;
            }
        });
    }

    private void ApplyMeasurement(SpeedMeasurement measurement)
    {
        SpeedText = $"{measurement.EstimatedSpeedMps:F2} m/s";
        ConfidenceText = $"{measurement.ConfidencePercent:F0}%";
        TotalTimeText = $"{measurement.TotalTime.TotalSeconds:F2} s";
        StepsPerSecondText = $"{measurement.StepsPerSecond:F2}";
        AverageIntervalText = $"{measurement.AverageIntervalSeconds:F3} s";
        Part1Text = FormatPart(measurement.Segments[0]);
        Part2Text = FormatPart(measurement.Segments[1]);
        Part3Text = FormatPart(measurement.Segments[2]);
        PatternText = measurement.PatternText;
        CompatibleGhostsText = measurement.CompatibleGhosts.Count == 0
            ? "Nenhum no catálogo atual"
            : string.Join(", ", measurement.CompatibleGhosts.Select(g => $"{g.Name} ({g.SpeedRangeText})"));
        ElapsedText = TotalTimeText;
        StepCount = measurement.StepTimestamps.Count;
    }

    private static string FormatPart(SpeedSegment segment) =>
        $"{segment.EstimatedSpeedMps:F2} m/s";

    private void RebuildHistory()
    {
        History.Clear();
        var total = _sessionReadings.Count;
        for (var i = 0; i < _sessionReadings.Count; i++)
        {
            History.Add(new HistoryItemViewModel(_sessionReadings[i], total - i));
        }
    }

    private void RefreshSessionComparison()
    {
        var result = _sessionComparison.Compare(_sessionReadings);
        SessionSummaryText = result.SummaryText;
        SessionSpeedsText = result.SpeedsText;
        SessionGhostsText = result.CommonGhostsText;
    }

    private void RefreshGhostEligibility()
    {
        var selectedEvidence = EvidenceOptions
            .Where(o => o.IsSelected)
            .Select(o => o.Type)
            .ToArray();

        var hasSpeedFilter = _sessionReadings.Count > 0;
        var hasEvidenceFilter = selectedEvidence.Length > 0;

        HashSet<string>? speedEligibleIds = null;
        if (hasSpeedFilter)
        {
            speedEligibleIds = _sessionReadings
                .Select(r => r.CompatibleGhosts.Select(g => g.Id).ToHashSet(StringComparer.OrdinalIgnoreCase))
                .Aggregate((a, b) =>
                {
                    a.IntersectWith(b);
                    return a;
                });
        }

        if (!hasSpeedFilter && !hasEvidenceFilter)
        {
            foreach (var item in Ghosts)
            {
                item.SetEligibility(null);
            }

            ResortGhosts();
            return;
        }

        foreach (var item in Ghosts)
        {
            var speedOk = !hasSpeedFilter || speedEligibleIds!.Contains(item.Ghost.Id);
            var evidenceOk = !hasEvidenceFilter || item.Ghost.MatchesEvidence(selectedEvidence);
            item.SetEligibility(speedOk && evidenceOk);
        }

        ResortGhosts();
    }

    private void OnEvidenceSelectionChanged()
    {
        RefreshEvidenceSummary();
        RefreshGhostEligibility();
    }

    private void RefreshEvidenceSummary()
    {
        var selected = EvidenceOptions.Where(o => o.IsSelected).Select(o => o.DisplayName).ToArray();
        SelectedEvidenceSummary = selected.Length == 0
            ? "Nenhuma evidência marcada"
            : string.Join(" · ", selected);
    }

    private void ResortGhosts()
    {
        var ordered = Ghosts
            .OrderByDescending(g => g.IsEligible == true)
            .ThenBy(g => g.IsEligible is null)
            .ThenBy(g => g.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Ghosts.Clear();
        foreach (var item in ordered)
        {
            Ghosts.Add(item);
        }
    }

    private void ResetDisplay()
    {
        ResetResultDisplay();
        StepCount = 0;
        ElapsedText = "0.00 s";
    }

    private void ResetResultDisplay()
    {
        SpeedText = "—";
        ConfidenceText = "—";
        TotalTimeText = "—";
        StepsPerSecondText = "—";
        AverageIntervalText = "—";
        Part1Text = "—";
        Part2Text = "—";
        Part3Text = "—";
        PatternText = "—";
        CompatibleGhostsText = "—";
    }

    private void ApplyHotkeysFromSettings()
    {
        var s = _settingsService.Current;
        _hotkeyService.RegisterAll(
            s.StartHotkey.VirtualKey, s.StartHotkey.Modifiers,
            s.StepHotkey.VirtualKey, s.StepHotkey.Modifiers,
            s.FinishHotkey.VirtualKey, s.FinishHotkey.Modifiers);
        RefreshHotkeyLabels();
    }

    private void RefreshHotkeyLabels()
    {
        var s = _settingsService.Current;
        StartHotkeyLabel = HotkeyDisplayHelper.Format(s.StartHotkey.VirtualKey, s.StartHotkey.Modifiers);
        var stepPrimary = HotkeyDisplayHelper.Format(s.StepHotkey.VirtualKey, s.StepHotkey.Modifiers);
        StepHotkeyLabel = s.StepHotkey.VirtualKey == HotkeyService.VkDigit1 && s.StepHotkey.Modifiers == 0
            ? stepPrimary
            : $"{stepPrimary} / 1";
        FinishHotkeyLabel = HotkeyDisplayHelper.Format(s.FinishHotkey.VirtualKey, s.FinishHotkey.Modifiers);
    }
}
