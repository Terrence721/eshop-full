using Microsoft.Extensions.Logging;
using NSubstitute;
using Webhooks.API.IntegrationEvents;
using Webhooks.API.Model;
using Webhooks.API.Services;

namespace Webhooks.API.UnitTests;

[TestClass]
public class OrderStatusChangedToPaidIntegrationEventHandlerTests
{
    [TestMethod]
    public async Task Handle_sends_the_OrderPaid_subscriptions_a_webhook_built_from_the_event()
    {
        var subscriptions = new[] { new WebhookSubscription { DestUrl = "https://example.com/hook", UserId = "user-1" } };
        var retriever = Substitute.For<IWebhooksRetriever>();
        retriever.GetSubscriptionsOfType(WebhookType.OrderPaid).Returns(subscriptions);
        var sender = Substitute.For<IWebhooksSender>();
        var handler = new OrderStatusChangedToPaidIntegrationEventHandler(
            retriever, sender, Substitute.For<ILogger<OrderStatusChangedToPaidIntegrationEventHandler>>());
        var @event = new OrderStatusChangedToPaidIntegrationEvent(1, [new OrderStockItem(10, 2)]);

        await handler.Handle(@event);

        await sender.Received(1).SendAll(subscriptions, Arg.Is<WebhookData>(d => d.Type == "OrderPaid"));
    }
}
