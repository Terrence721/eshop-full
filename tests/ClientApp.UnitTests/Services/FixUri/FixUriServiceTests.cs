using eShop.ClientApp.Models.Catalog;
using eShop.ClientApp.Models.Basket;
using eShop.ClientApp.Services.FixUri;
using eShop.ClientApp.Services.Settings;
using NSubstitute;

namespace eShop.ClientApp.UnitTests.Services.FixUri
{
    [TestClass]
    public class FixUriServiceTests
    {
        [TestMethod]
        public void FixCatalogItemPictureUri_WhenMocksEnabled_DoesNothing()
        {
            var settings = Substitute.For<ISettingsService>();
            settings.UseMocks.Returns(true);
            settings.DefaultEndpoint.Returns("http://default");
            settings.GatewayCatalogEndpointBase.Returns("http://10.0.0.2:5000");

            var service = new FixUriService(settings);
            var items = new List<CatalogItem> { new CatalogItem { Id = 4, PictureUri = "avares://ClientApp/Assets/Pics/4.webp" } };

            service.FixCatalogItemPictureUri(items);

            Assert.AreEqual("avares://ClientApp/Assets/Pics/4.webp", items[0].PictureUri);
        }

        [TestMethod]
        public void FixCatalogItemPictureUri_WhenRealModeAndGatewayDiffers_SetsPictureUri()
        {
            var settings = Substitute.For<ISettingsService>();
            settings.UseMocks.Returns(false);
            settings.DefaultEndpoint.Returns("http://default");
            settings.GatewayCatalogEndpointBase.Returns("http://10.0.0.2:5000");

            var service = new FixUriService(settings);
            var items = new List<CatalogItem> { new CatalogItem { Id = 4, PictureUri = "avares://ClientApp/Assets/Pics/4.webp" } };

            service.FixCatalogItemPictureUri(items);

            StringAssert.Contains(items[0].PictureUri, "api/catalog/items/4/pic");
            StringAssert.Contains(items[0].PictureUri, "10.0.0.2");
        }

        [TestMethod]
        public void FixCatalogItemPictureUri_Null_DoesNotThrow()
        {
            var settings = Substitute.For<ISettingsService>();
            settings.UseMocks.Returns(false);

            var service = new FixUriService(settings);

            service.FixCatalogItemPictureUri(null);
        }

        [TestMethod]
        public void FixBasketItemPictureUri_WhenRealMode_ReplacesIp()
        {
            var settings = Substitute.For<ISettingsService>();
            settings.UseMocks.Returns(false);
            settings.DefaultEndpoint.Returns("http://default");
            settings.IdentityEndpointBase.Returns("http://10.0.0.2:5000");

            var service = new FixUriService(settings);
            var items = new List<BasketItem> { new BasketItem { PictureUrl = "http://192.168.1.5:5000/pic/1" } };

            service.FixBasketItemPictureUri(items);

            Assert.AreEqual("http://10.0.0.2:5000/pic/1", items[0].PictureUrl);
        }

        [TestMethod]
        public void FixBasketItemPictureUri_NoIpInBase_DoesNothing()
        {
            var settings = Substitute.For<ISettingsService>();
            settings.UseMocks.Returns(false);
            settings.DefaultEndpoint.Returns("http://default");
            settings.IdentityEndpointBase.Returns("http://localhost:5000");

            var service = new FixUriService(settings);
            var original = "http://192.168.1.5:5000/pic/1";
            var items = new List<BasketItem> { new BasketItem { PictureUrl = original } };

            service.FixBasketItemPictureUri(items);

            Assert.AreEqual(original, items[0].PictureUrl);
        }
    }
}
