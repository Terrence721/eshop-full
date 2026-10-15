using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using eShop.ClientApp.ViewModels;
using eShop.ClientApp.Views;

namespace eShop.ClientApp;

public partial class App : Application
{
    // Set by each platform head's entry point before Avalonia starts, once its own
    // IBrowser implementation is registered alongside the shared AddEShopServices().
    public static IServiceProvider Services { get; set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var mainViewModel = Services.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView { DataContext = mainViewModel };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainViewModel
            };
        }

        // Same moment old MAUI's AppShell.OnHandlerChanged() kicked off navigation -- right
        // after the root window/view has a DataContext, check login state and route to
        // //Login or //Main/Catalog.
        _ = mainViewModel.InitializeAsyncCommand.ExecuteAsync(null);

        base.OnFrameworkInitializationCompleted();
    }
}
