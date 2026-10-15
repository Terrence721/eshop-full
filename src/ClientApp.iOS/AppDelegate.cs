using Avalonia;
using Avalonia.iOS;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace eShop.ClientApp.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the
// User Interface of the application, as well as listening (and optionally responding) to
// application events from iOS.
[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<App>
{
    // AvaloniaAppDelegate<App>.FinishedLaunching is sealed in this Avalonia version (confirmed by
    // reflection, not assumed), so CustomizeAppBuilder is the actual available hook -- it runs
    // while FinishedLaunching builds the AppBuilder, strictly before Avalonia's framework
    // initialization (and so before App.axaml.cs's OnFrameworkInitializationCompleted, which is
    // what first needs App.Services). AuthenticationSessionBrowser only reads AppDelegate.Window
    // lazily when a sign-in is actually invoked, by which point Window is long since set.
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        var services = new ServiceCollection();
        services.AddEShopServices();
        services.AddSingleton<IBrowser>(new AuthenticationSessionBrowser(this));

        App.Services = services.BuildServiceProvider();
        App.Services.UseEShopRoutes();

        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
