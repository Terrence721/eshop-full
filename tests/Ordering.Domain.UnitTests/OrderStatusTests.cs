using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;

namespace eShop.Ordering.Domain.UnitTests
{
    // OrderStatus's explicit ordinal values are persisted (Ordering.Infrastructure
    // stores the enum as an int column) - a reorder here would silently corrupt
    // every already-persisted order's status. Guards the real contract: the
    // assigned numbers, not just the member names.
    [TestClass]
    public class OrderStatusTests
    {
        [TestMethod]
        public void Ordinal_values_are_stable()
        {
            Assert.AreEqual(1, (int)OrderStatus.Submitted);
            Assert.AreEqual(2, (int)OrderStatus.AwaitingValidation);
            Assert.AreEqual(3, (int)OrderStatus.StockConfirmed);
            Assert.AreEqual(4, (int)OrderStatus.Paid);
            Assert.AreEqual(5, (int)OrderStatus.Shipped);
            Assert.AreEqual(6, (int)OrderStatus.Cancelled);
        }
    }
}
