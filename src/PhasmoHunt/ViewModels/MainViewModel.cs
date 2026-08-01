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
    private static readonly TimeSpan DemonCooldownThreshold = TimeSpan.FromSeconds(25);
    private static readonly TimeSpan ObamboStartOffset = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan ObamboCycleLength = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan IncenseDuration = TimeSpan.FromMinutes(3);

    private readonly SpeedCalculatorService _calculator = new();
    private readonly GhostCatalogService _ghostCatalog = new();
    private readonly SessionComparisonService _sessionComparison = new();
    private readonly SettingsService _settingsService;
    private readonly HotkeyService _hotkeyService;
    private readonly List<TimeSpan> _stepTimestamps = new();
    private readonly List<SpeedMeasurement> _sessionReadings = new();
    private readonly Stopwatch _sessionWatch = new();
    private readonly Stopwatch _demonCooldownWatch = new();
    private readonly Stopwatch _obamboWatch = new();
    private readonly Stopwatch _incenseWatch = new();
    private readonly DispatcherTimer _uiTimer;
    private readonly DispatcherTimer _idleTimer;
    private readonly DispatcherTimer _peculiarityUiTimer;

    private bool _disposed;
    private System.Windows.Media.ImageSource? _peaceIcon;
    private System.Windows.Media.ImageSource? _angryIcon;

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

        _peculiarityUiTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _peculiarityUiTimer.Tick += OnPeculiarityUiTimerTick;

        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        RefreshHotkeyLabels();
        DemonIcon = LoadPeculiarityIcon("demon-icon.png");
        _peaceIcon = LoadPeculiarityIcon("peace-icon.png");
        _angryIcon = LoadPeculiarityIcon("angry-icon.png");
        DemonCooldownText = "--:--";
        IsDemonCooldownSelected = false;
        IsDemonCooldownRunning = false;
        ObamboIcon = _peaceIcon;
        ObamboTimerText = "--:--";
        IsObamboRunning = false;
        IncenseIcon = LoadPeculiarityIcon("incense-icon.png");
        IncenseTimerText = "--:--";
        IsIncenseRunning = false;

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
    [ObservableProperty] private string _stepHotkeyLabel = "Botão lateral / 1";
    [ObservableProperty] private bool _isSettingsExpanded;
    [ObservableProperty] private bool _isEvidencePanelOpen;
    [ObservableProperty] private bool _isUiCompact;
    [ObservableProperty] private bool _hasSelectedEvidence;
    [ObservableProperty] private System.Windows.Media.ImageSource? _demonIcon;
    [ObservableProperty] private string _demonCooldownText = "--:--";
    [ObservableProperty] private bool _isDemonCooldownSelected;
    [ObservableProperty] private bool _isDemonCooldownRunning;
    [ObservableProperty] private System.Windows.Media.ImageSource? _obamboIcon;
    [ObservableProperty] private string _obamboTimerText = "--:--";
    [ObservableProperty] private bool _isObamboRunning;
    [ObservableProperty] private System.Windows.Media.ImageSource? _incenseIcon;
    [ObservableProperty] private string _incenseTimerText = "--:--";
    [ObservableProperty] private bool _isIncenseRunning;

    public string CompactToggleGlyph => IsUiCompact ? "▢" : "─";

    public bool ShowEvidencePanel => IsEvidencePanelOpen && !IsUiCompact;

    public bool ShowSettingsPanel => IsSettingsExpanded && !IsUiCompact;

    public bool CanFinish =>
        SessionState == MeasurementSessionState.Running
        && SpeedCalculatorService.CanCalculate(StepCount);

    partial void OnIsUiCompactChanged(bool value)
    {
        OnPropertyChanged(nameof(CompactToggleGlyph));
        OnPropertyChanged(nameof(ShowEvidencePanel));
        OnPropertyChanged(nameof(ShowSettingsPanel));
    }

    partial void OnIsEvidencePanelOpenChanged(bool value) => OnPropertyChanged(nameof(ShowEvidencePanel));

    partial void OnIsSettingsExpandedChanged(bool value) => OnPropertyChanged(nameof(ShowSettingsPanel));

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
    private void ToggleCompact()
    {
        IsUiCompact = !IsUiCompact;
        if (IsUiCompact)
        {
            IsSettingsExpanded = false;
            IsEvidencePanelOpen = false;
        }
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
        ElapsedText = "0.00 s";
        ResetResultDisplay();

        foreach (var option in EvidenceOptions)
        {
            option.IsSelected = false;
        }

        RefreshEvidenceSummary();

        _sessionReadings.Clear();
        History.Clear();
        RefreshSessionComparison();
        ResetDemonCooldown();
        ResetObamboCycle();
        ResetIncenseTimer();
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
        _peculiarityUiTimer.Stop();
        _uiTimer.Tick -= OnUiTimerTick;
        _idleTimer.Tick -= OnIdleTimerTick;
        _peculiarityUiTimer.Tick -= OnPeculiarityUiTimerTick;
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
            StepCount = 0;
            _stepTimestamps.Clear();
            ElapsedText = "0.00 s";
            return;
        }
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
                case HotkeyAction.Step:
                    RecordStepCommand.Execute(null);
                    break;
                case HotkeyAction.DemonCooldown:
                    ToggleDemonCooldown();
                    break;
                case HotkeyAction.ObamboCycle:
                    StartObamboCycle();
                    break;
                case HotkeyAction.IncenseTimer:
                    StartOrResetIncenseTimer();
                    break;
            }
        });
    }

    private void ToggleDemonCooldown()
    {
        if (!_demonCooldownWatch.IsRunning)
        {
            IsDemonCooldownSelected = false;
            _demonCooldownWatch.Restart();
            IsDemonCooldownRunning = true;
            DemonCooldownText = FormatMmSs(TimeSpan.Zero);
            EnsurePeculiarityUiTimer();
            return;
        }

        _demonCooldownWatch.Stop();
        IsDemonCooldownRunning = false;
        RefreshPeculiarityUiTimer();

        var elapsed = _demonCooldownWatch.Elapsed;
        DemonCooldownText = FormatMmSs(elapsed);
        IsDemonCooldownSelected = elapsed < DemonCooldownThreshold;
        RefreshGhostEligibility();
    }

    private void StartObamboCycle()
    {
        if (_obamboWatch.IsRunning)
        {
            return;
        }

        _obamboWatch.Restart();
        IsObamboRunning = true;
        UpdateObamboDisplay();
        EnsurePeculiarityUiTimer();
    }

    private void StartOrResetIncenseTimer()
    {
        _incenseWatch.Restart();
        IsIncenseRunning = true;
        UpdateIncenseDisplay();
        EnsurePeculiarityUiTimer();
    }

    private void ResetDemonCooldown()
    {
        _demonCooldownWatch.Reset();
        IsDemonCooldownRunning = false;
        IsDemonCooldownSelected = false;
        DemonCooldownText = "--:--";
        RefreshPeculiarityUiTimer();
    }

    private void ResetObamboCycle()
    {
        _obamboWatch.Reset();
        IsObamboRunning = false;
        ObamboTimerText = "--:--";
        ObamboIcon = _peaceIcon;
        RefreshPeculiarityUiTimer();
    }

    private void ResetIncenseTimer()
    {
        _incenseWatch.Reset();
        IsIncenseRunning = false;
        IncenseTimerText = "--:--";
        RefreshPeculiarityUiTimer();
    }

    private void EnsurePeculiarityUiTimer()
    {
        if (!_peculiarityUiTimer.IsEnabled
            && (_demonCooldownWatch.IsRunning || _obamboWatch.IsRunning || _incenseWatch.IsRunning))
        {
            _peculiarityUiTimer.Start();
        }
    }

    private void RefreshPeculiarityUiTimer()
    {
        if (_demonCooldownWatch.IsRunning || _obamboWatch.IsRunning || _incenseWatch.IsRunning)
        {
            EnsurePeculiarityUiTimer();
            return;
        }

        _peculiarityUiTimer.Stop();
    }

    private void OnPeculiarityUiTimerTick(object? sender, EventArgs e)
    {
        if (_demonCooldownWatch.IsRunning)
        {
            DemonCooldownText = FormatMmSs(_demonCooldownWatch.Elapsed);
        }

        if (_obamboWatch.IsRunning)
        {
            UpdateObamboDisplay();
        }

        if (_incenseWatch.IsRunning)
        {
            UpdateIncenseDisplay();
        }
    }

    private void UpdateIncenseDisplay()
    {
        var remaining = IncenseDuration - _incenseWatch.Elapsed;
        if (remaining <= TimeSpan.Zero)
        {
            remaining = TimeSpan.Zero;
            _incenseWatch.Stop();
            IsIncenseRunning = false;
            IncenseTimerText = FormatMmSs(remaining);
            RefreshPeculiarityUiTimer();
            return;
        }

        IncenseTimerText = FormatMmSs(remaining);
    }

    private void UpdateObamboDisplay()
    {
        // Começa em 01:00 (meio do ciclo calmo) e sobe sem parar até Limpar.
        var displayElapsed = ObamboStartOffset + _obamboWatch.Elapsed;
        ObamboTimerText = FormatMmSs(displayElapsed);
        ObamboIcon = IsObamboAngry(displayElapsed) ? _angryIcon : _peaceIcon;
    }

    private static bool IsObamboAngry(TimeSpan displayElapsed)
    {
        // 01:00–02:00 peace; a partir de 02:00 alterna a cada 2 minutos (angry, peace, angry…).
        if (displayElapsed < ObamboCycleLength)
        {
            return false;
        }

        var cyclesSinceAngryStart = (int)((displayElapsed - ObamboCycleLength) / ObamboCycleLength);
        return cyclesSinceAngryStart % 2 == 0;
    }

    private static string FormatMmSs(TimeSpan value)
    {
        var totalSeconds = (int)Math.Floor(value.TotalSeconds);
        if (totalSeconds < 0)
        {
            totalSeconds = 0;
        }

        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    private static System.Windows.Media.ImageSource? LoadPeculiarityIcon(string fileName)
    {
        try
        {
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri($"pack://application:,,,/Assets/Peculiarities/{fileName}", UriKind.Absolute);
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
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
        var hasPeculiarityFilter = IsDemonCooldownSelected;

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

        if (!hasSpeedFilter && !hasEvidenceFilter && !hasPeculiarityFilter)
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
            var peculiarityOk = !hasPeculiarityFilter || item.Ghost.Id.Equals("demon", StringComparison.OrdinalIgnoreCase);
            item.SetEligibility(speedOk && evidenceOk && peculiarityOk);
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
        HasSelectedEvidence = EvidenceOptions.Any(o => o.IsSelected);
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
        _hotkeyService.RegisterStepHotkeys();
        RefreshHotkeyLabels();
    }

    private void RefreshHotkeyLabels()
    {
        StepHotkeyLabel = "Botão lateral / 1";
    }
}
