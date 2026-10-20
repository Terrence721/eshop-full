using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Webhooks.API;
using Webhooks.API.Infrastructure;
using Webhooks.API.Model;
using Webhooks.API.Services;

namespace Webhooks.API.UnitTests;

[TestClass]
public class WebHooksApiTests
{
    private static WebhooksContext CreateContext() =>
        (WebhooksContext)Activator.CreateInstance(
            typeof(WebhooksContext),
            new DbContextOptionsBuilder<WebhooksContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)!;

    private static ClaimsPrincipal CreateUser(string userId) =>
        new(new ClaimsIdentity([new Claim("sub", userId)]));

    [TestMethod]
    public async Task GetAllAsync_returns_only_the_current_users_subscriptions()
    {
        using var context = CreateContext();
        context.Subscriptions.AddRange(
            new WebhookSubscription { Type = WebhookType.OrderPaid, DestUrl = "https://a.example.com", UserId = "user-1" },
            new WebhookSubscription { Type = WebhookType.OrderShipped, DestUrl = "https://b.example.com", UserId = "user-2" });
        await context.SaveChangesAsync();

        var result = await WebHooksApi.GetAllAsync(context, CreateUser("user-1"));

        Assert.AreEqual("https://a.example.com", result.Value!.Single().DestUrl);
    }

    [TestMethod]
    public async Task GetByIdAsync_returns_the_subscription_when_owned_by_the_user()
    {
        using var context = CreateContext();
        var subscription = new WebhookSubscription { Type = WebhookType.OrderPaid, DestUrl = "https://a.example.com", UserId = "user-1" };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var result = await WebHooksApi.GetByIdAsync(context, CreateUser("user-1"), subscription.Id);

        var ok = (Ok<WebhookSubscription>)result.Result;
        Assert.AreEqual("https://a.example.com", ok.Value!.DestUrl);
    }

    [TestMethod]
    public async Task GetByIdAsync_returns_not_found_when_owned_by_a_different_user()
    {
        using var context = CreateContext();
        var subscription = new WebhookSubscription { Type = WebhookType.OrderPaid, DestUrl = "https://a.example.com", UserId = "user-1" };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var result = await WebHooksApi.GetByIdAsync(context, CreateUser("user-2"), subscription.Id);

        Assert.IsInstanceOfType<NotFound<string>>(result.Result);
    }

    [TestMethod]
    public async Task CreateAsync_creates_the_subscription_when_the_grant_url_is_valid()
    {
        using var context = CreateContext();
        var request = new WebhookSubscriptionRequest { Url = "https://a.example.com/hook", GrantUrl = "https://a.example.com/grant", Event = "OrderPaid", Token = "tok" };
        var grantTester = Substitute.For<IGrantUrlTesterService>();
        grantTester.TestGrantUrl(request.Url, request.GrantUrl, "tok").Returns(true);

        var result = await WebHooksApi.CreateAsync(request, grantTester, context, CreateUser("user-1"));

        Assert.IsInstanceOfType<Created>(result.Result);
        var saved = context.Subscriptions.Single();
        Assert.AreEqual("https://a.example.com/hook", saved.DestUrl);
        Assert.AreEqual(WebhookType.OrderPaid, saved.Type);
        Assert.AreEqual("user-1", saved.UserId);
    }

    [TestMethod]
    public async Task CreateAsync_returns_bad_request_when_the_grant_url_is_invalid()
    {
        using var context = CreateContext();
        var request = new WebhookSubscriptionRequest { Url = "https://a.example.com/hook", GrantUrl = "https://a.example.com/grant", Event = "OrderPaid" };
        var grantTester = Substitute.For<IGrantUrlTesterService>();
        grantTester.TestGrantUrl(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await WebHooksApi.CreateAsync(request, grantTester, context, CreateUser("user-1"));

        Assert.IsInstanceOfType<BadRequest<string>>(result.Result);
        Assert.HasCount(0, context.Subscriptions.ToList());
    }

    [TestMethod]
    public async Task DeleteAsync_removes_the_subscription_and_returns_accepted()
    {
        using var context = CreateContext();
        var subscription = new WebhookSubscription { Type = WebhookType.OrderPaid, DestUrl = "https://a.example.com", UserId = "user-1" };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var result = await WebHooksApi.DeleteAsync(context, CreateUser("user-1"), subscription.Id);

        Assert.IsInstanceOfType<Accepted>(result.Result);
        Assert.HasCount(0, context.Subscriptions.ToList());
    }

    [TestMethod]
    public async Task DeleteAsync_returns_not_found_when_owned_by_a_different_user()
    {
        using var context = CreateContext();
        var subscription = new WebhookSubscription { Type = WebhookType.OrderPaid, DestUrl = "https://a.example.com", UserId = "user-1" };
        context.Subscriptions.Add(subscription);
        await context.SaveChangesAsync();

        var result = await WebHooksApi.DeleteAsync(context, CreateUser("user-2"), subscription.Id);

        Assert.IsInstanceOfType<NotFound<string>>(result.Result);
        Assert.HasCount(1, context.Subscriptions.ToList());
    }
}
