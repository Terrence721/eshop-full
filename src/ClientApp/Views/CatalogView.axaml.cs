using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Input;
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
            if (CatalogModal is not null && e.Source == CatalogModal && DataContext is CatalogViewModel vm)
            {
                vm.CloseCatalogOverlay();
            }
        }

        private void OnLoginModalPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (LoginModal is not null && e.Source == LoginModal && e.Source is Border)
            {
                LoginModal.IsVisible = false;
            }
        }

        private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var image = this.FindControl<Image>("RootImage");
            Visual surface = image is not null ? image : RootGrid;
            var pos = e.GetPosition(surface);
            var bounds = surface.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            var x = pos.X / bounds.Width;
            var y = pos.Y / bounds.Height;

            if (DataContext is not CatalogViewModel vm)
            {
                return;
            }

            bool overlayVisible = vm.IsCatalogOverlayVisible;
            int? brandId = null;
            int? typeId = null;
            bool knownHotspot = false;

            // Brand filter row.
            if (x >= 0.040 && x <= 0.085 && y >= 0.780 && y <= 0.930) { /* All */ knownHotspot = true; }
            else if (x >= 0.080 && x <= 0.130 && y >= 0.760 && y <= 0.930) { brandId = 9; knownHotspot = true; } // AirStrider
            else if (x >= 0.140 && x <= 0.165 && y >= 0.760 && y <= 0.930) { brandId = 5; knownHotspot = true; } // B&R
            else if (x >= 0.150 && x <= 0.175 && y >= 0.795 && y <= 0.815) { brandId = 8; knownHotspot = true; } // Grolltex
            // (no duplicate needed)
            // Lower brand/type row.
            else if (x >= 0.050 && x <= 0.095 && y >= 0.740 && y <= 0.775) { brandId = 1; knownHotspot = true; } // Daybird
            else if (x >= 0.110 && x <= 0.145 && y >= 0.930 && y <= 0.975) { brandId = 2; knownHotspot = true; } // Gravitator (old row)
            else if (x >= 0.110 && x <= 0.135 && y >= 0.750 && y <= 0.770) { brandId = 2; knownHotspot = true; } // Gravitator (measured)
            else if (x >= 0.070 && x <= 0.095 && y >= 0.795 && y <= 0.815) { brandId = 10; knownHotspot = true; } // Green Equipment
            else if (x >= 0.060 && x <= 0.075 && y >= 0.785 && y <= 0.805) { brandId = 6; knownHotspot = true; } // Raptor Elite
            else if (x >= 0.120 && x <= 0.155 && y >= 0.620 && y <= 0.730) { brandId = 7; knownHotspot = true; } // Solstix (broad region)
            else if (x >= 0.055 && x <= 0.085 && y >= 0.750 && y <= 0.770) { brandId = 11; knownHotspot = true; } // Zephyr (measured 0.0674,0.7560)
            else if (x >= 0.115 && x <= 0.135 && y >= 0.765 && y <= 0.780) { brandId = 13; knownHotspot = true; } // XE (measured 0.1243,0.7693)
            else if (x >= 0.065 && x <= 0.085 && y >= 0.825 && y <= 0.850) { brandId = 3; knownHotspot = true; } // WildRunner (measured 0.0723,0.8360) (measured 0.0667,0.7938)
            else if (x >= 0.100 && x <= 0.125 && y >= 0.720 && y <= 0.735) { brandId = 4; knownHotspot = true; } // Quester (measured)
            else if (x >= 0.050 && x <= 0.070 && y >= 0.785 && y <= 0.805) { brandId = 12; knownHotspot = true; } // Legend (measured 0.056,0.792)

            if (overlayVisible && !knownHotspot)
            {
                vm.CloseCatalogOverlay();
                return;
            }

            if (knownHotspot)
            {
                vm.CloseCatalogOverlay();
                _ = vm.ShowCatalogOverlayAsync(brandId, typeId);
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



        private async void OnAllCatalogClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is CatalogViewModel vm)
            {
                await vm.ShowCatalogOverlayAsync(null, null);
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
            if (DataContext is CatalogViewModel viewModel && e.AddedItems.Count > 0 &&
                e.AddedItems[0] is CatalogTypeSelectionViewModel selection)
            {
                viewModel.SelectCatalogTypeCommand.Execute(selection.Value);
                viewModel.ApplyFilterCommand.Execute(null);
            }
        }

        private void OnBrandFilterChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is CatalogViewModel viewModel && e.AddedItems.Count > 0 &&
                e.AddedItems[0] is CatalogBrandSelectionViewModel selection)
            {
                viewModel.SelectCatalogBrandCommand.Execute(selection.Value);
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
