using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;

namespace eShop.ClientApp.ViewModels;

public partial class BasketViewModel(
    IAppEnvironmentService appEnvironmentService,
    INavigationService navigationService) : ViewModelBase(navigationService)
{
    private readonly IAppEnvironmentService _appEnvironmentService = appEnvironmentService;
    private readonly ObservableCollectionEx<BasketItem> _basketItems = [];

    public int BadgeCount => _basketItems?.Sum(basketItem => basketItem.Quantity) ?? 0;

    public decimal Total => _basketItems?.Sum(basketItem => basketItem.Quantity * basketItem.UnitPrice) ?? 0m;

    public bool IsEmpty => (_basketItems?.Count ?? 0) == 0;

    public IReadOnlyList<BasketItem> BasketItems => _basketItems;

    public override async Task InitializeAsync()
    {
        // Update Basket
        var basket = await _appEnvironmentService.BasketService.GetBasketAsync();

        if ((basket?.Items?.Count ?? 0) > 0)
        {
            await _basketItems.ReloadDataAsync(
                async innerList =>
                {
                    foreach (var basketItem in basket!.Items.ToArray())
                    {
                        var catalogItem =
                            await _appEnvironmentService.CatalogService.GetCatalogItemAsync(basketItem.ProductId);

                        if (catalogItem is null)
                        {
                            continue;
                        }

                        basketItem.PictureUrl = catalogItem.PictureUri ?? string.Empty;
                        basketItem.ProductName = catalogItem.Name;
                        basketItem.UnitPrice = catalogItem.Price;
                        await AddBasketItemAsync(basketItem, innerList);
                    }
                });
        }
    }

    [RelayCommand]
    private Task AddAsync(BasketItem item)
    {
        return AddBasketItemAsync(item, _basketItems);
    }

    private async Task AddBasketItemAsync(BasketItem item, IList<BasketItem> basketItems)
    {
        basketItems.Add(item);

        var basket = await _appEnvironmentService.BasketService.GetBasketAsync();

        if (basket != null)
        {
            basket.AddItemToBasket(item);
            await _appEnvironmentService.BasketService.UpdateBasketAsync(basket);
        }

        ReCalculateTotal();
    }

    [RelayCommand]
    private async Task DeleteAsync(BasketItem item)
    {
        _basketItems.Remove(item);

        var basket = await _appEnvironmentService.BasketService.GetBasketAsync();
        if (basket != null)
        {
            basket.RemoveItemFromBasket(item);
            await _appEnvironmentService.BasketService.UpdateBasketAsync(basket);
        }

        ReCalculateTotal();
    }

    public async Task ClearBasketItems()
    {
        _basketItems.Clear();

        await _appEnvironmentService.BasketService.ClearBasketAsync();

        ReCalculateTotal();
    }

    private void ReCalculateTotal()
    {
        OnPropertyChanged(nameof(BadgeCount));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (_basketItems?.Any() ?? false)
        {
            _appEnvironmentService.BasketService.LocalBasketItems = _basketItems;
            await NavigationService.NavigateToAsync("Checkout");
        }
    }

    // Upstream relied on Shell's automatic back button for this; Avalonia has no Shell chrome
    // (see CatalogItemViewModel.DismissCommand for the same gap), so Basket needs its own.
    [RelayCommand]
    private async Task DismissAsync()
    {
        await NavigationService.PopAsync();
    }
}
