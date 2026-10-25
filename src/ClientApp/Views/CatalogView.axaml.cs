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
        }

        private void OnAvatarClick(object? sender, RoutedEventArgs e)

        {

            try { System.IO.File.AppendAllText("C:/temp/desktop-click.log", "clicked;"); } catch { }

            if (LoginModal is not null)

            {

                LoginModal.IsVisible = true;

                if (StatusText is not null) StatusText.Text = string.Empty;

                if (SuccessText is not null) SuccessText.Text = string.Empty;

                if (UsernameBox is not null) UsernameBox.Text = string.Empty;

                if (PasswordBox is not null) PasswordBox.Text = string.Empty;

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
