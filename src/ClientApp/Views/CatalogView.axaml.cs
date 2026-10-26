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
            var overlay = this.FindControl<Border>("CatalogOverlay");
            if (overlay is not null && e.Source == overlay && DataContext is CatalogViewModel vm)
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

            var loginModal = this.FindControl<Border>("LoginModal");
            if (loginModal is not null && !loginModal.IsVisible && x >= 0.86 && y <= 0.08 && !vm.IsCatalogOverlayVisible)
            {
                loginModal.IsVisible = true;
                return;
            }

            // Any click opens the catalog overlay, which contains dropdowns (with "All")
            // for precise filtering. Pixel hotspots are unreliable in this scrolled window.
            if (!vm.IsCatalogOverlayVisible)
            {
                _ = vm.ShowCatalogOverlayAsync(null, null);
            }
        }

        private void OnAvatarClick(object? sender, RoutedEventArgs e)
        {
            var loginModal = this.FindControl<Border>("LoginModal");
            if (loginModal is not null)
            {
                loginModal.IsVisible = true;
                var statusText = this.FindControl<TextBlock>("StatusText");
                var successText = this.FindControl<TextBlock>("SuccessText");
                var usernameBox = this.FindControl<TextBox>("UsernameBox");
                var passwordBox = this.FindControl<TextBox>("PasswordBox");
                if (statusText is not null) statusText.Text = string.Empty;
                if (successText is not null) successText.Text = string.Empty;
                if (usernameBox is not null) usernameBox.Text = string.Empty;
                if (passwordBox is not null) passwordBox.Text = string.Empty;
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
            var loginModal = this.FindControl<Border>("LoginModal");
            if (loginModal is not null)
            {
                loginModal.IsVisible = false;
            }
        }

        private async void OnSignIn(object? sender, RoutedEventArgs e)
        {
            var usernameBox = this.FindControl<TextBox>("UsernameBox");
            var passwordBox = this.FindControl<TextBox>("PasswordBox");
            var statusText = this.FindControl<TextBlock>("StatusText");
            var successText = this.FindControl<TextBlock>("SuccessText");
            var loginModal = this.FindControl<Border>("LoginModal");
            var username = usernameBox?.Text ?? string.Empty;
            var password = passwordBox?.Text ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                if (statusText is not null) statusText.Text = "Username and password are required.";
                if (successText is not null) successText.Text = string.Empty;
                return;
            }

            if (statusText is not null) statusText.Text = string.Empty;
            if (successText is not null) successText.Text = $"Login successful. Welcome, {username}!";

            await Task.Delay(900);
            if (loginModal is not null) loginModal.IsVisible = false;
            if (successText is not null) successText.Text = string.Empty;
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