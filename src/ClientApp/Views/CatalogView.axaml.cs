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

        private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var image = this.FindControl<Image>("RootImage");
            Visual surface = image is not null ? image : RootGrid;
            var pos = e.GetPosition(surface);
            var bounds = surface.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0) return;
            var x = pos.X / bounds.Width;
            var y = pos.Y / bounds.Height;
            if (x >= 0.040 && x <= 0.085 && y >= 0.880 && y <= 0.915)
            {
                if (DataContext is CatalogViewModel vm) { _ = vm.ShowCatalogOverlayAsync(null, null); }
            }
            else if (x >= 0.080 && x <= 0.130 && y >= 0.875 && y <= 0.915)
            {
                if (DataContext is CatalogViewModel vm) { _ = vm.ShowCatalogOverlayAsync(9, null); }
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
            try { System.IO.File.AppendAllText("C:/temp/desktop-hotspot.log", "[CatalogView] All button clicked\n"); } catch { }
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
