using eShop.ClientApp.Models.Catalog;

namespace eShop.ClientApp.UnitTests.Models.Catalog
{
    [TestClass]
    public class CatalogRootTests
    {
        [TestMethod]
        public void Defaults_AreZeroAndNull()
        {
            var root = new CatalogRoot();

            Assert.AreEqual(0, root.PageIndex);
            Assert.AreEqual(0, root.PageSize);
            Assert.AreEqual(0, root.Count);
            Assert.IsNull(root.Data);
        }

        [TestMethod]
        public void Properties_CanBeSet()
        {
            var items = new List<CatalogItem> { new CatalogItem { Name = "Item" } };

            var root = new CatalogRoot
            {
                PageIndex = 2,
                PageSize = 10,
                Count = 25,
                Data = items,
            };

            Assert.AreEqual(2, root.PageIndex);
            Assert.AreEqual(10, root.PageSize);
            Assert.AreEqual(25, root.Count);
            Assert.AreSame(items, root.Data);
        }
    }
}
