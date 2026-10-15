using Avalonia.Controls;
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
}
