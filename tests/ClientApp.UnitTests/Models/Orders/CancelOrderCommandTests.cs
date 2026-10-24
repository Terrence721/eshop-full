using eShop.ClientApp.Models.Orders;

namespace eShop.ClientApp.UnitTests.Models.Orders
{
    [TestClass]
    public class CancelOrderCommandTests
    {
        [TestMethod]
        public void OrderNumber_IsSetFromConstructor()
        {
            var command = new CancelOrderCommand(42);

            Assert.AreEqual(42, command.OrderNumber);
        }
    }
}
