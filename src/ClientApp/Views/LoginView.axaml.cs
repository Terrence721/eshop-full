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
            try { System.IO.File.AppendAllText("C:\\temp\\desktop-click.log", $"LoginView constructed {DateTime.Now:HH:mm:ss.fff}\n"); } catch { }
        }

        private void OnImageClick(object? sender, RoutedEventArgs e)
        {
            try
            {
                System.IO.File.AppendAllText("C:\\temp\\desktop-click.log", $"[LoginView] clicked at {DateTime.Now:HH:mm:ss.fff}\\n");
            }
            catch { }
            System.Console.WriteLine("[LoginView] Image area clicked (button).");

            if (DataContext is LoginViewModel vm)
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
