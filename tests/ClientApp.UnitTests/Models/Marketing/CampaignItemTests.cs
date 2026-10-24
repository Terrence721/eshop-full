using eShop.ClientApp.Models.Marketing;

namespace eShop.ClientApp.UnitTests.Models.Marketing
{
    [TestClass]
    public class CampaignItemTests
    {
        [TestMethod]
        public void DefaultValues_AreSet()
        {
            var item = new CampaignItem();

            Assert.AreEqual(0, item.Id);
            Assert.IsNull(item.Name);
            Assert.IsNull(item.Description);
            Assert.AreEqual(default, item.From);
            Assert.AreEqual(default, item.To);
            Assert.IsNull(item.PictureUri);
            Assert.IsNull(item.DetailsUri);
        }

        [TestMethod]
        public void Properties_CanBeSet()
        {
            var from = new DateTime(2026, 2, 1);
            var to = new DateTime(2026, 2, 28);

            var item = new CampaignItem
            {
                Id = 2,
                Name = "Buy One Get One",
                Description = "Selected gear",
                From = from,
                To = to,
                PictureUri = "campaign-item.png",
                DetailsUri = "details://2",
            };

            Assert.AreEqual(2, item.Id);
            Assert.AreEqual("Buy One Get One", item.Name);
            Assert.AreEqual("Selected gear", item.Description);
            Assert.AreEqual(from, item.From);
            Assert.AreEqual(to, item.To);
            Assert.AreEqual("campaign-item.png", item.PictureUri);
            Assert.AreEqual("details://2", item.DetailsUri);
        }
    }
}
