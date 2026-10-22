using eShop.Catalog.API.Infrastructure.Exceptions;
using eShop.Catalog.API.Model;

namespace eShop.Catalog.API.UnitTests
{
    [TestClass]
    public class CatalogItemTests
    {
        [TestMethod]
        public void RemoveStock_throws_when_stock_is_empty()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 0 };

            Assert.ThrowsExactly<CatalogDomainException>(() => item.RemoveStock(1));
        }

        [TestMethod]
        public void RemoveStock_throws_when_quantityDesired_is_zero()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 10 };

            Assert.ThrowsExactly<CatalogDomainException>(() => item.RemoveStock(0));
        }

        [TestMethod]
        public void RemoveStock_throws_when_quantityDesired_is_negative()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 10 };

            Assert.ThrowsExactly<CatalogDomainException>(() => item.RemoveStock(-1));
        }

        [TestMethod]
        public void RemoveStock_removes_the_full_quantity_when_stock_is_sufficient()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 10 };

            var removed = item.RemoveStock(4);

            Assert.AreEqual(4, removed);
            Assert.AreEqual(6, item.AvailableStock);
        }

        [TestMethod]
        public void RemoveStock_removes_only_whats_available_when_stock_is_insufficient()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 3 };

            var removed = item.RemoveStock(10);

            Assert.AreEqual(3, removed);
            Assert.AreEqual(0, item.AvailableStock);
        }

        [TestMethod]
        public void AddStock_adds_the_full_quantity_when_under_the_max_threshold()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 10, MaxStockThreshold = 100 };

            var added = item.AddStock(20);

            Assert.AreEqual(20, added);
            Assert.AreEqual(30, item.AvailableStock);
        }

        [TestMethod]
        public void AddStock_clamps_to_the_max_threshold_when_the_quantity_would_exceed_it()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 90, MaxStockThreshold = 100 };

            var added = item.AddStock(50);

            Assert.AreEqual(10, added);
            Assert.AreEqual(100, item.AvailableStock);
        }

        [TestMethod]
        public void AddStock_clears_OnReorder_even_when_already_false()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 10, MaxStockThreshold = 100, OnReorder = false };

            item.AddStock(5);

            Assert.IsFalse(item.OnReorder);
        }

        [TestMethod]
        public void AddStock_clears_OnReorder_when_previously_true()
        {
            var item = new CatalogItem("Widget") { AvailableStock = 10, MaxStockThreshold = 100, OnReorder = true };

            item.AddStock(5);

            Assert.IsFalse(item.OnReorder);
        }
    }
}
