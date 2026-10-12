using Microsoft.EntityFrameworkCore;
using Webhooks.API.Infrastructure;
using Webhooks.API.Model;
using Webhooks.API.Services;

namespace Webhooks.API.UnitTests;

[TestClass]
public class WebhooksRetrieverTests
{
    // WebhooksContext's DbSet<T> property is `required`, which a normal object-initializer
    // `new` call site would clobber (it runs after the base DbContext constructor's own
    // auto-wiring) - constructed via reflection instead, same technique as
    // Ordering.Infrastructure.UnitTests' TestOrderingContext.
    private static WebhooksContext CreateContext() =>
        (WebhooksContext)Activator.CreateInstance(
            typeof(WebhooksContext),
            new DbContextOptionsBuilder<WebhooksContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)!;

    [TestMethod]
    public async Task GetSubscriptionsOfType_returns_only_subscriptions_of_the_requested_type()
    {
        using var context = CreateContext();
        context.Subscriptions.AddRange(
            new WebhookSubscription { Type = WebhookType.OrderPaid, DestUrl = "https://a.example.com", UserId = "user-1" },
            new WebhookSubscription { Type = WebhookType.OrderShipped, DestUrl = "https://b.example.com", UserId = "user-1" },
            new WebhookSubscription { Type = WebhookType.OrderPaid, DestUrl = "https://c.example.com", UserId = "user-2" });
        await context.SaveChangesAsync();
        var retriever = new WebhooksRetriever(context);

        var result = (await retriever.GetSubscriptionsOfType(WebhookType.OrderPaid)).ToList();

        Assert.HasCount(2, result);
        CollectionAssert.AreEquivalent(new[] { "https://a.example.com", "https://c.example.com" }, result.Select(s => s.DestUrl).ToArray());
    }

    [TestMethod]
    public async Task GetSubscriptionsOfType_returns_empty_when_none_match()
    {
        using var context = CreateContext();
        context.Subscriptions.Add(new WebhookSubscription { Type = WebhookType.OrderShipped, DestUrl = "https://a.example.com", UserId = "user-1" });
        await context.SaveChangesAsync();
        var retriever = new WebhooksRetriever(context);

        var result = await retriever.GetSubscriptionsOfType(WebhookType.CatalogItemPriceChange);

        Assert.HasCount(0, result.ToList());
    }
}
