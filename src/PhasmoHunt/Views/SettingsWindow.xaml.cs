using System.Windows;
using System.Windows.Input;
using PhasmoHunt.ViewModels;

namespace PhasmoHunt.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private SettingsViewModel? Vm => DataContext as SettingsViewModel;

    private void TitleBar_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void HotkeyBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string name)
            Vm?.BeginCapture(name);
        e.Handled = true;
        if (sender is UIElement el)
            el.Focus();
    }

    private void Window_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Vm is null) return;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (Vm.TryCaptureKey(key, Keyboard.Modifiers))
            e.Handled = true;
    }

    private void PtBrFlag_OnClick(object sender, RoutedEventArgs e) =>
        Vm?.SelectLanguageCommand.Execute("pt-BR");

    private void EnFlag_OnClick(object sender, RoutedEventArgs e) =>
        Vm?.SelectLanguageCommand.Execute("en");

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (Vm is null) return;
        if (Vm.SaveCommand.CanExecute(null))
            Vm.SaveCommand.Execute(null);
        if (Vm.Saved)
        {
            DialogResult = true;
            Close();
        }
    }

    private void Window_OnClosed(object? sender, EventArgs e) =>
        Vm?.Detach();
}
