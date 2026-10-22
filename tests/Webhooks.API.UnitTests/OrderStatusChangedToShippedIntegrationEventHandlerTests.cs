using Microsoft.Extensions.Logging;
using NSubstitute;
using Webhooks.API.IntegrationEvents;
using Webhooks.API.Model;
using Webhooks.API.Services;

namespace Webhooks.API.UnitTests
{
    [TestClass]
    public class OrderStatusChangedToShippedIntegrationEventHandlerTests
    {
        [TestMethod]
        public async Task Handle_sends_the_OrderShipped_subscriptions_a_webhook_built_from_the_event()
        {
            var subscriptions = new[] { new WebhookSubscription { DestUrl = "https://example.com/hook", UserId = "user-1" } };
            var retriever = Substitute.For<IWebhooksRetriever>();
            retriever.GetSubscriptionsOfType(WebhookType.OrderShipped).Returns(subscriptions);
            var sender = Substitute.For<IWebhooksSender>();
            var handler = new OrderStatusChangedToShippedIntegrationEventHandler(
                retriever, sender, Substitute.For<ILogger<OrderStatusChangedToShippedIntegrationEventHandler>>());
            var @event = new OrderStatusChangedToShippedIntegrationEvent(1, "Shipped", "Alice");

            await handler.Handle(@event);

            await sender.Received(1).SendAll(subscriptions, Arg.Is<WebhookData>(d => d.Type == "OrderShipped"));
        }
    }
}
