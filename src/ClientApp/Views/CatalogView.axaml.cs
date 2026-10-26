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
            AddHandler(PointerPressedEvent, OnRootPointerPressed, RoutingStrategies.Tunnel, true);
        }

        private void OnCatalogOverlayPressed(object? sender, PointerPressedEventArgs e)
        {
            if (CatalogOverlay is not null && e.Source == CatalogOverlay && DataContext is CatalogViewModel vm)
            {
                vm.CloseCatalogOverlay();
            }
        }

        private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is not CatalogViewModel vm)
            {
                return;
            }

            var image = this.FindControl<Image>("RootImage");
            Visual surface = image is not null ? image : RootGrid;
            var pos = e.GetPosition(surface);
            var bounds = surface.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }
            var x = pos.X / bounds.Width;
            var y = pos.Y / bounds.Height;
            try { System.IO.File.AppendAllLines("C:/temp/desktop-hotspot.log", new[] { $"relative x={x:F4} y={y:F4}" }); } catch { }

            if (LoginModal is not null && !LoginModal.IsVisible && x >= 0.86 && y <= 0.08 && !vm.IsCatalogOverlayVisible)
            {
                LoginModal.IsVisible = true;
                return;
            }

            // Region-based hotspots.
            if (x >= 0.040 && x <= 0.085 && y >= 0.880 && y <= 0.915) { _ = vm.ShowCatalogOverlayAsync(null, null); }
            else if (x >= 0.080 && x <= 0.130 && y >= 0.760 && y <= 0.930) { _ = vm.ShowCatalogOverlayAsync(9, null); } // AirStrider
            else if (x >= 0.140 && x <= 0.165 && y >= 0.760 && y <= 0.930) { _ = vm.ShowCatalogOverlayAsync(5, null); } // B&R
            else if (x >= 0.050 && x <= 0.095 && y >= 0.930 && y <= 0.975) { _ = vm.ShowCatalogOverlayAsync(1, null); } // Daybird
            else if (x >= 0.110 && x <= 0.145 && y >= 0.930 && y <= 0.975) { _ = vm.ShowCatalogOverlayAsync(2, null); } // Gravitator
            else if (x >= 0.070 && x <= 0.095 && y >= 0.795 && y <= 0.815) { _ = vm.ShowCatalogOverlayAsync(10, null); } // Green Equipment
            else if (x >= 0.150 && x <= 0.175 && y >= 0.795 && y <= 0.815) { _ = vm.ShowCatalogOverlayAsync(8, null); } // Grolltex
            else if (x >= 0.050 && x <= 0.070 && y >= 0.785 && y <= 0.805) { _ = vm.ShowCatalogOverlayAsync(12, null); } // Legend
            else if (x >= 0.085 && x <= 0.100 && y >= 0.755 && y <= 0.775) { _ = vm.ShowCatalogOverlayAsync(null, 4); } // Bags (type)
            else if (x >= 0.110 && x <= 0.125 && y >= 0.750 && y <= 0.770) { _ = vm.ShowCatalogOverlayAsync(4, null); } // Quester
            else if (x >= 0.060 && x <= 0.075 && y >= 0.785 && y <= 0.805) { _ = vm.ShowCatalogOverlayAsync(6, null); } // Raptor Elite
            else if (x >= 0.120 && x <= 0.155 && y >= 0.620 && y <= 0.730) { _ = vm.ShowCatalogOverlayAsync(7, null); } // Solstix
            else if (x >= 0.065 && x <= 0.085 && y >= 0.825 && y <= 0.850) { _ = vm.ShowCatalogOverlayAsync(3, null); } // WildRunner
            else if (x >= 0.115 && x <= 0.135 && y >= 0.765 && y <= 0.780) { _ = vm.ShowCatalogOverlayAsync(13, null); } // XE
            else if (x >= 0.055 && x <= 0.085 && y >= 0.750 && y <= 0.770) { _ = vm.ShowCatalogOverlayAsync(11, null); } // Zephyr
            else if (!vm.IsCatalogOverlayVisible)
            {
                _ = vm.ShowCatalogOverlayAsync(null, null);
            }
        }

        private void OnAvatarClick(object? sender, RoutedEventArgs e)
        {
            if (LoginModal is not null)
            {
                LoginModal.IsVisible = true;
                if (StatusText is not null) StatusText.Text = string.Empty;
                if (SuccessText is not null) SuccessText.Text = string.Empty;
                if (UsernameBox is not null) UsernameBox.Text = string.Empty;
                if (PasswordBox is not null) PasswordBox.Text = string.Empty;
            }
        }

        private void OnCloseCatalogOverlay(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CatalogViewModel vm)
            {
                vm.CloseCatalogOverlay();
            }
        }

        private void OnCancelLogin(object? sender, RoutedEventArgs e)
        {
            if (LoginModal is not null)
            {
                LoginModal.IsVisible = false;
            }
        }

        private async void OnSignIn(object? sender, RoutedEventArgs e)
        {
            var username = UsernameBox?.Text ?? string.Empty;
            var password = PasswordBox?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                if (StatusText is not null) StatusText.Text = "Username and password are required.";
                if (SuccessText is not null) SuccessText.Text = string.Empty;
                return;
            }

            if (StatusText is not null) StatusText.Text = string.Empty;
            if (SuccessText is not null) SuccessText.Text = $"Login successful. Welcome, {username}!";

            await Task.Delay(900);
            if (LoginModal is not null) LoginModal.IsVisible = false;
            if (SuccessText is not null) SuccessText.Text = string.Empty;
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

        private void OnTypeFilterChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not CatalogViewModel viewModel || e.AddedItems.Count == 0)
            {
                return;
            }

            if (e.AddedItems[0] is CatalogTypeSelectionViewModel selection)
            {
                if (selection.Value?.Id == 0)
                {
                    viewModel.SelectedType = null;
                }
                else
                {
                    viewModel.SelectCatalogTypeCommand.Execute(selection.Value);
                }
                viewModel.ApplyFilterCommand.Execute(null);
            }
        }

        private void OnBrandFilterChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not CatalogViewModel viewModel || e.AddedItems.Count == 0)
            {
                return;
            }

            if (e.AddedItems[0] is CatalogBrandSelectionViewModel selection)
            {
                if (selection.Value?.Id == 0)
                {
                    viewModel.SelectedBrand = null;
                }
                else
                {
                    viewModel.SelectCatalogBrandCommand.Execute(selection.Value);
                }
                viewModel.ApplyFilterCommand.Execute(null);
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
}