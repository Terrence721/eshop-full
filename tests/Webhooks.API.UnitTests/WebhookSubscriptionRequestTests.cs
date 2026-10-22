using System.ComponentModel.DataAnnotations;
using Webhooks.API.Model;

namespace Webhooks.API.UnitTests
{
    [TestClass]
    public class WebhookSubscriptionRequestTests
    {
        private static WebhookSubscriptionRequest CreateValidRequest() => new()
        {
            Url = "https://client.example.com/hook",
            GrantUrl = "https://client.example.com/grant",
            Event = "OrderPaid",
        };

        private static List<ValidationResult> Validate(WebhookSubscriptionRequest request) =>
            request.Validate(new ValidationContext(request)).ToList();

        [TestMethod]
        public void Validate_returns_no_errors_for_a_well_formed_request()
        {
            var results = Validate(CreateValidRequest());

            Assert.HasCount(0, results);
        }

        [TestMethod]
        public void Validate_rejects_a_malformed_GrantUrl()
        {
            var request = CreateValidRequest();
            request.GrantUrl = "not-a-url";

            var results = Validate(request);

            Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(WebhookSubscriptionRequest.GrantUrl))));
        }

        [TestMethod]
        public void Validate_rejects_a_malformed_Url()
        {
            var request = CreateValidRequest();
            request.Url = "not-a-url";

            var results = Validate(request);

            Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(WebhookSubscriptionRequest.Url))));
        }

        [TestMethod]
        public void Validate_rejects_an_unrecognized_event_name()
        {
            var request = CreateValidRequest();
            request.Event = "NotARealEvent";

            var results = Validate(request);

            Assert.IsTrue(results.Any(r => r.MemberNames.Contains(nameof(WebhookSubscriptionRequest.Event))));
        }

        [TestMethod]
        public void Validate_accepts_an_event_name_regardless_of_case()
        {
            var request = CreateValidRequest();
            request.Event = "orderpaid";

            var results = Validate(request);

            Assert.HasCount(0, results);
        }
    }
}
