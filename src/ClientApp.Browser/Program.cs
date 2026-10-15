using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Microsoft.Extensions.DependencyInjection;
using eShop.ClientApp;
using eShop.ClientApp.Browser;
using eShop.ClientApp.Services.Identity;

[SupportedOSPlatform("browser")]
internal sealed partial class Program
{
    private static async Task Main(string[] args)
    {
        await JSHost.ImportAsync("interop", "../js/interop.js");

        var services = new ServiceCollection();
        services.AddEShopServices();

        // Overrides AddEShopServices' own IIdentityService registration: GetService<T> resolves
        // the last registration for a type, and AppEnvironmentService's factory resolves
        // IIdentityService through the container rather than constructing IdentityService
        // itself, so this is picked up there without any further wiring. No IBrowser is
        // registered for this platform at all -- see BrowserIdentityService for why Desktop's
        // IBrowser-based approach doesn't apply here.
        services.AddSingleton<IIdentityService, BrowserIdentityService>();

        App.Services = services.BuildServiceProvider();
        App.Services.UseEShopRoutes();

        // If the IdP just redirected back here mid-sign-in, finish that exchange now, before the
        // Avalonia UI (and its own "am I logged in" check in AvaloniaNavigationService) starts --
        // see BrowserIdentityService.TryCompletePendingLoginAsync.
        var identityService = (BrowserIdentityService)App.Services.GetRequiredService<IIdentityService>();
        await identityService.TryCompletePendingLoginAsync();

        await BuildAvaloniaApp()
            .WithInterFont()
            .StartBrowserAppAsync("out");
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}
