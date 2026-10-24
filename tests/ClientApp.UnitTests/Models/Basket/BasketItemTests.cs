using eShop.ClientApp.Models.Basket;

namespace eShop.ClientApp.UnitTests.Models.Basket
{
    [TestClass]
    public class BasketItemTests
    {
        [TestMethod]
        public void Total_ComputesQuantityTimesUnitPrice()
        {
            var item = new BasketItem { Quantity = 3, UnitPrice = 12.50m };

            Assert.AreEqual(37.50m, item.Total);
        }

        [TestMethod]
        public void HasNewPrice_TrueWhenOldUnitPriceNonZero()
        {
            var item = new BasketItem { OldUnitPrice = 9.99m };

            Assert.IsTrue(item.HasNewPrice);
        }

        [TestMethod]
        public void HasNewPrice_FalseWhenOldUnitPriceZero()
        {
            var item = new BasketItem { OldUnitPrice = 0m };

            Assert.IsFalse(item.HasNewPrice);
        }

        [TestMethod]
        public void ToString_IncludesProductIdAndQuantity()
        {
            var item = new BasketItem { ProductId = 7, Quantity = 4 };

            Assert.AreEqual("Product Id: 7, Quantity: 4", item.ToString());
        }

        [TestMethod]
        public void Quantity_StartsAtDefault()
        {
            var item = new BasketItem();

            Assert.AreEqual(0, item.Quantity);
        }
    }
}
