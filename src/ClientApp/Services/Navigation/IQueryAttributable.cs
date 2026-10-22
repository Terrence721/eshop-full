namespace eShop.ClientApp.Services
{
    // Avalonia has no built-in equivalent to MAUI's Microsoft.Maui.Controls.IQueryAttributable
    // (which Shell used to hand navigation parameters to a page's BindingContext automatically).
    // Same shape, implemented by AvaloniaNavigationService's own route/parameter handling instead.
    public interface IQueryAttributable
    {
        void ApplyQueryAttributes(IDictionary<string, object> query);
    }
}
