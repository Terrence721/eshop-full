using eShop.ClientApp.Models.Marketing;

namespace eShop.ClientApp.UnitTests.Models.Marketing
{
    [TestClass]
    public class CampaignTests
    {
        [TestMethod]
        public void DefaultValues_AreSet()
        {
            var campaign = new Campaign();

            Assert.AreEqual(0, campaign.Id);
            Assert.IsNull(campaign.Name);
            Assert.IsNull(campaign.Description);
            Assert.AreEqual(default, campaign.From);
            Assert.AreEqual(default, campaign.To);
            Assert.IsNull(campaign.PictureUri);
        }

        [TestMethod]
        public void Properties_CanBeSet()
        {
            var from = new DateTime(2026, 1, 1);
            var to = new DateTime(2026, 1, 31);

            var campaign = new Campaign
            {
                Id = 1,
                Name = "Winter Sale",
                Description = "Discounts on winter gear",
                From = from,
                To = to,
                PictureUri = "campaign.png",
            };

            Assert.AreEqual(1, campaign.Id);
            Assert.AreEqual("Winter Sale", campaign.Name);
            Assert.AreEqual("Discounts on winter gear", campaign.Description);
            Assert.AreEqual(from, campaign.From);
            Assert.AreEqual(to, campaign.To);
            Assert.AreEqual("campaign.png", campaign.PictureUri);
        }
    }
}
