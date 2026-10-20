using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using eShop.ClientApp.ViewModels;

namespace eShop.ClientApp.Views;

public partial class CatalogView : UserControl
{
    public CatalogView()
    {
        InitializeComponent();
    }

    // Avalonia's ListBox has no bindable equivalent to MAUI CollectionView's
    // SelectionChangedCommand property -- just a plain .NET event, handled here the same way
    // the old upstream wiring conceptually did (event in, ViewCatalogItemCommand out).
    private void OnProductSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is CatalogViewModel viewModel && e.AddedItems.Count > 0)
        {
            viewModel.ViewCatalogItemCommand.Execute(e.AddedItems[0]);
        }
    }

    // App-exit is a platform concern (no equivalent on mobile/browser heads), not ViewModel
    // business logic, so it is handled here rather than via a command on CatalogViewModel.
    private void OnQuitClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }
}
