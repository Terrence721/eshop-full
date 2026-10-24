using eShop.ClientApp.Models.Basket;

namespace eShop.ClientApp.UnitTests.Models.Basket
{
    [TestClass]
    public class CustomerBasketTests
    {
        [TestMethod]
        public void Items_StartsEmpty()
        {
            var basket = new CustomerBasket();

            Assert.IsEmpty(basket.Items);
            Assert.AreEqual(0, basket.ItemCount);
        }

        [TestMethod]
        public void AddItemToBasket_NewItem_AddsAndCountsQuantity()
        {
            var basket = new CustomerBasket();
            var item = new BasketItem { ProductId = 1, Quantity = 2 };

            basket.AddItemToBasket(item);

            Assert.HasCount(1, basket.Items);
            Assert.AreEqual(2, basket.ItemCount);
        }

        [TestMethod]
        public void AddItemToBasket_ExistingProductId_IncrementsQuantity()
        {
            var basket = new CustomerBasket();
            basket.AddItemToBasket(new BasketItem { ProductId = 1, Quantity = 1 });

            basket.AddItemToBasket(new BasketItem { ProductId = 1, Quantity = 5 });

            Assert.HasCount(1, basket.Items);
            Assert.AreEqual(2, basket.ItemCount);
        }

        [TestMethod]
        public void RemoveItemFromBasket_RemovesMatchingProduct()
        {
            var basket = new CustomerBasket();
            basket.AddItemToBasket(new BasketItem { ProductId = 1, Quantity = 1 });
            basket.AddItemToBasket(new BasketItem { ProductId = 2, Quantity = 1 });

            basket.RemoveItemFromBasket(new BasketItem { ProductId = 1 });

            Assert.HasCount(1, basket.Items);
            Assert.AreEqual(2, basket.Items[0].ProductId);
        }

        [TestMethod]
        public void ClearBasket_EmptiesItems()
        {
            var basket = new CustomerBasket();
            basket.AddItemToBasket(new BasketItem { ProductId = 1, Quantity = 1 });

            basket.ClearBasket();

            Assert.IsEmpty(basket.Items);
            Assert.AreEqual(0, basket.ItemCount);
        }
    }
}
