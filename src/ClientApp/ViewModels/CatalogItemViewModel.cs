using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;
using eShop.ClientApp.Messages;
using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Models.Catalog;
using eShop.ClientApp.Services;
using eShop.ClientApp.Services.AppEnvironment;

namespace eShop.ClientApp.ViewModels
{
    public partial class CatalogItemViewModel(IAppEnvironmentService appEnvironmentService, INavigationService navigationService) : ViewModelBase(navigationService)
    {
        private readonly IAppEnvironmentService _appEnvironmentService = appEnvironmentService;

        [ObservableProperty] public partial CatalogItem? CatalogItem { get; set; }

        [ObservableProperty] public partial Bitmap? ItemImage { get; set; }

        public override void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            base.ApplyQueryAttributes(query);

            CatalogItem = query.ValueAs<CatalogItem>("CatalogItem");
            _ = LoadImageAsync();
        }

        private async Task LoadImageAsync()
        {
            var uri = CatalogItem?.PictureUri;
            if (string.IsNullOrWhiteSpace(uri))
            {
                ItemImage = null;
                return;
            }

            try
            {
                Bitmap? bitmap = null;
                if (uri.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
                {
                    bitmap = await Task.Run(() =>
                    {
                        using var stream = AssetLoader.Open(new Uri(uri));
                        return new Bitmap(stream);
                    });
                }
                else
                {
                    bitmap = await Task.Run(async () =>
                    {
                        using var http = new HttpClient();
                        using var stream = await http.GetStreamAsync(uri);
                        return new Bitmap(stream);
                    });
                }

                ItemImage = bitmap;
            }
            catch
            {
                ItemImage = null;
            }
        }

        [RelayCommand]
        private async Task AddCatalogItemAsync()
        {
            if (CatalogItem is null)
            {
                return;
            }

            var basket = await _appEnvironmentService.BasketService.GetBasketAsync();
            if (basket is not null)
            {
                basket.AddItemToBasket(
                    new BasketItem
                    {
                        ProductId = CatalogItem.Id,
                        ProductName = CatalogItem.Name,
                        PictureUrl = CatalogItem.PictureUri ?? string.Empty,
                        UnitPrice = CatalogItem.Price,
                        Quantity = 1
                    });

                var basketUpdate = await _appEnvironmentService.BasketService.UpdateBasketAsync(basket);

                WeakReferenceMessenger.Default
                    .Send(new ProductCountChangedMessage(basketUpdate.ItemCount));

                await NavigationService.PopAsync();
            }
        }

        [RelayCommand]
        private async Task DismissAsync()
        {
            await NavigationService.PopAsync();
        }
    }
}
