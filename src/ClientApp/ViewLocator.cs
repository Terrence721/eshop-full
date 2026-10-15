using Avalonia.Controls;
using Avalonia.Controls.Templates;
using eShop.ClientApp.ViewModels;
using eShop.ClientApp.Views;

namespace eShop.ClientApp;

/// <summary>
/// Given a view model, returns the corresponding view if possible.
/// </summary>
// An explicit registry instead of the original Type.GetType(string) name-convention reflection:
// confirmed by actually running a published Browser build (not just the formerly-suppressed
// IL2026 warning) that WASM's publish-time IL trimming cannot see a reference built from a
// dynamic string, concludes every View is unused, and strips them -- Type.GetType returned null
// for all of them at runtime, so nothing ever rendered. Every entry below is a real `new XyzView()`
// the trimmer can see and keep, and the same fix applies to iOS, which trims via the same
// mechanism and had the same IL2026 suppression for the same reason.
public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> Factories = new()
    {
        [typeof(LoginViewModel)] = () => new LoginView(),
        [typeof(CatalogViewModel)] = () => new CatalogView(),
        [typeof(CatalogItemViewModel)] = () => new CatalogItemView(),
        [typeof(BasketViewModel)] = () => new BasketView(),
        [typeof(CheckoutViewModel)] = () => new CheckoutView(),
        [typeof(ProfileViewModel)] = () => new ProfileView(),
        [typeof(OrderDetailViewModel)] = () => new OrderDetailView(),
        [typeof(SettingsViewModel)] = () => new SettingsView(),
        [typeof(MapViewModel)] = () => new MapView(),
    };

    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        if (Factories.TryGetValue(param.GetType(), out var factory))
        {
            var control = factory();

            // Relying on ContentPresenter to assign DataContext automatically was unreliable when
            // navigating back to a previously-used ViewModel instance (DataContext came up null,
            // so every binding on the freshly-built View silently failed): setting it explicitly
            // here removes the dependency on that implicit behavior entirely.
            control.DataContext = param;

            return control;
        }

        return new TextBlock { Text = "Not Found: " + param.GetType().FullName };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
