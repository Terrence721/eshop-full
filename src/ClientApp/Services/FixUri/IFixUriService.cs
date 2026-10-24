using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Models.Catalog;

namespace eShop.ClientApp.Services.FixUri
{
    public interface IFixUriService
    {
        void FixCatalogItemPictureUri(IEnumerable<CatalogItem>? catalogItems);
        void FixBasketItemPictureUri(IEnumerable<BasketItem>? basketItems);
    }
}
