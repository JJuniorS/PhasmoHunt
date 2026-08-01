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
        if (!_settingsApplied || WindowState != WindowState.Normal || _viewModel.IsUiCompact)
        {
            return;
        }

        _viewModel.PersistWindowBounds(Left, Top, Width, Height);
    }

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
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
        if (WindowState == WindowState.Normal && !_viewModel.IsUiCompact)
        {
            _viewModel.PersistWindowBounds(Left, Top, Width, Height);
        }

        _viewModel.Dispose();
    }
}
