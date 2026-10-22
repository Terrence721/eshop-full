using eShop.Ordering.Domain.AggregatesModel.OrderAggregate;
using eShop.Ordering.Domain.Exceptions;

namespace eShop.Ordering.Domain.UnitTests
{
    [TestClass]
    public class OrderItemTests
    {
        [TestMethod]
        public void Constructor_sets_all_properties()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 2.00m, "widget.png", 3);

            Assert.AreEqual(1, item.ProductId);
            Assert.AreEqual("Widget", item.ProductName);
            Assert.AreEqual(10.00m, item.UnitPrice);
            Assert.AreEqual(2.00m, item.Discount);
            Assert.AreEqual("widget.png", item.PictureUrl);
            Assert.AreEqual(3, item.Units);
        }

        [TestMethod]
        public void Constructor_defaults_units_to_one()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 0, "widget.png");

            Assert.AreEqual(1, item.Units);
        }

        [TestMethod]
        public void Constructor_throws_when_units_is_zero()
        {
            Assert.ThrowsExactly<OrderingDomainException>(
                () => new OrderItem(1, "Widget", 10.00m, 0, "widget.png", 0));
        }

        [TestMethod]
        public void Constructor_throws_when_units_is_negative()
        {
            Assert.ThrowsExactly<OrderingDomainException>(
                () => new OrderItem(1, "Widget", 10.00m, 0, "widget.png", -1));
        }

        [TestMethod]
        public void Constructor_throws_when_discount_exceeds_the_item_total()
        {
            Assert.ThrowsExactly<OrderingDomainException>(
                () => new OrderItem(1, "Widget", 10.00m, 11.00m, "widget.png", 1));
        }

        [TestMethod]
        public void Constructor_allows_a_discount_equal_to_the_item_total()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 10.00m, "widget.png", 1);

            Assert.AreEqual(10.00m, item.Discount);
        }

        [TestMethod]
        public void SetNewDiscount_updates_the_discount()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 0, "widget.png");

            item.SetNewDiscount(5.00m);

            Assert.AreEqual(5.00m, item.Discount);
        }

        [TestMethod]
        public void SetNewDiscount_throws_when_negative()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 0, "widget.png");

            Assert.ThrowsExactly<OrderingDomainException>(() => item.SetNewDiscount(-1));
        }

        [TestMethod]
        public void AddUnits_increases_units()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);

            item.AddUnits(3);

            Assert.AreEqual(5, item.Units);
        }

        [TestMethod]
        public void AddUnits_allows_zero()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);

            item.AddUnits(0);

            Assert.AreEqual(2, item.Units);
        }

        [TestMethod]
        public void AddUnits_throws_when_negative()
        {
            var item = new OrderItem(1, "Widget", 10.00m, 0, "widget.png", 2);

            Assert.ThrowsExactly<OrderingDomainException>(() => item.AddUnits(-1));
        }
    }
}
