using eShop.ClientApp.Models.Catalog;

namespace eShop.ClientApp.UnitTests.Models.Catalog
{
    [TestClass]
    public class CatalogTypeTests
    {
        [TestMethod]
        public void DefaultValues_AreSet()
        {
            var type = new CatalogType();

            Assert.AreEqual(0, type.Id);
            Assert.IsNull(type.Type);
        }

        [TestMethod]
        public void Properties_CanBeSet()
        {
            var type = new CatalogType { Id = 3, Type = "Mug" };

            Assert.AreEqual(3, type.Id);
            Assert.AreEqual("Mug", type.Type);
        }

        [TestMethod]
        public void ToString_ReturnsType()
        {
            var type = new CatalogType { Type = "T-Shirt" };

            Assert.AreEqual("T-Shirt", type.ToString());
        }
    }
}
