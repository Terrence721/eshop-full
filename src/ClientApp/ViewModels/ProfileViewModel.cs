using eShop.ClientApp.Models.Orders;
using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;

namespace eShop.ClientApp.ViewModels;

public partial class ProfileViewModel(
    IAppEnvironmentService appEnvironmentService,
    INavigationService navigationService) : ViewModelBase(navigationService)
{
    private readonly IAppEnvironmentService _appEnvironmentService = appEnvironmentService;
    private readonly ObservableCollectionEx<Order> _orders = [];

    [ObservableProperty] public partial Order? SelectedOrder { get; set; }

    public IList<Order> Orders => _orders;

    public bool IsEmpty => _orders.Count == 0;

    public override async Task InitializeAsync()
    {
        await RefreshOrdersAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await IsBusyFor(
            async () =>
            {
                // Logout
                await NavigationService.NavigateToAsync(
                    "//Login",
                    new Dictionary<string, object> { { "Logout", true } });
            });
    }

    // This real upstream bug was carried over verbatim with the rest of ProfileViewModel: calling
    // RefreshAsync from InitializeAsync meant its own "if (IsBusy) return" guard always saw
    // IsBusy already true (set by InitializeAsyncCommand's own IsBusyFor wrapper before
    // InitializeAsync even runs), so the order fetch below silently never executed on navigation.
    // RefreshOrdersAsync is the shared fetch; RefreshAsync keeps the guard for its own purpose
    // (ignoring a second manual refresh while one is already in flight).
    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        await IsBusyFor(RefreshOrdersAsync);
    }

    private async Task RefreshOrdersAsync()
    {
        var orders = await _appEnvironmentService.OrderService.GetOrdersAsync();

        _orders.ReloadData(orders);
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task OrderDetailAsync(Order order)
    {
        if (order is null || IsBusy)
        {
            return;
        }

        await NavigationService.NavigateToAsync(
            "OrderDetail",
            new Dictionary<string, object> { { nameof(Order.OrderNumber), order.OrderNumber } });
    }
}
