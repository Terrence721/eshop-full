using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

namespace eShop.ClientApp.Views;

public partial class BasketView : UserControl
{
    public BasketView()
    {
        InitializeComponent();
    }

    // App-exit is a platform concern (no equivalent on mobile/browser heads), not ViewModel
    // business logic, so it is handled here rather than via a command on BasketViewModel.
    private void OnQuitClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }
}
