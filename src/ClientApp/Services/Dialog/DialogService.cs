using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using eShop.ClientApp.Views;

namespace eShop.ClientApp.Services
{
    public class DialogService : IDialogService
    {
        public Task ShowAlertAsync(string message, string title, string buttonLabel)
        {
            // Avalonia has no built-in alert dialog (unlike MAUI's Shell.DisplayAlert). A real
            // modal Window only works under the classic desktop lifetime -- Android/iOS/Browser use
            // ISingleViewApplicationLifetime/IActivityApplicationLifetime, which have no OS-level
            // window to show a dialog against. Desktop is covered for real; mobile/browser alerts
            // are a known, flagged gap until the real navigation shell (with its own overlay region)
            // exists to host an in-content dialog uniformly across all four platforms.
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow is not null)
            {
                return new AlertWindow(message, title, buttonLabel).ShowDialog(desktop.MainWindow);
            }

            return Task.CompletedTask;
        }
    }
}
