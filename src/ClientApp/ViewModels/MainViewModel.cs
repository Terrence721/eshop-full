namespace eShop.ClientApp.ViewModels;

// Scaffold placeholder — proves the solution closes end-to-end. Replaced by the real
// navigation shell ViewModel (CurrentViewModel + tab/flyout state) in the UI step. Extends
// ObservableObject directly rather than ViewModelBase: ViewModelBase now requires a real
// INavigationService, which needs a fully wired AppEnvironmentService (including a real
// IBrowser for OIDC login) that doesn't exist until the composition-root step -- not worth
// building throwaway wiring just to satisfy a placeholder that's getting replaced anyway.
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to eShop!";
}
