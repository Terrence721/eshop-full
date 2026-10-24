using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Services.Basket;
using eShop.ClientApp.Services.Identity;
using eShop.ClientApp.Services.FixUri;
using eShop.ClientApp.Services.Settings;
using NSubstitute;

namespace eShop.ClientApp.UnitTests.Services.Basket
{
    [TestClass]
    public class BasketServiceTests
    {
        private IIdentityService Identity() 
        {
            var identity = Substitute.For<IIdentityService>();
            identity.GetAuthTokenAsync().Returns(string.Empty);
            return identity;
        }

        [TestMethod]
        public async Task GetBasketAsync_WithoutToken_ReturnsEmptyBasket()
        {
            var settings = Substitute.For<ISettingsService>();
            var fixUri = Substitute.For<IFixUriService>();
            var service = new BasketService(Identity(), settings, fixUri);

            var basket = await service.GetBasketAsync();

            Assert.IsNotNull(basket);
            Assert.IsEmpty(basket.Items);
        }

        [TestMethod]
        public async Task UpdateBasketAsync_WithoutToken_ReturnsInputBasket()
        {
            var settings = Substitute.For<ISettingsService>();
            var fixUri = Substitute.For<IFixUriService>();
            var service = new BasketService(Identity(), settings, fixUri);
            var basket = new CustomerBasket();
            basket.AddItemToBasket(new BasketItem { ProductId = 1, Quantity = 2 });

            var result = await service.UpdateBasketAsync(basket);

            Assert.AreSame(basket, result);
            Assert.HasCount(1, result.Items);
        }

        [TestMethod]
        public async Task ClearBasketAsync_WithoutToken_DoesNotThrow()
        {
            var settings = Substitute.For<ISettingsService>();
            var fixUri = Substitute.For<IFixUriService>();
            var service = new BasketService(Identity(), settings, fixUri);

            await service.ClearBasketAsync();
        }

        [TestMethod]
        public void Dispose_DoesNotThrow()
        {
            var settings = Substitute.For<ISettingsService>();
            var fixUri = Substitute.For<IFixUriService>();
            var service = new BasketService(Identity(), settings, fixUri);

            service.Dispose();
        }
    }
}
