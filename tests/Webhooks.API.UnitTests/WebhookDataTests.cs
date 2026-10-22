using System.Text.Json;
using Webhooks.API.Model;

namespace Webhooks.API.UnitTests
{
    [TestClass]
    public class WebhookDataTests
    {
        [TestMethod]
        public void Constructor_sets_the_type_from_the_hook_type_enum()
        {
            var data = new WebhookData(WebhookType.OrderShipped, new { OrderId = 1 });

            Assert.AreEqual("OrderShipped", data.Type);
        }

        [TestMethod]
        public void Constructor_serializes_the_payload_as_json()
        {
            var data = new WebhookData(WebhookType.OrderPaid, new { OrderId = 42, Status = "Paid" });

            using var payload = JsonDocument.Parse(data.Payload);
            Assert.AreEqual(42, payload.RootElement.GetProperty("OrderId").GetInt32());
            Assert.AreEqual("Paid", payload.RootElement.GetProperty("Status").GetString());
        }

        [TestMethod]
        public void Constructor_sets_When_to_the_current_time()
        {
            var before = DateTime.UtcNow;

            var data = new WebhookData(WebhookType.CatalogItemPriceChange, new { });

            Assert.IsTrue(data.When >= before && data.When <= DateTime.UtcNow);
        }
    }
}
