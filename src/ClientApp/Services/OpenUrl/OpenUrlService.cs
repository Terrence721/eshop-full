using System.Diagnostics;

namespace eShop.ClientApp.Services.OpenUrl;

// MAUI's Launcher.OpenAsync has no Avalonia equivalent. Process.Start with
// UseShellExecute=true is the standard cross-platform .NET way to hand a URL to the OS's
// default handler on Windows/macOS/Linux -- real for Desktop, same known gap as
// DialogService's modal alert: Android/iOS/Browser have no Process to start this way, and need
// their own platform-specific implementation later.
public class OpenUrlService : IOpenUrlService
{
    public Task OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });

        return Task.CompletedTask;
    }
}
