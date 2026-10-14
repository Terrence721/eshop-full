namespace eShop.ClientApp.Views;

// Registered as a route in AppShell.xaml.cs but never actually navigated to anywhere in
// upstream - CatalogView's own inline Expander handles filtering instead. Ported as-is.
public partial class FiltersView : ContentPage
{
    public FiltersView(CatalogViewModel viewModel)
    {
        BindingContext = viewModel;

        InitializeComponent();
    }
}
