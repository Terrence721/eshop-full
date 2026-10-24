using System.Diagnostics;
using System.Text.RegularExpressions;
using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Models.Catalog;
using eShop.ClientApp.Services.Settings;

namespace eShop.ClientApp.Services.FixUri
{
    public class FixUriService(ISettingsService settingsService) : IFixUriService
    {
        private const string ApiVersion = "api-version=2.0";

        private readonly ISettingsService _settingsService = settingsService;

        private static readonly Regex IpRegex = new(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Compiled);

        public void FixCatalogItemPictureUri(IEnumerable<CatalogItem>? catalogItems)
        {
            if (catalogItems is null)
            {
                return;
            }

            try
            {
                if (!_settingsService.UseMocks && _settingsService.GatewayCatalogEndpointBase != _settingsService.DefaultEndpoint)
                {
                    foreach (var catalogItem in catalogItems)
                    {
                        catalogItem.PictureUri = Path.Combine(_settingsService.GatewayCatalogEndpointBase, $"api/catalog/items/{catalogItem.Id}/pic?{ApiVersion}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void FixBasketItemPictureUri(IEnumerable<BasketItem>? basketItems)
        {
            if (basketItems is null)
            {
                return;
            }

            try
            {
                if (!_settingsService.UseMocks && _settingsService.IdentityEndpointBase != _settingsService.DefaultEndpoint)
                {
                    var localResult = IpRegex.Matches(_settingsService.IdentityEndpointBase);
                    if (localResult.Count > 0)
                    {
                        var localIp = localResult[0].Value;
                        foreach (var basketItem in basketItems)
                        {
                            var serverResult = IpRegex.Matches(basketItem.PictureUrl);
                            if (serverResult.Count == 0)
                            {
                                continue;
                            }

                            var serviceIp = serverResult[0].Value;
                            basketItem.PictureUrl = basketItem.PictureUrl.Replace(serviceIp, localIp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
