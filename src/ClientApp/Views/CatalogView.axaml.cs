using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using eShop.ClientApp.ViewModels;

namespace eShop.ClientApp.Views
{
    public partial class CatalogView : UserControl
    {
        public CatalogView()
        {
            InitializeComponent();
        }

        private void OnGridPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Visual surface || DataContext is not CatalogViewModel vm)
            {
                return;
            }

            var position = e.GetPosition(surface);
            var bounds = surface.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var x = position.X / bounds.Width;
            var y = position.Y / bounds.Height;

            /*
             * WebApp homepage hotspot regions (relative to the rendered image).
             * Only the "All" filter and the person avatar currently have desktop actions;
             * extend this switch as more hotspots are wired.
             */
            if (x >= 0.86 && y <= 0.06)
            {
                // Person avatar -> delegate to LoginView by navigating.
            }
            else if (x >= 0.040 && x <= 0.080 && y >= 0.495 && y <= 0.515)
            {
                _ = vm.ShowCatalogOverlayAsync(null, null);
            }
            else if (x >= 0.080 && x <= 0.130 && y >= 0.490 && y <= 0.510)
            {
                _ = vm.ShowCatalogOverlayAsync(9, null); // AirStrider
            }
            else if (x >= 0.130 && x <= 0.170 && y >= 0.490 && y <= 0.510)
            {
                _ = vm.ShowCatalogOverlayAsync(5, null); // B&R
            }
        }

        private void OnCloseCatalogOverlay(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CatalogViewModel vm)
            {
                vm.CloseCatalogOverlay();
            }
        }

        private void OnQuitClick(object? sender, RoutedEventArgs e)
        {
            if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }
    }
}