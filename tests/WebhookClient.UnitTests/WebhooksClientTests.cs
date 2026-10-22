using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eShop.WebhookClient.Services;

namespace eShop.WebhookClient.UnitTests
{
    [TestClass]
    public class WebhooksClientTests
    {
        private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
        {
            public HttpRequestMessage? LastRequest { get; private set; }
            public string? LastRequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                LastRequestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
                return respond(request);
            }
        }

        private static (WebhooksClient client, FakeHandler handler) CreateClient(Func<HttpRequestMessage, HttpResponseMessage> respond)
        {
            var handler = new FakeHandler(respond);
            var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://webhooks-api") };
            return (new WebhooksClient(httpClient), handler);
        }

        [TestMethod]
        public async Task AddWebHookAsync_posts_the_payload_to_api_webhooks()
        {
            var (client, handler) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.Created));
            var payload = new WebhookSubscriptionRequest { Url = "https://a.example.com", GrantUrl = "https://a.example.com/grant", Event = "OrderPaid", Token = "tok" };

            var response = await client.AddWebHookAsync(payload);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(HttpMethod.Post, handler.LastRequest!.Method);
            Assert.AreEqual("http://webhooks-api/api/webhooks", handler.LastRequest.RequestUri!.ToString());
            using var sentBody = JsonDocument.Parse(handler.LastRequestBody!);
            Assert.AreEqual("https://a.example.com", sentBody.RootElement.GetProperty("url").GetString());
        }

        [TestMethod]
        public async Task LoadWebhooks_returns_the_deserialized_response()
        {
            var (client, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[] { new WebhookResponse { DestUrl = "https://a.example.com" } }),
            });

            var result = (await client.LoadWebhooks()).ToList();

            Assert.AreEqual("https://a.example.com", result.Single().DestUrl);
        }

        [TestMethod]
        public async Task LoadWebhooks_returns_empty_when_the_response_body_is_null()
        {
            var (client, _) = CreateClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<IEnumerable<WebhookResponse>?>(null),
            });

            var result = await client.LoadWebhooks();

            Assert.HasCount(0, result.ToList());
        }
    }
}
