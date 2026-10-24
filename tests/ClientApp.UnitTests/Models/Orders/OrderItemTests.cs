using eShop.ClientApp.Models.Orders;

namespace eShop.ClientApp.UnitTests.Models.Orders
{
    [TestClass]
    public class OrderItemTests
    {
        [TestMethod]
        public void DefaultValues_AreSet()
        {
            var item = new OrderItem();

            Assert.AreEqual(0L, item.ProductId);
            Assert.IsNull(item.OrderId);
            Assert.AreEqual(0m, item.UnitPrice);
            Assert.IsNull(item.ProductName);
            Assert.IsNull(item.PictureUrl);
            Assert.AreEqual(0, item.Quantity);
            Assert.AreEqual(0m, item.Discount);
        }

        [TestMethod]
        public void Properties_CanBeSet()
        {
            var orderId = Guid.NewGuid();

            var item = new OrderItem
            {
                ProductId = 7,
                OrderId = orderId,
                UnitPrice = 12.50m,
                ProductName = "Mug",
                PictureUrl = "pic.png",
                Quantity = 3,
                Discount = 1.00m,
            };

            Assert.AreEqual(7L, item.ProductId);
            Assert.AreEqual(orderId, item.OrderId);
            Assert.AreEqual(12.50m, item.UnitPrice);
            Assert.AreEqual("Mug", item.ProductName);
            Assert.AreEqual("pic.png", item.PictureUrl);
            Assert.AreEqual(3, item.Quantity);
            Assert.AreEqual(1.00m, item.Discount);
        }

        [TestMethod]
        public void Total_ComputesQuantityTimesUnitPrice()
        {
            var item = new OrderItem { Quantity = 4, UnitPrice = 5.25m };

            Assert.AreEqual(21.00m, item.Total);
        }

        [TestMethod]
        public void ToString_IncludesProductIdAndQuantity()
        {
            var item = new OrderItem { ProductId = 9, Quantity = 2 };

            Assert.AreEqual("Product Id: 9, Quantity: 2", item.ToString());
        }
    }
}
