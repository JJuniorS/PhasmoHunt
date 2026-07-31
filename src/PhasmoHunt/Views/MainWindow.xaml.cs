using System.Windows;
using System.Windows.Input;
using PhasmoHunt.Services;
using PhasmoHunt.ViewModels;

namespace PhasmoHunt.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private bool _settingsApplied;

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
            _settingsApplied = true;
        }

        _viewModel.AttachHotkeys(this);
        _viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.Opacity))
            {
                Opacity = _viewModel.Opacity;
            }
        };
    }

    private void OnBoundsChanged(object? sender, EventArgs e)
    {
        if (!_settingsApplied || WindowState != WindowState.Normal)
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

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();

    private void OnClosed(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Normal)
        {
            _viewModel.PersistWindowBounds(Left, Top, Width, Height);
        }

        _viewModel.Dispose();
    }
}
