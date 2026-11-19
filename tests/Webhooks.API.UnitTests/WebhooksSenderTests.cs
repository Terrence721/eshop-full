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

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw new HttpRequestException("network failure");
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

        private static WebhooksSender CreateThrowingSender()
        {
            var handler = new ThrowingHandler();
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
            return new WebhooksSender(httpClientFactory, Substitute.For<ILogger<WebhooksSender>>());
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

        [TestMethod]
        public async Task SendAll_propagates_exception_when_http_send_fails()
        {
            var sender = CreateThrowingSender();
            var subscription = new WebhookSubscription { DestUrl = "https://client.example.com/hook", UserId = "user-1" };

            Exception? ex = null;
            try
            {
                await sender.SendAll([subscription], new WebhookData(WebhookType.OrderPaid, new { }));
            }
            catch (HttpRequestException e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.AreEqual("network failure", ex!.Message);
        }

        [TestMethod]
        public async Task SendAll_sends_requests_concurrently_not_sequentially()
        {
            var startTimes = new List<DateTime>();
            var tcs = new TaskCompletionSource<bool>();

            var handler = new HttpMessageHandlerWrapper(async (request, ct) =>
            {
                lock (startTimes)
                {
                    startTimes.Add(DateTime.UtcNow);
                }
                await tcs.Task.WaitAsync(ct);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });

            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

            var sender = new WebhooksSender(httpClientFactory, Substitute.For<ILogger<WebhooksSender>>());
            var subscriptions = new[]
            {
                new WebhookSubscription { DestUrl = "https://a.example.com", UserId = "user-1" },
                new WebhookSubscription { DestUrl = "https://b.example.com", UserId = "user-2" },
            };

            var sendTask = sender.SendAll(subscriptions, new WebhookData(WebhookType.OrderPaid, new { }));

            // Wait for both requests to start
            while (startTimes.Count < 2) await Task.Delay(10);
            var timeDiff = (startTimes[1] - startTimes[0]).TotalMilliseconds;

            // Complete both
            tcs.TrySetResult(true);
            await sendTask;

            // Both should start almost simultaneously (concurrent)
            Assert.IsTrue(timeDiff < 100, "Requests should start concurrently, not sequentially");
        }

        [TestMethod]
        public async Task SendAll_sets_content_type_to_application_json()
        {
            var (sender, handler) = CreateSender();
            var subscription = new WebhookSubscription { DestUrl = "https://client.example.com/hook", UserId = "user-1" };
            var data = new WebhookData(WebhookType.OrderPaid, new { OrderId = 1 });

            await sender.SendAll([subscription], data);

            var request = handler.Requests.Single();
            Assert.AreEqual("application/json", request.Content!.Headers.ContentType!.MediaType);
        }
    }

    // Helper for concurrency test
    internal sealed class HttpMessageHandlerWrapper : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _func;

        public HttpMessageHandlerWrapper(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> func)
        {
            _func = func;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _func(request, cancellationToken);
    }
}
