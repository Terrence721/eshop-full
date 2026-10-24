using eShop.ClientApp.Models.Marketing;

namespace eShop.ClientApp.UnitTests.Models.Marketing
{
    [TestClass]
    public class CampaignRootTests
    {
        [TestMethod]
        public void Defaults_AreZeroAndNull()
        {
            var root = new CampaignRoot();

            Assert.AreEqual(0, root.PageIndex);
            Assert.AreEqual(0, root.PageSize);
            Assert.AreEqual(0, root.Count);
            Assert.IsNull(root.Data);
        }

        [TestMethod]
        public void Properties_CanBeSet()
        {
            var items = new List<CampaignItem> { new CampaignItem { Name = "Campaign" } };

            var root = new CampaignRoot
            {
                PageIndex = 1,
                PageSize = 20,
                Count = 1,
                Data = items,
            };

            Assert.AreEqual(1, root.PageIndex);
            Assert.AreEqual(20, root.PageSize);
            Assert.AreEqual(1, root.Count);
            Assert.AreSame(items, root.Data);
        }
    }
}
