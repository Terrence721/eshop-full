using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.ViewModels;

namespace eShop.ClientApp.Views.Templates;

public partial class BasketItemTemplate : UserControl
{
    public BasketItemTemplate()
    {
        InitializeComponent();
    }

    private void OnDeleteClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not BasketItem item)
        {
            return;
        }

        if (this.FindAncestorOfType<ListBox>()?.DataContext is BasketViewModel basketViewModel)
        {
            basketViewModel.DeleteCommand.Execute(item);
        }
    }
}
