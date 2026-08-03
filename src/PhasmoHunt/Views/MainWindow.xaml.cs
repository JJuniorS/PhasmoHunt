using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using PhasmoHunt.Services;
using PhasmoHunt.ViewModels;

namespace PhasmoHunt.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _settingsApplied;
    private double _expandedHeight = 820;
    private double _expandedMinHeight = 640;
    private WindowState _previousState = WindowState.Normal;
    private Rect _preFillBounds;
    private bool _isWorkAreaFilled;

    public MainWindow()
    {
        InitializeComponent();

        var settingsService = new SettingsService();
        var hotkeyService = new HotkeyService();
        _viewModel = new MainViewModel(settingsService, hotkeyService);
        DataContext = _viewModel;

        Loaded += OnLoaded;
        LocationChanged += OnBoundsChanged;
        SizeChanged += OnBoundsChanged;
        StateChanged += OnStateChanged;
        Activated += OnActivated;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var settings = _viewModel.GetSettingsSnapshot();
        if (!_settingsApplied)
        {
            Left = settings.Left;
            Top = settings.Top;
            Width = settings.Width;
            Height = settings.Height;
            Opacity = settings.Opacity;
            _expandedHeight = settings.Height;
            _settingsApplied = true;
        }

        _viewModel.AttachHotkeys(this);
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.Opacity))
            {
                Opacity = _viewModel.Opacity;
            }
            else if (args.PropertyName == nameof(MainViewModel.IsUiCompact))
            {
                ApplyCompactMode(_viewModel.IsUiCompact);
            }
        };
    }

    private void ApplyCompactMode(bool compact)
    {
        if (compact)
        {
            if (WindowState == WindowState.Normal)
            {
                _expandedHeight = Height;
            }

            _expandedMinHeight = MinHeight;
            GhostsRow.Height = new GridLength(0);
            MinHeight = 0;
            SizeToContent = SizeToContent.Height;
            Dispatcher.BeginInvoke(() =>
            {
                var target = ActualHeight;
                SizeToContent = SizeToContent.Manual;
                Height = target;
                MinHeight = Math.Max(80, target * 0.9);
            }, DispatcherPriority.Loaded);
            return;
        }

        SizeToContent = SizeToContent.Manual;
        GhostsRow.Height = new GridLength(1, GridUnitType.Star);
        MinHeight = _expandedMinHeight > 0 ? _expandedMinHeight : 640;
        Height = Math.Max(_expandedHeight, MinHeight);
    }

    private void OnBoundsChanged(object? sender, EventArgs e)
    {
        if (!_settingsApplied || WindowState != WindowState.Normal || _viewModel.IsUiCompact || _isWorkAreaFilled)
        {
            return;
        }

        // Ignore invalid bounds while the Win32 host is minimized or mid-transition.
        if (Left <= -10000 || Top <= -10000 || Width <= 0 || Height <= 0)
        {
            return;
        }

        _viewModel.PersistWindowBounds(Left, Top, Width, Height);
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        var previous = _previousState;
        _previousState = WindowState;

        // Borderless + AllowsTransparency: true Maximize is unreliable (layout / hit-test / chrome).
        if (WindowState == WindowState.Maximized)
        {
            Dispatcher.BeginInvoke(FillWorkAreaInsteadOfMaximize, DispatcherPriority.ApplicationIdle);
            return;
        }

        // Restore after minimize often drops Topmost and leaves the composition host stale.
        if (previous == WindowState.Minimized && WindowState == WindowState.Normal)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (_isWorkAreaFilled)
                {
                    var work = SystemParameters.WorkArea;
                    Left = work.Left;
                    Top = work.Top;
                    Width = work.Width;
                    Height = work.Height;
                }

                RefreshAfterRestore();
            }, DispatcherPriority.ApplicationIdle);
        }
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        if (WindowState != WindowState.Minimized)
        {
            EnsureTopmost();
        }
    }

    /// <summary>
    /// Fills the monitor work area while staying in <see cref="WindowState.Normal"/>
    /// so transparency / hit-testing keep working.
    /// </summary>
    private void FillWorkAreaInsteadOfMaximize()
    {
        if (WindowState != WindowState.Maximized)
        {
            return;
        }

        if (!_isWorkAreaFilled)
        {
            var rb = RestoreBounds;
            if (rb.Width > 0 && rb.Height > 0)
            {
                _preFillBounds = new Rect(rb.Left, rb.Top, rb.Width, rb.Height);
            }
            else
            {
                _preFillBounds = new Rect(Left, Top, Width, Height);
            }
        }

        WindowState = WindowState.Normal;
        var work = SystemParameters.WorkArea;
        Left = work.Left;
        Top = work.Top;
        Width = work.Width;
        Height = work.Height;
        _isWorkAreaFilled = true;
        _previousState = WindowState.Normal;
        RefreshAfterRestore();
    }

    private void ExitWorkAreaFillIfNeeded()
    {
        if (!_isWorkAreaFilled)
        {
            return;
        }

        _isWorkAreaFilled = false;
        if (_preFillBounds.Width > 0 && _preFillBounds.Height > 0)
        {
            Left = _preFillBounds.Left;
            Top = _preFillBounds.Top;
            Width = _preFillBounds.Width;
            Height = _preFillBounds.Height;
        }
    }

    private void RefreshAfterRestore()
    {
        EnsureTopmost();

        // Nudge layout so the DWM/transparent host resizes correctly after minimize.
        var w = Width;
        var h = Height;
        if (w > 0 && h > 0)
        {
            Width = w + 0.5;
            Height = h + 0.5;
            Width = w;
            Height = h;
        }

        InvalidateVisual();
        UpdateLayout();
        Opacity = _viewModel.Opacity;
    }

    private void EnsureTopmost()
    {
        if (!Topmost)
        {
            Topmost = true;
            return;
        }

        // Toggle re-asserts Z-order with the desktop compositor after restore.
        Topmost = false;
        Topmost = true;
    }

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            // Double-click: toggle work-area fill (safe "fullscreen" for this window chrome).
            if (_isWorkAreaFilled)
            {
                ExitWorkAreaFillIfNeeded();
                RefreshAfterRestore();
            }
            else if (WindowState == WindowState.Normal)
            {
                _preFillBounds = new Rect(Left, Top, Width, Height);
                var work = SystemParameters.WorkArea;
                Left = work.Left;
                Top = work.Top;
                Width = work.Width;
                Height = work.Height;
                _isWorkAreaFilled = true;
                RefreshAfterRestore();
            }

            e.Handled = true;
            return;
        }

        // Dragging off a filled work area should restore prior size (like normal maximize drag-down).
        if (_isWorkAreaFilled)
        {
            ExitWorkAreaFillIfNeeded();
        }

        DragMove();
    }

    private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this,
            DataContext = _viewModel.CreateSettingsViewModel()
        };

        var result = settingsWindow.ShowDialog();
        if (result == true)
        {
            _viewModel.ApplySettingsFromDisk();
            Opacity = _viewModel.Opacity;
        }
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();

    private void OnClosed(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Normal && !_viewModel.IsUiCompact && !_isWorkAreaFilled)
        {
            if (Left > -10000 && Top > -10000 && Width > 0 && Height > 0)
            {
                _viewModel.PersistWindowBounds(Left, Top, Width, Height);
            }
        }
        else if (_isWorkAreaFilled && _preFillBounds.Width > 0)
        {
            _viewModel.PersistWindowBounds(
                _preFillBounds.Left,
                _preFillBounds.Top,
                _preFillBounds.Width,
                _preFillBounds.Height);
        }

        _viewModel.Dispose();
    }
}
