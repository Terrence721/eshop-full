using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using eShop.WebhookClient.Endpoints;
using eShop.WebhookClient.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace eShop.WebhookClient.UnitTests;

[TestClass]
public class HooksApiEndpointsTests
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

    private static (WebhooksClient client, FakeHandler handler) CreateWebhooksClient(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new FakeHandler(respond);
        return (new WebhooksClient(new HttpClient(handler) { BaseAddress = new Uri("http://webhooks-api") }), handler);
    }

    [TestMethod]
    public async Task GetHooksAsync_returns_the_loaded_webhooks()
    {
        var (client, _) = CreateWebhooksClient(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[] { new WebhookResponse { DestUrl = "https://a.example.com" } }),
        });

        var result = (Ok<IEnumerable<WebhookResponse>>)await HooksApiEndpoints.GetHooksAsync(client);

        Assert.AreEqual("https://a.example.com", result.Value!.Single().DestUrl);
    }

    [TestMethod]
    public void GetDefaultToken_returns_the_configured_token()
    {
        var options = Options.Create(new WebhookClientOptions { Token = "tok" });

        // Wraps an anonymous type - IValueHttpResult.Value (object?) sidesteps needing to
        // name it, same technique as BffEndpointsTests.GetUser_returns_the_claims....
        var ok = (IValueHttpResult)HooksApiEndpoints.GetDefaultToken(options);

        Assert.AreEqual("tok", ok.Value!.GetType().GetProperty("token")!.GetValue(ok.Value));
    }

    [TestMethod]
    public void BuildBaseUrl_uses_the_configured_SelfUrl_when_set()
    {
        var baseUrl = HooksApiEndpoints.BuildBaseUrl("https://configured.example.com/", "https", "ignored.example.com");

        Assert.AreEqual("https://configured.example.com/", baseUrl);
    }

    [TestMethod]
    public void BuildBaseUrl_derives_from_the_request_when_SelfUrl_is_not_set()
    {
        var baseUrl = HooksApiEndpoints.BuildBaseUrl(null, "https", "client.example.com");

        Assert.AreEqual("https://client.example.com/", baseUrl);
    }

    [TestMethod]
    public async Task RegisterHookAsync_registers_the_hook_using_the_derived_base_url()
    {
        var (client, handler) = CreateWebhooksClient(_ => new HttpResponseMessage(HttpStatusCode.Created));
        var options = Options.Create(new WebhookClientOptions());
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("client.example.com");

        var result = await HooksApiEndpoints.RegisterHookAsync(new RegisterHookRequest("tok"), client, options, httpContext);

        Assert.IsInstanceOfType<StatusCodeHttpResult>(result);
        Assert.AreEqual(201, ((StatusCodeHttpResult)result).StatusCode);
        using var sentBody = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.AreEqual("https://client.example.com/webhook-received", sentBody.RootElement.GetProperty("url").GetString());
        Assert.AreEqual("https://client.example.com/check", sentBody.RootElement.GetProperty("grantUrl").GetString());
        Assert.AreEqual("tok", sentBody.RootElement.GetProperty("token").GetString());
    }

    [TestMethod]
    public async Task GetReceivedAsync_returns_everything_in_the_repository()
    {
        var repository = new HooksRepository();
        var hook = new WebHookReceived { Data = "payload" };
        await repository.AddNew(hook);

        var result = (Ok<IEnumerable<WebHookReceived>>)await HooksApiEndpoints.GetReceivedAsync(repository);

        Assert.AreSame(hook, result.Value!.Single());
    }
}
