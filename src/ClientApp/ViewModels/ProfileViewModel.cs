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

    public override async Task InitializeAsync()
    {
        await RefreshAsync();
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
                    new Dictionary<string, object> {{"Logout", true}});
            });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        await IsBusyFor(
            async () =>
            {
                // Get orders
                var orders = await _appEnvironmentService.OrderService.GetOrdersAsync();

                _orders.ReloadData(orders);
            });
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
            new Dictionary<string, object> {{nameof(Order.OrderNumber), order.OrderNumber}});
    }
}
