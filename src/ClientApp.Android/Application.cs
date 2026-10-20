using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace eShop.ClientApp.Android;

[Application]
public class Application : AvaloniaAndroidApplication<App>
{
    protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    // Android's equivalent of Desktop's Main(): runs once, before any Activity (including
    // MainActivity) or the Avalonia app lifetime itself, which base.OnCreate() goes on to start.
    public override void OnCreate()
    {
        var services = new ServiceCollection();
        services.AddEShopServices();
        services.AddSingleton<IBrowser, AndroidAuthenticationBrowser>();

        App.Services = services.BuildServiceProvider();
        App.Services.UseEShopRoutes();

        base.OnCreate();
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
