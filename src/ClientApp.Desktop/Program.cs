using System.IO.Pipes;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace eShop.ClientApp.Desktop;

internal sealed class Program
{
    private const string SingleInstanceMutexName = "eShop.ClientApp.SingleInstance";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // When the system browser completes the OIDC flow and redirects to eshop://authcallback,
        // Windows launches a SECOND instance of this exe with that URL as argv[0] (the app is
        // already running and showing the browser at that point). If this is that second
        // instance, forward the URL to the real, already-running one over a named pipe instead
        // of starting Avalonia a second time. See AvaloniaAuthenticationBrowser for the other
        // half of this (registry registration + the pipe listener + completing the pending
        // OIDC call).
        var activationUrl = args.FirstOrDefault(a => a.StartsWith("eshop://", StringComparison.OrdinalIgnoreCase));

        using var singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var isFirstInstance);

        if (!isFirstInstance && activationUrl is not null)
        {
            ForwardActivationUrl(activationUrl);
            return;
        }

        var services = new ServiceCollection();
        services.AddEShopServices();
        services.AddSingleton<IBrowser, AvaloniaAuthenticationBrowser>();

        App.Services = services.BuildServiceProvider();
        App.Services.UseEShopRoutes();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    private static void ForwardActivationUrl(string url)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", AvaloniaAuthenticationBrowser.PipeName, PipeDirection.Out);
            client.Connect(timeout: 2000);

            using var writer = new StreamWriter(client) { AutoFlush = true };
            writer.WriteLine(url);
        }
        catch (TimeoutException)
        {
            // No first instance actually listening (shouldn't happen in practice -- the browser
            // can only be mid-OIDC-flow if the app that opened it is still running) -- nothing
            // sensible to do with an orphaned activation, so just drop it.
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
