using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Webhooks.API.Model;
using Webhooks.API.Services;

namespace Webhooks.API.UnitTests
{
    [TestClass]
    public class WebhooksSenderTests
    {
        // WebhooksSender sends through a real HttpClient (via IHttpClientFactory), so the only
        // way to observe what it actually sent is to supply the HttpClient's own message handler
        // and record what passes through it, rather than mocking HttpClient itself (sealed, and
        // SendAsync isn't virtual).
        private sealed class RecordingHandler : HttpMessageHandler
        {
            public List<HttpRequestMessage> Requests { get; } = [];

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }

        private static (WebhooksSender sender, RecordingHandler handler) CreateSender()
        {
            var handler = new RecordingHandler();
            // WebhooksSender calls the parameterless CreateClient() extension method, which is
            // static and can't be intercepted directly - it forwards to the real interface
            // method CreateClient(string name) under the hood, so that's what gets substituted.
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
            return (new WebhooksSender(httpClientFactory, Substitute.For<ILogger<WebhooksSender>>()), handler);
        }

        [TestMethod]
        public async Task SendAll_posts_the_serialized_webhook_data_to_the_subscriptions_DestUrl()
        {
            var (sender, handler) = CreateSender();
            var subscription = new WebhookSubscription { DestUrl = "https://client.example.com/hook", UserId = "user-1" };
            var data = new WebhookData(WebhookType.OrderPaid, new { OrderId = 1 });

            await sender.SendAll([subscription], data);

            var request = handler.Requests.Single();
            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual("https://client.example.com/hook", request.RequestUri!.ToString());
            var body = await request.Content!.ReadAsStringAsync();
            using var sentData = JsonDocument.Parse(body);
            Assert.AreEqual("OrderPaid", sentData.RootElement.GetProperty("Type").GetString());
            Assert.AreEqual(data.Payload, sentData.RootElement.GetProperty("Payload").GetString());
        }

        [TestMethod]
        public async Task SendAll_includes_the_token_header_when_the_subscription_has_one()
        {
            var (sender, handler) = CreateSender();
            var subscription = new WebhookSubscription { DestUrl = "https://client.example.com/hook", UserId = "user-1", Token = "secret-token" };

            await sender.SendAll([subscription], new WebhookData(WebhookType.OrderPaid, new { }));

            var request = handler.Requests.Single();
            Assert.AreEqual("secret-token", request.Headers.GetValues("X-eshop-whtoken").Single());
        }

        [TestMethod]
        public async Task SendAll_omits_the_token_header_when_the_subscription_has_none()
        {
            var (sender, handler) = CreateSender();
            var subscription = new WebhookSubscription { DestUrl = "https://client.example.com/hook", UserId = "user-1" };

            await sender.SendAll([subscription], new WebhookData(WebhookType.OrderPaid, new { }));

            var request = handler.Requests.Single();
            Assert.IsFalse(request.Headers.Contains("X-eshop-whtoken"));
        }

        [TestMethod]
        public async Task SendAll_sends_one_request_per_subscription()
        {
            var (sender, handler) = CreateSender();
            var subscriptions = new[]
            {
                new WebhookSubscription { DestUrl = "https://a.example.com", UserId = "user-1" },
                new WebhookSubscription { DestUrl = "https://b.example.com", UserId = "user-2" },
            };

            await sender.SendAll(subscriptions, new WebhookData(WebhookType.OrderPaid, new { }));

            CollectionAssert.AreEquivalent(
                new[] { "https://a.example.com/", "https://b.example.com/" },
                handler.Requests.Select(r => r.RequestUri!.ToString()).ToArray());
        }
    }
}
