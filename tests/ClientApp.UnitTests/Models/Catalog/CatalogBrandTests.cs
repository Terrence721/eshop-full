using eShop.ClientApp.Models.Catalog;

namespace eShop.ClientApp.UnitTests.Models.Catalog
{
    [TestClass]
    public class CatalogBrandTests
    {
        [TestMethod]
        public void DefaultValues_AreSet()
        {
            var brand = new CatalogBrand();

            Assert.AreEqual(0, brand.Id);
            Assert.IsNull(brand.Brand);
        }

        [TestMethod]
        public void Properties_CanBeSet()
        {
            var brand = new CatalogBrand { Id = 7, Brand = "Azure" };

            Assert.AreEqual(7, brand.Id);
            Assert.AreEqual("Azure", brand.Brand);
        }

        [TestMethod]
        public void ToString_ReturnsBrand()
        {
            var brand = new CatalogBrand { Brand = "Visual Studio" };

            Assert.AreEqual("Visual Studio", brand.ToString());
        }
    }
}
