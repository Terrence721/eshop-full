using eShop.ClientApp.Models.Orders;
using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;

namespace eShop.ClientApp.ViewModels;

public partial class OrderDetailViewModel(
    IAppEnvironmentService appEnvironmentService,
    INavigationService navigationService) : ViewModelBase(navigationService)
{
    private readonly IAppEnvironmentService _appEnvironmentService = appEnvironmentService;

    [ObservableProperty] public partial bool IsSubmittedOrder { get; set; }

    [ObservableProperty] public partial Order Order { get; set; }

    [ObservableProperty] public partial int OrderNumber { get; set; }

    [ObservableProperty] public partial string OrderStatusText { get; set; }

    public override async Task InitializeAsync()
    {
        await IsBusyFor(
            async () =>
            {
                // Get order detail info
                Order = await _appEnvironmentService.OrderService.GetOrderAsync(OrderNumber);
                IsSubmittedOrder = Order.OrderStatus.Equals("Submitted", StringComparison.OrdinalIgnoreCase);
                OrderStatusText = Order.OrderStatus;
            });
    }

    // Upstream relied on Shell's automatic back button for this; Avalonia has no Shell chrome
    // (see CatalogItemViewModel.DismissCommand for the same gap), so this needs its own.
    [RelayCommand]
    private async Task DismissAsync()
    {
        await NavigationService.PopAsync();
    }

    [RelayCommand]
    private async Task ToggleCancelOrderAsync()
    {
        await IsBusyFor(
            async () =>
            {
                var result = await _appEnvironmentService.OrderService.CancelOrderAsync(Order.OrderNumber);

                if (result)
                {
                    OrderStatusText = "Cancelled";
                }
                else
                {
                    Order = await _appEnvironmentService.OrderService.GetOrderAsync(Order.OrderNumber);
                    OrderStatusText = Order.OrderStatus;
                }

                IsSubmittedOrder = false;
            });

        // Return to the (now up-to-date, thanks to PopAsync re-running InitializeAsyncCommand)
        // order list instead of leaving the user stranded on a detail page for an order that no
        // longer needs any action.
        await NavigationService.PopAsync();
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("OrderNumber", out var orderNumber))
        {
            if (orderNumber is string orderNumberString && int.TryParse(orderNumberString, out var parsedOrderNumber))
            {
                OrderNumber = parsedOrderNumber;
            }
            else if (orderNumber is int intOrderNumber)
            {
                OrderNumber = intOrderNumber;
            }
        }
    }
}
