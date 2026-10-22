using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

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
