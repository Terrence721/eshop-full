using eShop.WebhookClient.Endpoints;
using eShop.WebhookClient.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace eShop.WebhookClient.UnitTests;

[TestClass]
public class WebhookEndpointsTests
{
    [TestMethod]
    public void CheckToken_returns_ok_when_validation_is_disabled_regardless_of_the_token()
    {
        var response = new DefaultHttpContext().Response;

        var result = WebhookEndpoints.CheckToken("wrong-token", response, validateToken: false, tokenToValidate: "tok");

        Assert.IsInstanceOfType<Ok>(result.Result);
    }

    [TestMethod]
    public void CheckToken_returns_ok_and_echoes_the_header_when_the_token_matches()
    {
        var response = new DefaultHttpContext().Response;

        var result = WebhookEndpoints.CheckToken("tok", response, validateToken: true, tokenToValidate: "tok");

        Assert.IsInstanceOfType<Ok>(result.Result);
        Assert.AreEqual("tok", response.Headers[WebhookEndpoints.WebhookCheckHeader].Single());
    }

    [TestMethod]
    public void CheckToken_returns_bad_request_when_the_token_does_not_match()
    {
        var response = new DefaultHttpContext().Response;

        var result = WebhookEndpoints.CheckToken("wrong-token", response, validateToken: true, tokenToValidate: "tok");

        Assert.IsInstanceOfType<BadRequest<string>>(result.Result);
        Assert.IsTrue(response.Headers[WebhookEndpoints.WebhookCheckHeader].Count == 0);
    }

    [TestMethod]
    public async Task ReceiveWebhookAsync_stores_the_hook_when_validation_is_disabled()
    {
        var repository = new HooksRepository();
        var request = new DefaultHttpContext().Request;
        var hook = new WebhookData { Payload = "{}", When = DateTime.UtcNow };

        var result = await WebhookEndpoints.ReceiveWebhookAsync(
            hook, request, Substitute.For<ILogger<Program>>(), repository, validateToken: false, tokenToValidate: "tok");

        Assert.IsInstanceOfType<Ok<WebHookReceived>>(result);
        Assert.AreEqual(1, (await repository.GetAll()).Count());
    }

    [TestMethod]
    public async Task ReceiveWebhookAsync_stores_the_hook_when_the_token_matches()
    {
        var repository = new HooksRepository();
        var context = new DefaultHttpContext();
        context.Request.Headers[WebhookEndpoints.WebhookCheckHeader] = "tok";
        var hook = new WebhookData { Payload = "{}", When = DateTime.UtcNow };

        var result = await WebhookEndpoints.ReceiveWebhookAsync(
            hook, context.Request, Substitute.For<ILogger<Program>>(), repository, validateToken: true, tokenToValidate: "tok");

        Assert.IsInstanceOfType<Ok<WebHookReceived>>(result);
        Assert.AreEqual(1, (await repository.GetAll()).Count());
    }

    [TestMethod]
    public async Task ReceiveWebhookAsync_rejects_and_does_not_store_when_the_token_does_not_match()
    {
        var repository = new HooksRepository();
        var context = new DefaultHttpContext();
        context.Request.Headers[WebhookEndpoints.WebhookCheckHeader] = "wrong-token";
        var hook = new WebhookData { Payload = "{}", When = DateTime.UtcNow };

        var result = await WebhookEndpoints.ReceiveWebhookAsync(
            hook, context.Request, Substitute.For<ILogger<Program>>(), repository, validateToken: true, tokenToValidate: "tok");

        Assert.IsInstanceOfType<BadRequest>(result);
        Assert.AreEqual(0, (await repository.GetAll()).Count());
    }
}
