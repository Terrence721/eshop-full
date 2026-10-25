using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using eShop.ClientApp.ViewModels;

namespace eShop.ClientApp.Views
{
    // Upstream's OnAppearing() did a MAUI-specific content-refresh rendering hack (not needed in
    // Avalonia) and called InvalidateMock(), which only ever sets IsMock back to its own
    // hardcoded false (the real assignment is commented out in LoginViewModel), so it's a
    // provable no-op. Nothing real left to port into a lifecycle hook here.
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Image image || DataContext is not LoginViewModel vm)
            {
                return;
            }

            var position = e.GetPosition(image);
            var bounds = image.Bounds;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            var x = position.X / bounds.Width;
            var y = position.Y / bounds.Height;

            // Person avatar is near the top (left or right depending on layout).
            if (y <= 0.08 && (x <= 0.12 || x >= 0.86))
            {
                vm.OpenLoginModalCommand.Execute(null);
            }
        }

        // App-exit is a platform concern (no equivalent on mobile/browser heads), not ViewModel
        // business logic, so it is handled here rather than via a command on LoginViewModel.
        private void OnQuitClick(object? sender, RoutedEventArgs e)
        {
            if (Application.Current?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }
    }
}
