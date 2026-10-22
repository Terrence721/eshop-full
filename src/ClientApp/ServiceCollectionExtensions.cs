using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;
using eShop.ClientApp.Services.Basket;
using eShop.ClientApp.Services.Catalog;
using eShop.ClientApp.Services.FixUri;
using eShop.ClientApp.Services.Identity;
using eShop.ClientApp.Services.Location;
using eShop.ClientApp.Services.OpenUrl;
using eShop.ClientApp.Services.Order;
using eShop.ClientApp.Services.RequestProvider;
using eShop.ClientApp.Services.Settings;
using eShop.ClientApp.Services.Theme;
using eShop.ClientApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using IBrowser = Duende.IdentityModel.OidcClient.Browser.IBrowser;

namespace eShop.ClientApp
{
    // The platform-agnostic half of the composition root. Each platform head (ClientApp.Desktop
    // today; .Android/.iOS later) registers its own IBrowser implementation first -- OIDC redirect
    // capture is inherently platform-specific, see AvaloniaAuthenticationBrowser for Desktop's --
    // since the live IIdentityService registered here depends on it. Matches the old MAUI
    // MauiProgram.cs's RegisterAppServices/RegisterViewModels pattern: Basket/Catalog/Order/Identity
    // are never registered as DI services in their own right, only AppEnvironmentService's factory
    // constructs the mock+live pairs directly, exactly as upstream did.
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEShopServices(this IServiceCollection services)
        {
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<INavigationService, AvaloniaNavigationService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IOpenUrlService, OpenUrlService>();
            services.AddSingleton<IFixUriService, FixUriService>();
            services.AddSingleton<ILocationService, LocationService>();
            services.AddSingleton<ITheme, Theme>();
            services.AddSingleton<IRequestProvider>(_ => new RequestProvider(null));

            services.AddSingleton<IIdentityService>(
                sp => new IdentityService(
                    sp.GetRequiredService<IBrowser>(),
                    sp.GetRequiredService<ISettingsService>(),
                    null));

            services.AddTransient<BasketMockService>();
            services.AddTransient<CatalogMockService>();
            services.AddTransient<OrderMockService>();
            services.AddTransient<IdentityMockService>();

            services.AddTransient(sp => new BasketService(
                sp.GetRequiredService<IIdentityService>(),
                sp.GetRequiredService<ISettingsService>(),
                sp.GetRequiredService<IFixUriService>()));

            services.AddTransient(sp => new CatalogService(
                sp.GetRequiredService<ISettingsService>(),
                sp.GetRequiredService<IRequestProvider>(),
                sp.GetRequiredService<IFixUriService>()));

            services.AddTransient(sp => new OrderService(
                sp.GetRequiredService<IIdentityService>(),
                sp.GetRequiredService<ISettingsService>(),
                sp.GetRequiredService<IRequestProvider>()));

            services.AddSingleton<IAppEnvironmentService>(
                sp =>
                {
                    var settingsService = sp.GetRequiredService<ISettingsService>();

                    var appEnvironmentService = new AppEnvironmentService(
                        sp.GetRequiredService<BasketMockService>() as IBasketService,
                        sp.GetRequiredService<BasketService>() as IBasketService,
                        sp.GetRequiredService<CatalogMockService>() as ICatalogService,
                        sp.GetRequiredService<CatalogService>() as ICatalogService,
                        sp.GetRequiredService<OrderMockService>() as IOrderService,
                        sp.GetRequiredService<OrderService>() as IOrderService,
                        sp.GetRequiredService<IdentityMockService>() as IIdentityService,
                        sp.GetRequiredService<IIdentityService>());

                    appEnvironmentService.UpdateDependencies(settingsService.UseMocks);

                    return appEnvironmentService;
                });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoginViewModel>();
            services.AddSingleton<CatalogViewModel>();
            services.AddSingleton<CatalogItemViewModel>();
            services.AddSingleton<BasketViewModel>();
            services.AddSingleton<CheckoutViewModel>();
            services.AddSingleton<ProfileViewModel>();
            services.AddSingleton<OrderDetailViewModel>();
            services.AddSingleton<MapViewModel>();
            services.AddSingleton<SettingsViewModel>();

            return services;
        }

        public static IServiceProvider UseEShopRoutes(this IServiceProvider services)
        {
            var navigationService = services.GetRequiredService<INavigationService>();

            navigationService.RegisterRoute("Login", () => services.GetRequiredService<LoginViewModel>());
            navigationService.RegisterRoute("Catalog", () => services.GetRequiredService<CatalogViewModel>());
            navigationService.RegisterRoute("Map", () => services.GetRequiredService<MapViewModel>());
            navigationService.RegisterRoute("Profile", () => services.GetRequiredService<ProfileViewModel>());
            navigationService.RegisterRoute("ViewCatalogItem", () => services.GetRequiredService<CatalogItemViewModel>());
            navigationService.RegisterRoute("Basket", () => services.GetRequiredService<BasketViewModel>());
            navigationService.RegisterRoute("Settings", () => services.GetRequiredService<SettingsViewModel>());
            navigationService.RegisterRoute("OrderDetail", () => services.GetRequiredService<OrderDetailViewModel>());
            navigationService.RegisterRoute("Checkout", () => services.GetRequiredService<CheckoutViewModel>());

            return services;
        }
    }
}
